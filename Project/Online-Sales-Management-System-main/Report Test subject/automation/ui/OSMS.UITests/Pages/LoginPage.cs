using OpenQA.Selenium;
using OSMS.UITests.Support;

namespace OSMS.UITests.Pages;

public sealed class LoginPage : PageBase
{
    private static readonly By EmailInput = By.Name("Email");
    private static readonly By PasswordInput = By.Id("passwordInput");
    private static readonly By SubmitButton = By.CssSelector("button[type='submit']");

    public LoginPage(IWebDriver driver, WaitHelper wait, AutomationSettings settings)
        : base(driver, wait, settings)
    {
    }

    public LoginPage Open()
    {
        OpenRelativeUrl("/Admin/Auth/Login");
        Wait.UrlContains("/Admin/Auth/Login");
        Wait.Visible(EmailInput);
        return this;
    }

    public void Login(LoginCredential credential)
    {
        SetInputValue(EmailInput, credential.Username);
        SetInputValue(PasswordInput, credential.Password);
        Wait.Clickable(SubmitButton).Click();
    }

    public string GetPageText() => ReadBodyText();
}
