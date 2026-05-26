namespace SIS.BLL.Services.DTOs.WeeklySchedule
{
    /// <summary>
    /// Müəllim həftəlik cədvəli toplu yenidən qoyduqda istifadə olunur.
    /// Köhnə şablonlar silinir, yenilər əlavə olunur (replace semantics).
    /// </summary>
    public class SetWeeklyScheduleDto
    {
        public int CourseOfferingId { get; set; }
        public List<WeeklySlotDto> Slots { get; set; } = new();
    }
}
