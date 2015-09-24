namespace WallpaperDownloader.App.DownloadStrategies
{
    using System;
    using System.Net;
    using OpenQA.Selenium;
    using Utilities;

    public class SearchDownloadStrategy : DownloadStrategy
    {
        public SearchDownloadStrategy(IWebDriver browser)
            : base(browser)
        {
        }

        public string SearchTerm { get; private set; }

        protected override void ExecuteCustomStrategy()
        {
            Console.Write("Search for: ");
            this.SearchTerm = Console.ReadLine();
        }

        protected override string GetBaseDownloadUrl()
        {
            return Constants.BaseSiteUrl + "search.html" + this.GetSearchTermQueryParameter();
        }

        protected override string GetDownloadUrlForPage(int pageNumber)
        {
            return Constants.BaseSiteUrl + "search.html/page/" + pageNumber + this.GetSearchTermQueryParameter();
        }

        private string GetSearchTermQueryParameter()
        {
            return "?q=" + WebUtility.UrlEncode(this.SearchTerm);
        }
    }
}
