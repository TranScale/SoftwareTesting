using OSMS.UITests.Pages;
using OSMS.UITests.Support;

namespace OSMS.UITests.Tests;

public sealed class AuthenticationTests : UiTestBase
{
    [Fact]
    [Trait("CaseId", "TC-UI-AUTH-001")]
    [Trait("AutomationId", "AUTO-UI-001")]
    public void AdminLoginSmokeSucceeds()
    {
        ExecuteWithFailureCapture("TC-UI-AUTH-001", () =>
        {
            var credential = TestData.GetCredential("UI-DATA-ACC-001");
            var loginPage = new LoginPage(Driver, Wait, Settings).Open();

            loginPage.Login(credential);

            var dashboardPage = new AdminDashboardPage(Driver, Wait, Settings);
            dashboardPage.WaitUntilLoaded();

            Assert.Contains("/Admin", Driver.Url, StringComparison.OrdinalIgnoreCase);
            CaptureCheckpoint("TC-UI-AUTH-001-success");
        });
    }

    [Fact]
    [Trait("CaseId", "TC-UI-AUTH-003")]
    [Trait("AutomationId", "AUTO-UI-002")]
    public void SalesUserIsDeniedAccessToPurchases()
    {
        ExecuteWithFailureCapture("TC-UI-AUTH-003", () =>
        {
            var credential = TestData.GetCredential("UI-DATA-ACC-002");
            var loginPage = new LoginPage(Driver, Wait, Settings).Open();

            loginPage.Login(credential);

            Driver.Navigate().GoToUrl(Settings.ToAbsoluteUri("/Admin/Purchases"));
            Wait.Condition(
                driver => !driver.Url.Contains("/Admin/Purchases", StringComparison.OrdinalIgnoreCase),
                "Timed out waiting for the sales user to be redirected away from the protected Purchases route.");

            var dashboardPage = new AdminDashboardPage(Driver, Wait, Settings);
            dashboardPage.WaitUntilLoaded();

            Assert.Contains("/Admin", Driver.Url, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("/Admin/Purchases", Driver.Url, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("Dashboard", Driver.FindElement(OpenQA.Selenium.By.TagName("body")).Text, StringComparison.OrdinalIgnoreCase);
            CaptureCheckpoint("TC-UI-AUTH-003-access-denied");
        });
    }
}
