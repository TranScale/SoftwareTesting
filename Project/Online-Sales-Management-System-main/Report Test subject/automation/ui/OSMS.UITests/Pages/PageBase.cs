using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using OSMS.UITests.Support;

namespace OSMS.UITests.Pages;

public abstract class PageBase
{
    protected PageBase(IWebDriver driver, WaitHelper wait, AutomationSettings settings)
    {
        Driver = driver;
        Wait = wait;
        Settings = settings;
    }

    protected IWebDriver Driver { get; }

    protected WaitHelper Wait { get; }

    protected AutomationSettings Settings { get; }

    protected void OpenRelativeUrl(string relativePath)
    {
        Driver.Navigate().GoToUrl(Settings.ToAbsoluteUri(relativePath));
    }

    protected string ReadBodyText()
    {
        return Driver.FindElement(By.TagName("body")).Text;
    }

    protected void SetInputValue(By locator, string value)
    {
        var input = Wait.Visible(locator);
        input.Clear();
        input.SendKeys(value);
    }

    protected void SelectByPartialText(By locator, string partialText)
    {
        var select = new SelectElement(Wait.Visible(locator));
        var option = select.Options.FirstOrDefault(x =>
            x.Text.Contains(partialText, StringComparison.OrdinalIgnoreCase));

        if (option == null)
        {
            throw new NoSuchElementException($"No option containing '{partialText}' was found for select '{locator}'.");
        }

        select.SelectByText(option.Text);
    }
}
