using CreditPro.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace CreditPro.Application.Interfaces
{
    public interface IAuditEventRepository
    {
        Task SaveEventAsync(AuditEvent auditEvent, CancellationToken cancellationToken = default);
        Task<List<AuditEvent>> GetEventsByApplicationIdAsync(Guid applicationId, CancellationToken cancellationToken = default);
    }
}