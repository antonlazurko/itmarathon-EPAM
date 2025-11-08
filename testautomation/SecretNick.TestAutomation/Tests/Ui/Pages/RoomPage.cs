using System.Globalization;
using Microsoft.Playwright;
using Tests.Helpers;

namespace Tests.Ui.Pages
{
    public class RoomPage(IPage page) : BasePage(page)
    {
        public async Task<string> GetRoomNameAsync()
        {
            var locator = Page.Locator("xpath=.//*[@class='room-details__content']//h2 | .//*[@class='room-info__title']").First;
            return await locator.GetTextSafeAsync();
        }

        public async Task<string> GetRoomDescriptionAsync()
        {
            var locator = Page.Locator("xpath=.//*[@class='room-details__description'] | .//*[@class='room-info__description']").First;
            return await locator.GetTextSafeAsync();
        }

        public async Task<string> GetExchangeDateAsync()
        {
            var locator = Page.Locator("xpath=.//*[contains(@class,'room-data-card')]//time | .//*[contains(@class,'info-card')][contains(.,'Exchange Date')]//*[@class='info-card__description']").First;
            return await locator.GetTextSafeAsync();
        }

        public async Task<string> GetGiftBudgetAsync()
        {
            var locator = Page.Locator("xpath=.//*[contains(@class,'room-data-card')][contains(.,'Gift Budget')]//p | .//*[contains(@class,'info-card')][contains(.,'Gift Budget')]//*[@class='info-card__description']").First;
            return await locator.GetTextSafeAsync();
        }

        public async Task ClickInviteNewMembersAsync()
        {
            await ClickButtonAsync("Invite New Members");
        }

        public async Task<bool> IsInviteModalVisibleAsync()
        {
            return await IsVisibleAsync(".//h3[.='Invite New Members']");
        }

        public async Task ClickGoBackToRoomAsync()
        {
            try
            {
                await ClickButtonAsync("Go Back To Room");
            }
            catch
            {
                await ClickButtonAsync("Go Back to Room");
            }
        }

        public async Task<int> GetParticipantsCountAsync()
        {
            var counterText = await GetTextAsync("(.//span[contains(@class,'counter')])[1]");
            var match = ValidationPatterns.Participants().Match(counterText);

            return match.Success ? int.Parse(match.Value) : 0;
        }

        public async Task<bool> IsMinimumPeopleWarningVisibleAsync()
        {
            return await IsVisibleAsync(".//p[.='You need at least 3 people in the room to enable drawing.']");
        }

        public async Task<bool> IsDrawNamesDisabledAsync()
        {
            return await Page.Locator("xpath=.//button[.='Draw Names']").IsDisabledAsync();
        }

        public async Task<bool> IsDrawReminderVisibleAsync()
        {
            var reminder1 = await IsVisibleAsync(".//p[.='Don't forget to hit the button to randomly pair everyone in the game.']");
            var reminder2 = await IsVisibleAsync(".//p[.='Don`t forget to hit the button to randomly pair everyone in the game.']");
            return reminder1 || reminder2;
        }

        public async Task<string> GetGifteeNameAsync()
        {
            var locator = Page.Locator("xpath=.//*[@class='giftee-info__title'] | .//*[@class='random-panel-content__name']").First;
            return await locator.GetTextSafeAsync();
        }

        public async Task<string> GetModalTitleAsync()
        {
            return await GetTextAsync("(.//h3[contains(@class,'modal__title')])[1]");
        }

        public async Task<string> GetPersonalInfoAsync()
        {
            var locator = Page.Locator("xpath=(.//*[contains(@class,'personal-info-content')])[1] | .//*[contains(@class,'personal-info__list')]").First;
            return await locator.GetTextSafeAsync();
        }

        public async Task<string> GetWishlistOrInterestsAsync()
        {
            // Wait for success message to disappear if present
            var successMessageLocator = Page.Locator("xpath=.//*[contains(text(),'Success! All participants are matched')]");
            try
            {
                await successMessageLocator.WaitForAsync(new LocatorWaitForOptions
                {
                    State = WaitForSelectorState.Hidden,
                    Timeout = 5000
                });
            }
            catch
            {
                // Message not present or already hidden, continue
            }

            // Wait for modal to be fully visible (it's rendered outside main DOM tree)
            // Try React modal first
            var reactModalLocator = Page.Locator(".modal-container .randomization-modal");
            if (await reactModalLocator.CountAsync() > 0)
            {
                // Wait for modal to be stable
                await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

                // Try wishlist (React)
                var wishlistLocator = reactModalLocator.Locator(".wishlist");
                if (await wishlistLocator.CountAsync() > 0)
                {
                    await wishlistLocator.WaitForAsync(new LocatorWaitForOptions
                    {
                        State = WaitForSelectorState.Visible,
                        Timeout = 5000
                    });
                    return await wishlistLocator.TextContentAsync() ?? String.Empty;
                }
            }

            // Try Ui modal
            var angularModalLocator = Page.Locator("app-modal-host app-giftee-info-modal");
            if (await angularModalLocator.CountAsync() > 0)
            {
                // Wait for modal to be stable
                await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

                // Try surprise section (Ui)
                var surpriseLocator = angularModalLocator.Locator("section.surprise");
                if (await surpriseLocator.CountAsync() > 0)
                {
                    await surpriseLocator.WaitForAsync(new LocatorWaitForOptions
                    {
                        State = WaitForSelectorState.Visible,
                        Timeout = 5000
                    });
                    return await surpriseLocator.TextContentAsync() ?? String.Empty;
                }

                // Try wishlist in Ui modal
                var wishlistLocator = angularModalLocator.Locator(".modal__content-item:has-text('Wishlist')");
                if (await wishlistLocator.CountAsync() > 0)
                {
                    await wishlistLocator.WaitForAsync(new LocatorWaitForOptions
                    {
                        State = WaitForSelectorState.Visible,
                        Timeout = 5000
                    });
                    return await wishlistLocator.TextContentAsync() ?? String.Empty;
                }
            }

            throw new Exception("Neither wishlist nor surprise section found");
        }

        public async Task<bool> IsCopyLinkVisibleForParticipantAsync(string participantFullName)
        {
            var locator1 = await IsVisibleAsync($".//*[contains(@class,'item-card')][contains(.,'{participantFullName}')]//*[@class='copy-button']");
            var locator2 = await IsVisibleAsync($".//li[contains(.,'{participantFullName}')]//*[@aria-label='Copy personal link']");
            return locator1 || locator2;
        }

        public async Task<(bool isToday, string message)> IsExchangeDateTodayAsync()
        {
            return await IsExchangeDateAsync(FormatDate(DateTime.Today));
        }

        public async Task<(bool isTrue, string message)> IsExchangeDateAsync(DateTime expectedDate)
        {
            return await IsExchangeDateAsync(FormatDate(expectedDate));
        }

        public async Task<(bool isTrue, string message)> IsExchangeDateAsync(string expectedDate)
        {
            var dateText = await GetExchangeDateAsync();
            return (dateText.Contains(expectedDate), $"Expected date: '{expectedDate}', actual date: {dateText}");
        }

        private static string FormatDate(DateTime date)
        {
            return date.ToString("dd MMM yyyy", CultureInfo.InvariantCulture);
        }

        public async Task<bool> IsDeleteButtonVisibleForParticipantAsync(string participantFullName)
        {
            // Check for delete button in participant card - React uses icon button with aria-label
            var deleteButtonLocator = Page.Locator($"xpath=.//*[contains(@class,'item-card')][contains(.,'{participantFullName}')]//button[@aria-label='delete' or contains(@aria-label,'Delete')] | .//li[contains(.,'{participantFullName}')]//button[@aria-label='delete' or contains(@aria-label,'Delete')] | .//*[contains(.,'{participantFullName}')]//*[@class='copy-button']//button[contains(@aria-label,'delete')]");
            
            if (await deleteButtonLocator.CountAsync() > 0)
            {
                return await deleteButtonLocator.IsVisibleAsync();
            }
            
            return false;
        }

        public async Task ClickDeleteButtonForParticipantAsync(string participantFullName)
        {
            var deleteButtonLocator = Page.Locator($"xpath=.//*[contains(@class,'item-card')][contains(.,'{participantFullName}')]//button[@aria-label='delete' or contains(@aria-label,'Delete')] | .//li[contains(.,'{participantFullName}')]//button[@aria-label='delete' or contains(@aria-label,'Delete')] | .//*[contains(.,'{participantFullName}')]//*[@class='copy-button']//button[contains(@aria-label,'delete')]");
            
            await deleteButtonLocator.WaitForAsync(new LocatorWaitForOptions 
            { 
                State = WaitForSelectorState.Visible,
                Timeout = 5000
            });
            await deleteButtonLocator.ClickAsync();
        }

        public async Task<bool> IsDeleteConfirmationModalVisibleAsync()
        {
            var modalLocator = Page.Locator("xpath=.//h3[.='Remove Participant'] | .//*[contains(@class,'modal')][contains(.,'Remove Participant')]");
            try
            {
                await modalLocator.WaitForAsync(new LocatorWaitForOptions
                {
                    State = WaitForSelectorState.Visible,
                    Timeout = 3000
                });
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<string> GetDeleteConfirmationModalTextAsync()
        {
            var modalTextLocator = Page.Locator("xpath=.//*[contains(@class,'modal')]//p[contains(.,'Are you sure')] | .//*[contains(@class,'modal__description')]");
            return await modalTextLocator.First.TextContentAsync() ?? string.Empty;
        }

        public async Task ClickConfirmDeleteButtonAsync()
        {
            var confirmButtonLocator = Page.Locator("xpath=.//button[contains(text(),'Confirm')] | .//button[contains(@class,'confirm')]");
            await confirmButtonLocator.WaitForAsync(new LocatorWaitForOptions 
            { 
                State = WaitForSelectorState.Visible,
                Timeout = 5000
            });
            await confirmButtonLocator.ClickAsync();
        }

        public async Task ClickCancelDeleteButtonAsync()
        {
            var cancelButtonLocator = Page.Locator("xpath=.//button[contains(text(),'Cancel')] | .//button[contains(@class,'cancel')]");
            await cancelButtonLocator.WaitForAsync(new LocatorWaitForOptions 
            { 
                State = WaitForSelectorState.Visible,
                Timeout = 5000
            });
            await cancelButtonLocator.ClickAsync();
        }

        public async Task<bool> IsSuccessMessageVisibleAsync(string message)
        {
            var successMessageLocator = Page.Locator($"xpath=.//*[contains(text(),'{message}')] | .//*[contains(@class,'success')][contains(text(),'{message}')]");
            try
            {
                await successMessageLocator.WaitForAsync(new LocatorWaitForOptions 
                { 
                    State = WaitForSelectorState.Visible,
                    Timeout = 5000
                });
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> IsParticipantVisibleAsync(string participantFullName)
        {
            var participantLocator = Page.Locator($"xpath=.//*[contains(@class,'item-card')][contains(.,'{participantFullName}')] | .//li[contains(.,'{participantFullName}')] | .//*[contains(@class,'participant')][contains(.,'{participantFullName}')]");
            return await participantLocator.CountAsync() > 0;
        }

        public async Task WaitForParticipantToDisappearAsync(string participantFullName, int timeoutMs = 10000)
        {
            var participantLocator = Page.Locator($"xpath=.//*[contains(@class,'item-card')][contains(.,'{participantFullName}')] | .//li[contains(.,'{participantFullName}')]");
            
            try
            {
                await participantLocator.WaitForAsync(new LocatorWaitForOptions
                {
                    State = WaitForSelectorState.Hidden,
                    Timeout = timeoutMs
                });
            }
            catch
            {
                // Participant might already be gone or locator changed
            }
        }
    }
}
