using BoDi;
using DocumentFormat.OpenXml.Bibliography;
using HitachiQA.Driver;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TechTalk.SpecFlow;
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
            if(Element(areaSwitcherTitle).Text!=area)
            {
                Element(areaSwitcher).Click();
                GetFlyoutElement(area).Click();
            }
            this.GetLeftPaneSiteMapButton(title).Click();

        }
        public Element GetCommandBarButton(string displayText) => Element($"{COMMAND_BAR_XPATH} //button[*//text()='{displayText}']");

        public Element CommandBarShowMoreOptionsButton => Element($"{COMMAND_BAR_XPATH} //button[contains(@id, 'OverflowButton')]");

        public Element GetEntityTab(string tabDisplayName) => Element($"//ul[contains(@id, 'tablist')] //li[*//text()='{tabDisplayName}']");
        public void NavigateToEntityTab(string tabDisplayName, bool related){
            if(related)
            {
                this.GetEntityTab("Related").Click();
                GetFlyoutElement(tabDisplayName).Click();
            }
            else
            {
                this.GetEntityTab(tabDisplayName).Click();
            }
        }

        public Element GetFlyoutElement(string text) => Element($"//*[@id='__flyoutRootNode'] //*[./*[text()='{text}']]");
        
        public Element AppBreadCrumb => Element("//*[@data-id=\"appBreadCrumbText\"]/..");

        public Element Grid => Element("( //div[contains(@id, 'entity_control-pcf_grid_control_container')] //*[@data-id='grid-container']  | //*[@data-id='data-set-body-container' and //*[@class='wj-cells'] ] )");

        public Element GetGrid(string gridName_or_logicalName) => Element($"//*[ (@aria-label='{gridName_or_logicalName}' or @data-control-name='{gridName_or_logicalName}' or @data-id='{gridName_or_logicalName}') and (.//*[contains(@id, '-pcf_grid_control_container')]//*[@data-id='grid-container']  | .//*[@data-id='data-set-body-container' and //*[@class='wj-cells'] ] )] ");

        public Element GetGridCommandBarButton(string gridName_or_logicalName, string displayName) => this.GetField(this.GetGrid(gridName_or_logicalName).locator, displayName);
        public Element GetRelatedGridCommandBarButton(string displayName)=> this.GetField(By.XPath("//*[contains(@data-lp-id, 'commandbar-SubGridAssociated')]"), displayName);
        public Dyn_EffectiveGrid GetEffectiveGrid(string gridName_or_LogicalName)=> new Dyn_EffectiveGrid(ObjectContainer, $"WebResource_{gridName_or_LogicalName}");

        public Dyn_QuickCreateTab QuickCreateTab =>  new Dyn_QuickCreateTab(ObjectContainer);
        public void SaveForm()
        {
            this.GetCommandBarButton("Save").Click();
            this.Element("//span[text()='Saving...']").assertElementIsPresent();
        }
        public void GridSearch(string input)
        {
            this.Element("//input[contains(@aria-label, 'Filter by keyword')]").SetFieldValue(input);
            this.PressEnter();
        }

        public void CreateLookupFieldRecord(string DisplayText_Or_LogicalName, Table inputs)
        {
            var field = this.GetField(DisplayText_Or_LogicalName);
            this.Element(field.locator.Locator.Criteria+"//*[@class='fa fa-search']").Click();
            foreach(var row in inputs.Rows)
            {
                this.GetField(row["FieldName"]).SetFieldValue(row["Value"]);
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
