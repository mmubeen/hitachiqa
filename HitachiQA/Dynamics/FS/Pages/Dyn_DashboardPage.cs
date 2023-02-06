using BoDi;
using HitachiQA.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace HitachiQA.Dynamics.FS.Pages
{
    public class Dyn_DashboardPage : Dyn_BasePage
    {
        public Dyn_DashboardPage(ObjectContainer ObjectContainer) : base(ObjectContainer)
        {
        }

        public Element DashboardSelector => Element($"//*[contains(@id,'ViewSelector') and contains(@id,'button')]");

        public Element DashboardSelection(string displayText) => Element($"//*[contains(@id,'ViewSelector')]//*[@aria-label='{displayText}']");

        public void SelectDashboard(string displayText)
        {
            this.DashboardSelector.Click();
            this.DashboardSelection(displayText).Click();
        }
    }
}
