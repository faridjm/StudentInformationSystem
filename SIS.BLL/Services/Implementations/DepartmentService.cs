using AutoMapper;
using SIS.BLL.Services.DTOs.Department;
using SIS.BLL.Services.Interfaces;
using SIS.Common.Pagination;
using SIS.DAL.UnitOfWork;
using SIS.Entities.Models;
using static SIS.BLL.Exceptions.AppException;

namespace SIS.BLL.Services.Implementations
{
    public class DepartmentService : IDepartmentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public DepartmentService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<PagedResult<DepartmentDto>> GetAllDepartmentsAsync(PagedRequest request)
        {
            var pagedDepartments = await _unitOfWork.Departments.GetPagedAsync(request);

            return new PagedResult<DepartmentDto>
            {
                Items = _mapper.Map<IEnumerable<DepartmentDto>>(pagedDepartments.Items),
                TotalCount = pagedDepartments.TotalCount,
                Page = pagedDepartments.Page,
                PageSize = pagedDepartments.PageSize
            };
        }

        public async Task<DepartmentDto?> GetDepartmentByIdAsync(int id)
        {
            var department = await _unitOfWork.Departments.GetDepartmentWithDetailsAsync(id);
            return _mapper.Map<DepartmentDto>(department);
        }

        public async Task CreateDepartmentAsync(CreateDepartmentDto dto)
        {
            var department = _mapper.Map<Department>(dto);
            await _unitOfWork.Departments.AddAsync(department);
            await _unitOfWork.SaveAsync();
        }

        public async Task UpdateDepartmentAsync(UpdateDepartmentDto dto)
        {
            var department = await _unitOfWork.Departments.GetByIdAsync(dto.Id);
            if (department == null)
                throw new NotFoundException("Departament tapılmadı");

            _mapper.Map(dto, department);
            department.UpdatedAt = DateTime.UtcNow;
            _unitOfWork.Departments.Update(department);
            await _unitOfWork.SaveAsync();
        }

        public async Task DeleteDepartmentAsync(int id)
        {
            var department = await _unitOfWork.Departments.GetByIdAsync(id);
            if (department == null)
                throw new NotFoundException("Departament tapılmadı");

            _unitOfWork.Departments.SoftDelete(department);
            await _unitOfWork.SaveAsync();
        }
    }

}
