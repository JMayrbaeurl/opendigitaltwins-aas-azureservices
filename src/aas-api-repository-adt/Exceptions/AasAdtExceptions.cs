namespace AAS.API.Repository.Adt
{
    class AdtException : Exception
    {
        public AdtException(string message) : base(message)
        {
        }

        public AdtException(string message, Exception? innerException) : base(message, innerException)
        {
        }
    }

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
