using OpenQA.Selenium;
using OSMS.UITests.Support;

namespace OSMS.UITests.Pages;

public sealed class ProductImportPage : PageBase
{
    private static readonly By FileInput = By.CssSelector("form[action='/Admin/Products/ImportExcelPreview'] input[type='file'][name='file']");
    private static readonly By SubmitButton = By.CssSelector("form[action='/Admin/Products/ImportExcelPreview'] button[type='submit']");

    public ProductImportPage(IWebDriver driver, WaitHelper wait, AutomationSettings settings)
        : base(driver, wait, settings)
    {
    }

    public ProductImportPage Open()
    {
        OpenRelativeUrl("/Admin/Products/ImportExcel");
        Wait.UrlContains("/Admin/Products/ImportExcel");
        Wait.Visible(FileInput);
        return this;
    }

    public ProductImportPreviewPage UploadAndPreview(string absoluteWorkbookPath)
    {
        if (!File.Exists(absoluteWorkbookPath))
        {
            throw new FileNotFoundException("The workbook for the product import test was not found.", absoluteWorkbookPath);
        }

        var fileInput = Wait.Visible(FileInput);
        fileInput.SendKeys(absoluteWorkbookPath);
        Wait.Condition(
            driver => !string.IsNullOrWhiteSpace(driver.FindElement(FileInput).GetAttribute("value")),
            "Timed out waiting for the product import file input to receive the workbook path.");
        Wait.Visible(SubmitButton).Click();
        return new ProductImportPreviewPage(Driver, Wait, Settings);
    }
}
