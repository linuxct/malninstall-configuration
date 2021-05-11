using System;

namespace space.linuxct.malninstall.Configuration.Common.Exceptions
{
    public class InvalidSignatureException : Exception
    {
        public InvalidSignatureException()
        {
        }

        public InvalidSignatureException(string message)
            : base(message)
        {
        }

        public InvalidSignatureException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}