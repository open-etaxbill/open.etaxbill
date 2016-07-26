using System;

namespace OpenETaxBill.Engine.Reporter
{
    public class ReporterException : Exception
    {
        public ReporterException()
        {
        }

        public ReporterException(string message)
            : base(message)
        {
        }

        public ReporterException(string message, Exception inner)
            : base(message, inner)
        {
        }
        protected ReporterException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
            : base(info, context)
        {
        }
    }
}