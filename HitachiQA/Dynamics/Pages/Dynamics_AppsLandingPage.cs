using BoDi;
using HitachiQA.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace HitachiQA.Dynamics.Pages
{
    public class Dynamics_AppsLandingPage : BasePage
    {
        public Dynamics_AppsLandingPage(ObjectContainer ObjectContainer) : base(ObjectContainer)
        {
            base.IFrameTitle = "AppLandingPage";
        }

        public Element GetModuleCard(string title) => Element($"//*[@title='{title}']");

        

    }
}
