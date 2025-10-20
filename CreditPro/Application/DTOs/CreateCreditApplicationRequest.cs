using System;

namespace CreditPro.Application.DTOs
{
    public record CreateCreditApplicationRequest(
    string CustomerId,
    decimal CreditAmount,
    DateTime ApplicationDate,
    string? CollateralDescription = null
);
}