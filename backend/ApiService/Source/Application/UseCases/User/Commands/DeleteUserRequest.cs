using CSharpFunctionalExtensions;
using FluentValidation.Results;
using MediatR;
using RoomAggregate = Epam.ItMarathon.ApiService.Domain.Aggregate.Room.Room;

namespace Epam.ItMarathon.ApiService.Application.UseCases.User.Commands
{
    /// <summary>
    /// Request for deleting a User.
    /// </summary>
    /// <param name="UserCode">Authorization code of the admin User.</param>
    /// <param name="UserId">Unique identifier of the User to delete.</param>
    public record DeleteUserRequest(string UserCode, ulong UserId)
        : IRequest<Result<RoomAggregate, ValidationResult>>;
}