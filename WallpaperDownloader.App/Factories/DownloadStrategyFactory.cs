namespace WallpaperDownloader.App.Factories
{
    using System;
    using DownloadStrategies;
    using OpenQA.Selenium;

    public static class DownloadStrategyFactory
    {
        public static IDownloadStrategy GetStrategy(DownloadStrategyType type, IWebDriver browser)
        {
            switch (type)
            {
                case DownloadStrategyType.Category:
                    return new CategoryDownloadStrategy(browser);
                case DownloadStrategyType.Search:
                    return new SearchDownloadStrategy(browser);
                default:
                    throw new InvalidOperationException("The download strategy choice is invalid.");
            }
        }
    }
}
