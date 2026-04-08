using OpenQA.Selenium;
using OSMS.UITests.Support;

namespace OSMS.UITests.Pages;

public sealed class AccessDeniedPage : PageBase
{
    private static readonly By PageTitle = By.CssSelector(".card-title");

    public AccessDeniedPage(IWebDriver driver, WaitHelper wait, AutomationSettings settings)
        : base(driver, wait, settings)
    {
    }

    public void WaitUntilLoaded()
    {
        Wait.UrlContains("/Admin/Auth/AccessDenied");
        Wait.Visible(PageTitle);
    }

    public string GetPageText() => ReadBodyText();
}
