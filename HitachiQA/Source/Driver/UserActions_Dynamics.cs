using OpenQA.Selenium.Support.UI;
using OpenQA.Selenium;
using System;
using HtmlAgilityPack;
using System.Collections.Generic;
using System.Linq;
using HitachiQA.Helpers;

namespace HitachiQA.Driver
{
    public partial class UserActions
    {

        public bool WaitForTransaction(int? wait_Seconds = null)
        {
            if (_applicationType?.ToLower() != "dynamics")
                return true;

            bool result = false;
            WebDriverWait webDriverWait = new WebDriverWait(this.WebDriver, TimeSpan.FromSeconds(ProcessWaitParam(wait_Seconds)));
            webDriverWait.IgnoreExceptionTypes(typeof(TimeoutException), typeof(NullReferenceException));
            try
            {
                if (JSExecutor.execute("return window.UCWorkBlockTracker") != null)
                {
                    result = webDriverWait.Until((IWebDriver d) => (bool)JSExecutor.execute("return window.UCWorkBlockTracker.isAppIdle()"));
                }
            }
            catch (Exception) { }
            return result;


        }

        private const string HORIZONTAL_SCROLL_BAR = "//div[@class='ag-body-horizontal-scroll' or contains(@class, 'ScrollbarLayout_main Scrollbar')]";

        private object GRID_SCROLL_JS_EXEC(By[] gridLocator, string command)
        {
            var scrollBarLoc = gridLocator.Select(xp => By.XPath($"{xp.Locator.Criteria} {HORIZONTAL_SCROLL_BAR}")).ToArray();
            if (!ElementExists(scrollBarLoc, out var element))
            {
                return null;
            }
            return JSExecutor.execute(command, element ?? throw new NullReferenceException("element"));
        }

        private object GRID_SCROLL_LEFT_COMMAND(By[] gridLocator) => GRID_SCROLL_JS_EXEC(gridLocator, "arguments[0].scrollBy(-400, 0);");
        private object GRID_SCROLL_ALL_RIGHT_COMMAND(By[] gridLocator) => GRID_SCROLL_JS_EXEC(gridLocator, "arguments[0].scrollBy(12000, 0);");
        private object GRID_QUERY_HOW_MUCH_UNTIL_RESET(By[] gridLocator) => GRID_SCROLL_JS_EXEC(gridLocator, "return arguments[0].scrollLeft;");

        public Dictionary<int, string> GetGridHeaders(By[] by)
        {

            this.WaitForTransaction();

            string gridCellXPath = $"//*[@role='columnheader' and @aria-colindex] | //*[@data-dyn-columnname]/*[@title or @data-dyn-qtip-title]";

            Dictionary<int, string> result = new Dictionary<int, string>();


            GRID_SCROLL_ALL_RIGHT_COMMAND(by);
            do
            {
                this.FindElementWaitUntilPresent(by.Select(l => By.XPath(l.Locator.Criteria + gridCellXPath)).ToArray());
                var gridElement = this.FindElementWaitUntilPresent(by);

                var gridDoc = new HtmlDocument();
                gridDoc.LoadHtml(gridElement.GetAttribute("innerHTML"));

                var headers = gridDoc.DocumentNode.SelectNodes(gridCellXPath);
                var iteration = 0;
                foreach (var header in headers)
                {

                    var index = int.Parse(header.GetAttributeValue("aria-colindex", "-1"));
                    string displayText = header.GetAttributeValue("title", null);
                    if (index == -1)
                    {
                        displayText = header.InnerText;
                    }
                    //PCF grids have title in a child node
                    if (displayText == null)
                    {
                        var rowDoc = new HtmlDocument();
                        rowDoc.LoadHtml(header.InnerHtml);

                        displayText = rowDoc.DocumentNode.SelectSingleNode("//*[@title or @data-dyn-qtip-title]")?.InnerText;
                        displayText = System.Web.HttpUtility.HtmlDecode(displayText ?? "");
                    }

                    iteration++;

                    if (index != -1 && !result.ContainsKey(index))
                    {
                        result.Add(index, displayText);
                    }
                    else if (index == -1)
                    {
                        result.Add(iteration, displayText);
                    }
                }

                GRID_SCROLL_LEFT_COMMAND(by);
            }
            while (GRID_QUERY_HOW_MUCH_UNTIL_RESET(by) is var queryResult && queryResult != null && Convert.ToDouble(queryResult) != 0);

            return result;
        }

        public List<Dictionary<string, string>> GetGridItems(By[] by)
        {
            this.WaitForTransaction();


            var headers = this.GetGridHeaders(by);
            var results = new List<Dictionary<string, string>>();

            var rowXPath = $"//*[@role='row' and (@aria-rowindex or descendant::*[@aria-colindex]) ]";

            GRID_SCROLL_ALL_RIGHT_COMMAND(by);
            do
            {
                var gridElement = this.FindElementWaitUntilPresent(by);
                var gridDoc = new HtmlDocument();
                gridDoc.LoadHtml(gridElement.GetAttribute("innerHTML"));

                var rows = gridDoc.DocumentNode.SelectNodes(rowXPath);
                //remove header (1st row)
                var headerNode = rows.FirstOrDefault(it => it.GetAttributeValue<int>("aria-rowindex", -1) == 1);
                headerNode.NullGuard();
                rows.Remove(headerNode);

                foreach (var rowNode in rows)
                {
                    var rowHTML = rowNode.InnerHtml;
                    var rowDoc = new HtmlDocument();
                    rowDoc.LoadHtml(rowHTML);
                    var row = rowDoc.DocumentNode;
                    var index = rowNode.GetAttributeValue<string>("aria-rowindex", null);
                    index.NullGuard();
                    //because we ignore the header
                    index = (int.Parse(index) - 1).ToString();

                    var rowDict = results.GetDictionaryByIndex(index);
                    if (rowDict == null)
                    {
                        rowDict = new Dictionary<string, string>();
                        rowDict.Add("index", index);
                        rowDict.Add("id", rowNode.GetAttributeValue<string>("row-id", ""));
                        results.Add(rowDict);
                    }

                    foreach (var header in headers)
                    {
                        var headerIndex = header.Key;
                        var headerName = header.Value ?? header.Key.ToString();
                        string cellValue = null;

                        var xpaths_and_atts = new Dictionary<string, string>()
                        {
                            {$"//*[@aria-colindex='{headerIndex}' and @col-id]  //*[@aria-label]", "aria-label" },
                            {$"//*[@aria-colindex='{headerIndex}' and @title]", "title" },
                            {$"//*[@aria-label='{headerName}' and @title]", "title" }

                        };

                        foreach (var item in xpaths_and_atts)
                        {
                            var xpath = item.Key;
                            var attr = item.Value;
                            var cellNodes = row.SelectNodes(xpath);
                            HtmlNode cellNode = null;
                            if (cellNodes?.Count > 1)
                            {
                                cellNode = cellNodes.FirstOrDefault(it => it.GetAttributeValue<string>("type", null) == "button");

                                if (cellNode == null)
                                    throw new NotImplementedException($"Found 2 cells with @aria-label attribute using the following xpath: {Log.stringify(by.Select(l => l.Locator.Criteria + rowXPath + xpath))}");
                            }
                            else if (cellNodes?.Count == 1)
                            {
                                cellNode = cellNodes[0];
                            }


                            if (cellNode != null)
                            {
                                cellValue = cellNode.GetAttributeValue(attr, "");
                                break;
                            }

                        }


                        if (rowDict.GetValueOrDefault(headerName) == null || !string.IsNullOrWhiteSpace(cellValue))
                        {
                            cellValue = System.Web.HttpUtility.HtmlDecode(cellValue ?? "");
                            rowDict[headerName] = cellValue;
                        }

                    }



                }

                GRID_SCROLL_LEFT_COMMAND(by);
            }
            while (Convert.ToDouble(GRID_QUERY_HOW_MUCH_UNTIL_RESET(by)) != 0);


            return results;
        }

        public void OpenGridRecord(By[] by, string columnName, string value)
        {
            this.WaitForTransaction();
            var gridItems = this.GetGridItems(by);

            var matchingRow = gridItems.FirstOrDefault(row => row.TryGetValue(columnName, out var colVal) && colVal == value);

            if (matchingRow == null)
            {
                throw new NotFoundException($"Couldn't find row matching {columnName}={value}");
            }

            var index = matchingRow["index"];

            var checkBoxLocDoubleClick = by.Select(l => By.XPath(l.Locator.Criteria + $"//*[@role='row' and @aria-rowindex={int.Parse(index) + 1} and descendant::*[@aria-colindex=1] ]//i[@data-icon-name]")).ToArray();
            if (ElementExists(checkBoxLocDoubleClick))
            {
                this.DoubleClick(checkBoxLocDoubleClick);
            }
            else
            {
                var checkBoxLoc = by.Select(l => By.XPath(l.Locator.Criteria + $"//*[@role='row' and @aria-rowindex={int.Parse(index) + 1}]//*[@title='Select or unselect row']")).ToArray();
                this.Click(checkBoxLoc);
                var firstCol = by.Select(l => By.XPath(l.Locator.Criteria + $"//*[@role='row' and @aria-rowindex={int.Parse(index) + 1}]//input[@aria-label]")).ToArray();
                this.Click(firstCol);

            }

            //var EditButtonLoc = By.XPath("((//div[@id='mainContent'] //*[contains(@data-id, 'Command')])[1] | //*[@data-id='OverflowFlyout']) //button[*//text()='Edit']");

            //this.FindElementWaitUntilClickable(by).Click();
            this.WaitForTransaction();

        }

        public void SelectGridRecord(By[] by, string columnName, string value)
        {
            var gridItems = this.GetGridItems(by);

            var matchingRow = gridItems.FirstOrDefault(row => row.TryGetValue(columnName, out var colVal) && colVal == value);

            if (matchingRow == null)
            {
                throw new NotFoundException($"Couldn't find row matching {columnName}={value}");
            }

            var index = matchingRow["index"];

            var checkBoxLoc = by.Select(l => By.XPath(l.Locator.Criteria + $"//*[@role='row' and @aria-rowindex={int.Parse(index) + 1} and descendant::*[@aria-colindex=1] ]//i[contains(@data-icon-name, 'Check')]/..")).ToArray();
            this.Click(checkBoxLoc);
        }

        public void SelectAllGridRecords(By[] by)
        {
            var checkBoxLoc = by.Select(l => By.XPath(l.Locator.Criteria + $"//*[@role='row' and @aria-rowindex=1 and descendant::*[@aria-colindex=1] ]//i[contains(@data-icon-name, 'Check')]/..")).ToArray();
            this.Click(checkBoxLoc);
        }

        public void SortGridColumn(By[] by, string columnName, string filterByString = "", bool descendingSort = false, string comparisonOperation = "")
        {
            this.WaitForTransaction();

            string gridCellXPath = $"//*[@role='columnheader']";
            var columnIndex = -1;
            GRID_SCROLL_ALL_RIGHT_COMMAND(by);
            do
            {
                var gridElement = this.FindElementWaitUntilPresent(by);
                var gridDoc = new HtmlDocument();
                gridDoc.LoadHtml(gridElement.GetAttribute("innerHTML"));

                var headers = gridDoc.DocumentNode.SelectNodes(gridCellXPath);

                foreach (var header in headers)
                {
                    var index = int.Parse(header.GetAttributeValue("aria-colindex", null));
                    var displayText = header.GetAttributeValue("title", null);

                    //PCF grids have title in a child node
                    if (displayText == null)
                    {
                        var rowDoc = new HtmlDocument();
                        rowDoc.LoadHtml(header.InnerHtml);

                        displayText = rowDoc.DocumentNode.SelectSingleNode("//*[@title]")?.InnerText;
                        displayText = System.Web.HttpUtility.HtmlDecode(displayText ?? "");
                    }

                    if (columnName == displayText)
                    {
                        columnIndex = index;
                    }

                }

                GRID_SCROLL_LEFT_COMMAND(by);
            }
            while (Convert.ToDouble(GRID_QUERY_HOW_MUCH_UNTIL_RESET(by)) != 0 && columnIndex == -1);

            var columnLocator = by.Select(l => By.XPath(l.Locator.Criteria + $"//*[@aria-colindex='{columnIndex}' and contains(@class,'header')]//*[@role='button']", by.First().IFrameLocator)).ToArray();
            var ascendingButton = By.XPath("//ul[contains(@class,'ContextualMenu-list is-open')]//li//button[@name='Sort A to Z']", by.First().IFrameLocator);
            var descendingButton = By.XPath("//ul[contains(@class,'ContextualMenu-list is-open')]//li//button[@name='Sort Z to A']", by.First().IFrameLocator);
            var filterByButton = By.XPath("//ul[contains(@class,'ContextualMenu-list is-open')]//li//button[@name='Filter by']", by.First().IFrameLocator);
            var comparisorOperatorDropdown = By.XPath("//*[contains(@class,'calloutMain')] //div[contains(@id,'Dropdown')]", by.First().IFrameLocator);
            var comparisonOperatorButton = By.XPath($"//*[contains(@class,'calloutMain')]//button[.//*[text()='{(comparisonOperation == "" ? "Equals" : comparisonOperation)}']]");
            var applyButon = By.XPath("//button[.//*[text()='Apply']]", by.First().IFrameLocator);

            Click(columnLocator);
            Click(filterByButton);
            Click(comparisorOperatorDropdown);
            Click(comparisonOperatorButton);

            this.SetFieldValue(By.XPath("//*[contains(@class,'calloutMain')]//*[contains(@class,'operatorsDropdownContainer')]/following-sibling::*[1]", by.First().IFrameLocator), filterByString);

            Click(applyButon);


            Click(columnLocator);
            if (descendingSort)
                Click(descendingButton);
            else
                Click(ascendingButton);

        }

        private void SetDatePickerValue(By[] autoGeneratedLocator, DateTime targetDate)
        {
            this.Click(autoGeneratedLocator.Select(l => By.XPath($"({l.Locator.Criteria}//input)[1]")).ToArray());
            var datePickerXPath = "//*[contains(@id, 'DatePicker-Callout')]";
            //
            //possible display fomats on currentItemButton 
            //February 2023  
            //2023
            //2020-2031
            //
            var currentItemButtonXPath = $"({datePickerXPath} //button[contains(@class,'currentItemButton') or contains(@aria-label, 'Year picker')])[last()]";
            var monthYearPicker = By.XPath($"{datePickerXPath} //*[contains(@class, 'monthPickerWrapper')]");
            var initialMonthYear = this.FindElementWaitUntilPresent(By.XPath($"{datePickerXPath}//*[contains(@class,'monthAndYear')]/span")).Text;

            if (!initialMonthYear.EndsWith(targetDate.Year.ToString()))
            {
                if (!this.ElementExists(monthYearPicker))
                {
                    this.Click(By.XPath(currentItemButtonXPath));
                }
                this.Click(By.XPath(currentItemButtonXPath));
                var yearRange = this.FindElementWaitUntilPresent(By.XPath(currentItemButtonXPath + "/span")).Text;
                string[] years = yearRange.Split('-');
                int startYear = int.Parse(years[0].Trim());
                int endYear = int.Parse(years[1].Trim());
                if (targetDate.Year >= startYear && targetDate.Year <= endYear)
                {
                    this.Click(By.XPath($"{datePickerXPath}//button[text()='{targetDate.Year}']"));
                }
                else
                {
                    throw new NotImplementedException("date picker for selecting year out of range needs to be implemented");
                }
                this.Click(By.XPath($"{datePickerXPath}//button[@aria-label='{targetDate.ToString("MMMM")}']"));
            }
            else if (!initialMonthYear.StartsWith(targetDate.ToString("MMMM")))
            {
                if (!this.ElementExists(monthYearPicker))
                {
                    this.Click(By.XPath(currentItemButtonXPath));
                }
                this.Click(By.XPath($"{datePickerXPath}//button[@aria-label='{targetDate.ToString("MMMM")}']"));
            }

            this.Click(By.XPath($"{datePickerXPath}//td[.//*[@aria-label='{targetDate.Day}, {targetDate.ToString("MMMM")}, {targetDate.Year}']]"));

        }
    }
}
