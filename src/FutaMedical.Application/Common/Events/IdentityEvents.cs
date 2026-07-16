using MediatR;

namespace FutaMedical.Application.Common.Events;

public record UserInvitedEvent(string Email, string SetupToken, string Role) : INotification;
