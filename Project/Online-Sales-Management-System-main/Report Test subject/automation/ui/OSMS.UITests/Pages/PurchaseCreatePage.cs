using System.Globalization;
using OpenQA.Selenium;
using OSMS.UITests.Support;

namespace OSMS.UITests.Pages;

public sealed class PurchaseCreatePage : PageBase
{
    private static readonly By SupplierSelect = By.Id("supplierSelect");
    private static readonly By ProductSelect = By.CssSelector("#itemsTable tbody tr:first-child select");
    private static readonly By QuantityInput = By.CssSelector("#itemsTable tbody tr:first-child input[name$='.Qty']");
    private static readonly By UnitCostInput = By.CssSelector("#itemsTable tbody tr:first-child input[name$='.UnitCost']");
    private static readonly By SubmitButton = By.CssSelector("form[action='/Admin/Purchases/Create'] button[type='submit']");

    public PurchaseCreatePage(IWebDriver driver, WaitHelper wait, AutomationSettings settings)
        : base(driver, wait, settings)
    {
    }

    public PurchaseCreatePage Open()
    {
        OpenRelativeUrl("/Admin/Purchases/Create");
        Wait.UrlContains("/Admin/Purchases/Create");
        Wait.Visible(SupplierSelect);
        return this;
    }

    public void FillDraftPurchase(string supplierName, string productToken, int quantity, decimal unitCost)
    {
        SelectByPartialText(SupplierSelect, supplierName);
        SelectByPartialText(ProductSelect, productToken);
        SetInputValue(QuantityInput, quantity.ToString(CultureInfo.InvariantCulture));
        SetInputValue(UnitCostInput, unitCost.ToString(CultureInfo.InvariantCulture));
    }

    public PurchaseDetailsPage Submit()
    {
        Wait.Clickable(SubmitButton).Click();
        return new PurchaseDetailsPage(Driver, Wait, Settings);
    }
}
