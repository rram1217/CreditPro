using CreditPro.Domain.Entities;
using CreditPro.Domain.Enums;
using Xunit;
using Assert = Xunit.Assert;

namespace CreditPro.Tests.Domain;

public class CreditApplicationTests
{
    [Fact]
    public void Constructor_WithValidData_CreatesApplication()
    {
        // Arrange
        var customerId = "CUST-12345";
        var creditAmount = 50000m;
        var applicationDate = DateTime.UtcNow;
        var collateralDescription = "Vehículo";

        // Act
        var application = new CreditApplication(
            customerId,
            creditAmount,
            applicationDate,
            collateralDescription);

        // Assert
        Assert.NotEqual(Guid.Empty, application.ApplicationId);
        Assert.Equal(customerId, application.CustomerId);
        Assert.Equal(creditAmount, application.CreditAmount);
        Assert.Equal(applicationDate, application.ApplicationDate);
        Assert.Equal(CreditApplicationStatus.Recibida, application.Status);
        Assert.Equal(collateralDescription, application.CollateralDescription);
    }

    [Theory]
    [InlineData(1000)]
    [InlineData(500)]
    public void Constructor_WithAmountTooLow_ThrowsArgumentException(decimal amount)
    {
        // Arrange
        var customerId = "CUST-12345";
        var applicationDate = DateTime.UtcNow;

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            new CreditApplication(customerId, amount, applicationDate));

        Assert.Contains("Credit amount must be greater than 1,000", exception.Message);
    }

    [Theory]
    [InlineData(150000)]
    [InlineData(200000)]
    public void Constructor_WithAmountTooHigh_ThrowsArgumentException(decimal amount)
    {
        // Arrange
        var customerId = "CUST-12345";
        var applicationDate = DateTime.UtcNow;

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            new CreditApplication(customerId, amount, applicationDate));

        Assert.Contains("Credit amount must be greater than 1,000 and less than 150,000", exception.Message);
    }

    [Theory]
    [InlineData(1001)]
    [InlineData(50000)]
    [InlineData(149999)]
    public void Constructor_WithValidAmount_CreatesApplication(decimal amount)
    {
        // Arrange
        var customerId = "CUST-12345";
        var applicationDate = DateTime.UtcNow;

        // Act
        var application = new CreditApplication(customerId, amount, applicationDate);

        // Assert
        Assert.Equal(amount, application.CreditAmount);
        Assert.Equal(CreditApplicationStatus.Recibida, application.Status);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithInvalidCustomerId_ThrowsArgumentException(string customerId)
    {
        // Arrange
        var creditAmount = 50000m;
        var applicationDate = DateTime.UtcNow;

        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            new CreditApplication(customerId, creditAmount, applicationDate));
    }

    [Fact]
    public void UpdateStatus_WithValidStatus_UpdatesSuccessfully()
    {
        // Arrange
        var application = new CreditApplication("CUST-12345", 50000m, DateTime.UtcNow);
        var initialUpdatedAt = application.UpdatedAt;
        Thread.Sleep(10);

        // Act
        application.UpdateStatus(CreditApplicationStatus.Aprobada);

        // Assert
        Assert.Equal(CreditApplicationStatus.Aprobada, application.Status);
        Assert.True(application.UpdatedAt > initialUpdatedAt);
    }

    [Fact]
    public void UpdateStatus_WithInvalidStatus_ThrowsArgumentException()
    {
        // Arrange
        var application = new CreditApplication("CUST-12345", 50000m, DateTime.UtcNow);
        var invalidStatus = (CreditApplicationStatus)999;

        // Act & Assert
        Assert.Throws<ArgumentException>(() => application.UpdateStatus(invalidStatus));
    }

    [Fact]
    public void Constructor_WithoutCollateral_CreatesApplicationWithNullCollateral()
    {
        // Arrange & Act
        var application = new CreditApplication("CUST-12345", 50000m, DateTime.UtcNow);

        // Assert
        Assert.Null(application.CollateralDescription);
    }
}