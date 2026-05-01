using AutoMapper;
using SIS.BLL.Services.DTOs.Announcement;
using SIS.BLL.Services.Interfaces;
using SIS.DAL.UnitOfWork;
using SIS.Entities.Models;

namespace SIS.BLL.Services.Implementations
{
    public class AnnouncementService : IAnnouncementService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public AnnouncementService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
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
            await _unitOfWork.Announcements.AddAsync(announcement);
            await _unitOfWork.SaveAsync();
        }

        public async Task DeleteAnnouncementAsync(int id)
        {
            var announcement = await _unitOfWork.Announcements.GetByIdAsync(id);
            if (announcement == null)
                throw new Exception("Elan tapılmadı");

            _unitOfWork.Announcements.SoftDelete(announcement);
            await _unitOfWork.SaveAsync();
        }
    }
}
