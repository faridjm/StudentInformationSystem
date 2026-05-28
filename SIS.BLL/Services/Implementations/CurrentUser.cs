using Microsoft.AspNetCore.Http;
using SIS.BLL.Services.Interfaces;
using SIS.Common.Constants;
using System.Security.Claims;

namespace SIS.BLL.Services.Implementations
{
    public class CurrentUser : ICurrentUser
    {
        private readonly IHttpContextAccessor _accessor;

        public CurrentUser(IHttpContextAccessor accessor)
        {
            _accessor = accessor;
        }

        public string? UserId =>
            _accessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        public bool IsAuthenticated =>
            _accessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;

        public bool IsAdmin() =>
            _accessor.HttpContext?.User?.IsInRole(RoleConstants.Admin) ?? false;

        public bool IsSuperAdmin() =>
            _accessor.HttpContext?.User?.IsInRole(RoleConstants.SuperAdmin) ?? false;

        public bool IsTeacher() =>
            _accessor.HttpContext?.User?.IsInRole(RoleConstants.Teacher) ?? false;

        public bool IsStudent() =>
            _accessor.HttpContext?.User?.IsInRole(RoleConstants.Student) ?? false;
    }
}
