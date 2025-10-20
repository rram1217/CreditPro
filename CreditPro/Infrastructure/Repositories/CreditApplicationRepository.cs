using CreditPro.Application.Interfaces;
using CreditPro.Domain.Entities;
using CreditPro.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace CreditPro.Infrastructure.Repositories
{
    public class CreditApplicationRepository : ICreditApplicationRepository
    {
        private readonly ApplicationDbContext _context;

        public CreditApplicationRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<CreditApplication> CreateAsync(
            CreditApplication application,
            CancellationToken cancellationToken = default)
        {
            _context.CreditApplications.Add(application);
            await _context.SaveChangesAsync(cancellationToken);
            return application;
        }

        public async Task<CreditApplication?> GetByIdAsync(
            Guid applicationId,
            CancellationToken cancellationToken = default)
        {
            return await _context.CreditApplications
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.ApplicationId == applicationId, cancellationToken);
        }

        public async Task UpdateAsync(
            CreditApplication application,
            CancellationToken cancellationToken = default)
        {
            _context.CreditApplications.Update(application);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}