@api
Feature: User Management API
  As a participant
  I want to manage my participation in gift exchanges
  So that I can join rooms and specify my preferences

Background:
  Given a room exists with invitation code

Rule: User Creation

  @positive
  Scenario: Join a room with wish list
    Given I have user data with wish list:
      | FirstName | John                 |
      | LastName  | Doe                  |
      | Phone     | +380123456789        |
      | Email     | john.doe@example.com |
    And I have wishes:
      | Name           | InfoLink                          |
      | PlayStation 5  | https://store.sony.com/ps5        |
      | LEGO Set      | https://lego.com/starwars         |
      | Book Collection|                                   |
    When I join the room
    Then I should be added successfully
    And I should receive my user code
    And my wishes should be saved

  @positive
  Scenario: Join a room with surprise preference
    Given I have user data with surprise preference:
      | FirstName   | Jane                           |
      | LastName    | Smith                          |
      | Phone       | +380987654321                  |
      | Email       | jane.smith@example.com         |
      | Interests   | I love books, tech, and sports |
    When I join the room
    Then I should be added successfully
    And my wish list should be empty
    But my interests should be saved

  @negative @validation
  Scenario Outline: Join room with invalid data
    Given I have user data:
      | Field       | Value        |
      | FirstName   | <FirstName>  |
      | LastName    | <LastName>   |
      | Phone       | <Phone>      |
      | Email       | <Email>      |
    When I try to join the room
    Then the request should fail with status 400
    And the error should mention "<Error>"

    Examples:
      | FirstName | LastName | Phone         | Email         | Error                          |
      |           | Doe      | +380123456789 | test@test.com | This field is required         |
      | John      |          | +380123456789 | test@test.com | This field is required         |
      | John      | Doe      |               | test@test.com | This field is required         |
      | John      | Doe      | invalid       | test@test.com | Phone number must be a valid   |
      | John      | Doe      | +380123456789 | invalid       | Email must be valid            |

Rule: User Retrieval

  @positive
  Scenario: Get all users in my room as regular user
    Given I am in a room with multiple users
    When I get the list of users using my code
    Then I should receive all users in the room
    And each user should have basic information including name and ID
    And each user should have gift preference indicator
    But users should not have sensitive data like phone or email or address

  @positive
  Scenario: Get user details as regular user
    Given I am a regular user
    And there are other users in the room
    When I try to get another user's details
    Then the request should return status 200
    But I should not see their sensitive information

  @positive
  Scenario: Get all users in my room as admin
    Given I am a room admin
    When I get the list of users using my code
    Then I should receive all users in the room
    And admin should see all user data including sensitive information

  @positive @authorization
  Scenario: Get specific user details as admin
    Given I am a room admin
    When I get user details by ID
    Then I should see complete user information
    And I should see their gift preferences

Rule: User Deletion

  @positive
  Scenario: Admin deletes a user from the room
    Given I have a room with multiple users
    And there is a user to delete in the room
    When I delete the user as admin
    Then the request should pass with status 200
    And the user should be deleted successfully
    And the user count should be 4

  @positive
  Scenario: Admin deletes multiple users from the room
    Given I have a room with 5 users
    And there is a user to delete in the room
    When I delete the user as admin
    And there is a user to delete in the room
    When I delete the user as admin
    Then the user count should be 3

  @negative @authorization
  Scenario: Regular user tries to delete another user
    Given I have a room with multiple users
    And I am a regular user
    And there is a user to delete in the room
    When I try to delete a user as a regular user
    Then the request should fail with status 400
    And the error should indicate that user is not an admin

  @negative
  Scenario: Admin tries to delete a non-existent user
    Given I have a room with multiple users
    When I try to delete a user that does not exist
    Then the request should fail with status 404

  @negative
  Scenario: Admin tries to delete a user from another room
    Given I have a room with multiple users
    When I try to delete a user from another room
    Then the request should fail with status 400
    And the error should indicate that user cannot be deleted

  @negative
  Scenario: Admin tries to delete themselves
    Given I have a room with multiple users
    When I try to delete the admin user
    Then the request should fail with status 400
    And the error should indicate that admin cannot be deleted

  @negative
  Scenario: Admin tries to delete a user from a closed room
    Given I have a room with multiple users
    And there is a user to delete in the room
    When I try to delete a user from a closed room
    Then the request should fail with status 400
    And the error should indicate that room is closed