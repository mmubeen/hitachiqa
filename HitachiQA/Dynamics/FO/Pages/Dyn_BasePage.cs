using BoDi;
using DocumentFormat.OpenXml.Bibliography;
using HitachiQA.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HitachiQA.Dynamics.FO.Pages
{
    public class Dyn_BasePage : BasePage
    {
        private const string COMMAND_BAR_XPATH = "((//div[@id='mainContent'] //*[contains(@data-id, 'Command')])[1] | //*[@data-id='OverflowFlyout'])";

        public readonly GlobalCommandBar GlobalCommandBar;
        public Dyn_BasePage(ObjectContainer ObjectContainer) : base(ObjectContainer)
        {
            this.GlobalCommandBar = ObjectContainer.Resolve<GlobalCommandBar>();
            KnownFieldXPaths.Add("//*[(@data-dyn-role='Label' or @data-dyn-role='Tile') and (text()='{input}' or @title='{input}')]");
            KnownFieldXPaths.Add("//*[@aria-label='{input}']");
        }

        public Element GetElementByControlName(string controlName) => Element($"//*[@data-dyn-controlname='{controlName}']");

        public Element GetElementByTitle(string displayName) => Element($"//*[@title='{displayName}']");

        public Element GetEntityTab(string tabDisplayName) => Element($"//ul[contains(@id, 'tablist')] //li[*//text()='{tabDisplayName}']");

        public Element Grid => Element("( //div[contains(@id, '-pcf_grid_control_container')] //*[@data-id='grid-container']  | //*[@data-id='data-set-body-container' and //*[@class='wj-cells'] ]  |  //*[@data-dyn-controlname='Grid' and @data-dyn-role='ReactList']) ");

        public Element GetGrid(string sectionName) => Element($"//section[@aria-label='{sectionName}' and {Grid.locator.Locator.Criteria}] ");

        public void SetGridQuickFilterValue(string filterByColumn, string Criteria)
        {
            Element($"//input[@name='GridFilter_Input']").SetFieldValue(Criteria);
            Element($"//li[contains(@class, 'quickFilter')][descendant::*[text()='{filterByColumn}']][descendant::*[text()='{Criteria}']]").Click();
        }
    }


    public class GlobalCommandBar : BasePage
    {
        public GlobalCommandBar(ObjectContainer ObjectContainer) : base(ObjectContainer) { }

        public void SwitchCompanyContext(string companyName)
        {
            CompanyButton.Click();
            Thread.Sleep(500);
            CompanyLookup.SetFieldValue(companyName);
            Thread.Sleep(2000);
        }
        public Element CompanyLookup => Element("//*[@id='NavBar'] //*[@id='navigationMainActionGroup'] //*[@id='CompanyButton']/..");
        public Element CompanyButton => getButton("CompanyButton");
        public Element NotificationButton => getButton("dynNavigationBarMessages_buttonNotifications");
        public Element Settings => getButton("navBarSettings_button");
        public Element Help => getButton("navBarHelpButton_button");
        private Element getButton(string id) => Element($"//*[@id='NavBar'] //*[@id='navigationMainActionGroup'] //*[@id='{id}']");

    }
}
