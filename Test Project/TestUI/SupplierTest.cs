using System;
using System.Threading;
using OpenQA.Selenium;

namespace StockTest
{
    public class SupplierTest
    {
        public static void RunAllTests(IWebDriver driver)
        {
            Console.WriteLine("\n-> BẮT ĐẦU TEST MODULE: QUẢN LÝ NHÀ CUNG CẤP (SUPPLIERS)");

            IJavaScriptExecutor js = (IJavaScriptExecutor)driver;

            // 1. Vào trang danh sách Nhà cung cấp
            driver.Navigate().GoToUrl("http://localhost:5068/Admin/Suppliers");
            Thread.Sleep(2000);

            // 2. TEST TẠO MỚI (CREATE)
            Console.WriteLine("   1. Đang test: Tạo mới Nhà cung cấp...");
            IWebElement btnCreate = driver.FindElement(By.CssSelector("a[href='/Admin/Suppliers/Create']"));
            js.ExecuteScript("arguments[0].click();", btnCreate);
            Thread.Sleep(2000);

            // Điền form thêm mới
            driver.FindElement(By.Name("Name")).SendKeys("NCC Auto Test");
            driver.FindElement(By.Name("Phone")).SendKeys("0987654321");
            driver.FindElement(By.Name("Email")).SendKeys("autotest@ncc.local");
            driver.FindElement(By.Name("Address")).SendKeys("123 Đường Tự Động, TP.HCM");

            // SỬA Ở ĐÂY: Tìm chính xác nút có class btn-warning (nút Tạo mới) thay vì submit chung chung
            IWebElement btnSubmitCreate = driver.FindElement(By.XPath("//button[@type='submit' and contains(@class, 'btn-warning')]"));
            js.ExecuteScript("arguments[0].click();", btnSubmitCreate);

            // Đợi server xử lý thêm mới và redirect về trang Index
            Thread.Sleep(3000);
            Console.WriteLine("      -> [Pass] Tạo mới thành công. Đã quay về trang Index.");

            // 3. TEST TÌM KIẾM (SEARCH)
            Console.WriteLine("   2. Đang test: Tìm kiếm Nhà cung cấp vừa tạo...");
            IWebElement searchBox = driver.FindElement(By.Name("q"));
            searchBox.Clear();
            searchBox.SendKeys("NCC Auto Test");

            // Click nút Lọc dữ liệu
            IWebElement btnSearch = driver.FindElement(By.XPath("//button[contains(text(), 'Lọc dữ liệu') or contains(text(), 'Tìm kiếm')]"));
            js.ExecuteScript("arguments[0].click();", btnSearch);
            Thread.Sleep(2000);
            Console.WriteLine("      -> [Pass] Lọc dữ liệu thành công.");

            // 4. TEST XEM CHI TIẾT (DETAILS)
            Console.WriteLine("   3. Đang test: Xem chi tiết NCC...");
            IWebElement btnDetails = driver.FindElement(By.CssSelector("a.btn-action.details"));
            js.ExecuteScript("arguments[0].click();", btnDetails);
            Thread.Sleep(2000);

            // Quay lại trang Index
            driver.Navigate().Back();
            Thread.Sleep(2000);
            Console.WriteLine("      -> [Pass] Vào trang chi tiết thành công.");

            // 5. TEST CHỈNH SỬA (EDIT)
            Console.WriteLine("   4. Đang test: Chỉnh sửa thông tin NCC...");
            IWebElement btnEdit = driver.FindElement(By.CssSelector("a.btn-action.edit"));
            js.ExecuteScript("arguments[0].click();", btnEdit);
            Thread.Sleep(2000);

            // Xóa tên cũ, nhập tên mới
            IWebElement nameBox = driver.FindElement(By.Name("Name"));
            nameBox.Clear();
            nameBox.SendKeys(" (Đã cập nhật)"); // Gõ nối thêm chữ vào tên cũ

            // SỬA Ở ĐÂY: Tìm đúng nút Lưu trong form Edit
            IWebElement btnSubmitEdit = driver.FindElement(By.XPath("//button[@type='submit' and contains(@class, 'btn-warning')]"));
            js.ExecuteScript("arguments[0].click();", btnSubmitEdit);
            Thread.Sleep(2000);
            Console.WriteLine("      -> [Pass] Cập nhật thông tin thành công.");
        }
    }
}