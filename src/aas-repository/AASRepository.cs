using AAS.API.Models;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace AAS.API.Repository
{
    public interface AASRepository
    {
        public Task<List<AssetAdministrationShell>> GetAllAdministrationShells();
    }

    public class AASRepositoryException : Exception, ISerializable
    {
        //
        // Summary:
        //     Initializes a new instance of the Azure.RequestFailedException class with a specified
        //     error message.
        //
        // Parameters:
        //   message:
        //     The message that describes the error.
        public AASRepositoryException(string message) : base(message)
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
        public AASRepositoryException(string message, Exception? innerException) : base(message, innerException)
        {
        }
    }
}
