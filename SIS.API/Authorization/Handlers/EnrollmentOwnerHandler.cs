using Microsoft.AspNetCore.Authorization;
using SIS.API.Authorization.Requirements;
using SIS.BLL.Services.Interfaces;
using SIS.Common.Constants;
using System.Security.Claims;

namespace SIS.API.Authorization.Handlers
{
    public class EnrollmentOwnerHandler : AuthorizationHandler<EnrollmentOwnerRequirement, int>
    {
        private readonly IEnrollmentService _enrollmentService;
        private readonly IStudentService _studentService;
        private readonly ITeacherService _teacherService;

        public EnrollmentOwnerHandler(
            IEnrollmentService enrollmentService,
            IStudentService studentService,
            ITeacherService teacherService)
        {
            _enrollmentService = enrollmentService;
            _studentService = studentService;
            _teacherService = teacherService;
        }

        protected override async Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            EnrollmentOwnerRequirement requirement,
            int enrollmentId)
        {
            var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                context.Fail();
                return;
            }

            // Admin, SuperAdmin, Moderator — hamısına icazə
            if (context.User.IsInRole(RoleConstants.Admin) ||
                context.User.IsInRole(RoleConstants.SuperAdmin) ||
                context.User.IsInRole(RoleConstants.Moderator))
            {
                context.Succeed(requirement);
                return;
            }

            var enrollment = await _enrollmentService.GetByIdAsync(enrollmentId);
            if (enrollment == null)
            {
                context.Fail();
                return;
            }

            // Student — yalnız öz enrollment-i
            if (context.User.IsInRole(RoleConstants.Student))
            {
                var student = await _studentService.GetStudentByUserIdAsync(userId);
                if (student != null && enrollment.StudentName == student.FullName)
                {
                    context.Succeed(requirement);
                    return;
                }
            }

            // Teacher — yalnız öz CourseOffering-inin enrollment-i
            if (context.User.IsInRole(RoleConstants.Teacher))
            {
                var teacher = await _teacherService.GetTeacherByUserIdAsync(userId);
                if (teacher != null && enrollment.TeacherName == teacher.FullName)
                {
                    context.Succeed(requirement);
                    return;
                }
            }

            context.Fail();
        }
    }


}
