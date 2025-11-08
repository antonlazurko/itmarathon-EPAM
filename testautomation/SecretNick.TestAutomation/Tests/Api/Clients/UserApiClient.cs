using Microsoft.Playwright;
using Serilog;
using Tests.Api.Models.Responses;
using Tests.Common.Models;

namespace Tests.Api.Clients
{
    public class UserApiClient(IAPIRequestContext apiContext) : ApiClientBase(apiContext)
    {
        public async Task<UserCreationResponse> CreateUserAsync(string roomCode, UserCreationDto user)
        {
            var url = $"/api/users?roomCode={roomCode}";
            var requestInfo = $"POST {url}\nBody: {SerializeRequest(user)}";

            Log.Information("Creating user via API: {RequestInfo}", requestInfo);

            var response = await ApiContext.PostAsync(url,
                new APIRequestContextOptions
                {
                    DataObject = user
                });

            await ValidateResponseAsync(response, 201, requestInfo);

            var result = await DeserializeResponseAsync<UserCreationResponse>(response);
            Log.Information("User created successfully: ID={UserId}, UserCode={UserCode}",
                result.Id, result.UserCode);

            return result;
        }

        public async Task<List<UserReadDto>> GetUsersAsync(string userCode)
        {
            var url = $"/api/users?userCode={userCode}";
            var requestInfo = $"GET {url}";

            Log.Information("Getting users via API: {RequestInfo}", requestInfo);

            var response = await ApiContext.GetAsync(url);

            await ValidateResponseAsync(response, 200, requestInfo);

            var result = await DeserializeResponseAsync<List<UserReadDto>>(response);
            Log.Information("Retrieved {Count} users", result.Count);

            return result;
        }

        public async Task<UserReadDto> GetUserByIdAsync(long userId, string userCode)
        {
            var url = $"/api/users/{userId}?userCode={userCode}";
            var requestInfo = $"GET {url}";

            Log.Information("Getting user by ID via API: {RequestInfo}", requestInfo);

            var response = await ApiContext.GetAsync(url);
            await ValidateResponseAsync(response, 200, requestInfo);

            var responseBody = await response.TextAsync();

            if (responseBody.TrimStart().StartsWith('['))
            {
                var users = await DeserializeResponseAsync<List<UserReadDto>>(response);
                var result = users.FirstOrDefault()
                    ?? throw new InvalidOperationException("No user found");
                Log.Information("User retrieved: {FirstName} {LastName}",
                    result.FirstName, result.LastName);
                return result;
            }

            var user = await DeserializeResponseAsync<UserReadDto>(response);
            Log.Information("User retrieved: {FirstName} {LastName}",
                user.FirstName, user.LastName);
            return user;
        }

        public async Task DeleteUserAsync(long userId, string userCode)
        {
            var url = $"/api/users/{userId}?userCode={userCode}";
            var requestInfo = $"DELETE {url}";

            Log.Information("Deleting user via API: {RequestInfo}", requestInfo);

            var response = await ApiContext.DeleteAsync(url);
            await ValidateResponseAsync(response, 200, requestInfo);

            Log.Information("User {UserId} deleted successfully", userId);
        }

        public async Task<(int StatusCode, string ResponseBody)> TryDeleteUserAsync(long userId, string userCode)
        {
            var url = $"/api/users/{userId}?userCode={userCode}";
            var requestInfo = $"DELETE {url}";

            Log.Information("Attempting to delete user via API: {RequestInfo}", requestInfo);

            try
            {
                var response = await ApiContext.DeleteAsync(url);
                var statusCode = response.Status;
                var responseBody = await response.TextAsync();
                
                Log.Information("Delete user response status: {StatusCode}", statusCode);
                return (statusCode, responseBody);
            }
            catch (ApiException ex)
            {
                Log.Warning("Delete user failed with status {StatusCode}: {Message}", 
                    ex.ActualStatus, ex.ResponseBody);
                return (ex.ActualStatus, ex.ResponseBody);
            }
        }
    }
}
