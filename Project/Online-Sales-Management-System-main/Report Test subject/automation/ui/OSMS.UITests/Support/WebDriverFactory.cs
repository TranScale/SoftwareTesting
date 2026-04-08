using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Edge;

namespace OSMS.UITests.Support;

public static class WebDriverFactory
{
    public static IWebDriver Create(AutomationSettings settings)
    {
        return settings.Browser switch
        {
            "edge" => CreateEdgeDriver(settings),
            _ => CreateChromeDriver(settings)
        };
    }

    private static IWebDriver CreateChromeDriver(AutomationSettings settings)
    {
        var options = new ChromeOptions();
        options.AcceptInsecureCertificates = true;
        options.AddArgument("--window-size=1600,1000");
        options.AddArgument("--disable-gpu");
        options.AddArgument("--no-sandbox");

        if (settings.Headless)
        {
            options.AddArgument("--headless=new");
        }

        return new ChromeDriver(options);
    }

    private static IWebDriver CreateEdgeDriver(AutomationSettings settings)
    {
        var options = new EdgeOptions();
        options.AcceptInsecureCertificates = true;
        options.AddArgument("--window-size=1600,1000");

        if (settings.Headless)
        {
            options.AddArgument("--headless=new");
        }

        return new EdgeDriver(options);
    }
}
