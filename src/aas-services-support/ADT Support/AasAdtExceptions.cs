using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AAS_Services_Support.ADT_Support
{
    public class NoSemanticIdFound : Exception
    {
        public NoSemanticIdFound(string message) : base(message)
        {
        }

        public NoSemanticIdFound(string message, Exception? innerException) : base(message, innerException)
        {
        }

    }
    public class AdtModelNotSupported : Exception
    {
        public AdtModelNotSupported(string message) : base(message)
        {
        }

        public AdtModelNotSupported(string message, Exception? innerException) : base(message, innerException)
        {
        }

    }
}
