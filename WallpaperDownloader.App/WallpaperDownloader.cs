namespace WallpaperDownloader.App
{
    using System;
    using System.IO;
    using System.Net;
    using OpenQA.Selenium;
    using OpenQA.Selenium.Firefox;
    using OpenQA.Selenium.Chrome;
    using OpenQA.Selenium.Interactions;
    using System.Linq;
    using System.Threading;

    public class WallpaperDownloader
    {
        public const string WallpapersDirectory = "../../Wallpapers";
        public const int DefaultDownloadTimeout = 1000;
        public static readonly string[] TemporaryFileExtensions = new[] { "*.part", "*.tmp", "*.crdownload" };

        private static IWebDriver browser;
        private static WebClient client = new WebClient();

        private static string baseUrl;
        private static int startPage;
        private static int endPage;

        public static void Main()
        {
            SetupBrowser();
            browser.Manage().Timeouts().ImplicitlyWait(TimeSpan.FromSeconds(5));

            browser.Navigate().GoToUrl("http://wallpaperswide.com/");
            string categoryLink = SelectCategory(browser);
            SelectPages(categoryLink);
            DownloadImages();
            EnsureDownloadsHaveFinished();
            browser.Quit();
        }

        private static void SetupBrowser()
        {
            Console.WriteLine("Select a browser to use:{0}1. Mozilla Firefox{0}2. Google Chrome", Environment.NewLine + "  ");
            int browserChoice = int.Parse(Console.ReadLine());
            switch (browserChoice)
            {
                case 1:
                    SetupFirefoxBrowser();
                    break;
                case 2:
                    SetupChromeBrowser();
                    break;
                default:
                    Console.WriteLine("Your browser choice is invalid.");
                    break;
            }

            Console.Clear();
        }

        private static void SetupFirefoxBrowser()
        {
            var profile = new FirefoxProfile();
            profile.AcceptUntrustedCertificates = true;
            profile.SetPreference("browser.download.folderList", 2);
            profile.SetPreference("browser.download.dir", Path.GetFullPath(WallpapersDirectory));
            profile.SetPreference("browser.helperApps.neverAsk.saveToDisk", "image/jpg,image/jpeg,image/png");
            browser = new FirefoxDriver(profile);
        }

        private static void SetupChromeBrowser()
        {
            var options = new ChromeOptions();
            options.AddUserProfilePreference("download.default_directory", Path.GetFullPath(WallpapersDirectory));
            browser = new ChromeDriver(options);
        }

        private static string SelectCategory(IWebDriver browser)
        {
            var categories = browser.FindElements(By.CssSelector(".side-panel.categories > li"));
            Console.WriteLine("Please choose a category:");
            for (int i = 1; i <= categories.Count; i++)
            {
                Console.WriteLine("{0}. {1}", i, categories[i - 1].Text);
            }

            Console.Write("Selected category number: ");
            int selectedCategoryIndex = int.Parse(Console.ReadLine()) - 1;
            string categoryLink = categories[selectedCategoryIndex].FindElement(By.TagName("a")).GetAttribute("href");
            browser.Navigate().GoToUrl(categoryLink);
            Console.WriteLine("Do you want see subcategories chooose a subcategory?");
            Console.WriteLine("1.Yes{0}2.No", Environment.NewLine);
            int wantSubcategoriesNumberChoose = int.Parse(Console.ReadLine());
            if (wantSubcategoriesNumberChoose == 1)
            {
                var subcategories = browser.FindElements(By.CssSelector(@".side-panel.categories > li[style=""padding-left:5px;""]"));
                if (subcategories.Any())
                {
                    Console.WriteLine("Please choose a subcategory:");
                    for (int i = 1; i <= subcategories.Count; i++)
                    {
                        Console.WriteLine("{0}. {1}", i, subcategories[i - 1].Text);
                    }

                    int selectedSubcategoryIndex = int.Parse(Console.ReadLine()) - 1;
                    categoryLink = subcategories[selectedSubcategoryIndex].FindElement(By.TagName("a")).GetAttribute("href");
                }
                else
                {
                    Console.WriteLine("Sorry this category don't have subcategories.");
                }
            }

            return categoryLink;
        }

        private static void SelectPages(string categoryLink)
        {
            browser.Navigate().GoToUrl(categoryLink);
            var lastPageElement = browser.FindElement(By.CssSelector(".pagination:last-of-type > a:nth-last-child(2)"));
            string lastPageUrl = lastPageElement.GetAttribute("href");
            baseUrl = lastPageUrl.Substring(0, lastPageUrl.LastIndexOf("/") + 1);
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
            Console.Clear();
            for (int page = startPage; page <= endPage; page++)
            {
                browser.Navigate().GoToUrl(baseUrl + page);
                var wallpaperPreviewsCount = browser.FindElements(By.CssSelector("li.wall img")).Count;
                for (int currentIndex = 1; currentIndex <= wallpaperPreviewsCount; currentIndex++)
                {
                    ((IJavaScriptExecutor)browser).ExecuteScript(
                        "[].forEach.call(document.querySelectorAll(\"#huddown\"), function(el) { el.style.display = \"block\"; })");

                    var wallpaperPreview = browser.FindElement(By.CssSelector("li.wall:nth-of-type(" + currentIndex + ") img"));
                    var arrow = browser.FindElement(By.CssSelector("li.wall:nth-of-type(" + currentIndex + ") #huddown"));
                    Actions actions = new Actions(browser);
                    actions
                        .MoveToElement(arrow)
                        .Click()
                        .Build()
                        .Perform();
                    TryDownloadImage();
                }
            }

            Console.ResetColor();
        }

        private static void TryDownloadImage()
        {
            try
            {
                var frame = browser.FindElement(By.Id("notifyFrame"));
                browser.SwitchTo().Frame(frame);
                browser.FindElement(By.Id("dwres_btn")).Click();
                browser.SwitchTo().DefaultContent();
                browser.FindElement(By.CssSelector(".ui-icon.bw-icon-b1")).Click();
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Image downloaded");
            }
            catch (NoSuchElementException)
            {
                // It's OK, just skip the image
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Image skipped");
            }
        }

        private static void EnsureDownloadsHaveFinished()
        {
            while (true)
            {
                var files = TemporaryFileExtensions.SelectMany(extension => Directory.GetFiles(WallpapersDirectory, extension));
                if (!files.Any())
                {
                    break;
                }

                Thread.Sleep(DefaultDownloadTimeout);
            }
        }
    }
}