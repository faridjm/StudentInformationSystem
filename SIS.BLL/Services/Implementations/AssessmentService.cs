using SIS.BLL.Services.DTOs.Assessment;
using SIS.BLL.Services.Interfaces;
using SIS.DAL.UnitOfWork;
using SIS.Entities.Models;
using static SIS.BLL.Exceptions.AppException;

namespace SIS.BLL.Services.Implementations
{
    public class AssessmentService : IAssessmentService
    {
        private readonly IUnitOfWork _unitOfWork;

        public AssessmentService(IUnitOfWork unitOfWork) { _unitOfWork = unitOfWork; }

        public async Task<IEnumerable<AssessmentItemDto>> GetByGroupAsync(int groupId)
        {
            var items = await _unitOfWork.AssessmentItems.GetByGroupAsync(groupId);

            // Qrupdakı real-time aktiv enrolled tələbə sayını bir dəfə hesabla
            var enrollments = await _unitOfWork.Enrollments.GetByGroupAsync(groupId);
            var totalStudents = enrollments
                .Count(e => !e.IsDeleted &&
                            e.Status == SIS.Entities.Enums.EnrollmentStatus.Active &&
                            e.Student != null && !e.Student.IsDeleted);

            return items.Select(a =>
            {
                var dto = MapToDto(a);
                dto.TotalStudentCount = totalStudents;
                return dto;
            });
        }

        public async Task<AssessmentItemDto?> GetByIdAsync(int id)
        {
            // Submissions include olmadan GradedCount yanlış olur — WithSubmissions istifadə edirik
            var item = await _unitOfWork.AssessmentItems.GetWithSubmissionsAsync(id);
            if (item == null) return null;

            var dto = MapToDto(item);
            // Total student count
            var enrollments = await _unitOfWork.Enrollments.GetByGroupAsync(item.GroupId);
            dto.TotalStudentCount = enrollments
                .Count(e => !e.IsDeleted &&
                            e.Status == SIS.Entities.Enums.EnrollmentStatus.Active &&
                            e.Student != null && !e.Student.IsDeleted);
            return dto;
        }

        public async Task<AssessmentItemDto> CreateAsync(CreateAssessmentItemDto dto)
        {
            var item = new AssessmentItem
            {
                GroupId = dto.GroupId, Type = dto.Type, Title = dto.Title,
                Date = dto.Date, MaxScore = dto.MaxScore, WeightPercent = dto.WeightPercent,
                Description = dto.Description, IsVisible = dto.IsVisible,
                CreatedAt = DateTime.UtcNow
            };
            await _unitOfWork.AssessmentItems.AddAsync(item);
            await _unitOfWork.SaveAsync();

            // Artıq qeydiyyatlı tələbələr üçün submission yarat
            var enrollments = await _unitOfWork.Enrollments.GetByGroupAsync(dto.GroupId);
            foreach (var enrollment in enrollments.Where(e => !e.IsDeleted))
            {
                await _unitOfWork.AssessmentSubmissions.AddAsync(new AssessmentSubmission
                {
                    AssessmentItemId = item.Id,
                    EnrollmentId = enrollment.Id,
                    IsGraded = false,
                    CreatedAt = DateTime.UtcNow
                });
            }
            if (enrollments.Any(e => !e.IsDeleted))
                await _unitOfWork.SaveAsync();

            return MapToDto(item);
        }

        public async Task UpdateAsync(UpdateAssessmentItemDto dto)
        {
            var item = await _unitOfWork.AssessmentItems.GetByIdAsync(dto.Id)
                ?? throw new NotFoundException("Tapılmadı");
            item.Title = dto.Title; item.Date = dto.Date;
            item.MaxScore = dto.MaxScore; item.WeightPercent = dto.WeightPercent;
            item.Description = dto.Description; item.IsVisible = dto.IsVisible;
            _unitOfWork.AssessmentItems.Update(item);
            await _unitOfWork.SaveAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var item = await _unitOfWork.AssessmentItems.GetByIdAsync(id)
                ?? throw new NotFoundException("Tapılmadı");
            _unitOfWork.AssessmentItems.SoftDelete(item);
            await _unitOfWork.SaveAsync();
        }

        public async Task<IEnumerable<AssessmentSubmissionDto>> GetSubmissionsAsync(int assessmentItemId)
        {
            var item = await _unitOfWork.AssessmentItems.GetWithSubmissionsAsync(assessmentItemId);
            if (item == null) return [];
            return item.Submissions.Select(s => MapSubmissionToDto(s, item.MaxScore));
        }

        public async Task<IEnumerable<AssessmentSubmissionDto>> GetMySubmissionsAsync(int enrollmentId)
        {
            var subs = await _unitOfWork.AssessmentSubmissions.GetByEnrollmentAsync(enrollmentId);
            return subs.Select(s => MapSubmissionToDto(s, s.AssessmentItem?.MaxScore ?? 0));
        }

        public async Task GradeAsync(GradeAssessmentDto dto)
        {
            var sub = await _unitOfWork.AssessmentSubmissions.GetByIdAsync(dto.SubmissionId)
                ?? throw new NotFoundException("Tapılmadı");
            var item = await _unitOfWork.AssessmentItems.GetByIdAsync(sub.AssessmentItemId)
                ?? throw new NotFoundException("Assessment tapılmadı");

            if (dto.Score > item.MaxScore)
                throw new BadRequestException($"Qiymət maksimum ({item.MaxScore}) keçə bilməz");

            sub.Score = dto.Score;
            sub.Feedback = dto.Feedback;
            sub.IsGraded = true;
            sub.GradedAt = DateTime.UtcNow;
            _unitOfWork.AssessmentSubmissions.Update(sub);
            await _unitOfWork.SaveAsync();
        }

        private static AssessmentItemDto MapToDto(AssessmentItem a) => new()
        {
            Id = a.Id, GroupId = a.GroupId, Type = a.Type, Title = a.Title,
            Date = a.Date, MaxScore = a.MaxScore, WeightPercent = a.WeightPercent,
            Description = a.Description, IsVisible = a.IsVisible,
            SubmissionCount = a.Submissions?.Count ?? 0,
            GradedCount = a.Submissions?.Count(s => s.IsGraded) ?? 0
        };

        private static AssessmentSubmissionDto MapSubmissionToDto(AssessmentSubmission s, decimal maxScore) => new()
        {
            Id = s.Id, AssessmentItemId = s.AssessmentItemId,
            AssessmentTitle = s.AssessmentItem?.Title ?? "",
            EnrollmentId = s.EnrollmentId,
            StudentName = s.Enrollment?.Student?.AppUser?.FullName ?? "",
            StudentNumber = s.Enrollment?.Student?.StudentNumber ?? "",
            Score = s.Score, MaxScore = maxScore,
            Feedback = s.Feedback, IsGraded = s.IsGraded, GradedAt = s.GradedAt
        };
    }
}
