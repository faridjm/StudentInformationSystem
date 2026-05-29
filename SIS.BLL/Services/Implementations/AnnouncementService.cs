using AutoMapper;
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
        private readonly IMapper _mapper;
        private readonly IEmailService _emailService;

        public AnnouncementService(IUnitOfWork unitOfWork, IMapper mapper, IEmailService emailService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _emailService = emailService;
        }

        public async Task<IEnumerable<AnnouncementDto>> GetGlobalAnnouncementsAsync()
        {
            var announcements = await _unitOfWork.Announcements.GetGlobalAnnouncementsAsync();
            return _mapper.Map<IEnumerable<AnnouncementDto>>(announcements);
        }

        public async Task<IEnumerable<AnnouncementDto>> GetCourseAnnouncementsAsync(int courseOfferingId)
        {
            var announcements = await _unitOfWork.Announcements.GetCourseAnnouncementsAsync(courseOfferingId);
            return _mapper.Map<IEnumerable<AnnouncementDto>>(announcements);
        }

        public async Task<IEnumerable<AnnouncementDto>> GetAnnouncementsForStudentAsync(int studentId)
        {
            var announcements = await _unitOfWork.Announcements.GetAnnouncementsForStudentAsync(studentId);
            return _mapper.Map<IEnumerable<AnnouncementDto>>(announcements);
        }

        public async Task CreateAnnouncementAsync(CreateAnnouncementDto dto, string authorId)
        {
            var announcement = _mapper.Map<Announcement>(dto);
            announcement.AuthorId = authorId;
            announcement.CreatedAt = DateTime.UtcNow;
            await _unitOfWork.Announcements.AddAsync(announcement);
            await _unitOfWork.SaveAsync();

            // Email göndər
            await SendAnnouncementEmailsAsync(dto.TargetType, dto.Title, dto.Content);
        }

        private async Task SendAnnouncementEmailsAsync(AnnouncementTarget target, string title, string content)
        {
            var tasks = new List<Task>();

            if (target == AnnouncementTarget.All || target == AnnouncementTarget.Students)
            {
                var students = await _unitOfWork.Students.GetStudentsWithGPAAsync();
                foreach (var s in students)
                {
                    var email = s.AppUser?.Email;
                    var name = s.AppUser?.FullName ?? "";
                    if (!string.IsNullOrEmpty(email))
                        tasks.Add(_emailService.SendAnnouncementAsync(email, name, title, content));
                }
            }

            if (target == AnnouncementTarget.All || target == AnnouncementTarget.Teachers)
            {
                var teachers = await _unitOfWork.Teachers.GetAllTeachersWithDetailsAsync();
                foreach (var t in teachers)
                {
                    var email = t.AppUser?.Email;
                    var name = t.AppUser?.FullName ?? "";
                    if (!string.IsNullOrEmpty(email))
                        tasks.Add(_emailService.SendAnnouncementAsync(email, name, title, content));
                }
            }

            await Task.WhenAll(tasks);
        }

        public async Task DeleteAnnouncementAsync(int id)
        {
            var announcement = await _unitOfWork.Announcements.GetByIdAsync(id);
            if (announcement == null)
                throw new NotFoundException("Elan tapılmadı");

            _unitOfWork.Announcements.SoftDelete(announcement);
            await _unitOfWork.SaveAsync();
        }
    }
}
