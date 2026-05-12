using AutoMapper;
using SIS.BLL.Services.DTOs.CourseOffering;
using SIS.BLL.Services.Interfaces;
using SIS.DAL.UnitOfWork;
using SIS.Entities.Models;
using static SIS.BLL.Exceptions.AppException;

namespace SIS.BLL.Services.Implementations
{
    public class CourseOfferingService : ICourseOfferingService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public CourseOfferingService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<IEnumerable<CourseOfferingListDto>> GetAllOfferingsAsync()
        {
            var offerings = await _unitOfWork.CourseOfferings.GetAllAsync();
            return _mapper.Map<IEnumerable<CourseOfferingListDto>>(offerings);
        }

        public async Task<CourseOfferingDetailDto?> GetOfferingByIdAsync(int id)
        {
            var offering = await _unitOfWork.CourseOfferings.GetOfferingWithDetailsAsync(id);
            return _mapper.Map<CourseOfferingDetailDto>(offering);
        }

        public async Task<IEnumerable<CourseOfferingListDto>> GetActiveOfferingsAsync()
        {
            var offerings = await _unitOfWork.CourseOfferings.GetActiveOfferingsAsync();
            return _mapper.Map<IEnumerable<CourseOfferingListDto>>(offerings);
        }

        public async Task<IEnumerable<CourseOfferingListDto>> GetOfferingsByTeacherAsync(int teacherId)
        {
            var offerings = await _unitOfWork.CourseOfferings.GetOfferingsByTeacherAsync(teacherId);
            return _mapper.Map<IEnumerable<CourseOfferingListDto>>(offerings);
        }

        public async Task<IEnumerable<CourseOfferingListDto>> GetOfferingsByTermAsync(int termId)
        {
            var offerings = await _unitOfWork.CourseOfferings.GetOfferingsByTermAsync(termId);
            return _mapper.Map<IEnumerable<CourseOfferingListDto>>(offerings);
        }

        public async Task CreateOfferingAsync(CreateCourseOfferingDto dto)
        {
            var offering = _mapper.Map<CourseOffering>(dto);
            await _unitOfWork.CourseOfferings.AddAsync(offering);
            await _unitOfWork.SaveAsync();
        }

        public async Task UpdateOfferingAsync(UpdateCourseOfferingDto dto)
        {
            var offering = await _unitOfWork.CourseOfferings.GetByIdAsync(dto.Id);
            if (offering == null)
                throw new NotFoundException("Kurs təklifi tapılmadı");

            _mapper.Map(dto, offering);
            offering.UpdatedAt = DateTime.UtcNow;
            _unitOfWork.CourseOfferings.Update(offering);
            await _unitOfWork.SaveAsync();
        }

        public async Task DeleteOfferingAsync(int id)
        {
            var offering = await _unitOfWork.CourseOfferings.GetByIdAsync(id);
            if (offering == null)
                throw new NotFoundException("Kurs təklifi tapılmadı");

            _unitOfWork.CourseOfferings.SoftDelete(offering);
            await _unitOfWork.SaveAsync();
        }
    }
}
