using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CreditPro.Application.DTOs
{
    public record CreateCreditApplicationResponse(
    Guid ApplicationId,
    string CustomerId,
    decimal CreditAmount,
    DateTime ApplicationDate,
    string Status,
    string? CollateralDescription
);
}