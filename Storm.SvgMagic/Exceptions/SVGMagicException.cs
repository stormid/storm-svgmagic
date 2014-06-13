using System;
using System.Net;
using System.Runtime.Serialization;

namespace Storm.SvgMagic.Exceptions
{
    [Serializable]
    public class SvgMagicException : Exception
    {
        public HttpStatusCode StatusCode { get; private set; }

        public SvgMagicException()
        {
            StatusCode = HttpStatusCode.InternalServerError;
        }

        public SvgMagicException(string message)
            : base(message)
        {
        }

        public SvgMagicException(string message, Exception inner)
            : base(message, inner)
        {
        }

        public SvgMagicException(string message, HttpStatusCode statusCode) : base(message)
        {
            StatusCode = statusCode;
        }

        public SvgMagicException(string message, Exception inner, HttpStatusCode statusCode) : base(message, inner)
        {
            StatusCode = statusCode;
        }

        protected SvgMagicException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }
}
