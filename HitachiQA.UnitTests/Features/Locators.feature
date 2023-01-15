Feature: Locators

A short summary of the feature

@NoBrowser
Scenario: Test Known Field Locators
	Given system loads known htmls for field type '<fieldTypeName>'
	When Known xpath is queried against the known html
	Then a field with the previously loaded type should return

	Examples: 
	| fieldTypeName          |
	| dropdown               |
	| lookup                 |
	| lookup_with_selection  |
	| textfield              |
	| textfield_autocomplete |
	| switch                 |
	| lookup_with_table      |
