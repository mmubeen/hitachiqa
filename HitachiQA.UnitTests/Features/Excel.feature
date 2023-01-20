@NoBrowser
Feature: Excel

Testing all the Excel functionalities present in Functions.cs

@Excel
Scenario: Parsing Excel Data
	Given User parses the input Excel file from Data folder
	When User gets the data present in the Excel sheet
	Then User validates the data is parsed correctly from the Excel
