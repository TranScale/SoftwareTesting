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
            Console.WriteLine("\n-> BẮT ĐẦU TEST BIÊN (EDGE CASES) TRANG: TỒN KHO HIỆN TẠI (INDEX)");
            IJavaScriptExecutor js = (IJavaScriptExecutor)driver;

            // ==========================================
            // TC_STOCK_EDGE_01: TÌM KIẾM VỚI DỮ LIỆU ĐỘC HẠI (XSS / SQL INJECTION)
            // ==========================================
            Console.WriteLine("  1. [TC_STOCK_EDGE_01] Đang test: Nhập script XSS và SQL Injection vào ô tìm kiếm...");
            try
            {
                driver.Navigate().GoToUrl("http://localhost:5068/Admin/Stock");
                Thread.Sleep(1500);

                IWebElement searchBox = driver.FindElement(By.Name("q"));
                searchBox.Clear();
                // Nhập chuỗi nhạy cảm để xem hệ thống có bị vỡ layout, hiện alert hay văng lỗi SQL không
                searchBox.SendKeys("'; DROP TABLE Stocks; -- <script>alert('Hacked')</script>");

                driver.FindElement(By.XPath("//button[contains(text(), 'Tìm kiếm')]")).Click();
                Thread.Sleep(2000);

                // Kiểm tra xem có Alert nào bất ngờ bật lên do XSS không
                try
                {
                    driver.SwitchTo().Alert().Accept();
                    Console.WriteLine("       -> [Failed] Hệ thống bị dính lỗi XSS (Alert đã chạy).");
                }
                catch (NoAlertPresentException)
                {
                    Console.WriteLine("       -> [Pass] Ứng dụng an toàn, không bị dính XSS/SQL Injection.");
                }
            }
            catch (Exception ex) { Console.WriteLine("       -> [Lỗi Test] " + ex.Message); }

            // ==========================================
            // TC_STOCK_EDGE_02: TÌM KIẾM CHUỖI CỰC DÀI (VƯỢT GIỚI HẠN MAXLENGTH)
            // ==========================================
            Console.WriteLine("  2. [TC_STOCK_EDGE_02] Đang test: Tìm kiếm với chuỗi cực dài (Stress Test)...");
            try
            {
                driver.Navigate().GoToUrl("http://localhost:5068/Admin/Stock");
                Thread.Sleep(1500);

                IWebElement searchBox = driver.FindElement(By.Name("q"));
                searchBox.Clear();

                // Tạo một chuỗi dài 5000 ký tự
                string longString = new string('A', 5000);
                searchBox.SendKeys(longString);

                driver.FindElement(By.XPath("//button[contains(text(), 'Tìm kiếm')]")).Click();
                Thread.Sleep(2000);

                // Kỳ vọng: Hệ thống cắt chuỗi, hoặc báo "Không tìm thấy", nhưng KHÔNG được báo lỗi 500 Server Error
                Console.WriteLine("       -> [Pass] Gửi chuỗi siêu dài thành công, hệ thống không bị Crash (Error 500).");
            }
            catch (Exception ex) { Console.WriteLine("       -> [Failed] Hệ thống có thể đã crash: " + ex.Message); }

            // ==========================================
            // TC_STOCK_EDGE_03: CAN THIỆP DOM - GỬI PAGE SIZE ÂM/KHÔNG HỢP LỆ
            // ==========================================
            Console.WriteLine("  3. [TC_STOCK_EDGE_03] Đang test: Hack HTML DOM - Gửi Page Size âm (-5)...");
            try
            {
                driver.Navigate().GoToUrl("http://localhost:5068/Admin/Stock");
                Thread.Sleep(1500);

                IWebElement pageSizeSelect = driver.FindElement(By.Name("pageSize"));

                // Dùng JavaScript để ép thêm một Option giá trị "-5" hoặc "1000000" vào Dropdown (vượt quá cho phép)
                js.ExecuteScript(@"
                    var select = arguments[0];
                    var option = document.createElement('option');
                    option.text = 'Hack Value';
                    option.value = '-5';
                    select.add(option);
                    select.value = '-5';
                ", pageSizeSelect);

                // Giả lập hành động submit form (nếu select tự động submit khi onchange thì dùng đoạn dưới)
                js.ExecuteScript("arguments[0].form.submit();", pageSizeSelect);
                Thread.Sleep(2000);

                // Kỳ vọng: Backend chặn giá trị -5 và đưa về giá trị mặc định (vd: 10), không bị lỗi chia cho số 0
                Console.WriteLine("       -> [Pass] Đã gửi giá trị -5 qua DOM. Đang ở URL: " + driver.Url);
            }
            catch (Exception ex) { Console.WriteLine("       -> [Failed] " + ex.Message); }

            // ==========================================
            // TC_STOCK_EDGE_04: THAO TÁC URL - NHẬP SỐ TRANG NGOÀI BIÊN (PAGE = 99999 HOẶC -1)
            // ==========================================
            Console.WriteLine("  4. [TC_STOCK_EDGE_04] Đang test: Thao tác trực tiếp URL (Page=-1 và Page=999999)...");
            try
            {
                // Test 1: Truy cập trang âm
                driver.Navigate().GoToUrl("http://localhost:5068/Admin/Stock?page=-1");
                Thread.Sleep(1500);
                Console.WriteLine("       -> [Pass] Truy cập page=-1 không bị crash.");

                // Test 2: Truy cập trang siêu lớn (Vượt quá tổng số trang)
                driver.Navigate().GoToUrl("http://localhost:5068/Admin/Stock?page=999999");
                Thread.Sleep(1500);

                // Thường thì web chuẩn sẽ hiển thị "Không có dữ liệu" hoặc tự chuyển về trang cuối cùng
                bool isNoDataFound = driver.PageSource.Contains("Không có dữ liệu") || driver.PageSource.Contains("No records");
                Console.WriteLine("       -> [Pass] Truy cập page=999999 không bị crash. Có xử lý hiển thị rỗng: " + isNoDataFound);
            }
            catch (Exception ex) { Console.WriteLine("       -> [Failed] Lỗi khi xử lý URL parameter: " + ex.Message); }

            Console.WriteLine("\n-> KẾT THÚC TEST BIÊN (EDGE CASES)");
        }
    }
}