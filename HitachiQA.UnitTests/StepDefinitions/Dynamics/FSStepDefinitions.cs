using HitachiQA.Dynamics.FS.Pages;
using HitachiQA.Helpers;

namespace HitachiQA.UnitTests.StepDefinitions.Dynamics
{
    [Binding]
    public class FSStepDefinitions
    {
        private Dyn_BasePage Page { get; }
        private SharedData SharedData { get; }
        private Dyn_AppsLandingPage AppsLandingPage { get; }


        public FSStepDefinitions(Dyn_BasePage Page, SharedData SD, Dyn_AppsLandingPage FS_ALP)
        {
            this.Page = Page;
            this.SharedData = SD;
            this.AppsLandingPage = FS_ALP;
        }


        [When(@"user navigates to '([^']*)' app")]
        public void WhenUserNavigatesToApp(string title)
        {
            AppsLandingPage.GetModuleCard(title).Click();
            SharedData.SetValue("Application", "CurrentApplication", title);

        }

        [Then(@"user should be in the previously navigated app")]
        public void ThenUserShouldBeInThePreviouslyNavigatedApp()
        {
            Page.AppBreadCrumb.GetAttribute("aria-label").Should().BeEquivalentTo(SharedData.GetValue("Application", "CurrentApplication"));
        }
        [When(@"user opens left pane '([^']*)' entity")]
        public void WhenUserOpensLeftPaneEntity(string entityName)
        {
            Page.GetLeftPaneSiteMapButton(entityName).Click();
        }

        [When(@"user opens grid record having '([^']*)' equals '([^']*)'")]
        public void WhenUserOpensGridRecordHavingEquals(string columnName, string value)
        {
            Page.Grid.OpenGridRecord(columnName, value);
        }

        [When(@"user navigates to '([^']*)' tab")]
        public void WhenUserNavigatesToTab(string tab)
        {
            Page.GetEntityTab(tab).Click();
        }

        [When(@"user fills out FS UI form")]
        public void WhenUserFillsOutUIForm(Table table)
        {
            foreach (var row in table.Rows)
            {
                var fieldName = row["fieldName"];
                var value = row["value"];

                this.Page.GetField(fieldName).SetFieldValue(value);
            }
        }

        [When(@"user should be able to get grid items")]
        public void WhenUserShouldBeAbleToGetGridItems()
        {
            Log.Info(this.Page.Grid.GetGridItems()[0]);
        }


    }
}
