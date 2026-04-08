using OpenQA.Selenium;
using OSMS.UITests.Support;

namespace OSMS.UITests.Pages;

public sealed class AdminDashboardPage : PageBase
{
    private static readonly By DashboardTitle = By.CssSelector("h2");

    public AdminDashboardPage(IWebDriver driver, WaitHelper wait, AutomationSettings settings)
        : base(driver, wait, settings)
    {
    }

    public void WaitUntilLoaded()
    {
        Wait.Visible(DashboardTitle);
        Wait.BodyContains("Dashboard");
    }
}
