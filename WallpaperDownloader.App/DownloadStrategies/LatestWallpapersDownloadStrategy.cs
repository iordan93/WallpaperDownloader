namespace WallpaperDownloader.App.DownloadStrategies
{
    using OpenQA.Selenium;
    using Utilities;

    public class LatestWallpapersDownloadStrategy : DownloadStrategy
    {
        public LatestWallpapersDownloadStrategy(IWebDriver browser)
            : base(browser)
        {
        }

        protected override void ExecuteCustomStrategy()
        {
            return;
        }

        protected override string GetBaseDownloadUrl()
        {
            return Constants.BaseSiteUrl + "latest_wallpapers.html";
        }

        protected override string GetDownloadUrlForPage(int pageNumber)
        {
            return Constants.BaseSiteUrl + "latest_wallpapers.html/page/" + pageNumber;
        }
    }
}
