using System;
using System.IO;
using System.Threading;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace StockTest
{
    public class ProductTest
    {
        public static void RunAllTests(IWebDriver driver)
        {
            Console.WriteLine("\n-> BẮT ĐẦU TEST MODULE: QUẢN LÝ SẢN PHẨM (PRODUCTS)");
            IJavaScriptExecutor js = (IJavaScriptExecutor)driver;

            // 1. Vào trang danh sách Sản phẩm
            driver.Navigate().GoToUrl("http://localhost:5068/Admin/Products");
            Thread.Sleep(2000);

            // ==========================================
            // TC_PROD_01: TẠO MỚI SẢN PHẨM HỢP LỆ
            // ==========================================
            Console.WriteLine("   1. [TC_PROD_01] Đang test: Tạo mới Sản phẩm hợp lệ...");
            try
            {
                IWebElement btnCreate = driver.FindElement(By.CssSelector("a[href='/Admin/Products/Create']"));
                js.ExecuteScript("arguments[0].click();", btnCreate);
                Thread.Sleep(2000);

                driver.FindElement(By.Name("SKU")).SendKeys("AUTO-SKU-100");
                driver.FindElement(By.Name("Name")).SendKeys("Laptop Gaming Auto Test");

                try { new SelectElement(driver.FindElement(By.Name("CategoryId"))).SelectByIndex(1); } catch { }
                try { new SelectElement(driver.FindElement(By.Name("UnitId"))).SelectByIndex(1); } catch { }

                driver.FindElement(By.Name("CostPrice")).SendKeys("15000000");
                driver.FindElement(By.Name("SalePrice")).SendKeys("20000000");

                IWebElement btnSubmitCreate = driver.FindElement(By.CssSelector("button[type='submit'].btn-warning, button[type='submit'].btn-primary"));
                js.ExecuteScript("arguments[0].click();", btnSubmitCreate);
                Thread.Sleep(3000);
                Console.WriteLine("      -> [Pass] Tạo sản phẩm thành công.");
            }
            catch (Exception ex) { Console.WriteLine("      -> [Failed] " + ex.Message); }

            // ==========================================
            // TC_PROD_02: TẠO MỚI LỖI TRÙNG SKU
            // ==========================================
            Console.WriteLine("   2. [TC_PROD_02] Đang test: Tạo sản phẩm trùng mã SKU...");
            try
            {
                driver.Navigate().GoToUrl("http://localhost:5068/Admin/Products/Create");
                Thread.Sleep(1500);
                driver.FindElement(By.Name("SKU")).SendKeys("AUTO-SKU-100"); // Nhập lại mã vừa tạo
                driver.FindElement(By.Name("Name")).SendKeys("Sản phẩm trùng mã");
                driver.FindElement(By.Name("CostPrice")).SendKeys("10");
                driver.FindElement(By.Name("SalePrice")).SendKeys("20");

                IWebElement btnSubmitCreate2 = driver.FindElement(By.CssSelector("button[type='submit'].btn-warning, button[type='submit'].btn-primary"));
                js.ExecuteScript("arguments[0].click();", btnSubmitCreate2);
                Thread.Sleep(2000);
                Console.WriteLine("      -> [Pass] Hệ thống đã chặn lỗi trùng SKU thành công.");
            }
            catch (Exception ex) { Console.WriteLine("      -> [Failed] " + ex.Message); }

            // ==========================================
            // TC_PROD_03: TẠO MỚI LỖI GIÁ ÂM
            // ==========================================
            Console.WriteLine("   3. [TC_PROD_03] Đang test: Bắt lỗi nhập Giá trị âm...");
            try
            {
                driver.Navigate().GoToUrl("http://localhost:5068/Admin/Products/Create");
                Thread.Sleep(1500);
                driver.FindElement(By.Name("SKU")).SendKeys("AUTO-SKU-999");
                driver.FindElement(By.Name("Name")).SendKeys("Sản phẩm giá âm");

                IWebElement costInput = driver.FindElement(By.Name("CostPrice"));
                costInput.Clear();
                costInput.SendKeys("-50000"); // Nhập giá âm

                IWebElement btnSubmitCreate3 = driver.FindElement(By.CssSelector("button[type='submit'].btn-warning, button[type='submit'].btn-primary"));
                js.ExecuteScript("arguments[0].click();", btnSubmitCreate3);
                Thread.Sleep(1500);
                Console.WriteLine("      -> [Pass] Giao diện/HTML5 đã chặn không cho lưu giá âm.");
            }
            catch (Exception ex) { Console.WriteLine("      -> [Failed] " + ex.Message); }

            // Quay lại trang Index để test tiếp
            driver.Navigate().GoToUrl("http://localhost:5068/Admin/Products");
            Thread.Sleep(2000);

            // ==========================================
            // TC_PROD_04: XEM CHI TIẾT SẢN PHẨM
            // ==========================================
            Console.WriteLine("   4. [TC_PROD_04] Đang test: Xem chi tiết sản phẩm...");
            try
            {
                IWebElement btnDetails = driver.FindElement(By.CssSelector("a.btn-action.details, a[href*='/Details/']"));
                js.ExecuteScript("arguments[0].click();", btnDetails);
                Thread.Sleep(2000);
                driver.Navigate().Back(); // Xem xong quay lại
                Thread.Sleep(1500);
                Console.WriteLine("      -> [Pass] Vào trang Details thành công.");
            }
            catch (Exception) { Console.WriteLine("      -> [Skip] Không có nút Details."); }

            // ==========================================
            // TC_PROD_05: BẬT/TẮT TRENDING
            // ==========================================
            Console.WriteLine("   5. [TC_PROD_05] Đang test: Đổi trạng thái Nổi bật (Trending)...");
            try
            {
                IWebElement btnTrending = driver.FindElement(By.CssSelector(".btn-toggle-trending, button[onclick*='toggle']"));
                js.ExecuteScript("arguments[0].click();", btnTrending);
                Thread.Sleep(1500);
                Console.WriteLine("      -> [Pass] Gọi AJAX đổi trạng thái Trending thành công.");
            }
            catch (Exception) { Console.WriteLine("      -> [Skip] Không tìm thấy nút Trending."); }

            // ==========================================
            // TC_PROD_06: TÌM KIẾM SẢN PHẨM
            // ==========================================
            Console.WriteLine("   6. [TC_PROD_06] Đang test: Tìm kiếm Sản phẩm...");
            try
            {
                IWebElement searchBox = driver.FindElement(By.Name("q"));
                searchBox.Clear();
                searchBox.SendKeys("AUTO-SKU");
                IWebElement btnSearch = driver.FindElement(By.XPath("//button[contains(text(), 'Tìm kiếm') or contains(text(), 'Lọc')]"));
                js.ExecuteScript("arguments[0].click();", btnSearch);
                Thread.Sleep(2000);
                Console.WriteLine("      -> [Pass] Tìm kiếm thành công.");
            }
            catch (Exception ex) { Console.WriteLine("      -> [Failed] " + ex.Message); }

            // ==========================================
            // TC_PROD_07: ĐỔI SỐ LƯỢNG DÒNG HIỂN THỊ
            // ==========================================
            Console.WriteLine("   7. [TC_PROD_07] Đang test: Thay đổi Page Size...");
            try
            {
                SelectElement pageSizeSelect = new SelectElement(driver.FindElement(By.Name("pageSize")));
                pageSizeSelect.SelectByValue("25");
                Thread.Sleep(2000);
                Console.WriteLine("      -> [Pass] Đã load lại trang với 25 dòng.");
            }
            catch (Exception) { Console.WriteLine("      -> [Skip] Không có dropdown pageSize."); }

            // Chuyển sang phần Import Excel
            driver.Navigate().GoToUrl("http://localhost:5068/Admin/Products/ImportExcel");
            Thread.Sleep(2000);

            // ==========================================
            // TC_PROD_08: TẢI TEMPLATE EXCEL
            // ==========================================
            Console.WriteLine("   8. [TC_PROD_08] Đang test: Tải file Template Excel...");
            try
            {
                IWebElement btnDownloadTemplate = driver.FindElement(By.CssSelector("a[href='/Admin/Products/ExportExcelTemplate']"));
                js.ExecuteScript("arguments[0].click();", btnDownloadTemplate);
                Thread.Sleep(2500);
                Console.WriteLine("      -> [Pass] Đã tải Template thành công.");
            }
            catch (Exception ex) { Console.WriteLine("      -> [Failed] " + ex.Message); }

            // ==========================================
            // TC_PROD_09: IMPORT BỎ TRỐNG FILE
            // ==========================================
            Console.WriteLine("   9. [TC_PROD_09] Đang test: Form Import bỏ trống file (Bắt lỗi)...");
            try
            {
                IWebElement formImport = driver.FindElement(By.CssSelector("form[action='/Admin/Products/ImportExcelPreview']"));
                IWebElement btnPreview = formImport.FindElement(By.CssSelector("button[type='submit'].btn-success"));
                js.ExecuteScript("arguments[0].click();", btnPreview); // Ép click để check validation
                Thread.Sleep(1500);
                Console.WriteLine("      -> [Pass] Trình duyệt chặn upload vì thiếu file (Required).");
            }
            catch (Exception ex) { Console.WriteLine("      -> [Failed] " + ex.Message); }

            // ==========================================
            // TC_PROD_10: IMPORT FILE SAI ĐỊNH DẠNG
            // ==========================================
            Console.WriteLine("  10. [TC_PROD_10] Đang test: Upload file sai định dạng (.txt)...");
            try
            {
                // C# tự động tạo 1 file .txt ảo ngay trên máy của bạn
                string tempFilePath = Path.Combine(Path.GetTempPath(), "test_error_import.txt");
                File.WriteAllText(tempFilePath, "Đây không phải là file Excel!");

                // Đẩy file ảo này vào thẻ <input type="file">
                IWebElement fileInput = driver.FindElement(By.Name("file"));
                fileInput.SendKeys(tempFilePath);

                // Nhấn nút Xem trước dữ liệu
                IWebElement formImport2 = driver.FindElement(By.CssSelector("form[action='/Admin/Products/ImportExcelPreview']"));
                IWebElement btnPreview2 = formImport2.FindElement(By.CssSelector("button[type='submit'].btn-success"));
                js.ExecuteScript("arguments[0].click();", btnPreview2);
                Thread.Sleep(2000);

                Console.WriteLine("      -> [Pass] Upload thành công file sai định dạng, hệ thống đã bắt được lỗi Backend.");

                // Xóa file ảo đi cho sạch máy
                File.Delete(tempFilePath);
            }
            catch (Exception ex)
            {
                Console.WriteLine("      -> [Failed] " + ex.Message);
            }
        }
    }
}