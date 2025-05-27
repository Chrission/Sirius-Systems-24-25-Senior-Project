Feature: Login Page Content Check

Scenario: Log in greeting and instructions are on the page
    When I navigate to "Identity/Account/Login"
    Then I should see text "Log in and Get Birding!"
    And I should see text "Enter your Stork Dork login information below."