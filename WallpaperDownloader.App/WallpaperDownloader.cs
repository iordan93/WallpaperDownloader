namespace WallpaperDownloader.App
{
    using System;
    using System.IO;
    using System.Net;
    using OpenQA.Selenium;
    using OpenQA.Selenium.Firefox;

    public class WallpaperDownloader
    {
        public const string WallpapersDirectory = "../../Wallpapers";

        private static FirefoxDriver firefox;
        private static WebClient client = new WebClient();

        private static string baseUrl;
        private static int startPage;
        private static int endPage;

        public static void Main()
        {
            var profile = SetupBrowser();
            firefox = new FirefoxDriver(profile);
            firefox.Manage().Timeouts().ImplicitlyWait(TimeSpan.FromSeconds(5));

            firefox.Navigate().GoToUrl("http://wallpaperswide.com/");
            string categoryLink = SelectCategory(firefox);
            SelectPages(categoryLink);
            DownloadImages();
            firefox.Quit();
        }

        private static FirefoxProfile SetupBrowser()
        {
            var profile = new FirefoxProfile();
            profile.AcceptUntrustedCertificates = true;
            profile.SetPreference("browser.download.folderList", 2);
            profile.SetPreference("browser.download.dir", Path.GetFullPath(WallpapersDirectory));
            profile.SetPreference("browser.helperApps.neverAsk.saveToDisk", "image/jpg,image/jpeg,image/png");
            return profile;
        }

        private static string SelectCategory(FirefoxDriver firefox)
        {
            var categories = firefox.FindElementsByCssSelector(".side-panel.categories > li");
            Console.WriteLine("Please choose a category:");
            for (int i = 1; i <= categories.Count; i++)
            {
                Console.WriteLine("{0}. {1}", i, categories[i - 1].Text);
            }

            Console.Write("Selected category number: ");
            int selectedCategoryIndex = int.Parse(Console.ReadLine()) - 1;
            string categoryLink = categories[selectedCategoryIndex].FindElement(By.TagName("a")).GetAttribute("href");
            return categoryLink;
        }

        private static void SelectPages(string categoryLink)
        {
            firefox.Navigate().GoToUrl(categoryLink);
            var lastPageElement = firefox.FindElementByCssSelector(".pagination:last-of-type > a:nth-last-child(2)");
            string lastPageUrl = lastPageElement.GetAttribute("href");
            baseUrl = lastPageUrl.Substring(0, lastPageUrl.LastIndexOf("/"));
            string maxPageString = lastPageElement.Text;
            int maxPage = int.Parse(maxPageString);

            Console.Write("Please type the start page (1-{0}): ", maxPage);
            startPage = int.Parse(Console.ReadLine());
            if (startPage < 1 || startPage > maxPage)
            {
                throw new ArgumentOutOfRangeException("startPage");
            }

            Console.Write("Please type the end page ({0}-{1}): ", startPage, maxPage);
            endPage = int.Parse(Console.ReadLine());
            if (endPage < startPage || endPage > maxPage)
            {
                throw new ArgumentOutOfRangeException("endPage");
            }
        }

        private static void DownloadImages()
        {
            for (int page = startPage; page <= endPage; page++)
            {
                firefox.Navigate().GoToUrl(baseUrl + "/" + page);
                var wallpaperPreviewsCount = firefox.FindElementsByCssSelector("li.wall img").Count;
                for (int currentIndex = 1; currentIndex <= wallpaperPreviewsCount; currentIndex++)
                {
                    var wallpaperPreview = firefox.FindElementByCssSelector("li.wall:nth-of-type(" + currentIndex + ") img");
                    wallpaperPreview.Click();
                    firefox
                        .FindElementById("wallpaper-resolutions")
                        .FindElement(By.CssSelector("a[style]"))
                        .Click();
                    firefox.Navigate().Back();
                }
            }
        }
    }
}
