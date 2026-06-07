using SIS.BLL.Services.DTOs.Common;
using SIS.Entities.Enums;

namespace SIS.BLL.Services.DTOs.PublicAdmin
{
    // ============ COURSE PROFILE ============
    public class PublicCourseProfileRowDto
    {
        public int Id { get; set; }                  // profile id (0 if not yet created)
        public int CourseId { get; set; }
        public string CourseName { get; set; } = null!;
        public string? DepartmentName { get; set; }
        public bool CourseIsActive { get; set; }
        public string? PublicTitle { get; set; }
        public string? CoverImageUrl { get; set; }
        public bool IsVisible { get; set; }
        public bool IsFeatured { get; set; }
        public int SortOrder { get; set; }
    }
    public class PublicCourseProfileEditDto
    {
        public int CourseId { get; set; }
        public string? PublicTitle { get; set; }
        public string? PublicShortDescription { get; set; }
        public string? PublicDescription { get; set; }
        public string? CoverImageUrl { get; set; }
        public string? WhatYouWillLearn { get; set; }
        public string? CareerOutcomes { get; set; }
        public bool IsVisible { get; set; }
        public bool IsFeatured { get; set; }
        public int SortOrder { get; set; }
    }

    // ============ TEACHER PROFILE ============
    public class PublicTeacherProfileRowDto
    {
        public int Id { get; set; }
        public int TeacherId { get; set; }
        public string TeacherName { get; set; } = null!;
        public string? TeacherEmail { get; set; }
        public bool TeacherIsActive { get; set; }
        public string? DisplayName { get; set; }
        public string? PublicTitle { get; set; }
        public string? PhotoUrl { get; set; }
        public bool IsVisible { get; set; }
        public bool IsFeatured { get; set; }
        public int SortOrder { get; set; }
    }
    public class PublicTeacherProfileEditDto
    {
        public int TeacherId { get; set; }
        public string? DisplayName { get; set; }
        public string? PublicTitle { get; set; }
        public string? Bio { get; set; }
        public string? Specialization { get; set; }
        public string? PhotoUrl { get; set; }
        public string? LinkedInUrl { get; set; }
        public bool IsVisible { get; set; }
        public bool IsFeatured { get; set; }
        public int SortOrder { get; set; }
    }

    // ============ FACILITY ============
    public class PublicFacilityDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public string? Type { get; set; }
        public int? Capacity { get; set; }
        public string? ImageUrl { get; set; }
        public bool IsVisible { get; set; }
        public int SortOrder { get; set; }
    }

    // ============ GRADUATE STORY ============
    public class PublicGraduateStoryDto
    {
        public int Id { get; set; }
        public string FullName { get; set; } = null!;
        public string? PhotoUrl { get; set; }
        public string? GraduatedCourse { get; set; }
        public int? GraduationYear { get; set; }
        public string? CurrentPosition { get; set; }
        public string? Story { get; set; }
        public bool IsVisible { get; set; }
        public bool IsFeatured { get; set; }
        public int SortOrder { get; set; }
    }

    // ============ FAQ ============
    public class PublicFaqDto
    {
        public int Id { get; set; }
        public string Question { get; set; } = null!;
        public string Answer { get; set; } = null!;
        public string? Category { get; set; }
        public bool IsVisible { get; set; }
        public int SortOrder { get; set; }
    }

    // ============ CONTACT MESSAGE ============
    public class PublicContactMessageListDto
    {
        public int Id { get; set; }
        public string FullName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string? Phone { get; set; }
        public string? Subject { get; set; }
        public ContactMessageStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
    }
    public class PublicContactMessageDetailDto : PublicContactMessageListDto
    {
        public string Message { get; set; } = null!;
        public string? AdminNote { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
    public class CreatePublicContactMessageDto
    {
        public string FullName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string? Phone { get; set; }
        public string? Subject { get; set; }
        public string Message { get; set; } = null!;
    }
    public class UpdateContactStatusDto { public ContactMessageStatus Status { get; set; } }
    public class UpdateContactNoteDto   { public string? AdminNote { get; set; } }

    // ============ DASHBOARD STATS ============
    public class PublicAdminStatsDto
    {
        public int CoursesPublished { get; set; }
        public int CoursesTotal { get; set; }
        public int TeachersPublished { get; set; }
        public int TeachersTotal { get; set; }
        public int FacilitiesCount { get; set; }
        public int GraduatesCount { get; set; }
        public int FaqCount { get; set; }
        public int NewApplications { get; set; }
        public int NewContactMessages { get; set; }
    }
}
