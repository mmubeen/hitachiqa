@NoBrowser
Feature: CSV scenarios

Unit Tests for CSV Scenarios

@csv
Scenario: Parsing CSV File
	Given User parses the input CSV file from Data folder
	When User gets the data present in the CSV
	Then User validates the data is parsed correctly from the CSV
