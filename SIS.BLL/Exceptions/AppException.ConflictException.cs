using System.Net;

namespace SIS.BLL.Exceptions
{
    public abstract partial class AppException
    {
        public class ConflictException : AppException
        {
            public ConflictException(string message) : base(message, HttpStatusCode.Conflict)
            {
            }
        }

    }
}
