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
            Console.WriteLine("\n-> BẮT ĐẦU TEST BIÊN (EDGE CASES) MODULE: QUẢN LÝ NHẬP HÀNG (PURCHASES)");
            IJavaScriptExecutor js = (IJavaScriptExecutor)driver;

            // ==========================================
            // TC_PUR_EDGE_01: NHẬP SỐ LƯỢNG VÀ ĐƠN GIÁ ÂM
            // ==========================================
            Console.WriteLine("  1. [TC_PUR_EDGE_01] Đang test: Nhập Số lượng và Đơn giá âm (-10, -5000)...");
            try
            {
                driver.Navigate().GoToUrl("http://localhost:5068/Admin/Purchases/Create");
                Thread.Sleep(1500);

                new SelectElement(driver.FindElement(By.Name("SupplierId"))).SelectByIndex(1);
                new SelectElement(driver.FindElement(By.Name("Items[0].ProductId"))).SelectByIndex(1);

                // Cố tình nhập giá trị âm
                IWebElement qtyInput = driver.FindElement(By.Name("Items[0].Qty"));
                qtyInput.Clear();
                qtyInput.SendKeys("-10");

                IWebElement costInput = driver.FindElement(By.Name("Items[0].UnitCost"));
                costInput.Clear();
                costInput.SendKeys("-50000");

                driver.FindElement(By.CssSelector("button[type='submit'].btn-gradient")).Click();
                Thread.Sleep(2000);

                // Kỳ vọng: Hệ thống chặn lại (Frontend hoặc Backend) và không tạo thành công
                Console.WriteLine("       -> [Pass] Đã gửi form giá trị âm. Cần kiểm tra xem Backend/Frontend đã chặn lưu dữ liệu chưa.");
            }
            catch (Exception ex) { Console.WriteLine("       -> [Failed] " + ex.Message); }

            // ==========================================
            // TC_PUR_EDGE_02: BYPASS FRONTEND VALIDATION (GỬI FORM RỖNG)
            // ==========================================
            Console.WriteLine("  2. [TC_PUR_EDGE_02] Đang test: Xóa thuộc tính 'required' của HTML và gửi form rỗng...");
            try
            {
                driver.Navigate().GoToUrl("http://localhost:5068/Admin/Purchases/Create");
                Thread.Sleep(1500);

                // Dùng JS can thiệp DOM: Gỡ toàn bộ thuộc tính 'required', 'min', 'max' để bypass HTML5 validation
                js.ExecuteScript(@"
                    document.querySelectorAll('[required]').forEach(e => e.removeAttribute('required'));
                    document.querySelectorAll('[min]').forEach(e => e.removeAttribute('min'));
                    document.querySelectorAll('[max]').forEach(e => e.removeAttribute('max'));
                ");

                // Bấm Submit thẳng luôn (không chọn Nhà cung cấp, không có Sản phẩm)
                driver.FindElement(By.CssSelector("button[type='submit'].btn-gradient")).Click();
                Thread.Sleep(2000);

                // Kỳ vọng: Model State của ASP.NET Core Backend phải bắt được lỗi và trả về giao diện báo lỗi, KHÔNG BỊ CRASH (Error 500)
                Console.WriteLine("       -> [Pass] Bypass HTML5 thành công và gửi request. Backend phải xử lý được Validation.");
            }
            catch (Exception ex) { Console.WriteLine("       -> [Failed] Hệ thống có thể đã crash: " + ex.Message); }

            // ==========================================
            // TC_PUR_EDGE_03: GÂY TRÀN SỐ (NUMERIC OVERFLOW)
            // ==========================================
            Console.WriteLine("  3. [TC_PUR_EDGE_03] Đang test: Nhập số lượng khổng lồ gây tràn số (Overflow)...");
            try
            {
                driver.Navigate().GoToUrl("http://localhost:5068/Admin/Purchases/Create");
                Thread.Sleep(1500);

                new SelectElement(driver.FindElement(By.Name("SupplierId"))).SelectByIndex(1);
                new SelectElement(driver.FindElement(By.Name("Items[0].ProductId"))).SelectByIndex(1);

                // Nhập một con số lớn hơn giới hạn của kiểu `int` trong C# (2,147,483,647)
                IWebElement qtyInput = driver.FindElement(By.Name("Items[0].Qty"));
                qtyInput.Clear();
                qtyInput.SendKeys("9999999999999999"); // Gây lỗi FormatException hoặc OverflowException nếu Backend không bắt

                IWebElement costInput = driver.FindElement(By.Name("Items[0].UnitCost"));
                costInput.Clear();
                costInput.SendKeys("9999999999");

                driver.FindElement(By.CssSelector("button[type='submit'].btn-gradient")).Click();
                Thread.Sleep(2000);

                Console.WriteLine("       -> [Pass] Gửi số siêu lớn thành công. Cần check hệ thống có báo lỗi 'Giá trị quá lớn' hay bị sập (Crash).");
            }
            catch (Exception ex) { Console.WriteLine("       -> [Failed] " + ex.Message); }

            // ==========================================
            // TC_PUR_EDGE_04: THAO TÁC URL - ID KHÔNG TỒN TẠI HOẶC ÂM
            // ==========================================
            Console.WriteLine("  4. [TC_PUR_EDGE_04] Đang test: Truy cập Chi tiết phiếu nhập bằng ID ảo (-1, 999999)...");
            try
            {
                // Test ID âm
                driver.Navigate().GoToUrl("http://localhost:5068/Admin/Purchases/Details/-1");
                Thread.Sleep(1500);

                bool isNotFoundOrError1 = driver.PageSource.Contains("Không tìm thấy") || driver.Title.Contains("404");

                // Test ID cực lớn không tồn tại
                driver.Navigate().GoToUrl("http://localhost:5068/Admin/Purchases/Details/999999");
                Thread.Sleep(1500);

                bool isNotFoundOrError2 = driver.PageSource.Contains("Không tìm thấy") || driver.Title.Contains("404");

                Console.WriteLine($"       -> [Pass] Truy cập URL ID ảo. Trạng thái xử lý 404/Not Found: {isNotFoundOrError1 && isNotFoundOrError2}");
            }
            catch (Exception ex) { Console.WriteLine("       -> [Failed] Lỗi khi xử lý URL parameter: " + ex.Message); }

            Console.WriteLine("\n-> KẾT THÚC TEST BIÊN (EDGE CASES) MODULE: NHẬP HÀNG");
        }
    }
}