using SIS.BLL.Services.DTOs.Announcement;
using SIS.BLL.Services.Interfaces;
using SIS.DAL.UnitOfWork;
using SIS.Entities.Enums;
using SIS.Entities.Models;
using static SIS.BLL.Exceptions.AppException;

namespace SIS.BLL.Services.Implementations
{
    public class AnnouncementService : IAnnouncementService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEmailService _emailService;

        public AnnouncementService(IUnitOfWork unitOfWork, IEmailService emailService)
        {
            _unitOfWork = unitOfWork;
            _emailService = emailService;
        }

        public async Task<IEnumerable<AnnouncementDto>> GetAllAsync()
        {
            var items = await _unitOfWork.Announcements.GetAllWithAuthorsAsync();
            return items.Select(MapToDto);
        }

        public async Task<IEnumerable<AnnouncementDto>> GetGlobalAnnouncementsAsync()
        {
            var items = await _unitOfWork.Announcements.GetGlobalAnnouncementsAsync();
            return items.Select(MapToDto);
        }

        public async Task<IEnumerable<AnnouncementDto>> GetForStudentAsync(int studentId)
        {
            var items = await _unitOfWork.Announcements.GetForStudentAsync(studentId);
            return items.Select(MapToDto);
        }

        public async Task<IEnumerable<AnnouncementDto>> GetAnnouncementsForStudentAsync(int studentId)
            => await GetForStudentAsync(studentId);

        public async Task CreateAnnouncementAsync(CreateAnnouncementDto dto, string authorId)
        {
            var announcement = new Announcement
            {
                Title = dto.Title, Content = dto.Content,
                TargetType = dto.TargetType, AuthorId = authorId,
                AttachmentUrl = dto.AttachmentUrl,
                IsGlobal = dto.GroupId == null,
                GroupId = dto.GroupId,
                CreatedAt = DateTime.UtcNow
            };
            await _unitOfWork.Announcements.AddAsync(announcement);
            await _unitOfWork.SaveAsync();

            await SendAnnouncementEmailsAsync(dto.TargetType, dto.Title, dto.Content);
        }

        public async Task<IEnumerable<AnnouncementDto>> GetByGroupAsync(int groupId)
        {
            var items = await _unitOfWork.Announcements.GetByGroupAsync(groupId);
            return items.Select(MapToDto);
        }

        public async Task DeleteAnnouncementAsync(int id)
        {
            var a = await _unitOfWork.Announcements.GetByIdAsync(id)
                ?? throw new NotFoundException("Elan tapılmadı");
            _unitOfWork.Announcements.SoftDelete(a);
            await _unitOfWork.SaveAsync();
        }

        private async Task SendAnnouncementEmailsAsync(AnnouncementTarget target, string title, string content)
        {
            var tasks = new List<Task>();

            if (target == AnnouncementTarget.All || target == AnnouncementTarget.Students)
            {
                var students = await _unitOfWork.Students.GetAllWithUsersAsync();
                foreach (var s in students.Where(s => s.AppUser?.Email != null))
                    tasks.Add(_emailService.SendAnnouncementAsync(s.AppUser!.Email!, s.AppUser.FullName, title, content));
            }

            if (target == AnnouncementTarget.All || target == AnnouncementTarget.Teachers)
            {
                var teachers = await _unitOfWork.Teachers.GetAllWithUsersAsync();
                foreach (var t in teachers.Where(t => t.AppUser?.Email != null))
                    tasks.Add(_emailService.SendAnnouncementAsync(t.AppUser!.Email!, t.AppUser.FullName, title, content));
            }

            await Task.WhenAll(tasks);
        }

        private static AnnouncementDto MapToDto(Announcement a) => new()
        {
            Id = a.Id, Title = a.Title, Content = a.Content,
            AuthorName = a.Author?.FullName ?? "",
            TargetType = a.TargetType, CreatedAt = a.CreatedAt,
            IsGlobal = a.IsGlobal, AttachmentUrl = a.AttachmentUrl
        };
    }
}
