using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SIS.Entities.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace SIS.DAL.Configurations
{
    public class DepartmentConfiguration : IEntityTypeConfiguration<Department>
    {
        public void Configure(EntityTypeBuilder<Department> builder)
        {
            builder.HasKey(d => d.Id);

            builder.Property(d => d.Name)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(d => d.Code)
                .IsRequired()
                .HasMaxLength(10);

            builder.Property(d => d.Description)
                .HasMaxLength(500);

            
            builder.HasQueryFilter(d => !d.IsDeleted);
        }
    }
}
