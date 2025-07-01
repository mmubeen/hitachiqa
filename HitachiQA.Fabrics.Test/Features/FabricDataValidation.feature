@NoBrowser
Feature: FabricDataValidation
 
  Validate that we can connect to the Microsoft Fabric Lakehouse SQL endpoint and retrieve data.
 
  @fabric
  Scenario: Connect and query records from Lakehouse table
    Given We connect to the Fabric Lakehouse and query the records

 