using System;
using System.Threading;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace StockTest
{
    public class CategoryTest
    {
        public static void RunAllTests(IWebDriver driver)
        {
            Console.WriteLine("\n-> BẮT ĐẦU TEST NGOẠI LỆ (NEGATIVE TEST): MODULE DANH MỤC (CATEGORIES)");

            string duplicateCategoryName = "Duplicate Cat " + DateTime.Now.Ticks;

            // ---------------------------------------------------------
            // TEST CASE 1: BỎ TRỐNG TRƯỜNG BẮT BUỘC (Tên danh mục)
            // ---------------------------------------------------------
            Console.WriteLine("   1. Đang test: Cố tình bỏ trống Tên danh mục...");
            driver.Navigate().GoToUrl("http://localhost:5068/Admin/Categories/Create");
            Thread.Sleep(1000);

            // Chỉ điền mô tả, bỏ trống Tên (Name)
            IWebElement nameInput = driver.FindElement(By.Name("Name"));
            nameInput.Clear();
            driver.FindElement(By.Name("Description")).SendKeys("Cố tình không nhập tên để test validation.");

            // Bấm Lưu
            driver.FindElement(By.CssSelector("button[type='submit'].btn-gradient")).Click();
            Thread.Sleep(1000);

            try
            {
                // CÁCH MỚI: Đọc câu thông báo lỗi popup của trình duyệt (HTML5 Validation)
                string validationMessage = nameInput.GetAttribute("validationMessage");

                if (!string.IsNullOrEmpty(validationMessage))
                {
                    Console.WriteLine($"      -> [Pass] Trình duyệt (HTML5) đã chặn lại thành công. Thông báo: {validationMessage}");
                }
                else
                {
                    // Nếu không có lỗi HTML5, tìm thẻ span báo lỗi của Backend (phòng hờ trường hợp thuộc tính required bị xóa)
                    IWebElement errorSpan = driver.FindElement(By.CssSelector("span[data-valmsg-for='Name']"));
                    Console.WriteLine($"      -> [Pass] Backend đã chặn lại. Thông báo lỗi: {errorSpan.Text}");
                }
            }
            catch (NoSuchElementException)
            {
                Console.WriteLine("      -> [Failed] Giao diện không báo lỗi khi bỏ trống Tên danh mục!");
            }

            // ---------------------------------------------------------
            // TEST CASE 2: TẠO TRÙNG TÊN DANH MỤC (Duplicate Name)
            // ---------------------------------------------------------
            Console.WriteLine("   2. Đang test: Cố tình tạo 2 danh mục trùng tên...");
            // Lần 1: Tạo hợp lệ
            driver.Navigate().GoToUrl("http://localhost:5068/Admin/Categories/Create");
            driver.FindElement(By.Name("Name")).SendKeys(duplicateCategoryName);
            driver.FindElement(By.CssSelector("button[type='submit'].btn-gradient")).Click();
            Thread.Sleep(1500);

            // Lần 2: Cố tình tạo lại với đúng cái tên vừa xong
            driver.Navigate().GoToUrl("http://localhost:5068/Admin/Categories/Create");
            driver.FindElement(By.Name("Name")).SendKeys(duplicateCategoryName);
            driver.FindElement(By.CssSelector("button[type='submit'].btn-gradient")).Click();
            Thread.Sleep(1500);

            try
            {
                // Tìm thông báo lỗi tổng quát hoặc lỗi cho trường Name do Controller trả về
                IWebElement summaryError = driver.FindElement(By.CssSelector(".validation-summary-errors, .text-danger"));
                Console.WriteLine("      -> [Pass] Backend đã chặn tạo trùng tên. Báo lỗi: " + summaryError.Text);
            }
            catch (NoSuchElementException)
            {
                Console.WriteLine("      -> [Failed] Hệ thống cho phép tạo danh mục trùng tên!");
            }

            // ---------------------------------------------------------
            // TEST CASE 3: NHẬP MÃ ĐỘC VÀO MÔ TẢ (XSS INJECTION)
            // ---------------------------------------------------------
            Console.WriteLine("   3. Đang test: Cố tình nhập script mã độc vào Mô tả (XSS)...");
            driver.Navigate().GoToUrl("http://localhost:5068/Admin/Categories/Create");

            // Nhập thẻ script để xem web có bị vỡ hoặc chạy script không
            string xssName = "XSS Test " + DateTime.Now.Ticks;
            string xssPayload = "<script>alert('Website của bạn đã bị tấn công XSS!');</script> <b>In đậm</b>";

            driver.FindElement(By.Name("Name")).SendKeys(xssName);
            driver.FindElement(By.Name("Description")).SendKeys(xssPayload);
            driver.FindElement(By.CssSelector("button[type='submit'].btn-gradient")).Click();
            Thread.Sleep(2000);

            Console.WriteLine("      -> [Done] Đã submit payload XSS. Hãy kiểm tra xem màn hình Index có bật popup Alert không (nếu không bật thì web đã an toàn).");

            // ---------------------------------------------------------
            // TEST CASE 4: TRUY CẬP ID DANH MỤC KHÔNG TỒN TẠI
            // ---------------------------------------------------------
            Console.WriteLine("   4. Đang test: Truy cập trang Edit với ID ảo (Không tồn tại)...");
            // Gõ URL với ID bằng 9999999
            driver.Navigate().GoToUrl("http://localhost:5068/Admin/Categories/Edit/9999999");
            Thread.Sleep(1000);

            // Kiểm tra xem hệ thống trả về trang 404 / Not Found hay là bị crash màn hình lỗi code
            string pageSource = driver.PageSource;
            if (pageSource.Contains("404") || pageSource.Contains("Not Found"))
            {
                Console.WriteLine("      -> [Pass] Controller đã xử lý tốt, trả về trang 404 Not Found.");
            }
            else if (pageSource.Contains("NullReferenceException") || pageSource.Contains("Exception"))
            {
                Console.WriteLine("      -> [Failed] Hệ thống bị Crash (văng lỗi Exception) khi ID không tồn tại!");
            }
            else
            {
                Console.WriteLine("      -> [Warning] Cần tự kiểm tra lại UI xem hệ thống phản hồi thế nào với ID ảo.");
            }
        }
    }
}