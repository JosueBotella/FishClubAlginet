namespace FishClubAlginet.Tests.Seeds;

public class RolesSeedTests
{
    private readonly Mock<RoleManager<IdentityRole>> _roleManagerMock;

    public RolesSeedTests()
    {
        var roleStoreMock = new Mock<IRoleStore<IdentityRole>>();
        _roleManagerMock = new Mock<RoleManager<IdentityRole>>(
            roleStoreMock.Object,
            Array.Empty<IRoleValidator<IdentityRole>>(),
            new Mock<ILookupNormalizer>().Object,
            new IdentityErrorDescriber(),
            new Mock<ILogger<RoleManager<IdentityRole>>>().Object);
    }

    [Fact]
    public async Task SeedAsync_WhenNoRolesExist_ShouldCreateBothRoles()
    {
        // Arrange
        _roleManagerMock.Setup(x => x.RoleExistsAsync(It.IsAny<string>()))
            .ReturnsAsync(false);
        _roleManagerMock.Setup(x => x.CreateAsync(It.IsAny<IdentityRole>()))
            .ReturnsAsync(IdentityResult.Success);

        // Act
        await RolesSeed.SeedAsync(_roleManagerMock.Object);

        // Assert
        _roleManagerMock.Verify(x => x.CreateAsync(It.IsAny<IdentityRole>()), Times.Exactly(2));
    }

    [Fact]
    public async Task SeedAsync_WhenAllRolesExist_ShouldNotCreateAnyRole()
    {
        // Arrange
        _roleManagerMock.Setup(x => x.RoleExistsAsync(It.IsAny<string>()))
            .ReturnsAsync(true);

        // Act
        await RolesSeed.SeedAsync(_roleManagerMock.Object);

        // Assert
        _roleManagerMock.Verify(x => x.CreateAsync(It.IsAny<IdentityRole>()), Times.Never);
    }

    [Fact]
    public async Task SeedAsync_WhenAdminRoleExists_ShouldOnlyCreateFishermanRole()
    {
        // Arrange
        _roleManagerMock.Setup(x => x.RoleExistsAsync(ApplicationConstants.Roles.Admin))
            .ReturnsAsync(true);
        _roleManagerMock.Setup(x => x.RoleExistsAsync(ApplicationConstants.Roles.Fisherman))
            .ReturnsAsync(false);
        _roleManagerMock.Setup(x => x.CreateAsync(It.IsAny<IdentityRole>()))
            .ReturnsAsync(IdentityResult.Success);

        // Act
        await RolesSeed.SeedAsync(_roleManagerMock.Object);

        // Assert
        _roleManagerMock.Verify(
            x => x.CreateAsync(It.Is<IdentityRole>(r => r.Name == ApplicationConstants.Roles.Fisherman)),
            Times.Once);
        _roleManagerMock.Verify(
            x => x.CreateAsync(It.Is<IdentityRole>(r => r.Name == ApplicationConstants.Roles.Admin)),
            Times.Never);
    }

    [Fact]
    public async Task SeedAsync_WhenFishermanRoleExists_ShouldOnlyCreateAdminRole()
    {
        // Arrange
        _roleManagerMock.Setup(x => x.RoleExistsAsync(ApplicationConstants.Roles.Admin))
            .ReturnsAsync(false);
        _roleManagerMock.Setup(x => x.RoleExistsAsync(ApplicationConstants.Roles.Fisherman))
            .ReturnsAsync(true);
        _roleManagerMock.Setup(x => x.CreateAsync(It.IsAny<IdentityRole>()))
            .ReturnsAsync(IdentityResult.Success);

        // Act
        await RolesSeed.SeedAsync(_roleManagerMock.Object);

        // Assert
        _roleManagerMock.Verify(
            x => x.CreateAsync(It.Is<IdentityRole>(r => r.Name == ApplicationConstants.Roles.Admin)),
            Times.Once);
        _roleManagerMock.Verify(
            x => x.CreateAsync(It.Is<IdentityRole>(r => r.Name == ApplicationConstants.Roles.Fisherman)),
            Times.Never);
    }

    [Fact]
    public async Task SeedAsync_ShouldSeedExactlyAdminAndFishermanRoles()
    {
        // Arrange
        var seededRoles = new List<string>();
        _roleManagerMock.Setup(x => x.RoleExistsAsync(It.IsAny<string>()))
            .ReturnsAsync(false);
        _roleManagerMock.Setup(x => x.CreateAsync(It.IsAny<IdentityRole>()))
            .Callback<IdentityRole>(role => seededRoles.Add(role.Name!))
            .ReturnsAsync(IdentityResult.Success);

        // Act
        await RolesSeed.SeedAsync(_roleManagerMock.Object);

        // Assert
        Assert.Contains(ApplicationConstants.Roles.Admin, seededRoles);
        Assert.Contains(ApplicationConstants.Roles.Fisherman, seededRoles);
        Assert.Equal(2, seededRoles.Count);
    }
}
