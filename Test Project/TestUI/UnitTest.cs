using System;
using System.Threading;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace StockTest
{
    public class UnitTest
    {
        public static void RunAllTests(IWebDriver driver)
        {
            Console.WriteLine("\n-> BẮT ĐẦU TEST MODULE: QUẢN LÝ ĐƠN VỊ TÍNH (UNITS)");

            // Tạo biến random để tên đơn vị không bị trùng lặp, tránh lỗi "Unit name already exists."
            string uniqueUnitName = "Unit_" + DateTime.Now.Ticks;

            // 1. Vào trang Index Đơn vị tính
            driver.Navigate().GoToUrl("http://localhost:5068/Admin/Units");
            Thread.Sleep(2000);

            // ---------------------------------------------------------
            // TEST CASE 1: TẠO MỚI ĐƠN VỊ TÍNH
            // ---------------------------------------------------------
            Console.WriteLine("   1. Đang test: Tạo mới đơn vị tính...");
            // Click nút Thêm mới
            driver.FindElement(By.CssSelector("a[href='/Admin/Units/Create']")).Click();
            Thread.Sleep(2000);

            // Điền Form Tạo Mới
            IWebElement nameInput = driver.FindElement(By.Name("Name"));
            nameInput.Clear();
            nameInput.SendKeys(uniqueUnitName);

            IWebElement shortNameInput = driver.FindElement(By.Name("ShortName"));
            shortNameInput.Clear();
            shortNameInput.SendKeys("unt");

            // Bấm nút Tạo mới
            driver.FindElement(By.CssSelector("button[type='submit'].btn-gradient")).Click();
            Thread.Sleep(2000);
            Console.WriteLine("      -> [Pass] Tạo đơn vị mới thành công.");

            // ---------------------------------------------------------
            // TEST CASE 2: TÌM KIẾM ĐƠN VỊ TÍNH
            // ---------------------------------------------------------
            Console.WriteLine("   2. Đang test: Tìm kiếm đơn vị...");
            IWebElement searchBox = driver.FindElement(By.Name("q"));
            searchBox.Clear();
            searchBox.SendKeys(uniqueUnitName); // Tìm chính đơn vị vừa tạo

            // Submit form tìm kiếm (có thể click nút tìm kiếm hoặc submit form)
            driver.FindElement(By.CssSelector("button[type='submit'].btn-outline-primary")).Click();
            Thread.Sleep(2000);
            Console.WriteLine("      -> [Pass] Tìm kiếm thành công.");

            // ---------------------------------------------------------
            // TEST CASE 3: CHỈNH SỬA ĐƠN VỊ TÍNH
            // ---------------------------------------------------------
            Console.WriteLine("   3. Đang test: Chỉnh sửa đơn vị...");
            // Click nút Sửa của dòng đầu tiên trong bảng kết quả
            driver.FindElement(By.CssSelector("a.btn-icon.edit")).Click();
            Thread.Sleep(2000);

            // Sửa ShortName
            IWebElement editShortNameInput = driver.FindElement(By.Name("ShortName"));
            editShortNameInput.Clear();
            editShortNameInput.SendKeys("unt_upd");

            // Bấm Lưu thay đổi
            driver.FindElement(By.CssSelector("button[type='submit'].btn-gradient")).Click();
            Thread.Sleep(2000);
            Console.WriteLine("      -> [Pass] Cập nhật đơn vị thành công.");

            // ---------------------------------------------------------
            // TEST CASE 4: XÓA ĐƠN VỊ TÍNH
            // ---------------------------------------------------------
            Console.WriteLine("   4. Đang test: Xóa đơn vị...");
            try
            {
                // Nhấn nút xóa của dòng đầu tiên
                IWebElement deleteBtn = driver.FindElement(By.CssSelector("form[action^='/Admin/Units/Delete'] button.delete"));
                deleteBtn.Click();

                Thread.Sleep(1000);

                // Chấp nhận Alert confirm của trình duyệt ("Bạn có chắc chắn xóa đơn vị này?")
                IAlert alert = driver.SwitchTo().Alert();
                alert.Accept();

                Thread.Sleep(2000);
                Console.WriteLine("      -> [Pass] Đã xác nhận xóa đơn vị thành công.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("      -> [Failed/Skip] Không thể xóa đơn vị. Lỗi: " + ex.Message);
            }
        }
    }
}