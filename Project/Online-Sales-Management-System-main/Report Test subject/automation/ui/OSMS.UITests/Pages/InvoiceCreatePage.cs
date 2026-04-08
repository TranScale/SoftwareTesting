using System.Globalization;
using OpenQA.Selenium;
using OSMS.UITests.Support;

namespace OSMS.UITests.Pages;

public sealed class InvoiceCreatePage : PageBase
{
    private static readonly By CustomerSelect = By.Id("customerSelect");
    private static readonly By ProductSelect = By.CssSelector("#itemsTable tbody tr:first-child select.product-select");
    private static readonly By QuantityInput = By.CssSelector("#itemsTable tbody tr:first-child input.qty-input");
    private static readonly By SubmitButton = By.CssSelector("form[action='/Admin/Invoices/Create'] button[type='submit']");

    public InvoiceCreatePage(IWebDriver driver, WaitHelper wait, AutomationSettings settings)
        : base(driver, wait, settings)
    {
    }

    public InvoiceCreatePage Open()
    {
        OpenRelativeUrl("/Admin/Invoices/Create");
        Wait.UrlContains("/Admin/Invoices/Create");
        Wait.Visible(CustomerSelect);
        return this;
    }

    public void FillWalkInInvoice(string productToken, int quantity)
    {
        SelectByPartialText(ProductSelect, productToken);
        SetInputValue(QuantityInput, quantity.ToString(CultureInfo.InvariantCulture));
    }

    public InvoiceDetailsPage Submit()
    {
        Wait.Clickable(SubmitButton).Click();
        return new InvoiceDetailsPage(Driver, Wait, Settings);
    }
}
