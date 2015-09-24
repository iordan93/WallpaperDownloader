namespace WallpaperDownloader.App.DownloadStrategies
{
    using System;
    using System.Linq;
    using OpenQA.Selenium;

    public class CategoryDownloadStrategy : DownloadStrategy
    {
        public CategoryDownloadStrategy(IWebDriver browser)
            : base(browser)
        {
        }

        public string CategoryUrl { get; private set; }

        protected override void ExecuteCustomStrategy()
        {
            var categories = this.Browser.FindElements(By.CssSelector(".side-panel.categories > li"));
            Console.WriteLine("Please choose a category:");
            for (int i = 1; i <= categories.Count; i++)
            {
                Console.WriteLine("{0}. {1}", i, categories[i - 1].Text);
            }

            Console.Write("Selected category number: ");
            int selectedCategoryIndex = int.Parse(Console.ReadLine()) - 1;
            this.CategoryUrl = categories[selectedCategoryIndex].FindElement(By.TagName("a")).GetAttribute("href");
            string imagesInCategoryCount = categories[selectedCategoryIndex].FindElement(By.TagName("small")).Text;
            this.Browser.Navigate().GoToUrl(this.CategoryUrl);
            ChooseSubcategory(imagesInCategoryCount);
        }

        protected override string GetBaseDownloadUrl()
        {
            return this.CategoryUrl;
        }

        protected override string GetDownloadUrlForPage(int pageNumber)
        {
            return this.GetBaseDownloadUrl() + "/page/" + pageNumber;
        }

        private void ChooseSubcategory(string imagesInCategoryCount)
        {
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
                if (selectedSubcategoryIndex >= 0 && selectedSubcategoryIndex < subcategories.Count)
                {
                    this.CategoryUrl = subcategories[selectedSubcategoryIndex].FindElement(By.TagName("a")).GetAttribute("href");
                }
            }
        }

    }
}
