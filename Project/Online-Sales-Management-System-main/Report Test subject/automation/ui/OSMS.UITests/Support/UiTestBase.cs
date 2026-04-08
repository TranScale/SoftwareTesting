using OpenQA.Selenium;

namespace OSMS.UITests.Support;

public abstract class UiTestBase : IDisposable
{
    protected UiTestBase()
    {
        Settings = AutomationSettings.Load();
        TestData = UiTestDataCatalog.Load(Settings);
        Driver = WebDriverFactory.Create(Settings);
        Driver.Manage().Timeouts().ImplicitWait = TimeSpan.Zero;
        Wait = new WaitHelper(Driver, TimeSpan.FromSeconds(Settings.DefaultTimeoutSeconds));
        Screenshots = new ScreenshotHelper(Settings);
    }

    protected AutomationSettings Settings { get; }

    protected UiTestDataCatalog TestData { get; }

    protected IWebDriver Driver { get; }

    protected WaitHelper Wait { get; }

    protected ScreenshotHelper Screenshots { get; }

    protected string CaptureCheckpoint(string name)
    {
        return Screenshots.Capture(Driver, name);
    }

    protected void ExecuteWithFailureCapture(string name, Action action)
    {
        try
        {
            action();
        }
        catch
        {
            try
            {
                CaptureCheckpoint($"{name}-failure");
            }
            catch
            {
                // Best-effort capture only.
            }

            throw;
        }
    }

    public void Dispose()
    {
        Driver.Quit();
        Driver.Dispose();
    }
}
