using System.Net;

namespace SIS.BLL.Exceptions
{
    public abstract partial class AppException
    {
        public class UnauthorizedException : AppException
        {
            public UnauthorizedException(string message) : base(message, HttpStatusCode.Unauthorized)
            {
            }
        }

    }
}
