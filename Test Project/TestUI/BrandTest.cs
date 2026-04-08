using System;
using System.Threading;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace StockTest
{
    public class BrandTest
    {
        public static void RunAllTests(IWebDriver driver)
        {
            Console.WriteLine("\n-> BẮT ĐẦU TEST NGOẠI LỆ (NEGATIVE TEST): MODULE THƯƠNG HIỆU");

            string duplicateName = "Apple_" + DateTime.Now.Ticks;

            // ---------------------------------------------------------
            // TEST CASE 1: BỎ TRỐNG TRƯỜNG BẮT BUỘC (Empty Name)
            // ---------------------------------------------------------
            Console.WriteLine("   1. Đang test: Cố tình bỏ trống tên thương hiệu...");
            driver.Navigate().GoToUrl("http://localhost:5068/Admin/Brands/Create");
            Thread.Sleep(1000);

            // Xóa rỗng tên và bấm Submit luôn
            driver.FindElement(By.Name("Name")).Clear();
            driver.FindElement(By.CssSelector("button[type='submit'].btn-primary")).Click();
            Thread.Sleep(1000);

            // Kiểm tra xem hệ thống có hiển thị câu thông báo lỗi (Validation) không
            try
            {
                IWebElement errorSpan = driver.FindElement(By.CssSelector("span[data-valmsg-for='Name']"));
                Console.WriteLine($"      -> [Pass] Hệ thống đã chặn lại. Lỗi hiển thị: {errorSpan.Text}");
            }
            catch (NoSuchElementException)
            {
                Console.WriteLine("      -> [Failed] Hệ thống không báo lỗi khi bỏ trống Tên!");
            }

            // ---------------------------------------------------------
            // TEST CASE 2: TẠO TRÙNG TÊN THƯƠNG HIỆU (Duplicate Name)
            // ---------------------------------------------------------
            Console.WriteLine("   2. Đang test: Cố tình tạo 2 thương hiệu trùng tên...");
            // Bước A: Tạo thương hiệu lần 1 (Chắc chắn thành công)
            driver.Navigate().GoToUrl("http://localhost:5068/Admin/Brands/Create");
            driver.FindElement(By.Name("Name")).SendKeys(duplicateName);
            driver.FindElement(By.CssSelector("button[type='submit'].btn-primary")).Click();
            Thread.Sleep(1500);

            // Bước B: Tạo thương hiệu lần 2 với cùng cái tên đó
            driver.Navigate().GoToUrl("http://localhost:5068/Admin/Brands/Create");
            driver.FindElement(By.Name("Name")).SendKeys(duplicateName);
            driver.FindElement(By.CssSelector("button[type='submit'].btn-primary")).Click();
            Thread.Sleep(1500);

            // Kiểm tra xem logic trong Controller có bắt lỗi "Brand name already exists." không
            try
            {
                // Thông báo lỗi tổng quát thường nằm trong validation-summary
                IWebElement summaryError = driver.FindElement(By.CssSelector(".validation-summary-errors, .text-danger"));
                Console.WriteLine("      -> [Pass] Hệ thống đã chặn tạo trùng tên. Báo lỗi: " + summaryError.Text);
            }
            catch (NoSuchElementException)
            {
                Console.WriteLine("      -> [Failed] Hệ thống cho phép tạo trùng tên!");
            }

            // ---------------------------------------------------------
            // TEST CASE 3: NHẬP MÃ ĐỘC (XSS INJECTION TEST)
            // ---------------------------------------------------------
            Console.WriteLine("   3. Đang test: Cố tình nhập mã độc (XSS) vào tên thương hiệu...");
            driver.Navigate().GoToUrl("http://localhost:5068/Admin/Brands/Create");

            // Nhập thử 1 đoạn script cảnh báo
            string xssPayload = "<script>alert('Bị hack!');</script>";
            driver.FindElement(By.Name("Name")).SendKeys(xssPayload);
            driver.FindElement(By.CssSelector("button[type='submit'].btn-primary")).Click();
            Thread.Sleep(1500);

            // Tùy thuộc vào .NET Core có tự động mã hóa (encode) html hay ném lỗi HttpRequestValidationException
            Console.WriteLine("      -> [Done] Đã submit payload XSS. Cần kiểm tra UI hoặc Database xem hệ thống có mã hóa thẻ <script> thành &lt;script&gt; không.");

            // ---------------------------------------------------------
            // TEST CASE 4: TRUY CẬP ID KHÔNG TỒN TẠI (Invalid ID)
            // ---------------------------------------------------------
            Console.WriteLine("   4. Đang test: Truy cập trang Edit với ID không tồn tại...");
            // Cố tình gõ ID rất lớn trên URL (Vd: ID = 9999999)
            driver.Navigate().GoToUrl("http://localhost:5068/Admin/Brands/Edit/9999999");
            Thread.Sleep(1000);

            // Code của bạn có dòng: if (entity == null) return NotFound();
            // Do đó ta sẽ check xem Page Title hoặc Page Source có chữ "Not Found" hoặc lỗi 404 không
            if (driver.PageSource.Contains("404") || driver.PageSource.Contains("Not Found"))
            {
                Console.WriteLine("      -> [Pass] Hệ thống trả về 404 Not Found đúng như thiết kế.");
            }
            else
            {
                Console.WriteLine("      -> [Warning] Hệ thống không hiển thị trang lỗi 404 rõ ràng (hoặc giao diện chưa xử lý trang 404).");
            }
        }
    }
}