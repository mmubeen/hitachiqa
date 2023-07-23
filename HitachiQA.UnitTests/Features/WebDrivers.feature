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
	#firefox is broken atm
	#| Firefox |
	| Edge    |


Scenario: Navigating to a site
	Given Browser is up
	Then user should land on HSAL homepage selenium

@NoBrowser
Scenario: Browser options can be set
	Given user loads option "<Options>" into the browser
	When "Chrome" is invoked
	Then "<Options>" should be set to the browser
Examples: 
	| Options               |
	| --start-maximized     |
	| --window-size=840,640 |
	