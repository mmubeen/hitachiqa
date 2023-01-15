@Dynamics
Feature: FS
Tests the dynamics functionality of the IConfiguration object

Scenario: User Login
	Given user landed in dynamics login page
	When user signs in
	Then user should be signed in to 'Dynamics 365'

Scenario: navigating to app
	Given user is signed in to 'Dynamics 365'
	When user navigates to 'Dynamics 365 — custom' app
	Then user should be in the previously navigated app

Scenario: Get Grid Items
	Given user is signed in to 'Dynamics 365'
	When user navigates to 'Dynamics 365 — custom' app
	And user opens left pane 'Accounts' entity
	And user should be able to get grid items

Scenario: Navigating and performing various actions
	Given user is signed in to 'Dynamics 365'
	When user navigates to 'Dynamics 365 — custom' app
	And user opens left pane 'Accounts' entity
	And user opens grid record having 'Account Name' equals 'Miguel'
	And user fills out FS UI form
	| fieldName       | value                         |
	| name            | Miguel                        |
	| telephone1      | 201 790 0720                  |
	| fax             | 2017900720                    |
	| websiteurl      | miguel.com                    |
	| parentaccountid | fourth coffee                 |
	| address1_line1  | 151 fair st paterson nj 07501 |
	And user navigates to 'Details' tab
	And user fills out FS UI form
	| fieldName    | value     |
	| industrycode | Financial |