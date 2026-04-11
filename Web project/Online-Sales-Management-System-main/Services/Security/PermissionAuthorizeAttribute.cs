using System;
using Microsoft.AspNetCore.Authorization;

namespace OnlineSalesManagementSystem.Services.Security
{
    /// <summary>
    /// Helper attribute for permission-based authorization.
    ///
    /// Why this exists:
    /// - In C#, attribute arguments must be compile-time constants.
    /// - So you can't write: [Authorize(Policy = PermissionConstants.PolicyName(...))]
    /// - Instead, you use: [PermissionAuthorize(PermissionConstants.Modules.X, PermissionConstants.Actions.Y)]
    ///   and this attribute will compute the real policy string at runtime.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public sealed class PermissionAuthorizeAttribute : AuthorizeAttribute
    {
        public PermissionAuthorizeAttribute(string module, string action)
        {
            Policy = PermissionConstants.PolicyName(module, action);
        }
    }
}
