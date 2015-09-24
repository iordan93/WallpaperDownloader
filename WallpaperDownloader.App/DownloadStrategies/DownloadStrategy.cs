namespace WallpaperDownloader.App.DownloadStrategies
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using OpenQA.Selenium;
    using OpenQA.Selenium.Interactions;
    using Utilities;

    public abstract class DownloadStrategy : IDownloadStrategy
    {
        private int startPage;
        private int endPage;

        public DownloadStrategy(IWebDriver browser)
        {
            this.Browser = browser;
        }

        public IWebDriver Browser { get; private set; }

        public int StartPage
        {
            get
            {
                return this.startPage;
            }

            private set
            {
                if (value < 1 || value > this.MaxPage)
                {
                    throw new ArgumentOutOfRangeException("startPage");
                }

                this.startPage = value;
            }
        }

        public int EndPage
        {
            get
            {
                return this.endPage;
            }

            private set
            {
                if (value < this.StartPage || value > this.MaxPage)
                {
                    throw new ArgumentOutOfRangeException("endPage");
                }

                this.endPage = value;
            }
        }

        public int MaxPage { get; private set; }

        public void Execute()
        {
            this.ExecuteCustomStrategy();
            this.SelectPages();
            Console.Clear();
            this.DownloadImages();
            this.EnsureDownloadsHaveFinished();
            this.Browser.Quit();
        }

        protected abstract void ExecuteCustomStrategy();

        protected abstract string GetBaseDownloadUrl();

        protected abstract string GetDownloadUrlForPage(int pageNumber);

        private void SelectPages()
        {
            this.Browser.Navigate().GoToUrl(this.GetBaseDownloadUrl());
            var lastPageElement = this.Browser.FindElement(By.CssSelector(".pagination:last-of-type > a:nth-last-child(2)"));
            string lastPageUrl = lastPageElement.GetAttribute("href");
            this.MaxPage = int.Parse(lastPageElement.Text);

            Console.Write("Please type the start page (1-{0}): ", this.MaxPage);
            this.StartPage = int.Parse(Console.ReadLine());
            Console.Write("Please type the end page ({0}-{1}): ", this.StartPage, this.MaxPage);
            this.EndPage = int.Parse(Console.ReadLine());
        }

        private void DownloadImages()
        {
            for (int page = this.StartPage; page <= this.EndPage; page++)
            {
                string downloadUrl = this.GetDownloadUrlForPage(page);
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
                    this.TryDownloadImage();
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
