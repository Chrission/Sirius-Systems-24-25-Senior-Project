Feature: Register Page Content Check

Scenario: Register greeting and instructions are on the page
    When I navigate to "Identity/Account/Register"
    Then I should see text "Join the Stork Dork Community!"
    And I should see text "Get straight to Birding!"