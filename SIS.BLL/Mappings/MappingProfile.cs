// AutoMapper profile - minimal, most mapping is now done manually in services
// Kept for backward compatibility with any remaining AutoMapper usages

namespace SIS.BLL.Mappings
{
    public class MappingProfile : AutoMapper.Profile
    {
        public MappingProfile()
        {
            // All mappings moved to manual service implementations
        }
    }
}
