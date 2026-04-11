using System;
using System.Threading;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace StockTest
{
    class Program
    {
        static void Main(string[] args)
        {
            // Khởi tạo Driver 1 lần
            IWebDriver driver = new ChromeDriver();

            try
            {
                driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10);
                driver.Manage().Window.Maximize();

                // GỌI CÁC CLASS KHÁC VÀO ĐÂY ĐỂ CHẠY

                //============== [BẮT ĐẦU TEST CASES - TRẦN VIẾT HẢI]
                //Trần Viết Hải ]
                //==============
                LoginTest.PerformLogin(driver);
                //StockIndexTest.RunAllTests(driver);
                //PurchaseTest.RunAllTests(driver);
                SupplierTest.RunAllTests(driver);

                //================
                //Lê Thị Thúy Nhi]
                //================
                //ProductTest.RunAllTests(driver);

                //================
                //Nguyễn Bá Thịnh]
                //================
                // CategoryTest.RunAllTests(driver);
                // UnitTest.RunAllTests(driver);
                // BrandTest.RunAllTests(driver);

                Console.WriteLine("\n=== ĐÃ HOÀN THÀNH TOÀN BỘ TEST CASES ===");
            }
            catch (Exception ex)
            {
                Console.WriteLine("\n[!] Có lỗi xảy ra trong quá trình chạy: " + ex.Message);
            }
            finally
            {
                Thread.Sleep(3000);
                driver.Quit();
            }
        }
    }
}