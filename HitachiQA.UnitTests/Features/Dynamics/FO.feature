@Dynamics
Feature: FO

A short summary of the feature

Scenario: Navigating and performing various actions
	Given user is signed in to 'Finance and Operations'
	And user is in company 'USMF' context
	When user clicks on 'Bank management' tile
	And user clicks on 'Import bank statements'
	And user fills out FO UI form
	| fieldName                                                         | value |
	| Import statement for multiple bank accounts in all legal entities | No    |
	| Bank account                                                      | USMF  |
	| Reconcile after import                                            | Yes   |
