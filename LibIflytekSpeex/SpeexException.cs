// See https://aka.ms/new-console-template for more information

using System.Runtime.Serialization;

namespace LibIflytekSpeex
{
    public class SpeexException : Exception
    {
        public SpeexException()
        {
        }

        public SpeexException(string? message) : base(message)
        {
        }

        public SpeexException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        protected SpeexException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}