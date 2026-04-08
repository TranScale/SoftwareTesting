using OSMS.UITests.Pages;
using OSMS.UITests.Support;

namespace OSMS.UITests.Tests;

public sealed class InvoiceFlowTests : UiTestBase
{
    [Fact]
    [Trait("CaseId", "TC-UI-INV-001")]
    [Trait("AutomationId", "AUTO-UI-004")]
    public void PrivilegedUserCanCreateWalkInInvoice()
    {
        ExecuteWithFailureCapture("TC-UI-INV-001", () =>
        {
            var credential = TestData.GetCredential("UI-DATA-ACC-001");
            var invoiceData = TestData.GetKeyValueData("UI-DATA-INV-001");
            var productData = TestData.GetKeyValueData("UI-DATA-PRD-008");
            var loginPage = new LoginPage(Driver, Wait, Settings).Open();

            loginPage.Login(credential);

            var createPage = new InvoiceCreatePage(Driver, Wait, Settings).Open();
            createPage.FillWalkInInvoice(
                invoiceData["Product"],
                int.Parse(invoiceData["Qty"]));

            var detailsPage = createPage.Submit();
            detailsPage.WaitUntilLoaded();
            var pageText = detailsPage.GetPageText();

            Assert.Contains("INV-", detailsPage.GetHeaderText(), StringComparison.OrdinalIgnoreCase);
            Assert.Contains("walk-in", pageText, StringComparison.OrdinalIgnoreCase);
            Assert.Contains(productData["Name"], pageText, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("thanh toán", detailsPage.GetStatusText(), StringComparison.OrdinalIgnoreCase);
            CaptureCheckpoint("TC-UI-INV-001-created");
        });
    }
}
