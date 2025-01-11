using HtmlAgilityPack;
using Microsoft.Playwright;

namespace HitachiQA
{
    public static class FieldAutoDetector
    {

        public static Dictionary<string, string> KnownXPaths = new Dictionary<string, string> {
                { "//select[contains(@data-id, 'option-set-select')]", "dropdown" },
                { "//input[@type='text' and contains(@data-id, 'text-box-text')]", "textfield" },
                { "//input[@type='text' and contains(@data-id, 'text-input')]", "textfield" },
                { "//input[@type='text' and contains(@data-id, 'quickFind_text')]", "textfield" },
                { "//textarea[@type='text' and @aria-autocomplete='list'] ", "textfield_autocomplete" },
                { "//input[@type='text' and contains(@data-id, 'textInputBox_with_filter')]", "lookup" },
                { "//*[@role='link' and contains(@id, 'selected_tag')]", "lookup_with_selection" },
                { "//*[@role='switch']", "switch" },
                { "//input[@type='text' and following-sibling::*[contains(@data-dyn-bind, 'Lookup')]]", "lookup_with_table" },
                { "//following-sibling::*/select", "dropdown"},
                { "//input[@type='checkbox']", "checkbox" },
                { "//input[contains(@class, 'editable-lookup') and following-sibling::*[.//*[@class='fa fa-search']]]", "lookup_with_dialog"},
                { "//td[@data-hslcolumnname][.//input[@type='text'] and .//input[@type='submit']]", "effective_grid_lookup"},
                { "//textarea[not(@type) and not(@aria-autocomplete)]", "textfield"},
                { "//input[@data-role='numerictextbox']", "numerictextbox"},
                { "//input[@type='text' and contains(@data-bind,'currency')]", "textfield"},
                { "//div[@role='combobox']", "combobox"},
                { "//*[contains(@class,'enumValuesDropdown')]//*[@role='combobox']", "enum_combobox"},
                { "//self::*[.//*[contains(@id, 'DatePicker')]]", "datepicker"},
                { "//input[@role='combobox' and not(contains(@id, 'DatePicker'))]", "input_with_combobox"},
                { "//div[contains(@class,'ms-TextField is-disabled')]//button", "textfieldreadonly"},
                { "//input[@data-role='dropdownlist']/../..", "dropdown_listbox" },
                { "//select", "dropdown"},
                { "//input", "textfield"}
        };

        /// <summary>
        /// Selenium
        /// </summary>
        public static KeyValuePair<string, string> FindKnownXPathMatchingPair(HtmlDocument fieldDoc, out HtmlNode node, string criteria)
        {
            KeyValuePair<string, string>? matchingPair = null;
            node = null;
            foreach (var pair in KnownXPaths)
            {
                var xpath = pair.Key;
                var type = pair.Value;
                var tempNode = fieldDoc.DocumentNode.SelectSingleNode(xpath);
                if (tempNode != null)
                {
                    node = tempNode;
                    matchingPair = pair;
                    break;
                }

            }

            if (node == null)
            {
                Log.Error($"Couldn't find a a match on any of the following xpaths: {string.Join("\n", KnownXPaths.Select(it => $"{it.Value} => {it.Key}"))}");
                throw new Exception($"error finding known xpath match for field located by {criteria}");

            }
            return matchingPair ?? throw new ArgumentNullException();
        }

        /// <summary>
        /// Playwright
        /// </summary>
        public static KeyValuePair<string, string> FindKnownXPathMatchingPair(HtmlDocument fieldDoc, ILocator locator)
        {
            KeyValuePair<string, string>? matchingPair = null;
            HtmlNode node = null;
            foreach (var pair in KnownXPaths)
            {
                var xpath = pair.Key;
                var type = pair.Value;
                var tempNode = fieldDoc.DocumentNode.SelectSingleNode(xpath);
                if (tempNode != null)
                {
                    node = tempNode;
                    matchingPair = pair;
                    break;
                }

            }

            if (node == null)
            {
                Console.WriteLine($"Couldn't find a a match on any of the following xpaths: {string.Join("\n", KnownXPaths.Select(it => $"{it.Value} => {it.Key}"))}");
                throw new Exception($"error finding known xpath match for field located by {locator}");

            }
            return matchingPair ?? throw new ArgumentNullException();
        }




        public static bool parseStrIntoBool(string input)
        {
            input = input.ToLower();
            switch (input)
            {
                case "1":
                case "yes":
                case "true":
                case "check":
                    return true;
                case "0":
                case "no":
                case "false":
                case "uncheck":
                    return false;
                default:
                    throw new NotImplementedException("Invalid input value. Expected values for true: 1, yes, true, check. Expected values for false: 0, no, false, uncheck.");
            }
        }
    }
}
