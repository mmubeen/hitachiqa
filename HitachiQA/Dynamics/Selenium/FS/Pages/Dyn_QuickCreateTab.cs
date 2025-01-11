using HitachiQA.Driver;
using Reqnroll.BoDi;

namespace HitachiQA.Dynamics.FS.Pages
{
    public class Dyn_QuickCreateTab : Dyn_BasePage
    {
        private string parentXPath;
        public Dyn_QuickCreateTab(ObjectContainer ObjectContainer) : base(ObjectContainer)
        {
            parentXPath = "//section[@data-id='quickCreateRoot']";
        }

        public new Element GetField(string fieldName_Or_LogicalName) => base.GetField(By.XPath(parentXPath), fieldName_Or_LogicalName);

        public Element Dialog => Element(parentXPath);

        public Element SaveAndCloseButton => this.GetField("quickCreateSaveAndCloseBtn");

        public string EntityLogicalName => this.Dialog.GetAttribute("data-lp-id").Replace("quick-create|quickCreateRoot|", string.Empty);


    }
}
