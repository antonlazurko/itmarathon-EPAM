using CSharpFunctionalExtensions;
using Epam.ItMarathon.ApiService.Application.UseCases.User.Commands;
using Epam.ItMarathon.ApiService.Domain.Abstract;
using Epam.ItMarathon.ApiService.Domain.Shared.ValidationErrors;
using FluentValidation.Results;
using MediatR;
using RoomAggregate = Epam.ItMarathon.ApiService.Domain.Aggregate.Room.Room;

namespace Epam.ItMarathon.ApiService.Application.UseCases.User.Handlers
{
    /// <summary>
    /// Handler for User deletion command.
    /// </summary>
    /// <param name="roomRepository">Implementation of <see cref="IRoomRepository"/> for operating with database.</param>
    public class DeleteUserHandler(IRoomRepository roomRepository, IUserReadOnlyRepository userRepository)
        : IRequestHandler<DeleteUserRequest, Result<RoomAggregate, ValidationResult>>
    {
        /// <inheritdoc/>
        public async Task<Result<RoomAggregate, ValidationResult>> Handle(DeleteUserRequest request,
            CancellationToken cancellationToken)
        {
            // Get room by admin's userCode
            var roomResult = await roomRepository.GetByUserCodeAsync(request.UserCode, cancellationToken);
            if (roomResult.IsFailure)
            {
                return Result.Failure<RoomAggregate, ValidationResult>(new NotFoundError([
                    new ValidationFailure(nameof(request.UserCode), "User not found")
                ]));
            }

            // Verify that requesting user is admin
            var adminUser = roomResult.Value.Users.First(user => user.AuthCode.Equals(request.UserCode));
            if (!adminUser.IsAdmin)
            {
                return Result.Failure<RoomAggregate, ValidationResult>(new BadRequestError([
                    new ValidationFailure(nameof(request.UserCode), "User is not an admin")
                ]));
            }

            // Find user globally and validate room membership
            var globalUserResult = await userRepository.GetByIdAsync(request.UserId, cancellationToken);
            if (globalUserResult.IsFailure)
            {
                return Result.Failure<RoomAggregate, ValidationResult>(new NotFoundError([
                    new ValidationFailure(nameof(request.UserId), "User not found")
                ]));
            }

            if (globalUserResult.Value.RoomId != roomResult.Value.Id)
            {
                // User exists but is not in admin's room
                return Result.Failure<RoomAggregate, ValidationResult>(new BadRequestError([
                    new ValidationFailure(nameof(request.UserId), "User does not belong to admin's room")
                ]));
            }

            // Find user to delete inside the room aggregate
            var userToDelete = roomResult.Value.Users.FirstOrDefault(user => user.Id.Equals(request.UserId));
            if (userToDelete is null)
            {
                // This should not happen because we checked global user's RoomId, but handle defensively
                return Result.Failure<RoomAggregate, ValidationResult>(new NotFoundError([
                    new ValidationFailure(nameof(request.UserId), "User not found in room")
                ]));
            }

            // Cannot delete admin
            if (userToDelete.IsAdmin)
            {
                return Result.Failure<RoomAggregate, ValidationResult>(new BadRequestError([
                    new ValidationFailure(nameof(request.UserId), "Cannot delete admin user")
                ]));
            }

            // Room must not be closed (drawn)
            if (roomResult.Value.ClosedOn is not null)
            {
                return Result.Failure<RoomAggregate, ValidationResult>(new BadRequestError([
                    new ValidationFailure("room.ClosedOn", "Room is already closed.")
                ]));
            }

            roomResult.Value.Users.Remove(userToDelete);

            // Update room in database
            var updateResult = await roomRepository.UpdateAsync(roomResult.Value, cancellationToken);
            if (updateResult.IsFailure)
            {
                return Result.Failure<RoomAggregate, ValidationResult>(new BadRequestError([
                    new ValidationFailure(nameof(request.UserId), updateResult.Error)
                ]));
            }

            // Return updated room aggregate
            return Result.Success<RoomAggregate, ValidationResult>(roomResult.Value);
        }
    }
}