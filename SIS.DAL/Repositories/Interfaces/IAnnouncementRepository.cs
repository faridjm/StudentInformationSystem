using SIS.Entities.Models;

namespace SIS.DAL.Repositories.Interfaces
{
    public interface IAnnouncementRepository : IGenericRepository<Announcement>
    {
        Task<IEnumerable<Announcement>> GetAllWithAuthorsAsync();

        /// <summary>Adminin görmə görüşü — bütün elanlar (Recipients ilə birlikdə).</summary>
        Task<IEnumerable<Announcement>> GetAllWithRecipientsAsync();

        Task<Announcement?> GetWithRecipientsAsync(int announcementId);

        /// <summary>Konkret istifadəçi üçün gələn elanlar (recipient əsasında).</summary>
        Task<IEnumerable<Announcement>> GetForUserAsync(string userId);

        /// <summary>Geriyə uyğunluq — köhnə Student panelindən çağırılan metod.</summary>
        Task<IEnumerable<Announcement>> GetForStudentAsync(int studentId);

        /// <summary>Geriyə uyğunluq — köhnə "qlobal" siyahı.</summary>
        Task<IEnumerable<Announcement>> GetGlobalAnnouncementsAsync();

        Task<IEnumerable<Announcement>> GetByGroupAsync(int groupId);
    }
}
