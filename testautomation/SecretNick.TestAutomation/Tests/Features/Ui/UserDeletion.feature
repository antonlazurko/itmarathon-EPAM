@ui
Feature: User Deletion
  As a room admin
  I want to delete participants from my room
  So that I can manage the gift exchange participants

Background:
  Given the API is available

Rule: Admin can delete participants

  @positive
  Scenario: Admin deletes a participant successfully
    Given a room exists with 4 participants via API
    And I am on the home page
    When I navigate to room page with admin code
    Then I should see participants count 4
    And I should see delete button for the first non-admin participant
    When I click delete button for the first non-admin participant
    Then I should see delete confirmation modal
    When I confirm delete in the modal
    Then I should see success message "Participant was successfully removed"
    And the first non-admin participant should not be visible
    And I should see participants count 3

  @positive
  Scenario: Admin cancels deletion
    Given a room exists with 4 participants via API
    And I am on the home page
    When I navigate to room page with admin code
    Then I should see participants count 4
    And I should see delete button for the first non-admin participant
    When I click delete button for the first non-admin participant
    Then I should see delete confirmation modal
    When I cancel delete in the modal
    Then I should not see delete confirmation modal
    And the first non-admin participant should be visible
    And I should see participants count 4

  @negative
  Scenario: Regular user cannot see delete button
    Given a room exists with 4 participants via API
    And I am a regular user in the room
    And I am on the home page
    When I navigate to room page with regular user code
    Then I should see participants count 4
    And I should not see delete button for any participant

Rule: Delete button visibility

  @positive
  Scenario: Admin cannot delete themselves
    Given a room exists with 3 participants via API
    And I am on the home page
    When I navigate to room page with admin code
    Then I should see delete button for the first non-admin participant
    But I should not see delete button for the admin participant

