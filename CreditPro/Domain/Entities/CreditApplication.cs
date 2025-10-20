using CreditPro.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CreditPro.Domain.Entities
{
    public class CreditApplication
    {
        public Guid ApplicationId { get; private set; }
        public string CustomerId { get; private set; }
        public decimal CreditAmount { get; private set; }
        public DateTime ApplicationDate { get; private set; }
        public CreditApplicationStatus Status { get; private set; }
        public string? CollateralDescription { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime UpdatedAt { get; private set; }

        private CreditApplication() { } // For EF Core

        public CreditApplication(
            string customerId,
            decimal creditAmount,
            DateTime applicationDate,
            string? collateralDescription = null)
        {
            if (string.IsNullOrWhiteSpace(customerId))
                throw new ArgumentException("Customer ID cannot be empty", nameof(customerId));

            if (creditAmount <= 1000 || creditAmount >= 150000)
                throw new ArgumentException("Credit amount must be greater than 1,000 and less than 150,000", nameof(creditAmount));

            ApplicationId = Guid.NewGuid();
            CustomerId = customerId;
            CreditAmount = creditAmount;
            ApplicationDate = applicationDate;
            Status = CreditApplicationStatus.Recibida;
            CollateralDescription = collateralDescription;
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }

        public void UpdateStatus(CreditApplicationStatus newStatus)
        {
            if (!Enum.IsDefined(typeof(CreditApplicationStatus), newStatus))
                throw new ArgumentException("Invalid status", nameof(newStatus));

            Status = newStatus;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}