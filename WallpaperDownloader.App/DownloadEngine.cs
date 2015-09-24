namespace WallpaperDownloader.App
{
    using System;
    using System.Net;
    using DownloadStrategies;
    using Factories;
    using OpenQA.Selenium;
    using Utilities;

    public class DownloadEngine
    {
        public IWebDriver Browser { get; private set; }

        public IDownloadStrategy Strategy { get; private set; }

        public void Run()
        {
            Console.WriteLine(
                "Select a browser to use:{0}1. Mozilla Firefox{0}2. Google Chrome",
                Environment.NewLine);
            Console.Write("Selected browser: ");
            int browserType = int.Parse(Console.ReadLine());
            this.Browser = BrowserFactory.GetBrowser((BrowserType)browserType);
            Console.Clear();

            this.Browser.Navigate().GoToUrl(Constants.BaseSiteUrl);

            Console.WriteLine(
                "Select a download strategy:{0}1. Category{0}2. Search{0}3. Latest wallpapers{0}4. Top wallpapers",
                Environment.NewLine);
            Console.Write("Selected strategy: ");
            int downloadStrategyType = int.Parse(Console.ReadLine());
            this.Strategy = DownloadStrategyFactory.GetStrategy((DownloadStrategyType)downloadStrategyType, this.Browser);
            this.Strategy.Execute();
        }
    }
}
