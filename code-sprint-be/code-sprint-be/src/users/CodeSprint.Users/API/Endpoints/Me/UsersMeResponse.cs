namespace CodeSprint.Users.API.Endpoints.Me;

/// <summary>
/// Response contract for <c>GET /users/me</c>. Shapes the BFF-facing payload;
/// keeps the <see cref="Domain.User"/> aggregate out of the transport layer.
/// </summary>
public record UsersMeResponse(
    string Id,
    string Handle,
    string DisplayName,
    string Email,
    string? Avatar,
    int Points,
    DateTime MemberSince
);
