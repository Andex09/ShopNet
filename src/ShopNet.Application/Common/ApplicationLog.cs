using Microsoft.Extensions.Logging;

namespace ShopNet.Application.Common;

/// <summary>Structured, source-generated log messages for business events.</summary>
internal static partial class ApplicationLog
{
    [LoggerMessage(EventId = 1000, Level = LogLevel.Information, Message = "User {UserId} registered")]
    public static partial void UserRegistered(this ILogger logger, int userId);

    [LoggerMessage(EventId = 1001, Level = LogLevel.Warning, Message = "Failed login attempt for user name {UserName}")]
    public static partial void LoginFailed(this ILogger logger, string userName);

    [LoggerMessage(EventId = 1002, Level = LogLevel.Information, Message = "User {UserId} signed in")]
    public static partial void LoginSucceeded(this ILogger logger, int userId);

    [LoggerMessage(EventId = 1100, Level = LogLevel.Information, Message = "Product {ProductId} created")]
    public static partial void ProductCreated(this ILogger logger, int productId);

    [LoggerMessage(EventId = 1101, Level = LogLevel.Information, Message = "Product {ProductId} deleted")]
    public static partial void ProductDeleted(this ILogger logger, int productId);

    [LoggerMessage(EventId = 1200, Level = LogLevel.Information, Message = "User {UserId} created with role {Role}")]
    public static partial void UserCreated(this ILogger logger, int userId, string role);

    [LoggerMessage(EventId = 1201, Level = LogLevel.Information, Message = "User {UserId} deleted")]
    public static partial void UserDeleted(this ILogger logger, int userId);

    [LoggerMessage(EventId = 1300, Level = LogLevel.Information, Message = "Order {OrderId} created for user {UserId}, total {Total}")]
    public static partial void OrderCreated(this ILogger logger, int orderId, int userId, decimal total);

    [LoggerMessage(EventId = 1301, Level = LogLevel.Information, Message = "Order {OrderId} deleted")]
    public static partial void OrderDeleted(this ILogger logger, int orderId);
}
