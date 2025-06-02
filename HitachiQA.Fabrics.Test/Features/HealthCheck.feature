@NoBrowser
Feature: HealthCheck


Scenario: Fabric Notebooks are present
	When user gets Notebooks from fabrics
	Then Notebooks should come back
