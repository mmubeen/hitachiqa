using System;
using System.Collections.Generic;
using System.Linq;

namespace HitachiQA.Driver
{
    public partial class UserActions
    {         
        #region Mat Dropdown actions 
        public void SelectMatDropdownOptionByText(By[] DropdownLocator, string optionDisplayText)
        {
            Click(DropdownLocator);
            Click(By.XPath($"//mat-option[descendant::*[normalize-space(text())= '{optionDisplayText}']]"));
        }
        public void SelectMatDropdownOptionContainingText(By[] DropdownLocator, string optionDisplayText)
        {
            Click(DropdownLocator);
            Click(By.XPath($"//mat-option[descendant::*[contains(normalize-space(text()), '{optionDisplayText}')]]"));
        }

        public void SelectMatDropdownOptionByIndex(By[] DropdownLocator, int LogicalIndex)
        {
            Click(DropdownLocator);
            Click(By.XPath($"//mat-option[{LogicalIndex + 1}]"));
        }

        public void SelectMatDropdownOptionByIndex(By[] DropdownLocator, int LogicalIndex, out string selectionDisplayName)
        {
            Click(DropdownLocator);
            try
            {
                WaitForElementToDisappear(By.XPath("//mat-option[descendant::*[normalize-space(text())= 'Searching...']]"));
            }
            catch (Exception)
            {
            }
            var options = FindElementsWaitUntilVisible(By.XPath($"//mat-option"));
            selectionDisplayName = string.Join("", this.WebDriver.FindElements(By.XPath($"(//mat-option)[{LogicalIndex + 1}]/descendant::*").Locator).Select(it => it.Text.Trim()).Distinct());
            Click(By.XPath($"//mat-option[{LogicalIndex + 1}]"));
        }

        public IEnumerable<String> GetAllMatDropdownOptions(By[] DropdownLocator)
        {
            var dropdown = FindElementWaitUntilClickable(DropdownLocator);
            dropdown.Click();
            var options = FindElementsWaitUntilVisible(By.XPath($"//mat-option"));

            int currentOption = 1;
            foreach (var option in options)
            {
                List<string> innerText = FindElementsWaitUntilVisible(By.XPath($"(//mat-option)[{currentOption}]/descendant::*")).Select(it => it.Text.Trim()).Distinct().ToList();
                currentOption++;
                yield return string.Join("", innerText);
            }
        }

        #endregion Mat Dropdown actions

        #region Checkbox

        public void SetMattCheckboxState(By[] MattCheckBoxLocator, bool state)
        {
            var mattCheckBox = FindElementWaitUntilVisible(MattCheckBoxLocator);

            while (GetCheckboxState(By.Id(mattCheckBox.GetAttribute("id") + "-input")) != state)
            {
                mattCheckBox.Click();
            }
        }

        public bool GetMattCheckboxState(By[] MattCheckBoxLocator)
        {
            var mattCheckBox = FindElementWaitUntilClickable(MattCheckBoxLocator);
            return GetCheckboxState(By.Id(mattCheckBox.GetAttribute("id") + "-input"));
        }

        #endregion Checkbox

    }
}
