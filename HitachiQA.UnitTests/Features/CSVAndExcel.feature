@NoBrowser
Feature: CSV and Excel
Unit Tests for CSV & Excel parsing

Scenario: Parsing CSV File
	Given User parses the input CSV file from Data folder
	When User gets the data present in the CSV
	Then User validates the data is parsed correctly from the CSV

Scenario: Parsing Excel Data
	Given User parses the input Excel file from Data folder
	When User gets the data present in the Excel sheet
	Then User validates the data is parsed correctly from the Excel
