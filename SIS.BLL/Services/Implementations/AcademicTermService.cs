using AutoMapper;
using SIS.BLL.Services.DTOs.AcademicTerm;
using SIS.BLL.Services.Interfaces;
using SIS.DAL.UnitOfWork;
using SIS.Entities.Models;
using static SIS.BLL.Exceptions.AppException;

namespace SIS.BLL.Services.Implementations
{
    public class AcademicTermService : IAcademicTermService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public AcademicTermService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<IEnumerable<AcademicTermDto>> GetAllTermsAsync()
        {
            var terms = await _unitOfWork.AcademicTerms.GetAllTermsAsync();
            return _mapper.Map<IEnumerable<AcademicTermDto>>(terms);
        }

        public async Task<AcademicTermDto?> GetActiveTermAsync()
        {
            var term = await _unitOfWork.AcademicTerms.GetActiveTermAsync();
            return _mapper.Map<AcademicTermDto>(term);
        }

        public async Task<AcademicTermDto?> GetTermByIdAsync(int id)
        {
            var term = await _unitOfWork.AcademicTerms.GetByIdAsync(id);
            return _mapper.Map<AcademicTermDto>(term);
        }

        public async Task CreateTermAsync(CreateAcademicTermDto dto)
        {
            await ValidateAgainstYearAsync(dto.AcademicYearId,
                dto.RegistrationStart, dto.GradeSubmissionDeadline);

            var term = _mapper.Map<AcademicTerm>(dto);
            await _unitOfWork.AcademicTerms.AddAsync(term);
            await _unitOfWork.SaveAsync();
        }

        public async Task UpdateTermAsync(UpdateAcademicTermDto dto)
        {
            var term = await _unitOfWork.AcademicTerms.GetByIdAsync(dto.Id);
            if (term == null)
                throw new NotFoundException("Termin tapılmadı");

            await ValidateAgainstYearAsync(dto.AcademicYearId,
                dto.RegistrationStart, dto.GradeSubmissionDeadline);

            _mapper.Map(dto, term);
            term.UpdatedAt = DateTime.UtcNow;
            _unitOfWork.AcademicTerms.Update(term);
            await _unitOfWork.SaveAsync();
        }

        public async Task SetActiveTermAsync(int id)
        {
            var allTerms = await _unitOfWork.AcademicTerms.GetAllAsync();
            foreach (var t in allTerms)
            {
                t.IsActive = false;
                _unitOfWork.AcademicTerms.Update(t);
            }

            var term = await _unitOfWork.AcademicTerms.GetByIdAsync(id);
            if (term == null)
                throw new NotFoundException("Termin tapılmadı");

            term.IsActive = true;
            term.UpdatedAt = DateTime.UtcNow;
            _unitOfWork.AcademicTerms.Update(term);
            await _unitOfWork.SaveAsync();
        }

        public async Task DeleteTermAsync(int id)
        {
            var term = await _unitOfWork.AcademicTerms.GetByIdAsync(id);
            if (term == null)
                throw new NotFoundException("Termin tapılmadı");

            _unitOfWork.AcademicTerms.SoftDelete(term);
            await _unitOfWork.SaveAsync();
        }

        /// <summary>
        /// Term-in bütün tarix aralığının seçilmiş AcademicYear-in
        /// daxilində olduğunu yoxlayır.
        /// </summary>
        private async Task ValidateAgainstYearAsync(int yearId, DateTime firstDate, DateTime lastDate)
        {
            var year = await _unitOfWork.AcademicYears.GetByIdAsync(yearId);
            if (year == null)
                throw new NotFoundException("Seçilmiş dərs sezonu tapılmadı");

            if (firstDate < year.StartDate || lastDate > year.EndDate)
                throw new BadRequestException(
                    $"Termin tarixləri seçilmiş sezonun aralığında olmalıdır " +
                    $"({year.StartDate:dd.MM.yyyy} – {year.EndDate:dd.MM.yyyy})");
        }
    }
}
