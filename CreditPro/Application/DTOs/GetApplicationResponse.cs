using CreditPro.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CreditPro.Application.DTOs
{

    public record GetApplicationResponse(
        CreditApplicationDto Application,
        List<AuditEvent> AuditHistory
    );

    public record CreditApplicationDto(
        Guid ApplicationId,
        string CustomerId,
        decimal CreditAmount,
        DateTime ApplicationDate,
        string Status,
        string? CollateralDescription
    );
}