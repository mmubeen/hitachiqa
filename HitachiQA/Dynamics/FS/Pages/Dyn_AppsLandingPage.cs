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
    public class Dyn_AppsLandingPage : Dyn_BasePage
    {
        public Dyn_AppsLandingPage(ObjectContainer ObjectContainer) : base(ObjectContainer)
        {
            base.IFrameTitle = "AppLandingPage";
        }

        public Element GetModuleCard(string title) => Element($"//*[@title='{title}']");


        

    }
}
