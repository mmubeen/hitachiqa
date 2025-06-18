@NoBrowser
Feature: HealthCheck

Background:
    Given the configured Fabric workspace is available
    And GitHub repository contains entity definitions

  Scenario Outline: Fabric entities match their GitHub definitions
    Given the expected '<EntityType>' definitions are loaded from GitHub
    When the actual '<EntityType>' entities are retrieved from the Fabric workspace
    Then the actual entities should match the expected definitions
    And all required properties should be present
    And no unexpected entities should exist in the workspace

    Examples:
      | EntityType   |
      | DataPipeline |
      | Environment  |
      | Notebook     |
      | Lakehouse    |
      | Warehouse    |
