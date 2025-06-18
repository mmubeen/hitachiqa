using HitachiQA.Helpers;

namespace HitachiQA.Driver
{
    public class Element
    {
        public By[] locators;

        private readonly UserActions UserActions;
        public Element(string xpath, UserActions userActions)
        {
            UserActions = userActions;
            locators = new By[] { By.XPath(xpath) };
        }
        public Element(OpenQA.Selenium.By locator, UserActions userActions)
        {
            UserActions = userActions;
            locators = new By[] { new By(locator) };
        }

        public Element(By locator, UserActions userActions)
        {
            UserActions = userActions;
            locators = new By[] { locator };
        }
        public Element(By[] locator, UserActions userActions)
        {
            UserActions = userActions;
            locators = locator;
        }

        public override string ToString()
        {
            return string.Join(",", locators.Select(l => l.ToString()));
        }


        //
        //  General Element Actions
        //

        public bool ElementExists() => UserActions.ElementExists(locators);
        public void Click()
        {
            UserActions.Click(locators);
        }

        public void DoubleClick()
        {
            UserActions.DoubleClick(locators);
        }

        public bool Click(int? wait_Seconds = null, bool optional = false)
        {
            return UserActions.Click(locators, UserActions.ProcessWaitParam(wait_Seconds), optional);
        }

        public bool TryClick(double waitSeconds = 0)
        {
            return UserActions.TryClick(locators, waitSeconds);
        }

        public string GetDomProperty(string attributeName)
        {
            return UserActions.GetDomProperty(locators, attributeName);
        }

        public bool IsDisabled => UserActions.GetIsDisabled(locators);

        public string Text => GetElementText();

        public string GetElementText()
        {
            return UserActions.getElementText(locators);
        }

        public string GetInnerText()
        {
            return string.Join("", this.GetInnerTexts());
        }

        public List<string> GetInnerTexts()
        {
            return UserActions.FindElementsWaitUntilVisible(locators).Select(it => it.Text.Trim()).ToList();
        }

        public void assertElementContainsText(string text)
        {
            string elementText = this.GetElementText();

            elementText.Should().Contain(text, $"Element {locators} \ntext: {elementText}  did not contain expected \ntext: {text}");
        }

        public void assertElementTextEquals(string text)
        {
            string elementText = this.GetElementText();
            elementText.Should().Be(text, $"Element {locators} \ntext: {elementText}  did not contain expected \ntext: {text}");

        }

        public void assertElementInnerTextEquals(string text)
        {
            string innerText = this.GetInnerText();

            innerText.Should().Be(text, $"Element {locators.ToString()} \ninner text: {innerText} did not equal expected\n      text: {text}");

        }


        /// <summary>
        ///  Waits for the element to be vissible in the page
        /// </summary>
        /// <param name="optional">if set to true failure will be contained and no exception will be thrown </param>
        public bool assertElementIsVisible(int? wait_Seconds = null, bool optional = false)
        {
            try
            {
                UserActions.FindElementWaitUntilVisible(locators, UserActions.ProcessWaitParam(wait_Seconds));
                return true;
            }
            catch (Exception ex)
            {
                Functions.HandleFailure($"Element located {locators.ToString()} was not vissible in the UI", ex, optional);
            }
            return false;
        }

        /// <summary>
        ///  Waits for the element to be present in the page (an elements could be present and not visible)
        /// </summary>
        /// <param name="optional">if set to true failure will be contained and no exception will be thrown </param>

        public bool assertElementIsPresent(int? wait_Seconds = null, bool optional = false)
        {
            try
            {
                UserActions.FindElementWaitUntilPresent(locators, UserActions.ProcessWaitParam(wait_Seconds));
                return true;
            }
            catch (Exception ex)
            {
                Functions.HandleFailure($"Element located {locators.ToString()} was not present in the HTML", ex, optional);
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
            try
            {
                UserActions.WaitForElementToDisappear(locators, UserActions.ProcessWaitParam(wait_Seconds));
                return true;
            }
            catch (Exception ex)
            {
                Functions.HandleFailure($"Element located {locators.ToString()} was still vissible in the UI after {wait_Seconds} seconds", ex, optional);
            }
            return false;
        }

        public bool AssertRadioButtonState(bool state, bool optional = false)
        {
            bool isSelected = this.IsRadioButtonSelected();
            if (optional)
            {
                return (state == isSelected) ? true : false;
            }
            else if (state != isSelected)
            {
                throw Functions.HandleFailure($"Radio Button state did not match expected {state} \n {this}");
            }
            else
            {
                return true;
            }
        }

        public OpenQA.Selenium.IWebElement WaitUntilClickable(int? wait_Seconds = null, bool optional = false)
        {
            return UserActions.FindElementWaitUntilClickable(locators, UserActions.ProcessWaitParam(wait_Seconds));
        }

        [Obsolete("please use SetFieldValue(string value) instead")]
        public void setValue(string fieldType, string value)
        {
            throw new NotImplementedException();
            //switch (fieldType.ToLower())
            //{
            //    case "input":
            //        this.setText(value);
            //        break;
            //    case "dropdown":
            //        this.SelectMatDropdownOptionByText(value);
            //        break;
            //    default:
            //        Functions.handleFailure(new NotImplementedException($"Field type: {fieldType} is not implemented"));
            //        break;
            //}
        }

        public void SetFieldValue(string value)
        {
            UserActions.SetFieldValue(locators, value);
        }
        public string GetFieldValue()
        {
            return UserActions.GetFieldValue(locators);
        }
        public List<string> GetFieldOptions()
        {
            return UserActions.GetFieldOptions(locators);
        }
        public void OpenFieldValue()
        {
            UserActions.OpenFieldValue(locators);
        }
        public void AssertFieldIsReadOnlyDynamics()
        {
            var readonlyField = locators.Select(l => By.XPath(l.Locator.Criteria + "//*[contains(@data-id,'locked-icon')]", l.IFrameLocator)).ToArray();

            new Element(readonlyField, UserActions).assertElementIsPresent();
        }
        public void AssertFieldIsNotReadOnlyDynamics()
        {
            //
            //because we need to add a condition, the xpath on this field might not end with ]. 
            //so we add //*[(self::<xpath>)] around xpath 
            //allowing it to end with a condition so we can attach the 2nd condition
            //
            var non_readonlyFieldXPath = locators.Select(l => By.XPath($"//*[(self::{l.Locator.Criteria[2..]})][not(.//*[contains(@data-id,'locked-icon')])]", l.IFrameLocator)).ToArray();

            new Element(non_readonlyFieldXPath, UserActions).assertElementIsPresent();
        }

        //
        //  Text Fields Actions
        //
        [Obsolete("please use SetFieldValue(string value) instead")]
        public void setText(string TextToEnter, int? wait_Seconds = null)
        {
            UserActions.setText(locators, TextToEnter, UserActions.ProcessWaitParam(wait_Seconds));
        }

        [Obsolete("please use GetFieldValue() instead")]
        public string getTextFieldText(int? wait_Seconds = null)
        {
            return UserActions.getTextFieldText(locators, UserActions.ProcessWaitParam(wait_Seconds));
        }

        [Obsolete("please use SetFieldValue(string.empty) instead")]
        public void clearTextField()
        {
            UserActions.clearTextField(locators);
        }

        [Obsolete("please use GetFieldValue().should().be(expected) instead")]
        public void assertTextFieldTextEquals(string expected)
        {
            string elementText = this.getTextFieldText();

            elementText.Should().Be(expected, $"Text Field {locators.ToString()} \ntext: {elementText} did not equal expected\ntext: {expected}");

        }


        //
        // RADIO BUTTON
        //
        public Boolean IsRadioButtonSelected()
        {
            return UserActions.IsRadioButtonSelected(locators);
        }

        //
        //Checkbox button
        //
        public void setMattCheckboxState(bool state)
        {
            UserActions.SetMattCheckboxState(locators, state);
        }

        //
        // TABLE HANDLING
        //

        public IEnumerable<Dictionary<string, string>> parseUITable()
        {
            if (locators.Count() > 1)
            {
                throw new NotImplementedException("parseUITable for more than 1 locator not implemented");
            }
            return UserActions.parseUITable(locators.First());
        }

        public List<Dictionary<string, string>> GetGridItems()
        {
            return UserActions.GetGridItems(locators);
        }
        /// <summary>
        /// To be executed on a Grid element of dynamics
        /// Opens first record found with a matching column name or value.
        /// Fails if no record found
        /// </summary>
        public void OpenGridRecord(string columnName, string value)
        {
            UserActions.OpenGridRecord(locators, columnName, value);
        }
        /// <summary>
        /// To be executed on a Grid element of dynamics
        /// Opens record at a given index
        /// Fails if no record found
        /// </summary>
        public void OpenGridRecord(int LogicalIndex)
        {
            UserActions.OpenGridRecord(locators, "index", (LogicalIndex + 1).ToString());
        }
        /// <summary>
        /// To be executed on a Grid element of dynamics
        /// selects the first record found with a matching column name or value.
        /// Fails if no record found
        /// </summary>
        public void SelectGridRecord(string columnName, string value)
        {
            UserActions.SelectGridRecord(locators, columnName, value);
        }

        /// <summary>
        /// To be executed on a Grid element of dynamics
        /// selects record at a given index
        /// Fails if no record found
        /// </summary>
        public void SelectGridRecord(int LogicalIndex)
        {
            UserActions.SelectGridRecord(locators, "index", (LogicalIndex + 1).ToString());
        }

        /// <summary>
        /// To be executed on a Grid element of dynamics
        /// selects the select all button on the header row
        /// </summary>
        public void SelectAllGridRecords()
        {
            this.UserActions.SelectAllGridRecords(locators);
        }

        /// <summary>
        /// To be executed on a Grid element of dynamics
        /// selects the select all button on the header row
        /// </summary>
        public void SortGridColumn(string columnName, string filterByString = "", bool descendingSort = false, string comparisonOperation = "")
        {
            this.UserActions.SortGridColumn(locators, columnName, filterByString, descendingSort, comparisonOperation);
        }

        /// <summary>
        /// simulates the user's action of drag and drop to upload files
        /// </summary>
        public void UploadFile(string filePath)
        {
            UserActions.UploadFile(locators, filePath);
        }

    }
}