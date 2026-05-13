using Microsoft.AspNetCore.Authorization;
using SIS.API.Authorization.Requirements;
using SIS.BLL.Services.Interfaces;
using SIS.Common.Constants;
using System.Security.Claims;

namespace SIS.API.Authorization.Handlers
{
    public class GradeAuthHandler : AuthorizationHandler<GradeAuthRequirement, int>
    {
        private readonly IEnrollmentService _enrollmentService;
        private readonly ITeacherService _teacherService;

        public GradeAuthHandler(
            IEnrollmentService enrollmentService,
            ITeacherService teacherService)
        {
            _enrollmentService = enrollmentService;
            _teacherService = teacherService;
        }

        protected override async Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            GradeAuthRequirement requirement,
            int enrollmentId)
        {
            var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                context.Fail();
                return;
            }

            // Admin, SuperAdmin — hamısına icazə
            if (context.User.IsInRole(RoleConstants.Admin) ||
                context.User.IsInRole(RoleConstants.SuperAdmin))
            {
                context.Succeed(requirement);
                return;
            }

            // Teacher — yalnız öz dərsi
            if (context.User.IsInRole(RoleConstants.Teacher))
            {
                var teacher = await _teacherService.GetTeacherByUserIdAsync(userId);
                if (teacher == null)
                {
                    context.Fail();
                    return;
                }

                var enrollment = await _enrollmentService.GetEnrollmentByIdAsync(enrollmentId);
                if (enrollment != null && enrollment.TeacherName == teacher.FullName)
                {
                    context.Succeed(requirement);
                    return;
                }
            }

            context.Fail();
        }
    }


}
