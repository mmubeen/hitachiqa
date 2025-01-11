using HitachiQA.Driver;
using Reqnroll.BoDi;

namespace HitachiQA.Dynamics.FS.Pages
{
    public class Dyn_AppsLandingPage : Dyn_BasePage
    {
        public Dyn_AppsLandingPage(ObjectContainer ObjectContainer) : base(ObjectContainer)
        {
            base.IFrameTitle = "AppLandingPage";
        }

        public Element GetModuleCard(string title) => Element($"//*[@title='{title}']");




    }
}
