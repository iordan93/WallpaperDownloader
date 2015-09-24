namespace WallpaperDownloader.App.DownloadStrategies
{
    using OpenQA.Selenium;
    using Utilities;

    public class TopWallpapersDownloadStrategy : DownloadStrategy
    {
        public TopWallpapersDownloadStrategy(IWebDriver browser)
            : base(browser)
        {
        }

        protected override void ExecuteCustomStrategy()
        {
            return;
        }

        protected override string GetBaseDownloadUrl()
        {
            return Constants.BaseSiteUrl + "top_wallpapers.html";
        }

        protected override string GetDownloadUrlForPage(int pageNumber)
        {
            return Constants.BaseSiteUrl + "top_wallpapers.html/page/" + pageNumber;
        }
    }
}
