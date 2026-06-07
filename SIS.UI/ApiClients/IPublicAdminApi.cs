using Refit;
using SIS.BLL.Services.DTOs.PublicAdmin;
using SIS.Entities.Enums;

namespace SIS.UI.ApiClients
{
    /// <summary>PublicAdmin paneli üçün API client (admin/superadmin protected).</summary>
    public interface IPublicAdminApi
    {
        [Get("/api/publicadmin/stats")]
        Task<PublicAdminStatsDto> StatsAsync();

        // course profiles
        [Get("/api/publicadmin/course-profiles")]
        Task<List<PublicCourseProfileRowDto>> ListCourseProfilesAsync();
        [Get("/api/publicadmin/course-profiles/{courseId}")]
        Task<PublicCourseProfileEditDto> GetCourseProfileAsync(int courseId);
        [Post("/api/publicadmin/course-profiles")]
        Task UpsertCourseProfileAsync([Body] PublicCourseProfileEditDto dto);
        [Post("/api/publicadmin/course-profiles/{courseId}/toggle-visibility")]
        Task ToggleCourseVisibilityAsync(int courseId);

        // teacher profiles
        [Get("/api/publicadmin/teacher-profiles")]
        Task<List<PublicTeacherProfileRowDto>> ListTeacherProfilesAsync();
        [Get("/api/publicadmin/teacher-profiles/{teacherId}")]
        Task<PublicTeacherProfileEditDto> GetTeacherProfileAsync(int teacherId);
        [Post("/api/publicadmin/teacher-profiles")]
        Task UpsertTeacherProfileAsync([Body] PublicTeacherProfileEditDto dto);
        [Post("/api/publicadmin/teacher-profiles/{teacherId}/toggle-visibility")]
        Task ToggleTeacherVisibilityAsync(int teacherId);

        // facilities
        [Get("/api/publicadmin/facilities")]
        Task<List<PublicFacilityDto>> ListFacilitiesAsync([Query] bool onlyVisible = false);
        [Get("/api/publicadmin/facilities/{id}")]
        Task<PublicFacilityDto> GetFacilityAsync(int id);
        [Post("/api/publicadmin/facilities")]
        Task<HttpResponseMessage> CreateFacilityAsync([Body] PublicFacilityDto dto);
        [Put("/api/publicadmin/facilities")]
        Task UpdateFacilityAsync([Body] PublicFacilityDto dto);
        [Delete("/api/publicadmin/facilities/{id}")]
        Task DeleteFacilityAsync(int id);

        // graduates
        [Get("/api/publicadmin/graduates")]
        Task<List<PublicGraduateStoryDto>> ListGraduatesAsync([Query] bool onlyVisible = false);
        [Get("/api/publicadmin/graduates/{id}")]
        Task<PublicGraduateStoryDto> GetGraduateAsync(int id);
        [Post("/api/publicadmin/graduates")]
        Task<HttpResponseMessage> CreateGraduateAsync([Body] PublicGraduateStoryDto dto);
        [Put("/api/publicadmin/graduates")]
        Task UpdateGraduateAsync([Body] PublicGraduateStoryDto dto);
        [Delete("/api/publicadmin/graduates/{id}")]
        Task DeleteGraduateAsync(int id);

        // faqs
        [Get("/api/publicadmin/faqs")]
        Task<List<PublicFaqDto>> ListFaqsAsync([Query] bool onlyVisible = false);
        [Get("/api/publicadmin/faqs/{id}")]
        Task<PublicFaqDto> GetFaqAsync(int id);
        [Post("/api/publicadmin/faqs")]
        Task<HttpResponseMessage> CreateFaqAsync([Body] PublicFaqDto dto);
        [Put("/api/publicadmin/faqs")]
        Task UpdateFaqAsync([Body] PublicFaqDto dto);
        [Delete("/api/publicadmin/faqs/{id}")]
        Task DeleteFaqAsync(int id);

        // contact messages
        [Get("/api/publicadmin/contact-messages")]
        Task<List<PublicContactMessageListDto>> ListContactAsync([Query] ContactMessageStatus? status = null);
        [Get("/api/publicadmin/contact-messages/{id}")]
        Task<PublicContactMessageDetailDto> GetContactAsync(int id);
        [Put("/api/publicadmin/contact-messages/{id}/status")]
        Task SetContactStatusAsync(int id, [Body] UpdateContactStatusDto dto);
        [Put("/api/publicadmin/contact-messages/{id}/note")]
        Task SetContactNoteAsync(int id, [Body] UpdateContactNoteDto dto);
    }

    /// <summary>Public form (anonymous) — Contact mesajı submit.</summary>
    public interface IPublicContactApi
    {
        [Post("/api/public/contact")]
        Task<HttpResponseMessage> SubmitAsync([Body] CreatePublicContactMessageDto dto);
    }
}
