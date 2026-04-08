using OSMS.UITests.Pages;
using OSMS.UITests.Support;

namespace OSMS.UITests.Tests;

public sealed class ProductImportTests : UiTestBase
{
    [Fact]
    [Trait("CaseId", "TC-UI-IMP-002")]
    [Trait("AutomationId", "AUTO-UI-005")]
    public void ProductImportPreviewShowsExpectedValidAndInvalidCounts()
    {
        ExecuteWithFailureCapture("TC-UI-IMP-002", () =>
        {
            var credential = TestData.GetCredential("UI-DATA-ACC-003");
            var workbookPath = TestData.GetAbsolutePath("UI-DATA-IMP-001");
            var loginPage = new LoginPage(Driver, Wait, Settings).Open();

            loginPage.Login(credential);

            var importPage = new ProductImportPage(Driver, Wait, Settings).Open();
            var previewPage = importPage.UploadAndPreview(workbookPath);
            previewPage.WaitUntilLoaded();

            var counts = previewPage.GetSummaryCounts();
            var pageText = previewPage.GetPageText();

            Assert.Equal(6, counts.TotalRows);
            Assert.Equal(1, counts.ValidRows);
            Assert.Equal(5, counts.InvalidRows);
            Assert.Equal(5, previewPage.GetInvalidRowCount());
            Assert.Contains("QAIMP001", pageText, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("QAIMPDUP", pageText, StringComparison.OrdinalIgnoreCase);
            CaptureCheckpoint("TC-UI-IMP-002-preview");
        });
    }
}
