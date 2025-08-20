using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using mt_backend.Models;
using mt_backend.Services;
using System.Threading.Tasks;

public class NotificationServiceTests
{
    [Fact]
    public async Task NotifyUserAsync_LogsNotificationMessage()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<NotificationService>>();
        var service = new NotificationService(loggerMock.Object);
        var user = new User { Email = "test@example.com" };
        var message = "Test notification";

        // Act
        await service.NotifyUserAsync(user, message);

        // Assert
        loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"Notification to {user.Email}: {message}")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }
}