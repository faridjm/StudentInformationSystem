using System.Net;

namespace SIS.BLL.Exceptions
{
    public abstract partial class AppException
    {
        public class NotFoundException : AppException
        {
            public NotFoundException(string message) : base(message, HttpStatusCode.NotFound)
            {
            }
        }

    }
}
