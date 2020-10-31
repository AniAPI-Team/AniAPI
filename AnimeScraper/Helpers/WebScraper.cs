namespace AnimeScraper.Helpers
{
    public class WebScraper
    {
        #region Singleton

        private static WebScraper _instance;
        public static WebScraper Instance
        {
            get
            {
                return _instance ?? (_instance = new WebScraper());
            }
        }

        #endregion


    }
}
