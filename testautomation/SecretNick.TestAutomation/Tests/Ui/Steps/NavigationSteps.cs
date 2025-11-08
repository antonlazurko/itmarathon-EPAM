using Microsoft.Playwright;
using Reqnroll;
using Tests.Api.Models.Responses;

namespace Tests.Ui.Steps
{
    [Binding]
    public class NavigationSteps(
            ScenarioContext scenarioContext,
            IPage page)
    {
        [Given("I am on the home page")]
        public async Task GivenIAmOnTheHomePage()
        {
            var baseUrl = scenarioContext.Get<string>("baseUrl");

            await page.GotoAsync(baseUrl);
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        }

        [When("I navigate to room page with admin code")]
        public async Task WhenINavigateToRoomPageWithAdminCode()
        {
            var response = scenarioContext.Get<RoomCreationResponse>("RoomCreationResponse");
            var baseUrl = scenarioContext.Get<string>("baseUrl");

            await page.GotoAsync($"{baseUrl}/room/{response.UserCode}");
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        }

        [When("I navigate to join page with invitation code")]
        public async Task WhenINavigateToJoinPageWithInvitationCode()
        {
            var response = scenarioContext.Get<RoomCreationResponse>("RoomCreationResponse");
            var baseUrl = scenarioContext.Get<string>("baseUrl");

            await page.GotoAsync($"{baseUrl}/join/{response.Room.InvitationCode}");
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        }

        [When("I navigate to room page with regular user code")]
        public async Task WhenINavigateToRoomPageWithRegularUserCode()
        {
            var regularUserCode = scenarioContext.Get<string>("RegularUserCode");
            var baseUrl = scenarioContext.Get<string>("baseUrl");

            await page.GotoAsync($"{baseUrl}/room/{regularUserCode}");
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        }

        [When("I refresh the page")]
        public async Task WhenIRefreshThePage()
        {
            await page.ReloadAsync();
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        }
    }
}
