using HitachiQA.Driver;
using Polly;
using Reqnroll.BoDi;
namespace HitachiQA.Dynamics.FS.Pages
{
    public class Dyn_BasePage : BasePage
    {
        private const string COMMAND_BAR_XPATH = "((//div[@id='mainContent'] //*[contains(@data-id, 'Command')])[1] | //*[@data-id='OverflowFlyout'])";

        public readonly GlobalCommandBar GlobalCommandBar;
        public Dyn_BasePage(ObjectContainer ObjectContainer) : base(ObjectContainer)
        {
            this.GlobalCommandBar = ObjectContainer.Resolve<GlobalCommandBar>();
        }

        public Element GetLeftPaneSiteMapButton(string title) => Element($"//*[@data-id='navbar-container'] //li[@aria-label='{title}' and contains(@id, 'sitemap-entity')]");

        public void NavigateToLeftPaneEntity(string area, string title)
        {
            var areaSwitcher = "//*[@id='areaSwitcherId']";
            var areaSwitcherTitle = $"{areaSwitcher}/span[text()]";
            if (Element(areaSwitcherTitle).Text != area)
            {
                Element(areaSwitcher).Click();
                GetFlyoutElement(area).Click();
            }
            this.GetLeftPaneSiteMapButton(title).Click();

        }
        public Element GetCommandBarButton(string displayText) => Element($"{COMMAND_BAR_XPATH} //button[*//text()='{displayText}']");

        public Element CommandBarShowMoreOptionsButton => Element($"{COMMAND_BAR_XPATH} //button[contains(@id, 'OverflowButton')]/..");

        public Element ToastNotificationCloseButton => Element($"//*[@data-pa-dialog-popup]//*[@alt='close']");
        public void ClickCommandBarButton(string displayText)
        {
            if (ToastNotificationCloseButton.ElementExists())
            {
                ToastNotificationCloseButton.Click();
            }
            var showMore = this.Element("(//div[@id='mainContent'] //*[contains(@data-id, 'Command')])[1]//button[@data-id='OverflowButton']/..");
            var retry = Policy
            .HandleResult<bool>(false)
            .WaitAndRetry(new[]
                {
                TimeSpan.FromSeconds(0),
                TimeSpan.FromSeconds(3),
                }
            );

            this.Element(COMMAND_BAR_XPATH).assertElementIsPresent();
            var targetCommand = GetCommandBarButton(displayText);

            retry.Execute(() =>
            {
                if (targetCommand.ElementExists())
                    return true;

                showMore.Click();
                if (targetCommand.ElementExists())
                    return true;

                showMore.Click();
                return false;
            });

            targetCommand.Click();

        }

        public Element GetEntityTab(string tabDisplayName) => Element($"//ul[contains(@id, 'tablist')] //li[*//text()='{tabDisplayName}']");
        public void NavigateToEntityTab(string tabDisplayName)
        {
            var retry = Policy
            .HandleResult<bool>(false)
            .WaitAndRetry(new[]
                {
                TimeSpan.FromSeconds(0),
                TimeSpan.FromSeconds(3),
                }
            );
            this.Element("//ul[@role='tablist']//li[text()]").assertElementIsPresent();
            var targetTab = this.Element($"(//ul[@role='tablist'] | //*[@id='__flyoutRootNode']  ) //*[self::div[@role='menuitem' and .//*[text()='{tabDisplayName}']] or self::li[text()='{tabDisplayName}']]");

            retry.Execute(() =>
            {
                if (targetTab.ElementExists())
                    return true;

                this.Element("//ul[@role='tablist']//*[@data-id='more_button']").Click();
                if (targetTab.ElementExists())
                    return true;

                this.Element("//ul[@role='tablist']//*[@data-id='more_button']").Click();
                return false;
            });

            targetTab.Click();
        }

        public Element GetFlyoutElement(string text) => GetField(By.XPath($"//*[@id='__flyoutRootNode']"), text);
        public Element AppBreadCrumb => Element("//*[@data-id=\"appBreadCrumbText\"]/..");

        public Element Grid => Element("( //div[contains(@id, 'entity_control-pcf_grid_control_container')] //*[@data-id='grid-container']  | //*[@data-id='data-set-body-container' and //*[@class='wj-cells'] ] )");

        public Element GetGrid(string gridName_or_logicalName) => Element($"//*[ (@aria-label='{gridName_or_logicalName}' or @data-control-name='{gridName_or_logicalName}' or @data-id='{gridName_or_logicalName}') and (.//*[contains(@id, '-pcf_grid_control_container')]//*[@data-id='grid-container']  | .//*[@data-id='data-set-body-container' and //*[@class='wj-cells'] ] )] ");

        public Element GetGridCommandBarButton(string gridName_or_logicalName, string displayName) => this.GetField(this.GetGrid(gridName_or_logicalName).locators.First(), displayName);

        public void ClickGridCommandBarButton(string gridName_or_logicalName, string displayName)
        {
            this.GetGrid(gridName_or_logicalName).assertElementIsPresent();

            if (this.GetGridCommandBarButton(gridName_or_logicalName, displayName).TryClick())
            {
                return;
            }
            else
            {
                this.GetGridCommandBarButton(gridName_or_logicalName, "OverflowButton").Click();
                this.GetFlyoutElement(displayName).Click();
                return;
            }

        }
        public bool checkGridCommandBarButtonExists(string gridName_or_logicalName, string displayName)
        {
            GetGrid(gridName_or_logicalName).assertElementIsPresent();
            if (GetGridCommandBarButton(gridName_or_logicalName, displayName).ElementExists())
                return true;

            GetGridCommandBarButton(gridName_or_logicalName, "OverflowButton").Click();
            var result = GetFlyoutElement(displayName).ElementExists();
            GetGridCommandBarButton(gridName_or_logicalName, "OverflowButton").Click();
            return result;
        }
        public Element GetRelatedGridCommandBarButton(string displayName) => this.GetField(By.XPath("//*[contains(@data-lp-id, 'commandbar-SubGridAssociated')]"), displayName);
        public Dyn_EffectiveGrid GetEffectiveGrid(string gridName_or_LogicalName) => new Dyn_EffectiveGrid(ObjectContainer, $"WebResource_{gridName_or_LogicalName}");

        public Dyn_QuickCreateTab QuickCreateTab => new Dyn_QuickCreateTab(ObjectContainer);
        public void SaveForm()
        {
            this.ClickCommandBarButton("Save");
            this.Element("//span[text()='Saving...']").assertElementIsPresent();
        }
        public void GridSearch(string input)
        {
            this.Element("//input[contains(@aria-label, 'Filter by keyword') or contains(@aria-label, 'Quick find')]").SetFieldValue(input);
            this.PressEnter();
        }

        public void CreateLookupFieldRecord(string DisplayText_Or_LogicalName, Table inputs)
        {
            var field = this.GetField(DisplayText_Or_LogicalName);
            this.Element(field.locators.Select(l => l.Locator.Criteria + "//*[@class='fa fa-search' or self::button[contains(@aria-label, 'Lookup')]]").ToArray()).Click();
            Thread.Sleep(500);
            this.Element("//button[contains(@id,'addNewBtn')]").TryClick();
            foreach (var row in inputs.Rows)
            {
                this.QuickCreateTab.GetField(row["FieldName"]).SetFieldValue(row["Value"]);
            }
        }

        public Element GridViewSelector => Element($"//*[contains(@id,'ViewSelector') and contains(@id,'button')] | //button[contains(@id,'ViewSelector')]");

        public Element GridViewSelection(string displayText) => Element($"//*[contains(@id,'ViewSelector')]//*[@aria-label='{displayText}'] | //button[.//*[text()='{displayText}']]");

        public void SelectGridView(string displayText)
        {
            this.GridViewSelector.Click();
            this.GridViewSelection(displayText).Click();
        }
    }
    public class GlobalCommandBar : BasePage
    {
        public GlobalCommandBar(ObjectContainer ObjectContainer) : base(ObjectContainer) { }


        public Element Search => getButton("searchLauncher");
        public Element Assistant => getButton("cardFeedContainerLauncher");
        public Element New => getButton("quickCreateLauncher_buttoncrm_header_global");
        public Element Settings => getButton("personalSettingsLauncher_buttoncrm_header_global");
        public Element Help => getButton("helpLauncher");
        private Element getButton(string id) => Element($"//*[@id='topBar'] //*[@data-id='CommandBar'] //*[@id='{id}']");

    }

}
