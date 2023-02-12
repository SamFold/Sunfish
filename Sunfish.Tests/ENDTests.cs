using Sunfish;

namespace Sunfish.Tests;

public class ENDTests
{
    [Fact]
    public void Test1()
    {
        //Arrange
        var endHelper = new ENDCalculator();

        //Act
        var result = endHelper.GetEquivalentNarcoticDepth(30, 30);

        //Assert
        Assert.Equal(18, result);

        //Assert.Equal();
    }





}
