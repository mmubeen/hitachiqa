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
    public class Dyn_QuickCreateTab : Dyn_BasePage
    {
        private string parentXPath;
        public Dyn_QuickCreateTab(ObjectContainer ObjectContainer) : base(ObjectContainer)
        {
            parentXPath = "//section[@data-id='quickCreateRoot']";
        }

       public new Element GetField(string fieldName_Or_LogicalName)=> base.GetField(By.XPath(parentXPath), fieldName_Or_LogicalName);

       public Element Dialog => Element(parentXPath);

       public Element SaveAndCloseButton => this.GetField("quickCreateSaveAndCloseBtn");

        

    }
}
