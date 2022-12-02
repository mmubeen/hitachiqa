using BoDi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TechTalk.SpecFlow;

namespace HitachiQA.Source.Helpers
{
    [Binding]
    public class SharedData : Dictionary<string, Dictionary<string, object>>
    {
        [BeforeScenario]
        public static void InstantiateFormData(ObjectContainer objectContainer)
        {
            objectContainer.RegisterInstanceAs(new SharedData());
        }
        public SharedData()
        {

        }

        public void SetValue(Table specflowTable)
        {
            string parent = "ParentName";
            string child = "FieldName";
            string value = "FieldValue";

            foreach (var row in specflowTable.Rows)
            {
                this.SetValue(row[parent], row[child], row[value]);
            }
        }
        public void SetValue(string parent, Table specflowTable)
        {
            string child = "FieldName";
            string value = "FieldValue";

            foreach (var row in specflowTable.Rows)
            {
                this.SetValue(parent, row[child], row[value]);
            }
        }

        public void SetValue(string parentName, string fieldName, object value)
        {
            if (!this.ContainsKey(parentName))
            {
                this[parentName] = new Dictionary<string, object>();
            }
            if (!this[parentName].ContainsKey(fieldName))
            {
                this[parentName].Add(fieldName, "");
            }

            this[parentName][fieldName] = value;
        }


        public string GetValue(string parentName, string fieldName)
        {
            return (string)this[parentName][fieldName];
        }
        public T GetValue<T>(string parentName, string fieldName)
        {
            return (T)this[parentName][fieldName];
        }
    }
}
