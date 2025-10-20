using CreditPro.Application.Interfaces;
using CreditPro.Domain.Exceptions;
using CreditPro.Application.DTOs;

namespace CreditPro.Application.UseCases
{
    public class GetApplicationWithHistoryUseCase
    {
        private readonly ICreditApplicationRepository _applicationRepository;
        private readonly IAuditEventRepository _auditRepository;

        public GetApplicationWithHistoryUseCase(
            ICreditApplicationRepository applicationRepository,
            IAuditEventRepository auditRepository)
        {
            _applicationRepository = applicationRepository;
            _auditRepository = auditRepository;
        }

        public async Task<GetApplicationResponse> ExecuteAsync(
            Guid applicationId,
            CancellationToken cancellationToken = default)
        {
            // Get application from PostgreSQL
            var application = await _applicationRepository.GetByIdAsync(applicationId, cancellationToken)
                ?? throw new NotFoundException($"Application with ID {applicationId} not found");

            // Get audit history from DynamoDB
            var auditHistory = await _auditRepository.GetEventsByApplicationIdAsync(
                applicationId,
                cancellationToken
            );

            var applicationDto = new CreditApplicationDto(
                application.ApplicationId,
                application.CustomerId,
                application.CreditAmount,
                application.ApplicationDate,
                application.Status.ToString(),
                application.CollateralDescription
            );

            return new GetApplicationResponse(applicationDto, auditHistory);
        }
    }
}