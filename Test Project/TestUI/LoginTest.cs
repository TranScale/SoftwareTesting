using System;
using System.Threading;
using OpenQA.Selenium;

namespace StockTest
{
    public class LoginTest
    {
        public static void PerformLogin(IWebDriver driver)
        {
            Console.WriteLine("-> Đang thực hiện Đăng nhập...");

            // LƯU Ý ROUTING: Dựa vào form asp-area="Admin" asp-controller="Auth"
            // Đường dẫn của bạn có thể là /Admin/Auth/Login (hoặc /Login nếu bạn cấu hình route riêng). 
            // Hãy sửa lại link localhost bên dưới cho chuẩn xác nhé!
            driver.Navigate().GoToUrl("http://localhost:5068/Admin/Auth/Login");
            Thread.Sleep(1000); // Đợi form load ra một chút

            // 1. Nhập Email (Do asp-for="Email" sinh ra)
            IWebElement emailBox = driver.FindElement(By.Name("Email"));
            emailBox.Clear();
            emailBox.SendKeys("admin@osms.local");

            // 2. Nhập Mật khẩu (Do asp-for="Password" sinh ra)
            IWebElement passwordBox = driver.FindElement(By.Name("Password"));
            passwordBox.Clear();
            passwordBox.SendKeys("Admin@12345");

            // (Tùy chọn) Click checkbox Ghi nhớ đăng nhập
            // driver.FindElement(By.Id("remember")).Click();

            // 3. Click nút Đăng nhập
            driver.FindElement(By.CssSelector("button[type='submit']")).Click();

            // Đợi server xử lý kiểm tra tài khoản và chuyển hướng về trang Admin
            Thread.Sleep(2000);

            Console.WriteLine("   [Pass] Đăng nhập thành công!");
        }
    }
}