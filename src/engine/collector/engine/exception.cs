using System;
using System.Runtime.Serialization;

namespace OpenETaxBill.Engine.Collector
{
    /// <summary>
    /// 
    /// </summary>
    public class CollectException : Exception
    {
        /// <summary>
        /// 
        /// </summary>
        public CollectException()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        public CollectException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="inner"></param>
        public CollectException(string message, Exception inner)
            : base(message, inner)
        {
        }
 
        /// <summary>
        /// 
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        protected CollectException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
            : base(info, context)
        {
        }
    }
}