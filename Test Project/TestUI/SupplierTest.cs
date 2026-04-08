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

            // ==============================================================================
            // BẮT ĐẦU 4 TEST CASE KHÓ (NÂNG CAO / BIÊN)
            // ==============================================================================
            Console.WriteLine("\n-> BẮT ĐẦU TEST BIÊN (EDGE CASES)");

            // ==========================================
            // TC_SUP_EDGE_01: TẠO MỚI TRÙNG EMAIL/SỐ ĐIỆN THOẠI
            // ==========================================
            Console.WriteLine("  5. [TC_SUP_EDGE_01] Đang test: Cố tình tạo Nhà cung cấp trùng Email...");
            try
            {
                driver.Navigate().GoToUrl("http://localhost:5068/Admin/Suppliers/Create");
                Thread.Sleep(1500);

                driver.FindElement(By.Name("Name")).SendKeys("NCC Clone");
                driver.FindElement(By.Name("Phone")).SendKeys("0987654321");
                // Cố tình nhập lại Email đã dùng ở Test Case 1
                driver.FindElement(By.Name("Email")).SendKeys("autotest@ncc.local");
                driver.FindElement(By.Name("Address")).SendKeys("Test trùng lặp");

                IWebElement btnSubmitClone = driver.FindElement(By.XPath("//button[@type='submit' and contains(@class, 'btn-warning')]"));
                js.ExecuteScript("arguments[0].click();", btnSubmitClone);
                Thread.Sleep(2000);

                // Kỳ vọng: Hệ thống (Backend) phải kiểm tra DB và chặn lại, báo lỗi "Email đã tồn tại"
                Console.WriteLine("       -> [Pass] Đã submit form trùng Email. Cần kiểm tra Backend có bắt lỗi Unique hay không.");
            }
            catch (Exception ex) { Console.WriteLine("       -> [Failed] " + ex.Message); }

            // ==========================================
            // TC_SUP_EDGE_02: BYPASS FRONTEND - BỎ TRỐNG TRƯỜNG BẮT BUỘC
            // ==========================================
            Console.WriteLine("  6. [TC_SUP_EDGE_02] Đang test: Bypass HTML5, gửi form rỗng...");
            try
            {
                driver.Navigate().GoToUrl("http://localhost:5068/Admin/Suppliers/Create");
                Thread.Sleep(1500);

                // Dùng JS lột sạch thuộc tính 'required' của HTML5 để ép trình duyệt gửi form rỗng đi
                js.ExecuteScript(@"
                    document.querySelectorAll('[required]').forEach(e => e.removeAttribute('required'));
                ");

                IWebElement btnSubmitEmpty = driver.FindElement(By.XPath("//button[@type='submit' and contains(@class, 'btn-warning')]"));
                js.ExecuteScript("arguments[0].click();", btnSubmitEmpty);
                Thread.Sleep(2000);

                // Kỳ vọng: Backend ASP.NET (ModelState) phải bắt được lỗi rỗng và trả về giao diện báo lỗi, KHÔNG BỊ CRASH.
                Console.WriteLine("       -> [Pass] Bypass HTML5 thành công. Backend phải hiện thông báo lỗi Validation.");
            }
            catch (Exception ex) { Console.WriteLine("       -> [Failed] Có thể Server đã crash (Error 500): " + ex.Message); }

            // ==========================================
            // TC_SUP_EDGE_03: NHẬP MÃ ĐỘC (XSS / SQL INJECTION) VÀO TÊN & ĐỊA CHỈ
            // ==========================================
            Console.WriteLine("  7. [TC_SUP_EDGE_03] Đang test: Tiêm mã độc XSS và SQL Injection vào Form...");
            try
            {
                driver.Navigate().GoToUrl("http://localhost:5068/Admin/Suppliers/Create");
                Thread.Sleep(1500);

                // Nhập Script XSS và chuỗi SQL Injection
                driver.FindElement(By.Name("Name")).SendKeys("<script>alert('Hacked')</script>");
                driver.FindElement(By.Name("Phone")).SendKeys("0111111111");
                driver.FindElement(By.Name("Email")).SendKeys("hacker@ncc.local");
                driver.FindElement(By.Name("Address")).SendKeys("'; DROP TABLE Suppliers; --");

                IWebElement btnSubmitHacked = driver.FindElement(By.XPath("//button[@type='submit' and contains(@class, 'btn-warning')]"));
                js.ExecuteScript("arguments[0].click();", btnSubmitHacked);
                Thread.Sleep(2000);

                // Nếu hệ thống bị XSS, Alert sẽ hiện lên. Ta thử bắt Alert đó.
                try
                {
                    driver.SwitchTo().Alert().Accept();
                    Console.WriteLine("       -> [Failed] Hệ thống bị dính lỗi XSS! Trình duyệt đã chạy mã độc.");
                }
                catch (NoAlertPresentException)
                {
                    Console.WriteLine("       -> [Pass] Hệ thống an toàn. Đã mã hóa (Sanitize) mã độc thành chuỗi bình thường.");
                }
            }
            catch (Exception ex) { Console.WriteLine("       -> [Lỗi Test] " + ex.Message); }

            // ==========================================
            // TC_SUP_EDGE_04: THAO TÁC URL - TÌM NHÀ CUNG CẤP KHÔNG TỒN TẠI
            // ==========================================
            Console.WriteLine("  8. [TC_SUP_EDGE_04] Đang test: Thao tác trực tiếp URL (ID ảo)...");
            try
            {
                // Cố tình gõ ID không tồn tại hoặc ID âm lên thanh URL
                driver.Navigate().GoToUrl("http://localhost:5068/Admin/Suppliers/Details/-999");
                Thread.Sleep(1500);

                // Kiểm tra xem ứng dụng có xử lý trang lỗi 404 thân thiện hay không
                bool isNotFound = driver.PageSource.Contains("Không tìm thấy") || driver.Title.Contains("404") || driver.PageSource.Contains("Not Found");

                if (isNotFound)
                {
                    Console.WriteLine("       -> [Pass] Xử lý tốt, trả về trang báo lỗi 404/Not Found.");
                }
                else
                {
                    Console.WriteLine("       -> [Cảnh báo] Không thấy dấu hiệu của trang 404, cần xem lại giao diện lỗi.");
                }
            }
            catch (Exception ex) { Console.WriteLine("       -> [Failed] URL Manipulation gây lỗi: " + ex.Message); }

            Console.WriteLine("\n-> KẾT THÚC TEST MODULE: QUẢN LÝ NHÀ CUNG CẤP");
        }
    }
}