using System.Net;

namespace SIS.BLL.Exceptions
{
    public abstract partial class AppException
    {
        public class BadRequestException : AppException
        {
            public BadRequestException(string message) : base(message, HttpStatusCode.BadRequest)
            {
            }
        }

    }
}
