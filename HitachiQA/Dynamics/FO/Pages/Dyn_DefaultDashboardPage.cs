using BoDi;
using HitachiQA.Driver;

namespace HitachiQA.Dynamics.FO.Pages
{
    public class Dyn_DefaultDashboardPage : Dyn_BasePage
    {
        public Dyn_DefaultDashboardPage(ObjectContainer ObjectContainer) : base(ObjectContainer)
        {

        }

        public Element GetDashboardTile(string displayName) => Element($"//*[@title='{displayName}']");
    }
}
