@checklist
Feature: Checklist Management

Scenario: Verify checklist display
    Given I log in as "mcaldwell@a.com" with password "Mcaldwell_01"
    When I navigate to "Checklists"
    Then I should see either "No checklists found. Create one?" or existing checklist data


@checklist
Scenario: Create a new checklist with selected birds
    Given I log in as "mcaldwell@a.com" with password "Mcaldwell_01"
    When I navigate to "Checklists"
    And I click "Create New Checklist"
    And I enter "Spring Migration 2024" as the checklist name
    And I search for and select the following birds:
        | Bird Name          |
        | American Robin     |
        | Northern Cardinal  |
    And I submit the checklist form
    Then I should be redirected to the Checklist index page


Scenario: Verify checklist details
    Given I log in as "mcaldwell@a.com" with password "Mcaldwell_01"
    When I navigate to "Checklists"
    And I click on the details button
    Then the page should not crash
