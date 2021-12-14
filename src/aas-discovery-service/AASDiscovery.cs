using System;
using System.Runtime.Serialization;
using AAS.API.Interfaces;
using AAS.API.Services;

namespace AAS.API.Discovery
{
    public interface AASDiscovery : BasicDiscovery
    {
    }

    public class AASDiscoveryException : AASServiceException
    {
        //
        // Summary:
        //     Initializes a new instance of the Azure.RequestFailedException class with a specified
        //     error message.
        //
        // Parameters:
        //   message:
        //     The message that describes the error.
        public AASDiscoveryException(string message) : base(message)
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
        public AASDiscoveryException(string message, Exception? innerException) : base(message, innerException)
        {
        }
    }
}
