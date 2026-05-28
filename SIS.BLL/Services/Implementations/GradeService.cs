using AutoMapper;
using SIS.BLL.Helpers;
using SIS.BLL.Services.DTOs.Grade;
using SIS.BLL.Services.Interfaces;
using SIS.Common.Time;
using SIS.DAL.UnitOfWork;
using SIS.Entities.Enums;
using SIS.Entities.Models;
using static SIS.BLL.Exceptions.AppException;

namespace SIS.BLL.Services.Implementations
{
    public class GradeService : IGradeService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IStudentService _studentService;
        private readonly IActivityLogger _activityLogger;
        private readonly IClock _clock;
        private readonly ICurrentUser _currentUser;

        public GradeService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            IStudentService studentService,
            IActivityLogger activityLogger,
            IClock clock,
            ICurrentUser currentUser)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _studentService = studentService;
            _activityLogger = activityLogger;
            _clock = clock;
            _currentUser = currentUser;
        }

        public async Task<IEnumerable<GradeDto>> GetGradesByEnrollmentAsync(int enrollmentId)
        {
            var grades = await _unitOfWork.Grades.GetGradesByEnrollmentAsync(enrollmentId);
            return _mapper.Map<IEnumerable<GradeDto>>(grades);
        }

        public async Task<IEnumerable<GradeDto>> GetGradesByStudentAsync(int studentId)
        {
            var grades = await _unitOfWork.Grades.GetGradesByStudentAsync(studentId);
            return _mapper.Map<IEnumerable<GradeDto>>(grades);
        }

        public async Task AddGradeAsync(CreateGradeDto dto)
        {
            await EnsureGradingAllowedAsync(dto.EnrollmentId);

            var grade = _mapper.Map<Grade>(dto);
            await _unitOfWork.Grades.AddAsync(grade);
            await _unitOfWork.SaveAsync();

            var enrollment = await _unitOfWork.Enrollments.GetByIdAsync(dto.EnrollmentId);
            if (enrollment != null)
                await _studentService.UpdateGPAAsync(enrollment.StudentId);

            await _activityLogger.LogAsync("Create", "Grade", grade.Id.ToString(),
                null, new { dto.EnrollmentId, dto.Type, dto.Score });
        }

        public async Task UpdateGradeAsync(UpdateGradeDto dto)
        {
            var grade = await _unitOfWork.Grades.GetByIdAsync(dto.Id);
            if (grade == null)
                throw new NotFoundException("Qiymət tapılmadı");

            await EnsureGradingAllowedAsync(grade.EnrollmentId);

            var oldValue = new { grade.Score };

            _mapper.Map(dto, grade);
            grade.UpdatedAt = _clock.UtcNow;
            _unitOfWork.Grades.Update(grade);
            await _unitOfWork.SaveAsync();

            var enrollment = await _unitOfWork.Enrollments.GetByIdAsync(grade.EnrollmentId);
            if (enrollment != null)
                await _studentService.UpdateGPAAsync(enrollment.StudentId);

            await _activityLogger.LogAsync("Update", "Grade", dto.Id.ToString(),
                oldValue, new { dto.Score });
        }

        public async Task DeleteGradeAsync(int id)
        {
            var grade = await _unitOfWork.Grades.GetByIdAsync(id);
            if (grade == null)
                throw new NotFoundException("Qiymət tapılmadı");

            await EnsureGradingAllowedAsync(grade.EnrollmentId);

            _unitOfWork.Grades.SoftDelete(grade);
            await _unitOfWork.SaveAsync();

            await _activityLogger.LogAsync("Delete", "Grade", id.ToString(),
                new { grade.Score, grade.Type }, null);
        }

        /// <summary>
        /// Qiymət əməliyyatları yalnız Active, ExamPeriod və ya GradeReview fazasında icazəlidir.
        /// Closed fazasında bütün dəyişikliklər kilidlidir.
        /// </summary>
        private async Task EnsureGradingAllowedAsync(int enrollmentId)
        {
            var enrollment = await _unitOfWork.Enrollments.GetEnrollmentWithDetailsAsync(enrollmentId);
            if (enrollment == null)
                throw new NotFoundException("Qeydiyyat tapılmadı");

            if (_currentUser.CanBypassPhaseRules) return;

            TermPhaseGuard.EnsurePhase(_clock, enrollment.CourseOffering.AcademicTerm,
                "Qiymət əməliyyatı",
                TermPhase.Active, TermPhase.ExamPeriod, TermPhase.GradeReview);
        }
    }
}
