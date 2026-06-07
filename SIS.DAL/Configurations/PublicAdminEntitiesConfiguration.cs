using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SIS.Entities.Models;

namespace SIS.DAL.Configurations
{
    public class PublicCourseProfileConfiguration : IEntityTypeConfiguration<PublicCourseProfile>
    {
        public void Configure(EntityTypeBuilder<PublicCourseProfile> b)
        {
            b.HasKey(x => x.Id);
            b.Property(x => x.PublicTitle).HasMaxLength(200);
            b.Property(x => x.PublicShortDescription).HasMaxLength(500);
            b.Property(x => x.PublicDescription).HasMaxLength(4000);
            b.Property(x => x.CoverImageUrl).HasMaxLength(500);
            b.Property(x => x.WhatYouWillLearn).HasMaxLength(4000);
            b.Property(x => x.CareerOutcomes).HasMaxLength(4000);

            b.HasOne(x => x.Course)
                .WithMany()
                .HasForeignKey(x => x.CourseId)
                .OnDelete(DeleteBehavior.Cascade);

            b.HasIndex(x => x.CourseId).IsUnique();
            b.HasIndex(x => x.IsVisible);
            b.HasIndex(x => x.SortOrder);

            b.HasQueryFilter(x => !x.IsDeleted);
        }
    }

    public class PublicTeacherProfileConfiguration : IEntityTypeConfiguration<PublicTeacherProfile>
    {
        public void Configure(EntityTypeBuilder<PublicTeacherProfile> b)
        {
            b.HasKey(x => x.Id);
            b.Property(x => x.DisplayName).HasMaxLength(150);
            b.Property(x => x.PublicTitle).HasMaxLength(200);
            b.Property(x => x.Bio).HasMaxLength(2000);
            b.Property(x => x.Specialization).HasMaxLength(300);
            b.Property(x => x.PhotoUrl).HasMaxLength(500);
            b.Property(x => x.LinkedInUrl).HasMaxLength(500);

            b.HasOne(x => x.Teacher)
                .WithMany()
                .HasForeignKey(x => x.TeacherId)
                .OnDelete(DeleteBehavior.Cascade);

            b.HasIndex(x => x.TeacherId).IsUnique();
            b.HasIndex(x => x.IsVisible);
            b.HasIndex(x => x.SortOrder);

            b.HasQueryFilter(x => !x.IsDeleted);
        }
    }

    public class PublicFacilityConfiguration : IEntityTypeConfiguration<PublicFacility>
    {
        public void Configure(EntityTypeBuilder<PublicFacility> b)
        {
            b.HasKey(x => x.Id);
            b.Property(x => x.Title).IsRequired().HasMaxLength(200);
            b.Property(x => x.Description).HasMaxLength(1000);
            b.Property(x => x.Type).HasMaxLength(80);
            b.Property(x => x.ImageUrl).HasMaxLength(500);
            b.HasIndex(x => x.IsVisible);
            b.HasIndex(x => x.SortOrder);
            b.HasQueryFilter(x => !x.IsDeleted);
        }
    }

    public class PublicGraduateStoryConfiguration : IEntityTypeConfiguration<PublicGraduateStory>
    {
        public void Configure(EntityTypeBuilder<PublicGraduateStory> b)
        {
            b.HasKey(x => x.Id);
            b.Property(x => x.FullName).IsRequired().HasMaxLength(150);
            b.Property(x => x.PhotoUrl).HasMaxLength(500);
            b.Property(x => x.GraduatedCourse).HasMaxLength(200);
            b.Property(x => x.CurrentPosition).HasMaxLength(200);
            b.Property(x => x.Story).HasMaxLength(2000);
            b.HasIndex(x => x.IsVisible);
            b.HasIndex(x => x.IsFeatured);
            b.HasIndex(x => x.SortOrder);
            b.HasQueryFilter(x => !x.IsDeleted);
        }
    }

    public class PublicFaqConfiguration : IEntityTypeConfiguration<PublicFaq>
    {
        public void Configure(EntityTypeBuilder<PublicFaq> b)
        {
            b.HasKey(x => x.Id);
            b.Property(x => x.Question).IsRequired().HasMaxLength(300);
            b.Property(x => x.Answer).IsRequired().HasMaxLength(3000);
            b.Property(x => x.Category).HasMaxLength(80);
            b.HasIndex(x => x.IsVisible);
            b.HasIndex(x => x.SortOrder);
            b.HasQueryFilter(x => !x.IsDeleted);
        }
    }

    public class PublicContactMessageConfiguration : IEntityTypeConfiguration<PublicContactMessage>
    {
        public void Configure(EntityTypeBuilder<PublicContactMessage> b)
        {
            b.HasKey(x => x.Id);
            b.Property(x => x.FullName).IsRequired().HasMaxLength(150);
            b.Property(x => x.Email).IsRequired().HasMaxLength(200);
            b.Property(x => x.Phone).HasMaxLength(40);
            b.Property(x => x.Subject).HasMaxLength(200);
            b.Property(x => x.Message).IsRequired().HasMaxLength(2000);
            b.Property(x => x.AdminNote).HasMaxLength(2000);
            b.Property(x => x.Status).HasConversion<int>();
            b.HasIndex(x => x.Status);
            b.HasIndex(x => x.CreatedAt);
            b.HasIndex(x => x.Email);
            b.HasQueryFilter(x => !x.IsDeleted);
        }
    }
}
