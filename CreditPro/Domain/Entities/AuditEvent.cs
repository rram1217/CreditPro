using CreditPro.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CreditPro.Domain.Entities
{
    public class AuditEvent
    {
        public string ApplicationId { get; set; } = string.Empty;
        public string Timestamp { get; set; } = string.Empty;
        public string EventType { get; set; } = string.Empty;
        public string NewState { get; set; } = string.Empty;
        public Dictionary<string, object> Details { get; set; } = new();

        public static AuditEvent CreateCreationEvent(
            Guid applicationId,
            string customerId,
            decimal creditAmount)
        {
            return new AuditEvent
            {
                ApplicationId = applicationId.ToString(),
                Timestamp = DateTime.UtcNow.ToString("o"),
                EventType = "Creación",
                NewState = CreditApplicationStatus.Recibida.ToString(),
                Details = new Dictionary<string, object>
            {
                { "creditAmount", creditAmount },
                { "customerId", customerId }
            }
            };
        }

        public static AuditEvent CreateStatusUpdateEvent(
            Guid applicationId,
            string previousStatus,
            string newStatus,
            string? notes = null)
        {
            var details = new Dictionary<string, object>
        {
            { "previousStatus", previousStatus }
        };

            if (!string.IsNullOrWhiteSpace(notes))
            {
                details.Add("notes", notes);
            }

            return new AuditEvent
            {
                ApplicationId = applicationId.ToString(),
                Timestamp = DateTime.UtcNow.ToString("o"),
                EventType = "Actualización de Estado",
                NewState = newStatus,
                Details = details
            };
        }
    }
}