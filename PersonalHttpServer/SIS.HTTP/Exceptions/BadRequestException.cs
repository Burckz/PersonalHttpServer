using System;
using System.Collections.Generic;
using System.Text;

namespace SIS.HTTP.Exceptions
{
    public class BadRequestException : Exception
    {
        public override string Message => "The  Request was malformed or contains unsupported elements.";
    }
}
