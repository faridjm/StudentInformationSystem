using SIS.BLL.Services.DTOs.PublicAdmin;
using SIS.BLL.Services.DTOs.Common;
using SIS.Entities.Enums;

namespace SIS.BLL.Services.Interfaces
{
    /// <summary>
    /// PublicAdmin panel üçün konsolidasiya olunmuş idarəetmə servisi.
    /// — Course/Teacher profilləri (akademik entity-lərə əlavə, onları dəyişmir)
    /// — Facilities, Graduates, FAQ — public marketing kontenti
    /// — Public Contact Messages
    /// — Dashboard statistikaları
    /// Yalnız Admin/SuperAdmin üçün API-də qorunur.
    /// </summary>
    public interface IPublicAdminService
    {
        // ---------- DASHBOARD ----------
        Task<PublicAdminStatsDto> GetStatsAsync();

        // ---------- COURSE PROFILES ----------
        Task<List<PublicCourseProfileRowDto>> GetCourseProfilesAsync();
        Task<PublicCourseProfileEditDto?> GetCourseProfileAsync(int courseId);
        Task UpsertCourseProfileAsync(PublicCourseProfileEditDto dto);
        Task ToggleCourseVisibilityAsync(int courseId);

        // ---------- TEACHER PROFILES ----------
        Task<List<PublicTeacherProfileRowDto>> GetTeacherProfilesAsync();
        Task<PublicTeacherProfileEditDto?> GetTeacherProfileAsync(int teacherId);
        Task UpsertTeacherProfileAsync(PublicTeacherProfileEditDto dto);
        Task ToggleTeacherVisibilityAsync(int teacherId);

        // ---------- FACILITIES ----------
        Task<List<PublicFacilityDto>> GetFacilitiesAsync(bool onlyVisible = false);
        Task<PublicFacilityDto?> GetFacilityAsync(int id);
        Task<int> CreateFacilityAsync(PublicFacilityDto dto);
        Task UpdateFacilityAsync(PublicFacilityDto dto);
        Task DeleteFacilityAsync(int id);

        // ---------- GRADUATES ----------
        Task<List<PublicGraduateStoryDto>> GetGraduatesAsync(bool onlyVisible = false);
        Task<PublicGraduateStoryDto?> GetGraduateAsync(int id);
        Task<int> CreateGraduateAsync(PublicGraduateStoryDto dto);
        Task UpdateGraduateAsync(PublicGraduateStoryDto dto);
        Task DeleteGraduateAsync(int id);

        // ---------- FAQ ----------
        Task<List<PublicFaqDto>> GetFaqsAsync(bool onlyVisible = false);
        Task<PublicFaqDto?> GetFaqAsync(int id);
        Task<int> CreateFaqAsync(PublicFaqDto dto);
        Task UpdateFaqAsync(PublicFaqDto dto);
        Task DeleteFaqAsync(int id);

        // ---------- CONTACT MESSAGES ----------
        /// <summary>Public form-dan anonim mesaj qəbulu.</summary>
        Task<int> CreateContactMessageAsync(CreatePublicContactMessageDto dto);
        Task<List<PublicContactMessageListDto>> GetContactMessagesAsync(ContactMessageStatus? status = null);
        Task<PublicContactMessageDetailDto?> GetContactMessageAsync(int id);
        Task UpdateContactStatusAsync(int id, UpdateContactStatusDto dto);
        Task UpdateContactNoteAsync(int id, UpdateContactNoteDto dto);
    }
}
