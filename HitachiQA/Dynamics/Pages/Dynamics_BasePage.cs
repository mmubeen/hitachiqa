using BoDi;
using DocumentFormat.OpenXml.Bibliography;
using HitachiQA.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HitachiQA.Dynamics.Pages
{
    public class Dynamics_BasePage : BasePage
    {
        private const string COMMAND_BAR_XPATH = "((//div[@id='mainContent'] //*[contains(@data-id, 'Command')])[1] | //*[@data-id='OverflowFlyout'])";

        public readonly GlobalCommandBar GlobalCommandBar;
        private readonly Dynamics_AppsLandingPage AppsLandingPage;
        public Dynamics_BasePage(ObjectContainer ObjectContainer) : base(ObjectContainer)
        {
            this.GlobalCommandBar = ObjectContainer.Resolve<GlobalCommandBar>();
            this.AppsLandingPage = ObjectContainer.Resolve<Dynamics_AppsLandingPage>();
        }

        public Element GetModuleCard(string title) => AppsLandingPage.GetModuleCard(title);

        public Element GetLeftPaneSiteMapButton(string title) => Element($"//*[@data-id='navbar-container'] //li[@aria-label='{title}']");

        public Element GetLeftPaneSiteMapButton(string areaGroup, string title) => Element($"//*[@data-id='navbar-container'] //ul[@aria-label='{areaGroup}'] //li[@aria-label='{title}']");

        public Element GetCommandBarButton(string displayText) => Element($"{COMMAND_BAR_XPATH} //button[*//text()='{displayText}']");

        public Element CommandBarShowMoreOptionsButton => Element($"{COMMAND_BAR_XPATH} //button[contains(@id, 'OverflowButton')]");

        public Element GetEntityTab(string tabDisplayName) => Element($"//ul[contains(@id, 'tablist')] //li[*//text()='{tabDisplayName}']");

        public Element AppBreadCrumb => Element("//*[@data-id=\"appBreadCrumbText\"]/..");

        public Element Grid => Element("( //div[contains(@id, '-pcf_grid_control_container')] //*[@data-id='grid-container']  | //*[@data-id='data-set-body-container' and //*[@class='wj-cells'] ] )");

        public Element GetGrid(string sectionName) => Element($"//section[@aria-label='{sectionName}' and {Grid.locator.Locator.Criteria}] ");
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
