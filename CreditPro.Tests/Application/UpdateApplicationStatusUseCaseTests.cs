using CreditPro.Application.DTOs;
using CreditPro.Application.Interfaces;
using CreditPro.Application.UseCases;
using CreditPro.Domain.Entities;
using CreditPro.Domain.Exceptions;
using Moq;
using Xunit;
using Assert = Xunit.Assert;

namespace CreditPro.Tests.Application;

public class UpdateApplicationStatusUseCaseTests
{
    private readonly Mock<ICreditApplicationRepository> _mockAppRepository;
    private readonly Mock<IAuditEventRepository> _mockAuditRepository;
    private readonly UpdateApplicationStatusUseCase _useCase;

    public UpdateApplicationStatusUseCaseTests()
    {
        _mockAppRepository = new Mock<ICreditApplicationRepository>();
        _mockAuditRepository = new Mock<IAuditEventRepository>();
        _useCase = new UpdateApplicationStatusUseCase(
            _mockAppRepository.Object,
            _mockAuditRepository.Object);
    }

    [Fact]
    public async Task ExecuteAsync_WithValidRequest_UpdatesStatusAndCreatesAuditEvent()
    {
        // Arrange
        var applicationId = Guid.NewGuid();
        var application = new CreditApplication("CUST-12345", 50000m, DateTime.UtcNow);
        var request = new UpdateStatusRequest("Aprobada", "Cliente confiable");

        _mockAppRepository
            .Setup(x => x.GetByIdAsync(applicationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(application);

        // Act
        var response = await _useCase.ExecuteAsync(applicationId, request);

        // Assert
        Assert.Equal(applicationId, response.ApplicationId);
        Assert.Equal("Recibida", response.PreviousStatus);
        Assert.Equal("Aprobada", response.NewStatus);

        _mockAppRepository.Verify(
            x => x.UpdateAsync(It.IsAny<CreditApplication>(), It.IsAny<CancellationToken>()),
            Times.Once);

        _mockAuditRepository.Verify(
            x => x.SaveEventAsync(It.IsAny<AuditEvent>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WithNonExistentApplication_ThrowsNotFoundException()
    {
        // Arrange
        var applicationId = Guid.NewGuid();
        var request = new UpdateStatusRequest("Aprobada");

        _mockAppRepository
            .Setup(x => x.GetByIdAsync(applicationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((CreditApplication?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() =>
            _useCase.ExecuteAsync(applicationId, request));
    }

    [Theory]
    [InlineData("InvalidStatus")]
    [InlineData("Pendiente")]
    [InlineData("")]
    public async Task ExecuteAsync_WithInvalidStatus_ThrowsArgumentException(string invalidStatus)
    {
        // Arrange
        var applicationId = Guid.NewGuid();
        var application = new CreditApplication("CUST-12345", 50000m, DateTime.UtcNow);
        var request = new UpdateStatusRequest(invalidStatus);

        _mockAppRepository
            .Setup(x => x.GetByIdAsync(applicationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(application);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _useCase.ExecuteAsync(applicationId, request));
    }

    [Theory]
    [InlineData("Aprobada")]
    [InlineData("Rechazada")]
    [InlineData("EnAnalisis")]
    public async Task ExecuteAsync_WithValidStatuses_UpdatesSuccessfully(string status)
    {
        // Arrange
        var applicationId = Guid.NewGuid();
        var application = new CreditApplication("CUST-12345", 50000m, DateTime.UtcNow);
        var request = new UpdateStatusRequest(status);

        _mockAppRepository
            .Setup(x => x.GetByIdAsync(applicationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(application);

        // Act
        var response = await _useCase.ExecuteAsync(applicationId, request);

        // Assert
        Assert.Equal(status, response.NewStatus);
    }
}