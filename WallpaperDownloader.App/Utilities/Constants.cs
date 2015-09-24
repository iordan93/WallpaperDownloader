namespace WallpaperDownloader.App.Utilities
{
    public static class Constants
    {
        public const int DefaultDownloadTimeout = 1000;
        public const int DefaultImplicitBrowserTimeout = 5;

        public const string WallpapersDirectory = "../../Wallpapers";
        public const string DriversDirectory = "../../Drivers";
        public const string ImageMimeTypes = "image/jpg,image/jpeg,image/png";
        public const string BaseSiteUrl = "http://wallpaperswide.com/";

        public static readonly string[] TemporaryFileExtensions = new[] { "*.part", "*.tmp", "*.crdownload" };
    }
}
