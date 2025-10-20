
using CreditPro.Application.Interfaces;
using CreditPro.Application.UseCases;
using CreditPro.Domain.Entities;
using CreditPro.Application.DTOs;
using Moq;
using Xunit;
using Assert = Xunit.Assert;

namespace CreditPro.Tests.Application;

public class CreateCreditApplicationUseCaseTests
{
    private readonly Mock<ICreditApplicationRepository> _mockAppRepository;
    private readonly Mock<IAuditEventRepository> _mockAuditRepository;
    private readonly CreateCreditApplicationUseCase _useCase;

    public CreateCreditApplicationUseCaseTests()
    {
        _mockAppRepository = new Mock<ICreditApplicationRepository>();
        _mockAuditRepository = new Mock<IAuditEventRepository>();
        _useCase = new CreateCreditApplicationUseCase(
            _mockAppRepository.Object,
            _mockAuditRepository.Object);
    }

    [Fact]
    public async Task ExecuteAsync_WithValidRequest_CreatesApplicationAndAuditEvent()
    {
        // Arrange
        var request = new CreateCreditApplicationRequest(
            "CUST-12345",
            50000m,
            DateTime.UtcNow,
            "Vehículo");

        _mockAppRepository
            .Setup(x => x.CreateAsync(It.IsAny<CreditApplication>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((CreditApplication app, CancellationToken ct) => app);

        // Act
        var response = await _useCase.ExecuteAsync(request);

        // Assert
        Assert.NotEqual(Guid.Empty, response.ApplicationId);
        Assert.Equal(request.CustomerId, response.CustomerId);
        Assert.Equal(request.CreditAmount, response.CreditAmount);
        Assert.Equal("Recibida", response.Status);

        _mockAppRepository.Verify(
            x => x.CreateAsync(It.IsAny<CreditApplication>(), It.IsAny<CancellationToken>()),
            Times.Once);

        _mockAuditRepository.Verify(
            x => x.SaveEventAsync(It.IsAny<AuditEvent>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Theory]
    [InlineData(500)]
    [InlineData(200000)]
    public async Task ExecuteAsync_WithInvalidAmount_ThrowsArgumentException(decimal amount)
    {
        // Arrange
        var request = new CreateCreditApplicationRequest(
            "CUST-12345",
            amount,
            DateTime.UtcNow);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _useCase.ExecuteAsync(request));
    }

    [Fact]
    public async Task ExecuteAsync_SavesCorrectAuditEventType()
    {
        // Arrange
        var request = new CreateCreditApplicationRequest(
            "CUST-12345",
            50000m,
            DateTime.UtcNow);

        AuditEvent? savedEvent = null;
        _mockAuditRepository
            .Setup(x => x.SaveEventAsync(It.IsAny<AuditEvent>(), It.IsAny<CancellationToken>()))
            .Callback<AuditEvent, CancellationToken>((evt, ct) => savedEvent = evt);

        _mockAppRepository
            .Setup(x => x.CreateAsync(It.IsAny<CreditApplication>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((CreditApplication app, CancellationToken ct) => app);

        // Act
        await _useCase.ExecuteAsync(request);

        // Assert
        Assert.NotNull(savedEvent);
        Assert.Equal("Creación", savedEvent.EventType);
        Assert.Equal("Recibida", savedEvent.NewState);
        Assert.Contains("creditAmount", savedEvent.Details.Keys);
        Assert.Contains("customerId", savedEvent.Details.Keys);
    }
}