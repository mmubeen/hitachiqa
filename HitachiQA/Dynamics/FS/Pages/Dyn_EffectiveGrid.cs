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
    public class Dyn_EffectiveGrid : Dyn_BasePage
    {
        public Dyn_EffectiveGrid(ObjectContainer ObjectContainer, string iFrameId) : base(ObjectContainer)
        {
            base.IFrameId = iFrameId;
        }

        public Element GetEditButton(int index) => Element($"(//a[@title='Edit'])[{index+1}]");
        public Element GetOpenButton(int index) => Element($"(//a[@title='Open'])[{index+1}]");
        public Element SaveRecordButton => Element($"(//td//a[@role='button'])[1]");

        public void SelectRecord(int index) => Element($"(//td//input[@type='checkbox'])[{index}]");

        public void SelectRecord(string columnDisplayName, string value)
        {
            var items = this.GetItems();
            var item = items.FirstOrDefault(it=> it[columnDisplayName]==value);
            if(item==null)
            {
                Log.Error("effective grid data:");
                Log.Error(items);
                throw new Exception($"Couldn't find {columnDisplayName}=={value}");
            }
            this.SelectRecord(int.Parse(item["index"]));
        }

        public List<Dictionary<string, string>> GetItems()=> Element("//body[.//th]").parseUITable().ToList();

       
        

    }
}
