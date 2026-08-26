using System.IdentityModel.Tokens.Jwt;
using CardiacMonitor.Data;
using CardiacMonitor.DTOs;
using CardiacMonitor.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CardiacMonitor.Tests.Services;

public class AuthServiceTests
{
    // اخترنا المصادقة ضمن مخاطر Day05 لأنها تتعامل مع الدخول وإعادة استخدام الرموز.
    private const string JwtKey =
        "TestOnlySigningKeyThatIsLongEnoughForHmacSha256-123456789";

    [Fact]
    public async Task LoginAsync_WhenEmailIsUnknown_ReturnsGenericFailure()
    {
        // Arrange 
        await using var context = CreateContext();
        var userManager = CreateUserManagerMock();
        var request = new LoginRequest("missing@example.com", "WrongPassword123!");

        userManager
            .Setup(manager => manager.FindByEmailAsync(request.Email))
            .ReturnsAsync((IdentityUser?)null);

        var service = CreateService(userManager, context);

        // Act
        var result = await service.LoginAsync(request);

        // Assert 
        Assert.False(result.IsSuccess);
        Assert.Equal("Invalid email or password.", result.Message);
        Assert.Null(result.Token);
        Assert.Null(result.RefreshToken);
        userManager.Verify(
            manager => manager.CheckPasswordAsync(
                It.IsAny<IdentityUser>(),
                It.IsAny<string>()),
            Times.Never);
    }

    [Fact]
    public async Task LoginAsync_WhenCredentialsAreValid_IssuesAndStoresTokenPair()
    {
        // Arrange 
        await using var context = CreateContext();
        var userManager = CreateUserManagerMock();
        var user = CreateDoctorUser();
        var request = new LoginRequest(user.Email!, "Doctor@123");

        ConfigureValidLogin(userManager, user, request);
        var service = CreateService(userManager, context);

        // Act
        var result = await service.LoginAsync(request);

        // Assert 
        Assert.True(result.IsSuccess);
        Assert.False(string.IsNullOrWhiteSpace(result.Token));
        Assert.False(string.IsNullOrWhiteSpace(result.RefreshToken));

        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(result.Token);
        var storedToken = await context.RefreshTokens.SingleAsync();

        Assert.Equal(user.Id, jwt.Subject);
        Assert.Equal(jwt.Id, storedToken.JwtId);
        Assert.Equal(user.Id, storedToken.UserId);
        Assert.Equal(result.RefreshToken, storedToken.Token);
        Assert.False(storedToken.IsUsed);
        Assert.False(storedToken.IsRevoked);
        Assert.True(storedToken.ExpiryDate > DateTime.UtcNow.AddDays(6));
        userManager.Verify(
            manager => manager.GetRolesAsync(user),
            Times.Once);
    }

    [Fact]
    public async Task RefreshTokenAsync_WhenRefreshTokenIsReused_RejectsSecondAttempt()
    {
        // Arrange  
        await using var context = CreateContext();
        var userManager = CreateUserManagerMock();
        var user = CreateDoctorUser();
        var loginRequest = new LoginRequest(user.Email!, "Doctor@123");

        ConfigureValidLogin(userManager, user, loginRequest);
        userManager
            .Setup(manager => manager.FindByIdAsync(user.Id))
            .ReturnsAsync(user);

        var service = CreateService(userManager, context, durationInMinutes: -1);
        var loginResult = await service.LoginAsync(loginRequest);
        var refreshRequest = new TokenRequest(
            loginResult.Token!,
            loginResult.RefreshToken!);

        // Act
        var firstRefresh = await service.RefreshTokenAsync(refreshRequest);
        var reusedRefresh = await service.RefreshTokenAsync(refreshRequest);

        // Assert 
        Assert.True(firstRefresh.IsSuccess);
        Assert.False(reusedRefresh.IsSuccess);
        Assert.Equal("Refresh token has already been used.", reusedRefresh.Message);
        Assert.Equal(2, await context.RefreshTokens.CountAsync());
        Assert.True(await context.RefreshTokens
            .Where(token => token.Token == refreshRequest.RefreshToken)
            .Select(token => token.IsUsed)
            .SingleAsync());
    }

    private static AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase($"AuthServiceTests-{Guid.NewGuid()}")
            .Options;

        return new AppDbContext(options);
    }

    private static Mock<UserManager<IdentityUser>> CreateUserManagerMock()
    {
        var store = new Mock<IUserStore<IdentityUser>>();

        return new Mock<UserManager<IdentityUser>>(
            store.Object,
            null!,
            null!,
            null!,
            null!,
            null!,
            null!,
            null!,
            null!);
    }

    private static Mock<RoleManager<IdentityRole>> CreateRoleManagerMock()
    {
        var store = new Mock<IRoleStore<IdentityRole>>();

        return new Mock<RoleManager<IdentityRole>>(
            store.Object,
            Array.Empty<IRoleValidator<IdentityRole>>(),
            new UpperInvariantLookupNormalizer(),
            new IdentityErrorDescriber(),
            Mock.Of<ILogger<RoleManager<IdentityRole>>>());
    }

    private static IConfiguration CreateConfiguration(int durationInMinutes)
    {
        var values = new Dictionary<string, string?>
        {
            ["Jwt:Key"] = JwtKey,
            ["Jwt:Issuer"] = "CardiacMonitorTests",
            ["Jwt:Audience"] = "CardiacMonitorTests",
            ["Jwt:DurationInMinutes"] = durationInMinutes.ToString()
        };

        return new ConfigurationBuilder()
            .AddInMemoryCollection(values)
            .Build();
    }

    private static AuthService CreateService(
        Mock<UserManager<IdentityUser>> userManager,
        AppDbContext context,
        int durationInMinutes = 15)
    {
        return new AuthService(
            userManager.Object,
            CreateRoleManagerMock().Object,
            CreateConfiguration(durationInMinutes),
            context);
    }

    private static IdentityUser CreateDoctorUser()
    {
        return new IdentityUser
        {
            Id = "doctor-test-id",
            UserName = "doctor@example.com",
            Email = "doctor@example.com"
        };
    }

    private static void ConfigureValidLogin(
        Mock<UserManager<IdentityUser>> userManager,
        IdentityUser user,
        LoginRequest request)
    {
        userManager
            .Setup(manager => manager.FindByEmailAsync(request.Email))
            .ReturnsAsync(user);
        userManager
            .Setup(manager => manager.CheckPasswordAsync(user, request.Password))
            .ReturnsAsync(true);
        userManager
            .Setup(manager => manager.GetRolesAsync(user))
            .ReturnsAsync(new[] { "Doctor" });
    }
}
