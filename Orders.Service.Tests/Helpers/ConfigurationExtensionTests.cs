using Microsoft.Extensions.Configuration;
using Moq;
using Orders.Service.Helpers;

namespace Orders.Service.Tests.Helpers;

public class ConfigurationExtensionTests
{
    [Fact]
    public void RetrieveConfigurationValue_Should_Return_Configuration_Value()
    {
        // Arrange
        var configurationName = "SampleKey";
        var expectedValue = "SampleValue";

        var configurationMock = new Mock<IConfiguration>();
        configurationMock.Setup(c => c[configurationName])
            .Returns(expectedValue);

        // Act
        var result = configurationMock.Object.RetrieveConfigurationValue(configurationName);

        // Assert
        Assert.Equal(expectedValue, result);
    }
}