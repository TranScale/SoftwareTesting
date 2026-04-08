using System;
using System.Threading;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace StockTest
{
    public class StockIndexTest
    {
        public static void RunAllTests(IWebDriver driver)
        {
            Console.WriteLine("\n-> BẮT ĐẦU TEST TRANG: TỒN KHO HIỆN TẠI (INDEX)");
            driver.Navigate().GoToUrl("http://localhost:5068/Admin/Stock");
            Thread.Sleep(2000);

            // 1. TÌM KIẾM
            Console.WriteLine("   1. Đang test: Tìm kiếm...");
            IWebElement searchBox = driver.FindElement(By.Name("q"));
            searchBox.Clear();
            searchBox.SendKeys("Laptop");
            driver.FindElement(By.XPath("//button[contains(text(), 'Tìm kiếm')]")).Click();
            Thread.Sleep(2000);
            Console.WriteLine("      -> [Pass] Tìm kiếm hoàn tất.");

            // 2. NÚT RESET
            Console.WriteLine("   2. Đang test: Nút Reset...");
            driver.FindElement(By.XPath("//a[contains(@href, '/Admin/Stock') and contains(., 'Reset')]")).Click();
            Thread.Sleep(2000);
            Console.WriteLine("      -> [Pass] Reset form thành công.");

            // 3. ĐỔI SỐ DÒNG (PAGE SIZE)
            Console.WriteLine("   3. Đang test: Đổi số lượng hiển thị (25 dòng)...");
            SelectElement pageSizeSelect = new SelectElement(driver.FindElement(By.Name("pageSize")));
            pageSizeSelect.SelectByValue("25");
            Thread.Sleep(2000);
            Console.WriteLine("      -> [Pass] Đổi số dòng thành công.");

            // 4. CHUYỂN TRANG
            Console.WriteLine("   4. Đang test: Các nút chuyển hướng...");
            driver.FindElement(By.CssSelector("a[href='/Admin/Stock/Low']")).Click();
            Thread.Sleep(1500);
            driver.Navigate().Back();
            Thread.Sleep(1500);

            driver.FindElement(By.CssSelector("a[href='/Admin/Stock/Movements']")).Click();
            Thread.Sleep(1500);
            Console.WriteLine("      -> [Pass] Chuyển hướng các trang liên kết thành công.");
        }
    }
}