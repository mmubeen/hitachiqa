using BoDi;
using DocumentFormat.OpenXml.Drawing.Charts;
using HitachiQA.Hooks.Browsers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TechTalk.SpecFlow;

namespace HitachiQA.Source.Hooks.Browsers
{
    [Binding]
    public class SharedBrowserHook
    {
        public BrowserIndicator BrowserIndicator { get; init; }
        public IObjectContainer ObjectContainer { get; init; }
        public SharedBrowserHook(BrowserIndicator bi, IObjectContainer oc)
        {
            BrowserIndicator = bi;
            ObjectContainer = oc;
        }

        [AfterScenario(Order = 1)]
        public void handleScreenshot(ScenarioContext SC)
        {
            if (BrowserIndicator.IsBrowserFeature && SC.TestError != null)
            {
                try
                {
                    this.ObjectContainer.Resolve<ScreenShot>().Error();
                }
                catch (Exception ex)
                {
                    Log.Warn($"error taking screenshot\n {ex.Message} \n{ex.StackTrace}");
                }
            }
        }

    }
}
