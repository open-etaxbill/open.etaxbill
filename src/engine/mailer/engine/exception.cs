using System;

namespace OpenETaxBill.Engine.Mailer
{
    /// <summary>
    /// 
    /// </summary>
    public class MailerException : Exception
    {
        /// <summary>
        /// 
        /// </summary>
        public MailerException()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        public MailerException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="inner"></param>
        public MailerException(string message, Exception inner)
            : base(message, inner)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        protected MailerException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
            : base(info, context)
        {
        }
    }
}