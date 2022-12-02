Feature: WebDrivers
Unit tests to make sure webdrivers are working



Scenario: Webdriver autoinvoke
	Then UserActions instance should be added to the Object Container

@NoBrowser
Scenario: Browser can be invoked
	When "<browser>" is invoked
	Then verify "<browser>" is open

Examples: 
	| browser |
	| Chrome  |
	| Firefox |
	| Edge    |


Scenario: Navigating to a site
	Given Browser is up
	Then user should land on HSAL homepage