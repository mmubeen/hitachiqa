@NoBrowser
Feature: Logger
Tests the logger functionality



Scenario: stringify works as expected
	 When user stringifies '<Input>'
	 Then the expected '<Outcome>' should be returned 

Examples: 
| Input      | Outcome        |
| string[]   | Newtonsoft_WAY |
| NULL       | NULL           |
| Dictionary | Newtonsoft_WAY |
| JArray     | Newtonsoft_WAY |
| JObject    | Newtonsoft_WAY |
| string     | SameAsInput    |
| long       | SameAsInput    |
| decimal    | SameAsInput    |




