using Reqnroll.BoDi;

namespace HitachiQA.Helpers
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

        public void SetValue(Table table)
        {
            string parent = "ParentName";
            string child = "FieldName";
            string value = "FieldValue";

            foreach (var row in table.Rows)
            {
                this.SetValue(row[parent], row[child], row[value]);
            }
        }
        public void SetValue(string parent, Table table)
        {
            string child = "FieldName";
            string value = "FieldValue";

            foreach (var row in table.Rows)
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
