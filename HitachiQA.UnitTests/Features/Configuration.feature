@NoBrowser
Feature: Configuration
Tests the buildout of the IConfiguration object


Scenario: NoBrowser Tag
	Then Webdriver related classes should not be initialized
	And NoBrowser tag on this feature should cascade down to this test
	

@NoBrowser
Scenario: _VARNAME flag funcitonality
	Given a test environment name is loaded
	When an environment variable is acquired with varname
	Then the system should look for the varname provided 

