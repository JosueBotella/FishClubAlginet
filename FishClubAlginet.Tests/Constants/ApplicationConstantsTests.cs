namespace FishClubAlginet.Tests.Constants;

public class ApplicationConstantsTests
{
    [Fact]
    public void Roles_Admin_ShouldHaveCorrectValue()
    {
        Assert.Equal("Admin", ApplicationConstants.Roles.Admin);
    }

    [Fact]
    public void Roles_Fisherman_ShouldHaveCorrectValue()
    {
        Assert.Equal("Fisherman", ApplicationConstants.Roles.Fisherman);
    }

    [Fact]
    public void Roles_AdminAndFisherman_ShouldBeDistinct()
    {
        Assert.NotEqual(ApplicationConstants.Roles.Admin, ApplicationConstants.Roles.Fisherman);
    }
}
