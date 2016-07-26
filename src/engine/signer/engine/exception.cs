using System;

namespace OpenETaxBill.Engine.Signer
{
    public class SignerException : Exception
    {
        public SignerException()
        {
        }

        public SignerException(string message)
            : base(message)
        {
        }

        public SignerException(string message, Exception inner)
            : base(message, inner)
        {
        }
        protected SignerException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
            : base(info, context)
        {            
        }
    }
}