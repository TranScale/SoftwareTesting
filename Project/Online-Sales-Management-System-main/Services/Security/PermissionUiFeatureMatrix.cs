using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;

namespace OnlineSalesManagementSystem.Services.Security
{
    /// <summary>
    /// Helper used by the Permissions UI to only show checkboxes for actions that actually exist
    /// in the Admin controllers.
    /// </summary>
    public static class PermissionUiFeatureMatrix
    {
        private static readonly Lazy<IReadOnlyDictionary<string, HashSet<string>>> _supportedByModule =
            new Lazy<IReadOnlyDictionary<string, HashSet<string>>>(Build, isThreadSafe: true);

        public static bool Supports(string? module, string? action)
        {
            if (string.IsNullOrWhiteSpace(module) || string.IsNullOrWhiteSpace(action))
                return false;

            if (!_supportedByModule.Value.TryGetValue(module.Trim(), out var set))
                return false;

            return set.Contains(action.Trim());
        }

        public static bool AnySupports(string action)
        {
            if (string.IsNullOrWhiteSpace(action))
                return false;

            return _supportedByModule.Value.Values.Any(s => s.Contains(action.Trim()));
        }

        private static IReadOnlyDictionary<string, HashSet<string>> Build()
        {
            var dict = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);

            // Scan Admin controllers once and infer supported operations by common action names.
            var asm = typeof(PermissionUiFeatureMatrix).Assembly;

            Type[] types;
            try
            {
                types = asm.GetTypes();
            }
            catch (ReflectionTypeLoadException ex)
            {
                types = ex.Types.Where(t => t != null).Cast<Type>().ToArray();
            }

            var controllerTypes = types
                .Where(t =>
                    typeof(Controller).IsAssignableFrom(t) &&
                    t.Namespace != null &&
                    t.Namespace.Contains(".Areas.Admin.Controllers", StringComparison.Ordinal));

            foreach (var module in PermissionConstants.AllModules)
            {
                var supported = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                var ctrlType = FindControllerType(controllerTypes, module);
                if (ctrlType != null)
                {
                    var actions = ctrlType
                        .GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly)
                        .Where(m => !m.IsSpecialName)
                        .Select(m => m.Name)
                        .ToHashSet(StringComparer.OrdinalIgnoreCase);

                    // Show: Index/Details pages.
                    if (actions.Contains("Index") || actions.Contains("Details"))
                        supported.Add(PermissionConstants.Actions.Show);

                    // CRUD
                    if (actions.Contains("Create"))
                        supported.Add(PermissionConstants.Actions.Create);

                    if (actions.Contains("Edit"))
                        supported.Add(PermissionConstants.Actions.Edit);

                    if (actions.Contains("Delete"))
                        supported.Add(PermissionConstants.Actions.Delete);

                    // Special ops
                    if (actions.Contains("Approve"))
                        supported.Add(PermissionConstants.Actions.Approve);

                    if (actions.Any(a => a.StartsWith("Export", StringComparison.OrdinalIgnoreCase)))
                        supported.Add(PermissionConstants.Actions.Export);
                    // Fallback: if controller exists but doesn't follow naming, still let it be visible.
                    if (supported.Count == 0 && actions.Count > 0)
                        supported.Add(PermissionConstants.Actions.Show);
                }

                dict[module] = supported;
            }

            return dict;
        }

        private static Type? FindControllerType(IEnumerable<Type> controllerTypes, string module)
        {
            // Most controllers match "<Module>Controller".
            // A few modules map to plural controllers (e.g., Admin -> AdminsController).
            var candidates = new List<string>();

            if (module.EndsWith("s", StringComparison.OrdinalIgnoreCase))
            {
                candidates.Add(module + "Controller");
            }
            else
            {
                candidates.Add(module + "Controller");
                candidates.Add(module + "sController");
            }

            return controllerTypes.FirstOrDefault(t =>
                candidates.Any(c => string.Equals(t.Name, c, StringComparison.OrdinalIgnoreCase)));
        }
    }
}
