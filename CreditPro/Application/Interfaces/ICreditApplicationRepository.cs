using CreditPro.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace CreditPro.Application.Interfaces
{
    public interface ICreditApplicationRepository
    {
        Task<CreditApplication> CreateAsync(CreditApplication application, CancellationToken cancellationToken = default);
        Task<CreditApplication?> GetByIdAsync(Guid applicationId, CancellationToken cancellationToken = default);
        Task UpdateAsync(CreditApplication application, CancellationToken cancellationToken = default);
    }
}