namespace WallpaperDownloader.App
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Threading;
    using OpenQA.Selenium;
    using OpenQA.Selenium.Interactions;
    using Utilities;

    public class DownloadEngine
    {
        private WebClient client = new WebClient();

        private int startPage;
        private int endPage;

        public IWebDriver Browser { get; private set; }

        public void Run()
        {
            Console.WriteLine("Select a browser to use:{0}1. Mozilla Firefox{0}2. Google Chrome", Environment.NewLine);
            Console.Write("Selected browser: ");
            int browserType = int.Parse(Console.ReadLine());
            this.Browser = BrowserFactory.GetBrowser((BrowserType)browserType);
            Console.Clear();

            this.Browser.Navigate().GoToUrl(Constants.BaseSiteUrl);

            Console.WriteLine("Download from a category or search by a given term?{0}1. Category{0}2. Search", Environment.NewLine);
            int downloadTypeChoice = int.Parse(Console.ReadLine());

            string baseUrl = string.Empty;
            string searchTerm = string.Empty;
            switch (downloadTypeChoice)
            {
                case 1:
                    string categoryLink = SelectCategory();
                    baseUrl = SelectPages(categoryLink, null);
                    break;
                case 2:
                    Console.Write("Search for: ");
                    searchTerm = Console.ReadLine();
                    baseUrl = SelectPages("http://wallpaperswide.com/search.html", searchTerm);
                    break;
                default:
                    Console.WriteLine("Your download type choice is invalid.");
                    break;
            }

            DownloadImages(baseUrl, searchTerm);
            EnsureDownloadsHaveFinished();
            this.Browser.Quit();
        }

        private string SelectCategory()
        {
            var categories = this.Browser.FindElements(By.CssSelector(".side-panel.categories > li"));
            Console.WriteLine("Please choose a category:");
            for (int i = 1; i <= categories.Count; i++)
            {
                Console.WriteLine("{0}. {1}", i, categories[i - 1].Text);
            }

            Console.Write("Selected category number: ");
            int selectedCategoryIndex = int.Parse(Console.ReadLine()) - 1;
            string categoryLink = categories[selectedCategoryIndex].FindElement(By.TagName("a")).GetAttribute("href");
            string imagesInCategoryCount = categories[selectedCategoryIndex].FindElement(By.TagName("small")).Text;
            this.Browser.Navigate().GoToUrl(categoryLink);

            var subcategories = this.Browser.FindElements(By.CssSelector(".side-panel.categories > li[style=\"padding-left:5px;\"]"));
            if (subcategories.Any())
            {
                Console.WriteLine("Please choose a subcategory:");
                Console.WriteLine("0. All {0}", imagesInCategoryCount);
                for (int i = 1; i <= subcategories.Count; i++)
                {
                    Console.WriteLine("{0}. {1}", i, subcategories[i - 1].Text);
                }

                Console.Write("Selected subcategory number: ");
                int selectedSubcategoryIndex = int.Parse(Console.ReadLine()) - 1;
                if (selectedSubcategoryIndex >= 0)
                {
                    categoryLink = subcategories[selectedSubcategoryIndex].FindElement(By.TagName("a")).GetAttribute("href");
                }
            }

            return categoryLink;
        }

        // TODO: Overloaded methods?
        // TODO: Link / Url -> rename to be consistent
        private string SelectPages(string categoryLink, string searchTerm)
        {
            string downloadLink = categoryLink;
            if (!string.IsNullOrEmpty(searchTerm))
            {
                // TODO: Extract method
                downloadLink += "?q=" + WebUtility.UrlEncode(searchTerm);
            }

            this.Browser.Navigate().GoToUrl(downloadLink);
            var lastPageElement = this.Browser.FindElement(By.CssSelector(".pagination:last-of-type > a:nth-last-child(2)"));
            string lastPageUrl = lastPageElement.GetAttribute("href");
            string baseDownloadUrl = lastPageUrl.Substring(0, lastPageUrl.LastIndexOf("/") + 1);
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

            return baseDownloadUrl;
        }

        private void DownloadImages(string baseUrl, string searchTerm)
        {
            Console.Clear();
            for (int page = startPage; page <= endPage; page++)
            {
                string downloadUrl = baseUrl + page;
                if (!string.IsNullOrEmpty(searchTerm))
                {
                    downloadUrl += "?q=" + WebUtility.UrlEncode(searchTerm);
                }

                this.Browser.Navigate().GoToUrl(downloadUrl);
                var wallpaperPreviewsCount = this.Browser.FindElements(By.CssSelector("li.wall img")).Count;
                for (int currentIndex = 1; currentIndex <= wallpaperPreviewsCount; currentIndex++)
                {
                    ((IJavaScriptExecutor)this.Browser).ExecuteScript(
                        "[].forEach.call(document.querySelectorAll(\"#huddown\"), function(el) { el.style.display = \"block\"; })");

                    var wallpaperPreview = this.Browser.FindElement(By.CssSelector("li.wall:nth-of-type(" + currentIndex + ") img"));
                    var arrow = this.Browser.FindElement(By.CssSelector("li.wall:nth-of-type(" + currentIndex + ") #huddown"));
                    Actions actions = new Actions(this.Browser);
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

        private void TryDownloadImage()
        {
            try
            {
                var frame = this.Browser.FindElement(By.Id("notifyFrame"));
                this.Browser.SwitchTo().Frame(frame);
                this.Browser.FindElement(By.Id("dwres_btn")).Click();
                this.Browser.SwitchTo().DefaultContent();
                this.Browser.FindElement(By.CssSelector(".ui-icon.bw-icon-b1")).Click();
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

        private void EnsureDownloadsHaveFinished()
        {
            while (true)
            {
                var files = Constants.TemporaryFileExtensions
                    .SelectMany(extension => Directory.GetFiles(Constants.WallpapersDirectory, extension));
                if (!files.Any())
                {
                    break;
                }

                Thread.Sleep(Constants.DefaultDownloadTimeout);
            }
        }
    }
}
