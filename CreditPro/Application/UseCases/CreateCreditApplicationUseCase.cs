using CreditPro.Application.Interfaces;
using CreditPro.Domain.Entities;
using CreditPro.Application.DTOs;

namespace CreditPro.Application.UseCases
{
    public class CreateCreditApplicationUseCase
    {
        private readonly ICreditApplicationRepository _applicationRepository;
        private readonly IAuditEventRepository _auditRepository;

        public CreateCreditApplicationUseCase(
            ICreditApplicationRepository applicationRepository,
            IAuditEventRepository auditRepository)
        {
            _applicationRepository = applicationRepository;
            _auditRepository = auditRepository;
        }

        public async Task<CreateCreditApplicationResponse> ExecuteAsync(
            CreateCreditApplicationRequest request,
            CancellationToken cancellationToken = default)
        {
            // Create domain entity (validations are in the constructor)
            var application = new CreditApplication(
                request.CustomerId,
                request.CreditAmount,
                request.ApplicationDate,
                request.CollateralDescription
            );

            // Save to PostgreSQL
            await _applicationRepository.CreateAsync(application, cancellationToken);

            // Create and save audit event to DynamoDB
            var auditEvent = AuditEvent.CreateCreationEvent(
                application.ApplicationId,
                application.CustomerId,
                application.CreditAmount
            );
            await _auditRepository.SaveEventAsync(auditEvent, cancellationToken);

            return new CreateCreditApplicationResponse(
                application.ApplicationId,
                application.CustomerId,
                application.CreditAmount,
                application.ApplicationDate,
                application.Status.ToString(),
                application.CollateralDescription
            );
        }
    }
}