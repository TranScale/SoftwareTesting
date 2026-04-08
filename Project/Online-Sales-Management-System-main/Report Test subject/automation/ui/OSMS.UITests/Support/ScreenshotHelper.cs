using OpenQA.Selenium;

namespace OSMS.UITests.Support;

public sealed class ScreenshotHelper
{
    private readonly string _outputDirectory;

    public ScreenshotHelper(AutomationSettings settings)
    {
        _outputDirectory = RepositoryPathHelper.ResolveFromRepository(settings.ScreenshotsDirectory);
        Directory.CreateDirectory(_outputDirectory);
    }

    public string Capture(IWebDriver driver, string fileNamePrefix)
    {
        if (driver is not ITakesScreenshot screenshotDriver)
        {
            throw new InvalidOperationException("The active web driver does not support screenshots.");
        }

        var safePrefix = string.Concat(fileNamePrefix.Select(ch => char.IsLetterOrDigit(ch) ? ch : '-')).Trim('-');
        var finalPath = Path.Combine(_outputDirectory, $"{DateTime.UtcNow:yyyyMMdd_HHmmss}_{safePrefix}.png");
        screenshotDriver.GetScreenshot().SaveAsFile(finalPath);
        return finalPath;
    }
}
