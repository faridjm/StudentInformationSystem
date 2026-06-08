using SIS.Entities.Base;
using SIS.Entities.Enums;

namespace SIS.Entities.Models
{
    /// <summary>
    /// Real akademik dərs otağı (scheduling üçün). Qruplar bu otaqlara təyin edilir və
    /// sistem eyni otağın eyni vaxtda iki qrupa verilməsinin qarşısını alır.
    /// QEYD: public saytdakı görüntü üçün deyil — o, PublicFacility-dir.
    /// </summary>
    public class Room : BaseEntity
    {
        public string Name { get; set; } = null!;
        public string? Code { get; set; }
        public RoomType Type { get; set; } = RoomType.Classroom;
        public int Capacity { get; set; }
        public string? Location { get; set; }
        public string? Floor { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; } = true;

        public ICollection<Group> Groups { get; set; } = new List<Group>();
    }
}
