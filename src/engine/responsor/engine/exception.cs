using System;

namespace OpenETaxBill.Engine.Responsor
{
    public class ResponseException : Exception
    {
        public ResponseException()
        {
        }

        public ResponseException(string message)
            : base(message)
        {
        }

        public ResponseException(string message, Exception inner)
            : base(message, inner)
        {
        }
        protected ResponseException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
            : base(info, context)
        {
        }
    }
}