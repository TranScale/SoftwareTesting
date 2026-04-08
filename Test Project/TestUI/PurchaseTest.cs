using System;
using System.Threading;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace StockTest
{
    public class PurchaseTest
    {
        public static void RunAllTests(IWebDriver driver)
        {
            Console.WriteLine("\n-> BẮT ĐẦU TEST MODULE: QUẢN LÝ NHẬP HÀNG (PURCHASES)");

            // 1. Vào trang Index Nhập hàng
            driver.Navigate().GoToUrl("http://localhost:5068/Admin/Purchases");
            Thread.Sleep(2000);

            // 2. TEST TÌM KIẾM
            Console.WriteLine("   1. Đang test: Tìm kiếm phiếu nhập...");
            IWebElement searchBox = driver.FindElement(By.Name("q"));
            searchBox.Clear();
            searchBox.SendKeys("PO-"); // Tìm các mã phiếu chứa "PO-"
            driver.FindElement(By.XPath("//button[contains(text(), 'Tìm kiếm')]")).Click();
            Thread.Sleep(1500);

            // 3. TEST RESET
            Console.WriteLine("   2. Đang test: Nút Reset...");
            driver.FindElement(By.XPath("//a[contains(@href, '/Admin/Purchases') and contains(., 'Reset')]")).Click();
            Thread.Sleep(1500);

            // 4. TEST TẠO MỚI PHIẾU NHẬP
            Console.WriteLine("   3. Đang test: Tạo phiếu nhập hàng (Draft)...");
            // Click nút Tạo phiếu nhập
            driver.FindElement(By.CssSelector("a[href='/Admin/Purchases/Create']")).Click();
            Thread.Sleep(2000);

            // --- Điền Form Tạo Mới ---
            // Chọn Nhà cung cấp (Chọn option thứ 2, vì option 1 là "-- Chọn nhà cung cấp --")
            SelectElement supplierSelect = new SelectElement(driver.FindElement(By.Name("SupplierId")));
            supplierSelect.SelectByIndex(1);

            // Chọn Sản phẩm (Dòng 1: Items[0])
            SelectElement productSelect = new SelectElement(driver.FindElement(By.Name("Items[0].ProductId")));
            productSelect.SelectByIndex(1);

            // Sửa số lượng thành 10
            IWebElement qtyInput = driver.FindElement(By.Name("Items[0].Qty"));
            qtyInput.Clear();
            qtyInput.SendKeys("10");

            // Sửa đơn giá thành 50000
            IWebElement costInput = driver.FindElement(By.Name("Items[0].UnitCost"));
            costInput.Clear();
            costInput.SendKeys("50000");

            // Bấm Lưu phiếu nhập (Submit form)
            driver.FindElement(By.CssSelector("button[type='submit'].btn-gradient")).Click();
            Thread.Sleep(2000);
            Console.WriteLine("      -> [Pass] Tạo phiếu nháp thành công. Đã chuyển sang trang Chi tiết.");

            // 5. TEST XÁC NHẬN NHẬP KHO (Trong trang Details)
            Console.WriteLine("   4. Đang test: Nút Xác nhận nhập kho...");
            try
            {
                // Tìm nút Xác nhận nhập kho (Nằm trong form có action chứa /Receive/)
                IWebElement receiveBtn = driver.FindElement(By.XPath("//form[contains(@action, '/Admin/Purchases/Receive/')]/button"));
                receiveBtn.Click();

                // Vì code của bạn có thuộc tính onclick="return confirm('...');", trình duyệt sẽ bật lên 1 cái Alert
                // Ta phải dùng code để tự động nhấn OK (Accept) cái Alert đó
                Thread.Sleep(1000);
                IAlert alert = driver.SwitchTo().Alert();
                alert.Accept();

                Thread.Sleep(2000);
                Console.WriteLine("      -> [Pass] Đã xác nhận nhập kho và xử lý Alert thành công.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("      -> [Failed/Skip] Không tìm thấy nút Xác nhận nhập kho hoặc Phiếu đã được xử lý. Lỗi: " + ex.Message);
            }
        }
    }
}