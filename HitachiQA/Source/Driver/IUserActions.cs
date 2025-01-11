namespace HitachiQA.Driver
{
    public interface IUserActions
    {
        public int DEFAULT_WAIT_SECONDS { get; }
        public string LOADING_SCREEN_XPATH { get; }

        public int ProcessWaitParam(int? wait) => (int)(wait == null ? DEFAULT_WAIT_SECONDS : wait);
        public void waitForPageLoad(By? iframe = null);
        public bool WaitForTransaction(int? wait_Seconds = null);
        public void Navigate(string URL_OR_PATH);
        public string GetCurrentURL();
        public void Refresh();
        public void Back();
        public bool assertElementIsVisible(By locator, int? wait_Seconds = null, bool optional = false);
        public bool assertElementIsPresent(By locator, int? wait_Seconds = null, bool optional = false);
        public bool assertElementNotPresent(By locator, int? wait_Seconds = null, bool optional = false);
        public string getElementText(By ElementLocator, int? wait_Seconds = null);
        public List<string> GetInnerTexts(By locator);
        public bool Click(By ElementLocator, int? wait_Seconds = null, bool optional = false);
        public bool DoubleClick(By ElementLocator, int? wait_Seconds = null, bool optional = false);
        public bool GetIsDisabled(By elementLocator);
        public bool GetIsDisplayed(By elementLocator);
        public string GetAttribute(By ElementLocator, string attributeName);
        public void Hover(By by, int? wait_Seconds = null, bool optional = false);
        public void ScrollToBottom();
        public void ScrollToTop();
        public IEnumerable<Dictionary<string, string>> parseUITable(By datatable);
        public Dictionary<int, string?> GetUITableHeaders(By table);
        public Dictionary<int, string?> GetDynamicsGridHeader(By by);
        public List<Dictionary<string, string?>> GetDynamicsGridItems(By by);
        public void OpenDynamicsGridRecord(By by, string columnName, string value);
        public void SelectDynamicsGridRecord(By by, string columnName, string value);
        public void SelectAllDynamicsGridRecords(By by);
        public void SortDynamicsGridColumn(By by, string columnName, string filterByString = "", bool descendingSort = false, string comparisonOperation = "");
        public void SetFieldValue(By by, string value);
        public string GetFieldValue(By by);
        public List<string> GetFieldOptions(By by);
        public void OpenFieldValue(By by);
        public bool ElementExists(By locator);
        public bool ElementExists(By locator, out OpenQA.Selenium.IWebElement? element);
        public bool TryClick(By locator, double waitSeconds = 0);
        public void SendKeys(string key);
        public void UploadFile(By dropZone, string filePath);
        public OpenQA.Selenium.IAlert GetBrowserAlert(int? wait_Seconds = null);
        public void OpenNewWindow();
        public void OpenNewTab();
        public string Title { get; }



    }
}
