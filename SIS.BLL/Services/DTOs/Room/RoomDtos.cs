using SIS.Entities.Enums;

namespace SIS.BLL.Services.DTOs.Room
{
    public class RoomDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Code { get; set; }
        public RoomType Type { get; set; }
        public int Capacity { get; set; }
        public string? Location { get; set; }
        public string? Floor { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; }

        /// <summary>Bu otağa təyin edilmiş aktiv qrupların sayı.</summary>
        public int AssignedGroupsCount { get; set; }
    }

    public class CreateRoomDto
    {
        public string Name { get; set; } = null!;
        public string? Code { get; set; }
        public RoomType Type { get; set; } = RoomType.Classroom;
        public int Capacity { get; set; }
        public string? Location { get; set; }
        public string? Floor { get; set; }
        public string? Description { get; set; }
    }

    public class UpdateRoomDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Code { get; set; }
        public RoomType Type { get; set; } = RoomType.Classroom;
        public int Capacity { get; set; }
        public string? Location { get; set; }
        public string? Floor { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; } = true;
    }

    /// <summary>Conflict/available yoxlaması üçün bir dərs slotu (gün + saat aralığı).</summary>
    public class ScheduleSlotInput
    {
        public DayOfWeek DayOfWeek { get; set; }
        public string StartTime { get; set; } = null!; // "HH:mm"
        public string EndTime { get; set; } = null!;   // "HH:mm"
    }

    public class RoomConflictResultDto
    {
        public bool HasConflict { get; set; }
        public string? Message { get; set; }

        // Conflict detalları (UI üçün)
        public string? ConflictGroupName { get; set; }
        public string? ConflictCourseName { get; set; }
        public DayOfWeek? DayOfWeek { get; set; }
        public string? StartTime { get; set; }
        public string? EndTime { get; set; }
    }

    public class AvailableRoomDto
    {
        public int RoomId { get; set; }
        public string Name { get; set; } = null!;
        public RoomType Type { get; set; }
        public int Capacity { get; set; }
        public string? Location { get; set; }

        public bool IsAvailable { get; set; }
        /// <summary>Dolu olduqda səbəb: "Occupied by FS-90, Mon 09:00–12:00".</summary>
        public string? Reason { get; set; }
        /// <summary>Tutum qrup limitindən azdırsa true.</summary>
        public bool CapacityWarning { get; set; }
    }

    public class RoomScheduleSlotDto
    {
        public DayOfWeek DayOfWeek { get; set; }
        public string StartTime { get; set; } = null!;
        public string EndTime { get; set; } = null!;
        public string GroupName { get; set; } = null!;
        public string CourseName { get; set; } = null!;
        public string TeacherName { get; set; } = null!;
        public int GroupId { get; set; }
    }

    public class RoomScheduleDto
    {
        public RoomDto Room { get; set; } = null!;
        public List<RoomScheduleSlotDto> Slots { get; set; } = new();
    }

    /// <summary>Available rooms / conflict yoxlaması üçün ümumi sorğu (group schedule seçildikdə).</summary>
    public class RoomQueryRequest
    {
        public int? RoomId { get; set; }            // conflict yoxlaması üçün
        public DateTime GroupStart { get; set; }
        public DateTime GroupEnd { get; set; }
        public int? CapacityRequired { get; set; }
        public int? ExcludeGroupId { get; set; }
        public List<ScheduleSlotInput> Slots { get; set; } = new();
    }
}
