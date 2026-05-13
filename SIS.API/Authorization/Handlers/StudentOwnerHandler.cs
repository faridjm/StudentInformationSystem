using Microsoft.AspNetCore.Authorization;
using SIS.API.Authorization.Requirements;
using SIS.BLL.Services.Interfaces;
using SIS.Common.Constants;
using System.Security.Claims;

namespace SIS.API.Authorization.Handlers
{
    public class StudentOwnerHandler : AuthorizationHandler<StudentOwnerRequirement, int>
    {
        private readonly IStudentService _studentService;

        public StudentOwnerHandler(IStudentService studentService)
        {
            _studentService = studentService;
        }

        protected override async Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            StudentOwnerRequirement requirement,
            int studentId)
        {
            var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                context.Fail();
                return;
            }

            // Admin, SuperAdmin, Moderator, Teacher — hamısına icazə
            if (context.User.IsInRole(RoleConstants.Admin) ||
                context.User.IsInRole(RoleConstants.SuperAdmin) ||
                context.User.IsInRole(RoleConstants.Moderator) ||
                context.User.IsInRole(RoleConstants.Teacher))
            {
                context.Succeed(requirement);
                return;
            }

            // Student — yalnız öz record-u
            if (context.User.IsInRole(RoleConstants.Student))
            {
                var student = await _studentService.GetStudentByUserIdAsync(userId);
                if (student != null && student.Id == studentId)
                {
                    context.Succeed(requirement);
                    return;
                }
            }

            context.Fail();
        }
    }


}
