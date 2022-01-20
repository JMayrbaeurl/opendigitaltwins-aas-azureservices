using AAS.API.Services;
using System;

namespace AAS.API.Registry
{
    public interface AASRegistry : AAS.API.Interfaces.Registry, AAS.API.Interfaces.SubmodelRegistry
    {
    }
    public class AASRegistryException : AASServiceException
    {
        //
        // Summary:
        //     Initializes a new instance of the Azure.RequestFailedException class with a specified
        //     error message.
        //
        // Parameters:
        //   message:
        //     The message that describes the error.
        public AASRegistryException(string message) : base(message)
        {
        }
        //
        // Summary:
        //     Initializes a new instance of the Azure.RequestFailedException class with a specified
        //     error message, HTTP status code and a reference to the inner exception that is
        //     the cause of this exception.
        //
        // Parameters:
        //   message:
        //     The error message that explains the reason for the exception.
        //
        //   innerException:
        //     The exception that is the cause of the current exception, or a null reference
        //     (Nothing in Visual Basic) if no inner exception is specified.
        public AASRegistryException(string message, Exception? innerException) : base(message, innerException)
        {
        }
    }
}
