namespace WallpaperDownloader.App
{
    using System;
    using System.IO;
    using OpenQA.Selenium;
    using OpenQA.Selenium.Chrome;
    using OpenQA.Selenium.Firefox;
    using Utilities;

    public static class BrowserFactory
    {
        public static IWebDriver GetBrowser(BrowserType type)
        {
            IWebDriver browser;
            switch (type)
            {
                case BrowserType.Firefox:
                    browser = GetFirefoxBrowser();
                    break;
                case BrowserType.Chrome:
                    browser = GetChromeBrowser();
                    break;
                default:
                    throw new InvalidOperationException("The browser choice is invalid.");
            }

            browser.Manage().Timeouts().ImplicitlyWait(TimeSpan.FromSeconds(5));
            return browser;
        }

        private static IWebDriver GetFirefoxBrowser()
        {
            var profile = new FirefoxProfile();
            profile.AcceptUntrustedCertificates = true;
            profile.SetPreference("browser.download.folderList", 2);
            profile.SetPreference("browser.download.dir", Path.GetFullPath(Constants.WallpapersDirectory));
            profile.SetPreference("browser.helperApps.neverAsk.saveToDisk", Constants.ImageMimeTypes);
            var browser = new FirefoxDriver(profile);
            return browser;
        }

        private static IWebDriver GetChromeBrowser()
        {
            var service = ChromeDriverService.CreateDefaultService(Path.GetFullPath(Constants.DriversDirectory));
            var options = new ChromeOptions();
            options.AddUserProfilePreference("download.default_directory", Path.GetFullPath(Constants.WallpapersDirectory));
            var browser = new ChromeDriver(service, options);
            return browser;
        }
    }
}
