using System;
using System.Runtime.Serialization;

namespace FileOperationApp.Models
{
    [Serializable]
    internal class InvalidCommand : Exception
    {
        public InvalidCommand()
        {
        }

        public InvalidCommand(string message) : base(message)
        {
        }

        public InvalidCommand(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected InvalidCommand(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}