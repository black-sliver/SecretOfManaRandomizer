using JetBrains.Annotations;
using Xunit;

namespace SoMRandomizer.api.Tests;

[TestSubject(typeof(OpenWorld))]
public class OpenWorldTest
{
    [Fact]
    public void TestAllItemsGenerated()
    {
        Assert.True(OpenWorld.GetAllItemsManaged().Count > 0);
    }

    [Fact]
    public void TestAllLocationsGenerated()
    {
        Assert.True(OpenWorld.GetAllLocationsManaged().Count > 0);
    }
}
