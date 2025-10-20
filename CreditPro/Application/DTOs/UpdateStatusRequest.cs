using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CreditPro.Application.DTOs
{
    public record UpdateStatusRequest(
    string NewStatus,
    string? Notes = null
);
}