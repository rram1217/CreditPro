using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CreditPro.Domain.Exceptions
{
    public class NotFoundException : DomainException
    {
        public NotFoundException(string message) : base(message) { }
    }
}