using Microsoft.Playwright;
using Reqnroll.BoDi;

namespace HitachiQA.Dynamics.Playwright.CE
{
    public class AppLandingPage : BasePage
    {
        public AppLandingPage(IObjectContainer oc) : base(oc)
        {

        }
        public ILocator GetAppButton(string title) => FrameLocator("iframe#AppLandingPage").GetByTitle(title).First;
    }
}
