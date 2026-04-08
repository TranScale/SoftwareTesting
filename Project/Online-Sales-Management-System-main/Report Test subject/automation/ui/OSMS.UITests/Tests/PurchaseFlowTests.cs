using OSMS.UITests.Pages;
using OSMS.UITests.Support;

namespace OSMS.UITests.Tests;

public sealed class PurchaseFlowTests : UiTestBase
{
    [Fact]
    [Trait("CaseId", "TC-UI-PUR-001")]
    [Trait("CaseId", "TC-UI-PUR-007")]
    [Trait("AutomationId", "AUTO-UI-003")]
    public void PrivilegedUserCanCreateDraftPurchase()
    {
        ExecuteWithFailureCapture("TC-UI-PUR-001", () =>
        {
            var credential = TestData.GetCredential("UI-DATA-ACC-001");
            var purchaseData = TestData.GetKeyValueData("UI-DATA-PUR-001");
            var loginPage = new LoginPage(Driver, Wait, Settings).Open();

            loginPage.Login(credential);

            var createPage = new PurchaseCreatePage(Driver, Wait, Settings).Open();
            createPage.FillDraftPurchase(
                purchaseData["Supplier"],
                purchaseData["Product"],
                int.Parse(purchaseData["Qty"]),
                decimal.Parse(purchaseData["UnitCost"]));

            var detailsPage = createPage.Submit();
            detailsPage.WaitUntilLoaded();
            var pageText = detailsPage.GetPageText();

            Assert.Contains("PO-", detailsPage.GetHeaderText(), StringComparison.OrdinalIgnoreCase);
            Assert.Contains(purchaseData["Supplier"], pageText, StringComparison.OrdinalIgnoreCase);
            Assert.Contains(purchaseData["Product"], pageText, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("nháp", detailsPage.GetStatusText(), StringComparison.OrdinalIgnoreCase);
            CaptureCheckpoint("TC-UI-PUR-001-draft-created");
        });
    }
}
