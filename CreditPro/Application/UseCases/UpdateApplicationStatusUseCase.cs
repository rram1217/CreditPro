using CreditPro.Application.Interfaces;
using CreditPro.Domain.Entities;
using CreditPro.Domain.Enums;
using CreditPro.Domain.Exceptions;
using CreditPro.Application.DTOs;

namespace CreditPro.Application.UseCases
{
    public class UpdateApplicationStatusUseCase
    {
        private readonly ICreditApplicationRepository _applicationRepository;
        private readonly IAuditEventRepository _auditRepository;

        public UpdateApplicationStatusUseCase(
            ICreditApplicationRepository applicationRepository,
            IAuditEventRepository auditRepository)
        {
            _applicationRepository = applicationRepository;
            _auditRepository = auditRepository;
        }

        public async Task<UpdateStatusResponse> ExecuteAsync(
            Guid applicationId,
            UpdateStatusRequest request,
            CancellationToken cancellationToken = default)
        {
            // Get existing application
            var application = await _applicationRepository.GetByIdAsync(applicationId, cancellationToken)
                ?? throw new NotFoundException($"Application with ID {applicationId} not found");

            // Validate new status
            if (!Enum.TryParse<CreditApplicationStatus>(request.NewStatus, out var newStatus))
            {
                throw new ArgumentException($"Invalid status: {request.NewStatus}. Valid values are: Aprobada, Rechazada, EnAnalisis");
            }

            var previousStatus = application.Status.ToString();

            // Update status
            application.UpdateStatus(newStatus);

            // Save to PostgreSQL
            await _applicationRepository.UpdateAsync(application, cancellationToken);

            // Create and save audit event to DynamoDB
            var auditEvent = AuditEvent.CreateStatusUpdateEvent(
                application.ApplicationId,
                previousStatus,
                newStatus.ToString(),
                request.Notes
            );
            await _auditRepository.SaveEventAsync(auditEvent, cancellationToken);

            return new UpdateStatusResponse(
                application.ApplicationId,
                previousStatus,
                newStatus.ToString(),
                application.UpdatedAt
            );
        }
    }
}