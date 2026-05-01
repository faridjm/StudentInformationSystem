using AutoMapper;
using SIS.BLL.Services.DTOs.AcademicProgram;
using SIS.BLL.Services.Interfaces;
using SIS.DAL.UnitOfWork;
using SIS.Entities.Models;

namespace SIS.BLL.Services.Implementations
{
    public class AcademicProgramService : IAcademicProgramService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public AcademicProgramService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<IEnumerable<AcademicProgramDto>> GetAllProgramsAsync()
        {
            var programs = await _unitOfWork.AcademicPrograms.GetAllAsync();
            return _mapper.Map<IEnumerable<AcademicProgramDto>>(programs);
        }

        public async Task<AcademicProgramDto?> GetProgramByIdAsync(int id)
        {
            var program = await _unitOfWork.AcademicPrograms.GetProgramWithDetailsAsync(id);
            return _mapper.Map<AcademicProgramDto>(program);
        }

        public async Task<IEnumerable<AcademicProgramDto>> GetProgramsByDepartmentAsync(int departmentId)
        {
            var programs = await _unitOfWork.AcademicPrograms.GetProgramsByDepartmentAsync(departmentId);
            return _mapper.Map<IEnumerable<AcademicProgramDto>>(programs);
        }

        public async Task CreateProgramAsync(CreateAcademicProgramDto dto)
        {
            var program = _mapper.Map<AcademicProgram>(dto);
            await _unitOfWork.AcademicPrograms.AddAsync(program);
            await _unitOfWork.SaveAsync();
        }

        public async Task UpdateProgramAsync(UpdateAcademicProgramDto dto)
        {
            var program = await _unitOfWork.AcademicPrograms.GetByIdAsync(dto.Id);
            if (program == null) throw new Exception("Proqram tapılmadı");

            _mapper.Map(dto, program);
            program.UpdatedAt = DateTime.UtcNow;
            _unitOfWork.AcademicPrograms.Update(program);
            await _unitOfWork.SaveAsync();
        }

        public async Task DeleteProgramAsync(int id)
        {
            var program = await _unitOfWork.AcademicPrograms.GetByIdAsync(id);
            if (program == null) throw new Exception("Proqram tapılmadı");

            _unitOfWork.AcademicPrograms.SoftDelete(program);
            await _unitOfWork.SaveAsync();
        }
    }
}
