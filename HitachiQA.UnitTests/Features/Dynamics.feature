@Dynamics @ignore
Feature: Dynamics
Tests the dynamics functionality of the IConfiguration object

Scenario: User Login
	Given user landed in dynamics login page
	When user signs in
	Then user should be signed in

Scenario: navigating to app
	Given user is signed in
	When user navigates to 'Dynamics 365 — custom' app
	Then user should be in the previously navigated app

Scenario: Get Grid Items
	Given user is signed in
	When user navigates to 'Dynamics 365 — custom' app
	And user navigates to 'Accounts' page
