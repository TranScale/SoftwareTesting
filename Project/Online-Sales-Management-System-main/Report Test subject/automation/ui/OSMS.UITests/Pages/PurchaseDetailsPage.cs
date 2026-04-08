using OpenQA.Selenium;
using OSMS.UITests.Support;

namespace OSMS.UITests.Pages;

public sealed class PurchaseDetailsPage : PageBase
{
    private static readonly By Header = By.CssSelector("h2");
    private static readonly By StatusBadge = By.CssSelector(".badge-status");

    public PurchaseDetailsPage(IWebDriver driver, WaitHelper wait, AutomationSettings settings)
        : base(driver, wait, settings)
    {
    }

    public void WaitUntilLoaded()
    {
        Wait.UrlContains("/Admin/Purchases/Details");
        Wait.Visible(Header);
    }

    public string GetHeaderText() => Wait.Visible(Header).Text;

    public string GetStatusText() => Wait.Visible(StatusBadge).Text;

    public string GetPageText() => ReadBodyText();
}
