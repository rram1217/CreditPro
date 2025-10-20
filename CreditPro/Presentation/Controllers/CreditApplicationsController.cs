using CreditPro.Application.DTOs;
using CreditPro.Application.UseCases;
using CreditPro.Domain.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace CreditPro.Presentation.Controllers;

[ApiController]
[Route("api/credit-applications")]
public class CreditApplicationsController : ControllerBase
{
    private readonly CreateCreditApplicationUseCase _createUseCase;
    private readonly UpdateApplicationStatusUseCase _updateStatusUseCase;
    private readonly GetApplicationWithHistoryUseCase _getWithHistoryUseCase;
    private readonly ILogger<CreditApplicationsController> _logger;

    public CreditApplicationsController(
        CreateCreditApplicationUseCase createUseCase,
        UpdateApplicationStatusUseCase updateStatusUseCase,
        GetApplicationWithHistoryUseCase getWithHistoryUseCase,
        ILogger<CreditApplicationsController> logger)
    {
        _createUseCase = createUseCase;
        _updateStatusUseCase = updateStatusUseCase;
        _getWithHistoryUseCase = getWithHistoryUseCase;
        _logger = logger;
    }

    [HttpPost]
    [ProducesResponseType(typeof(CreateCreditApplicationResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CreateCreditApplicationResponse>> CreateApplication(
        [FromBody] CreateCreditApplicationRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Creating credit application for customer {CustomerId}", request.CustomerId);

            var response = await _createUseCase.ExecuteAsync(request, cancellationToken);

            _logger.LogInformation("Credit application created successfully with ID {ApplicationId}", response.ApplicationId);

            return CreatedAtAction(
                nameof(GetApplication),
                new { applicationId = response.ApplicationId },
                response);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Validation error creating credit application");
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating credit application");
            return StatusCode(500, new { error = "An error occurred while processing the request" });
        }
    }

    [HttpPatch("{applicationId}/status")]
    [ProducesResponseType(typeof(UpdateStatusResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<UpdateStatusResponse>> UpdateStatus(
        Guid applicationId,
        [FromBody] UpdateStatusRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Updating status for application {ApplicationId} to {NewStatus}",
                applicationId, request.NewStatus);

            var response = await _updateStatusUseCase.ExecuteAsync(applicationId, request, cancellationToken);

            _logger.LogInformation("Status updated successfully for application {ApplicationId}", applicationId);

            return Ok(response);
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning(ex, "Application not found: {ApplicationId}", applicationId);
            return NotFound(new { error = ex.Message });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Validation error updating status");
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating application status");
            return StatusCode(500, new { error = "An error occurred while processing the request" });
        }
    }

    [HttpGet("{applicationId}")]
    [ProducesResponseType(typeof(GetApplicationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<GetApplicationResponse>> GetApplication(
        Guid applicationId,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Getting application {ApplicationId} with history", applicationId);

            var response = await _getWithHistoryUseCase.ExecuteAsync(applicationId, cancellationToken);

            return Ok(response);
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning(ex, "Application not found: {ApplicationId}", applicationId);
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting application");
            return StatusCode(500, new { error = "An error occurred while processing the request" });
        }
    }
}