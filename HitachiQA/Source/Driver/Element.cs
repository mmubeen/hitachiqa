using System;
using System.Collections.Generic;
using HitachiQA.Helpers;
using System.Linq;
using FluentAssertions;
using HitachiQA.Driver;

namespace HitachiQA.Driver
{
    public class Element
    {
        public By locator;
        private readonly UserActions UserActions;
        public Element(string xpath, UserActions userActions)
        {
            this.UserActions = userActions;
            this.locator = By.XPath(xpath);
        }
        public Element(OpenQA.Selenium.By locator, UserActions userActions)
        {
            this.UserActions = userActions;
            this.locator = new By(locator);
        }

        public Element(By locator, UserActions userActions)
        {
            this.UserActions = userActions;
            this.locator = locator;
        }

        public override string ToString()
        {
            return this.locator.ToString();
        }
        
        public string Xpath
        {
            get
            {
                string loc = locator.ToString();
                if(loc.Contains("By.XPath:"))
                {
                    return loc.Substring(10);
                }
                else
                {
                    throw Functions.handleFailure(new NotImplementedException($"Locator string [{loc}] xpath conversion not built"));
                }     
            }
        }


        //
        //  General Element Actions
        //

        public void Click()
        {
            UserActions.Click(locator);
        }

        public void DoubleClick()
        {
            UserActions.DoubleClick(locator);
        }

        public bool Click(int? wait_Seconds = null, bool optional =false)
        {
            return UserActions.Click(locator, UserActions.ProcessWaitParam(wait_Seconds), optional);
        }

        public bool TryClick(int? wait_Seconds = null)
        {
            return this.Click(UserActions.ProcessWaitParam(wait_Seconds), true); 
        }

        public string GetAttribute(string attributeName)
        {
            return UserActions.GetAttribute(locator, attributeName);
        }

        public bool IsDisabled => UserActions.GetIsDisabled(locator);

        public string GetElementText()
        {
            return UserActions.getElementText(locator);
        }

        public string GetInnerText()
        {
            return string.Join("", this.GetInnerTexts());
        }

        public List<String> GetInnerTexts()
        {
            return UserActions.FindElementsWaitUntilVisible(locator).Select(it => it.Text.Trim()).ToList();
        }

        public void assertElementContainsText(string text)
        {
            string elementText = this.GetElementText();

            elementText.Should().Contain(text, $"Element {locator} \ntext: {elementText}  did not contain expected \ntext: {text}");
        }

        [Obsolete("please use  assertElementContainsText(string text)")]
        public bool assertElementContainsText(string text, bool optional = false)
        {
            string elementText = this.GetElementText();

            if (Assert.TextContains(elementText, text, true))
            {
                return true;
            }
            Functions.handleFailure(new Exception($"Element {locator.ToString()} \ntext: {elementText}  did not contain expected \ntext: {text}"), optional);
            return false;
        }

        public void assertElementTextEquals(string text)
        {
            string elementText = this.GetElementText();
            elementText.Should().Be(text, $"Element {locator} \ntext: {elementText}  did not contain expected \ntext: {text}");

        }

        [Obsolete("please use assertElementTextEquals(string text)")]
        public bool assertElementTextEquals(string text, bool optional = false)
        {
            string elementText = this.GetElementText();
            if (Assert.AreEqual(elementText, text, true))
            {
                return true;
            }
            Functions.handleFailure(new Exception($"Element {locator.ToString()} \ntext: {elementText} did not equal expected\ntext: {text}"), optional);
            return false;
        }

        public void assertElementInnerTextEquals(string text)
        {
            string innerText = this.GetInnerText();

            innerText.Should().Be(text, $"Element {locator.ToString()} \ninner text: {innerText} did not equal expected\n      text: {text}");

        }

        [Obsolete("please use assertElementInnerTextEquals(string text)")]
        public bool assertElementInnerTextEquals(string text, bool optional = false)
        {
            string innerText = this.GetInnerText();
            if (Assert.AreEqual(innerText, text, true))
            {
                return true;
            }
            Functions.handleFailure(new Exception($"Element {locator.ToString()} \ninner text: {innerText} did not equal expected\n      text: {text}"), optional);
            return false;
        }

        /// <summary>
        ///  Waits for the element to be vissible in the page
        /// </summary>
        /// <param name="optional">if set to true failure will be contained and no exception will be thrown </param>
        public bool assertElementIsVisible(int? wait_Seconds = null, bool optional = false)
        {
            try { UserActions.FindElementWaitUntilVisible(locator, UserActions.ProcessWaitParam(wait_Seconds));
                return true;
            }
            catch (Exception ex)
            {
                Functions.handleFailure($"Element located {locator.ToString()} was not vissible in the UI", ex, optional);
            }
            return false;
        }

        /// <summary>
        ///  Waits for the element to be present in the page (an elements could be present and not visible)
        /// </summary>
        /// <param name="optional">if set to true failure will be contained and no exception will be thrown </param>

        public bool assertElementIsPresent(int? wait_Seconds = null, bool optional = false)
        {
            try { UserActions.FindElementWaitUntilPresent(locator, UserActions.ProcessWaitParam(wait_Seconds));
                return true;
            }
            catch (Exception ex)
            {
                Functions.handleFailure($"Element located {locator.ToString()} was not present in the HTML", ex, optional);
            }
            return false;
        }

        /// <summary>
        ///  Waits for the element to disappear <br/>
        ///  note: if element was not present at the exact moment this function was called, true will be returned.
        /// </summary>
        /// <param name="optional">if set to true failure will be contained and no exception will be thrown </param>

        public bool assertElementNotPresent(int? wait_Seconds = null, bool optional = false)
        {
            try { UserActions.WaitForElementToDisappear(locator, UserActions.ProcessWaitParam(wait_Seconds));
                return true;
            }
            catch (Exception ex)
            {
                Functions.handleFailure($"Element located {locator.ToString()} was still vissible in the UI after {wait_Seconds} seconds", ex, optional);
            }
            return false;
        }

        public bool AssertRadioButtonState(bool state, bool optional=false)
        {
            bool isSelected = this.IsRadioButtonSelected();
            if (optional)
            {
                return (state == isSelected )? true : false;
            }
            else if (state != isSelected)
            {
                throw Functions.handleFailure($"Radio Button state did not match expected {state} \n {this}");                      
            }
            else
            {
                return true;
            }
        }

        public OpenQA.Selenium.IWebElement WaitUntilClickable(int? wait_Seconds = null, bool optional = false)
        {
            return UserActions.FindElementWaitUntilClickable(locator, UserActions.ProcessWaitParam(wait_Seconds));
        }

        [Obsolete("please use SetFieldValue(string value) instead")]
        public void setValue(string fieldType, string value)
        {
            switch (fieldType.ToLower())
            {
                case "input":
                    this.setText(value);
                    break;
                case "dropdown":
                    this.SelectMatDropdownOptionByText(value);
                    break;
                default:
                    Functions.handleFailure(new NotImplementedException($"Field type: {fieldType} is not implemented"));
                    break; 
            }
        }

        public void SetFieldValue(string value)
        {
           UserActions.SetFieldValue(locator, value);
        }

        //
        //  Text Fields Actions
        //
        public void setText(String TextToEnter, int? wait_Seconds = null)
        {
            UserActions.setText(locator, TextToEnter, UserActions.ProcessWaitParam(wait_Seconds));
        }

        public string getTextFieldText(int? wait_Seconds = null)
        {
           return  UserActions.getTextFieldText(locator, UserActions.ProcessWaitParam(wait_Seconds));
        }

        public void clearTextField()
        {
            UserActions.clearTextField(locator);
        }

        public void assertTextFieldTextEquals(string expected)
        {
            string elementText = this.getTextFieldText();

            elementText.Should().Be(expected, $"Text Field {locator.ToString()} \ntext: {elementText} did not equal expected\ntext: {expected}");

        }

        [Obsolete("Please use assertTextFieldTextEquals(string expected)")]
        public bool assertTextFieldTextEquals(string expected, bool optional=false)
        {
            string elementText = this.getTextFieldText();
            if (Assert.AreEqual(elementText, expected, true))
            {
                return true;
            }
            Functions.handleFailure(new Exception($"Text Field {locator.ToString()} \ntext: {elementText} did not equal expected\ntext: {expected}"), optional);
            return false;
        }

        // 
        // Dropdown actions 
        // 

        public void SelectMatDropdownOptionByText( string optionDisplayText)
        {
            UserActions.SelectMatDropdownOptionByText(locator, optionDisplayText);
        }

        public void SelectMatDropdownOptionContainingText(string optionDisplayText)
        {
            UserActions.SelectMatDropdownOptionContainingText(locator, optionDisplayText);
        }

        public void SelectMatDropdownOptionByIndex(int LogicalIndex, out string selectionDisplayName)
        {
            UserActions.SelectMatDropdownOptionByIndex(locator, LogicalIndex, out selectionDisplayName);
        }

        public void SelectMatDropdownOptionByIndex(int LogicalIndex)
        {
            UserActions.SelectMatDropdownOptionByIndex(locator, LogicalIndex);
        }

        public void AssertMatDropdownOptionsContain(string optionText)
        {
            List<string> dropdownOptions = UserActions.GetAllMatDropdownOptions(locator).ToList();

            dropdownOptions.Should().Contain(optionText);
        }

        [Obsolete("Please use AssertMatDropdownOptionsContain(string optionText)")]
        public bool AssertMatDropdownOptionsContain(string optionText, bool optional = false)
        {
            List<string> dropdownOptions = UserActions.GetAllMatDropdownOptions(locator).ToList();

            return Assert.Contains(dropdownOptions, optionText, optional);
        }

        public void AssertMatDropdownOptionsEqual(List<String> optionsText)
        {
            List<String> dropdownOptions = UserActions.GetAllMatDropdownOptions(locator).ToList();

            dropdownOptions.Should().BeEquivalentTo(optionsText);
        }

        [Obsolete("Please use AssertMatDropdownOptionsEqual(List<String> optionsText)")]
        public bool AssertMatDropdownOptionsEqual(List<String> optionsText, bool optional = false)
        {
            List<String> dropdownOptions = UserActions.GetAllMatDropdownOptions(locator).ToList();

            dropdownOptions.ForEach(it => Log.Debug("dropdownOptions: " + it));
            return Assert.AreEqual(dropdownOptions, optionsText, optional);
        }
        public IEnumerable<string> GetMatdropdownOptionsText()
        {
            return UserActions.GetAllMatDropdownOptions(locator);
        }

        //
        // RADIO BUTTON
        //
        public Boolean IsRadioButtonSelected()
        {
            return UserActions.IsRadioButtonSelected(locator);
        }

        //
        //Checkbox button
        //
        public void setMattCheckboxState(bool state)
        {
            UserActions.SetMattCheckboxState(locator, state);
        }

        //
        // TABLE HANDLING
        //

        public IEnumerable<Dictionary<String, String>> parseUITable()
        {
            return UserActions.parseUITable(this.Xpath);
        }

        public List<Dictionary<String, String?>> GetGridItems()
        {
            return UserActions.GetGridItems(locator);
        }
        /// <summary>
        /// To be executed on a Grid element
        /// Opens first record found with a matching column name or value.
        /// Fails if no record found
        /// </summary>
        public void OpenGridRecord(string columnName, string value)
        {
            UserActions.OpenGridRecord(locator, columnName, value); 
        }
    }
}