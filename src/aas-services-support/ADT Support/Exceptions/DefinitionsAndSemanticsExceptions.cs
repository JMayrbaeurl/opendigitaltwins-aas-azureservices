using System;

namespace AAS.ADT.Exceptions
{
    public class ConfigurationNotSetExceptions : Exception
    {
        public ConfigurationNotSetExceptions(string message) : base(message)
        {
        }

        public ConfigurationNotSetExceptions(string message, Exception? innerException) : base(message, innerException)
        {
        }

    }
}
