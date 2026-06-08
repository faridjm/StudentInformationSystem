using SIS.BLL.Services.DTOs.Room;
using SIS.BLL.Services.Interfaces;
using SIS.DAL.UnitOfWork;
using SIS.Entities.Models;
using static SIS.BLL.Exceptions.AppException;

namespace SIS.BLL.Services.Implementations
{
    /// <summary>
    /// Real akademik otaq idarəetməsi + scheduling conflict məntiqi.
    /// Public showcase (PublicFacility) ilə qarışdırılmır.
    /// </summary>
    public class RoomService : IRoomService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAuditLogService _audit;

        public RoomService(IUnitOfWork unitOfWork, IAuditLogService audit)
        {
            _unitOfWork = unitOfWork;
            _audit = audit;
        }

        public async Task<IEnumerable<RoomDto>> GetRoomsAsync()
        {
            var rooms = (await _unitOfWork.Rooms.GetAllAsync()).OrderBy(r => r.Name).ToList();
            return await MapManyWithCountsAsync(rooms);
        }

        public async Task<IEnumerable<RoomDto>> GetActiveRoomsAsync()
        {
            var rooms = (await _unitOfWork.Rooms.GetActiveRoomsAsync()).ToList();
            return await MapManyWithCountsAsync(rooms);
        }

        public async Task<RoomDto?> GetRoomByIdAsync(int id)
        {
            var room = await _unitOfWork.Rooms.GetByIdAsync(id);
            if (room == null) return null;
            var groups = await _unitOfWork.Rooms.GetRoomGroupsWithScheduleAsync(id);
            return MapToDto(room, groups.Count);
        }

        public async Task<RoomDto> CreateRoomAsync(CreateRoomDto dto)
        {
            ValidateRoom(dto.Name, dto.Capacity);

            if (await _unitOfWork.Rooms.NameExistsAsync(dto.Name.Trim()))
                throw new ConflictException($"\"{dto.Name}\" adlı otaq artıq mövcuddur");

            var room = new Room
            {
                Name = dto.Name.Trim(),
                Code = string.IsNullOrWhiteSpace(dto.Code) ? null : dto.Code.Trim(),
                Type = dto.Type,
                Capacity = dto.Capacity,
                Location = dto.Location?.Trim(),
                Floor = dto.Floor?.Trim(),
                Description = dto.Description?.Trim(),
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Rooms.AddAsync(room);
            await _unitOfWork.SaveAsync();

            await _audit.LogAsync("CreateRoom", "Room", room.Id.ToString(),
                $"Admin created room {room.Name} with capacity {room.Capacity}.");

            return MapToDto(room, 0);
        }

        public async Task UpdateRoomAsync(UpdateRoomDto dto)
        {
            ValidateRoom(dto.Name, dto.Capacity);

            var room = await _unitOfWork.Rooms.GetByIdAsync(dto.Id)
                ?? throw new NotFoundException("Otaq tapılmadı");

            if (await _unitOfWork.Rooms.NameExistsAsync(dto.Name.Trim(), dto.Id))
                throw new ConflictException($"\"{dto.Name}\" adlı başqa otaq artıq mövcuddur");

            bool wasActive = room.IsActive;

            room.Name = dto.Name.Trim();
            room.Code = string.IsNullOrWhiteSpace(dto.Code) ? null : dto.Code.Trim();
            room.Type = dto.Type;
            room.Capacity = dto.Capacity;
            room.Location = dto.Location?.Trim();
            room.Floor = dto.Floor?.Trim();
            room.Description = dto.Description?.Trim();
            room.IsActive = dto.IsActive;
            room.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.Rooms.Update(room);
            await _unitOfWork.SaveAsync();

            await _audit.LogAsync("UpdateRoom", "Room", room.Id.ToString(),
                $"Room {room.Name} updated.");

            if (wasActive != dto.IsActive)
                await _audit.LogAsync(dto.IsActive ? "ActivateRoom" : "DeactivateRoom", "Room",
                    room.Id.ToString(), $"Room {room.Name} {(dto.IsActive ? "activated" : "deactivated")}.");
        }

        public async Task SetActiveAsync(int id, bool isActive)
        {
            var room = await _unitOfWork.Rooms.GetByIdAsync(id)
                ?? throw new NotFoundException("Otaq tapılmadı");

            room.IsActive = isActive;
            room.UpdatedAt = DateTime.UtcNow;
            _unitOfWork.Rooms.Update(room);
            await _unitOfWork.SaveAsync();

            await _audit.LogAsync(isActive ? "ActivateRoom" : "DeactivateRoom", "Room",
                room.Id.ToString(), $"Room {room.Name} {(isActive ? "activated" : "deactivated")}.");
        }

        public async Task<RoomScheduleDto> GetRoomScheduleAsync(int roomId)
        {
            var room = await _unitOfWork.Rooms.GetByIdAsync(roomId)
                ?? throw new NotFoundException("Otaq tapılmadı");

            // Yalnız aktiv/davam edən və gələcək qruplar
            var today = DateTime.UtcNow.Date;
            var groups = await _unitOfWork.Rooms.GetRoomGroupsWithScheduleAsync(roomId);

            var slots = new List<RoomScheduleSlotDto>();
            foreach (var g in groups.Where(g => g.EndDate.Date >= today))
            {
                foreach (var d in g.ScheduleDays)
                {
                    slots.Add(new RoomScheduleSlotDto
                    {
                        DayOfWeek = d.DayOfWeek,
                        StartTime = d.StartTime.ToString("HH:mm"),
                        EndTime = d.EndTime.ToString("HH:mm"),
                        GroupName = g.Name,
                        CourseName = g.Course?.Name ?? "",
                        TeacherName = g.Teacher?.AppUser?.FullName ?? "",
                        GroupId = g.Id
                    });
                }
            }

            slots = slots.OrderBy(s => s.DayOfWeek).ThenBy(s => s.StartTime).ToList();

            return new RoomScheduleDto
            {
                Room = MapToDto(room, groups.Count),
                Slots = slots
            };
        }

        public async Task<RoomConflictResultDto> CheckRoomConflictAsync(
            int roomId, List<ScheduleSlotInput> slots,
            DateTime groupStart, DateTime groupEnd, int? excludeGroupId = null)
        {
            var parsed = ParseSlots(slots);
            if (parsed.Count == 0)
                return new RoomConflictResultDto { HasConflict = false };

            var existingGroups = await _unitOfWork.Rooms.GetRoomGroupsWithScheduleAsync(
                roomId, excludeGroupId, groupStart, groupEnd);

            foreach (var g in existingGroups)
            {
                foreach (var ed in g.ScheduleDays)
                {
                    foreach (var ns in parsed)
                    {
                        if (ed.DayOfWeek != ns.Day) continue;
                        // Time overlap: ExistingStart < NewEnd AND NewStart < ExistingEnd
                        if (ed.StartTime < ns.End && ns.Start < ed.EndTime)
                        {
                            return new RoomConflictResultDto
                            {
                                HasConflict = true,
                                ConflictGroupName = g.Name,
                                ConflictCourseName = g.Course?.Name,
                                DayOfWeek = ed.DayOfWeek,
                                StartTime = ed.StartTime.ToString("HH:mm"),
                                EndTime = ed.EndTime.ToString("HH:mm"),
                                Message = "Bu otaq seçilmiş dərs vaxtında artıq başqa qrup üçün ayrılıb."
                            };
                        }
                    }
                }
            }

            return new RoomConflictResultDto { HasConflict = false };
        }

        public async Task<IEnumerable<AvailableRoomDto>> GetAvailableRoomsAsync(
            List<ScheduleSlotInput> slots, DateTime groupStart, DateTime groupEnd,
            int? capacityRequired = null, int? excludeGroupId = null)
        {
            var rooms = (await _unitOfWork.Rooms.GetActiveRoomsAsync()).ToList();
            var result = new List<AvailableRoomDto>();

            foreach (var room in rooms)
            {
                var conflict = await CheckRoomConflictAsync(room.Id, slots, groupStart, groupEnd, excludeGroupId);

                var dto = new AvailableRoomDto
                {
                    RoomId = room.Id,
                    Name = room.Name,
                    Type = room.Type,
                    Capacity = room.Capacity,
                    Location = room.Location,
                    IsAvailable = !conflict.HasConflict,
                    CapacityWarning = capacityRequired.HasValue && room.Capacity < capacityRequired.Value
                };

                if (conflict.HasConflict)
                    dto.Reason = $"Occupied by {conflict.ConflictGroupName}, {ShortDay(conflict.DayOfWeek!.Value)} {conflict.StartTime}–{conflict.EndTime}";

                result.Add(dto);
            }

            // Boş otaqlar əvvəl, sonra ad üzrə
            return result.OrderByDescending(r => r.IsAvailable).ThenBy(r => r.Name);
        }

        // ── köməkçilər ──

        private static void ValidateRoom(string name, int capacity)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new BadRequestException("Otaq adı tələb olunur");
            if (capacity <= 0)
                throw new BadRequestException("Tutum 0-dan böyük olmalıdır");
        }

        private async Task<List<RoomDto>> MapManyWithCountsAsync(List<Room> rooms)
        {
            var list = new List<RoomDto>();
            foreach (var r in rooms)
            {
                var groups = await _unitOfWork.Rooms.GetRoomGroupsWithScheduleAsync(r.Id);
                list.Add(MapToDto(r, groups.Count));
            }
            return list;
        }

        private static RoomDto MapToDto(Room r, int assignedGroups) => new()
        {
            Id = r.Id,
            Name = r.Name,
            Code = r.Code,
            Type = r.Type,
            Capacity = r.Capacity,
            Location = r.Location,
            Floor = r.Floor,
            Description = r.Description,
            IsActive = r.IsActive,
            AssignedGroupsCount = assignedGroups
        };

        private static List<(DayOfWeek Day, TimeOnly Start, TimeOnly End)> ParseSlots(List<ScheduleSlotInput>? slots)
        {
            var list = new List<(DayOfWeek, TimeOnly, TimeOnly)>();
            if (slots == null) return list;
            foreach (var s in slots)
            {
                if (TimeOnly.TryParse(s.StartTime, out var st) && TimeOnly.TryParse(s.EndTime, out var en) && st < en)
                    list.Add((s.DayOfWeek, st, en));
            }
            return list;
        }

        private static string ShortDay(DayOfWeek d) => d switch
        {
            System.DayOfWeek.Monday => "Mon",
            System.DayOfWeek.Tuesday => "Tue",
            System.DayOfWeek.Wednesday => "Wed",
            System.DayOfWeek.Thursday => "Thu",
            System.DayOfWeek.Friday => "Fri",
            System.DayOfWeek.Saturday => "Sat",
            _ => "Sun"
        };
    }
}
