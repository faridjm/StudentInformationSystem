using AutoMapper;
using SIS.BLL.Services.DTOs.AcademicProgram;
using SIS.BLL.Services.DTOs.AcademicTerm;
using SIS.BLL.Services.DTOs.Announcement;
using SIS.BLL.Services.DTOs.Attendance;
using SIS.BLL.Services.DTOs.Course;
using SIS.BLL.Services.DTOs.CourseOffering;
using SIS.BLL.Services.DTOs.Department;
using SIS.BLL.Services.DTOs.Enrollment;
using SIS.BLL.Services.DTOs.Grade;
using SIS.BLL.Services.DTOs.Notification;
using SIS.BLL.Services.DTOs.Student;
using SIS.BLL.Services.DTOs.Teacher;
using SIS.Common.Helpers;
using SIS.Entities.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace SIS.BLL.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            
            CreateMap<Student, StudentListDto>()
                .ForMember(d => d.FullName, o => o.MapFrom(s => s.AppUser.FullName))
                .ForMember(d => d.FinCode, o => o.MapFrom(s => s.AppUser.FinCode))
                .ForMember(d => d.Email, o => o.MapFrom(s => s.AppUser.Email))
                .ForMember(d => d.ProgramName, o => o.MapFrom(s => s.AcademicProgram.Name))
                .ForMember(d => d.IsActive, o => o.MapFrom(s => s.AppUser.IsActive));

            CreateMap<Student, StudentDetailDto>()
                .ForMember(d => d.FullName, o => o.MapFrom(s => s.AppUser.FullName))
                .ForMember(d => d.FinCode, o => o.MapFrom(s => s.AppUser.FinCode))
                .ForMember(d => d.Email, o => o.MapFrom(s => s.AppUser.Email))
                .ForMember(d => d.PhoneNumber, o => o.MapFrom(s => s.AppUser.PhoneNumber))
                .ForMember(d => d.ProfilePhoto, o => o.MapFrom(s => s.AppUser.ProfilePhoto))
                .ForMember(d => d.ProgramName, o => o.MapFrom(s => s.AcademicProgram.Name))
                .ForMember(d => d.DepartmentName, o => o.MapFrom(s => s.AcademicProgram.Department.Name))
                .ForMember(d => d.IsActive, o => o.MapFrom(s => s.AppUser.IsActive));

            CreateMap<CreateStudentDto, Student>();
            CreateMap<UpdateStudentDto, Student>();

            
            CreateMap<Teacher, TeacherListDto>()
                .ForMember(d => d.FullName, o => o.MapFrom(s => s.AppUser.FullName))
                .ForMember(d => d.FinCode, o => o.MapFrom(s => s.AppUser.FinCode))
                .ForMember(d => d.Email, o => o.MapFrom(s => s.AppUser.Email))
                .ForMember(d => d.DepartmentName, o => o.MapFrom(s => s.Department.Name))
                .ForMember(d => d.IsActive, o => o.MapFrom(s => s.AppUser.IsActive));

            CreateMap<Teacher, TeacherDetailDto>()
                .ForMember(d => d.FullName, o => o.MapFrom(s => s.AppUser.FullName))
                .ForMember(d => d.FinCode, o => o.MapFrom(s => s.AppUser.FinCode))
                .ForMember(d => d.Email, o => o.MapFrom(s => s.AppUser.Email))
                .ForMember(d => d.PhoneNumber, o => o.MapFrom(s => s.AppUser.PhoneNumber))
                .ForMember(d => d.ProfilePhoto, o => o.MapFrom(s => s.AppUser.ProfilePhoto))
                .ForMember(d => d.DepartmentName, o => o.MapFrom(s => s.Department.Name))
                .ForMember(d => d.IsActive, o => o.MapFrom(s => s.AppUser.IsActive));

            CreateMap<CreateTeacherDto, Teacher>();
            CreateMap<UpdateTeacherDto, Teacher>();

            
            CreateMap<Course, CourseListDto>()
                .ForMember(d => d.DepartmentName, o => o.MapFrom(s => s.Department.Name));

            CreateMap<Course, CourseDetailDto>()
                .ForMember(d => d.DepartmentName, o => o.MapFrom(s => s.Department.Name));

            CreateMap<CreateCourseDto, Course>();
            CreateMap<UpdateCourseDto, Course>();

            
            CreateMap<CourseOffering, CourseOfferingListDto>()
                .ForMember(d => d.CourseName, o => o.MapFrom(s => s.Course.Name))
                .ForMember(d => d.CourseCode, o => o.MapFrom(s => s.Course.Code))
                .ForMember(d => d.TeacherName, o => o.MapFrom(s => s.Teacher.AppUser.FullName))
                .ForMember(d => d.TermName, o => o.MapFrom(s => s.AcademicTerm.Name))
                .ForMember(d => d.EnrolledCount, o => o.MapFrom(s => s.Enrollments.Count));

            CreateMap<CourseOffering, CourseOfferingDetailDto>()
                .ForMember(d => d.CourseName, o => o.MapFrom(s => s.Course.Name))
                .ForMember(d => d.CourseCode, o => o.MapFrom(s => s.Course.Code))
                .ForMember(d => d.Credits, o => o.MapFrom(s => s.Course.Credits))
                .ForMember(d => d.TeacherName, o => o.MapFrom(s => s.Teacher.AppUser.FullName))
                .ForMember(d => d.TermName, o => o.MapFrom(s => s.AcademicTerm.Name))
                .ForMember(d => d.EnrolledCount, o => o.MapFrom(s => s.Enrollments.Count));

            CreateMap<CreateCourseOfferingDto, CourseOffering>();
            CreateMap<UpdateCourseOfferingDto, CourseOffering>();

            
            CreateMap<Enrollment, EnrollmentListDto>()
                .ForMember(d => d.Id, o => o.MapFrom(s => s.Id))
                .ForMember(d => d.StudentName, o => o.MapFrom(s => s.Student.AppUser.FullName))
                .ForMember(d => d.StudentNumber, o => o.MapFrom(s => s.Student.StudentNumber))
                .ForMember(d => d.CourseName, o => o.MapFrom(s => s.CourseOffering.Course.Name))
                .ForMember(d => d.CourseCode, o => o.MapFrom(s => s.CourseOffering.Course.Code))
                .ForMember(d => d.TeacherName, o => o.MapFrom(s => s.CourseOffering.Teacher.AppUser.FullName))
                .ForMember(d => d.TermName, o => o.MapFrom(s => s.CourseOffering.AcademicTerm.Name));

            CreateMap<CreateEnrollmentDto, Enrollment>();

            CreateMap<Enrollment, EnrollmentDetailDto>()
                .ForMember(d => d.StudentId, o => o.MapFrom(s => s.StudentId))
                .ForMember(d => d.StudentName, o => o.MapFrom(s => s.Student.AppUser.FullName))
                .ForMember(d => d.TeacherId, o => o.MapFrom(s => s.CourseOffering.TeacherId))
                .ForMember(d => d.CourseName, o => o.MapFrom(s => s.CourseOffering.Course.Name))
                .ForMember(d => d.TeacherName, o => o.MapFrom(s => s.CourseOffering.Teacher.AppUser.FullName))
                .ForMember(d => d.TermName, o => o.MapFrom(s => s.CourseOffering.AcademicTerm.Name))
                .ForMember(d => d.AverageScore, o => o.Ignore())
                .ForMember(d => d.AttendancePercentage, o => o.Ignore());


            CreateMap<Grade, GradeDto>()
                .ForMember(d => d.LetterGrade,
                    o => o.MapFrom(s => GpaHelper.ScoreToLetterGrade(s.Score / s.MaxScore * 100)));

            CreateMap<CreateGradeDto, Grade>();
            CreateMap<UpdateGradeDto, Grade>();

            
            CreateMap<AttendanceRecord, AttendanceDto>()
                .ForMember(d => d.StudentName, o => o.MapFrom(s => s.Enrollment.Student.AppUser.FullName))
                .ForMember(d => d.StudentNumber, o => o.MapFrom(s => s.Enrollment.Student.StudentNumber));

            CreateMap<CreateAttendanceDto, AttendanceRecord>();

            
            CreateMap<Announcement, AnnouncementDto>()
                .ForMember(d => d.AuthorName, o => o.MapFrom(s => s.Author.FullName))
                .ForMember(d => d.CourseName,
                    o => o.MapFrom(s => s.CourseOffering != null ? s.CourseOffering.Course.Name : null));

            CreateMap<CreateAnnouncementDto, Announcement>();

            
            CreateMap<Notification, NotificationDto>();

            
            CreateMap<Department, DepartmentDto>()
                .ForMember(d => d.ProgramCount, o => o.MapFrom(s => s.Programs.Count))
                .ForMember(d => d.TeacherCount, o => o.MapFrom(s => s.Teachers.Count));

            CreateMap<CreateDepartmentDto, Department>();
            CreateMap<UpdateDepartmentDto, Department>();

            
            CreateMap<AcademicProgram, AcademicProgramDto>()
                .ForMember(d => d.DepartmentName, o => o.MapFrom(s => s.Department.Name))
                .ForMember(d => d.StudentCount, o => o.MapFrom(s => s.Students.Count));

            CreateMap<CreateAcademicProgramDto, AcademicProgram>();
            CreateMap<UpdateAcademicProgramDto, AcademicProgram>();

            CreateMap<AcademicTerm, AcademicTermDto>();
            CreateMap<CreateAcademicTermDto, AcademicTerm>();
            CreateMap<UpdateAcademicTermDto, AcademicTerm>();
        }
    }
}
