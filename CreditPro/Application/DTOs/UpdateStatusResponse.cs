using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CreditPro.Application.DTOs
{
    public record UpdateStatusResponse(
    Guid ApplicationId,
    string PreviousStatus,
    string NewStatus,
    DateTime UpdatedAt
);
}