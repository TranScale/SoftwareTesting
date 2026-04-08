using System.Text.RegularExpressions;
using OpenQA.Selenium;
using OSMS.UITests.Support;

namespace OSMS.UITests.Pages;

public sealed class ProductImportPreviewPage : PageBase
{
    private static readonly By SummaryNumbers = By.CssSelector(".row.g-3 .fs-4.fw-bold");
    private static readonly By Header = By.CssSelector("h3");
    private static readonly By InvalidRows = By.CssSelector("tbody tr.table-danger");

    public ProductImportPreviewPage(IWebDriver driver, WaitHelper wait, AutomationSettings settings)
        : base(driver, wait, settings)
    {
    }

    public void WaitUntilLoaded()
    {
        Wait.UrlContains("/Admin/Products/ImportExcelPreview");
        Wait.Visible(Header);
        Wait.VisibleAll(SummaryNumbers);
    }

    public (int TotalRows, int ValidRows, int InvalidRows) GetSummaryCounts()
    {
        var values = Wait.VisibleAll(SummaryNumbers)
            .Select(x => ParseFirstInteger(x.Text))
            .ToArray();

        if (values.Length < 3)
        {
            throw new InvalidOperationException("Could not read the expected three summary values from the import preview page.");
        }

        return (values[0], values[1], values[2]);
    }

    public int GetInvalidRowCount()
    {
        return Driver.FindElements(InvalidRows).Count;
    }

    public string GetPageText() => ReadBodyText();

    private static int ParseFirstInteger(string value)
    {
        var match = Regex.Match(value, @"\d+");
        if (!match.Success)
        {
            throw new FormatException($"Could not extract an integer from '{value}'.");
        }

        return int.Parse(match.Value);
    }
}
