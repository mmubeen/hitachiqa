
@playwright
Feature: Playwright
Tests the logger functionality


Scenario: Navigating to a site
	Given Playwright is up
	Then user should land on HSAL homepage playwright

@NoBrowser @ignore
Scenario: Playwright Interactive Login
	Given Configuration object is created for 'Playwright'
	When User gets a bearer token
	Then user should be able to successfully call the API