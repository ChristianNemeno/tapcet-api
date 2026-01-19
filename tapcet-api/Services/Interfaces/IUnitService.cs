using tapcet_api.DTO.Quiz;
using tapcet_api.DTO.Unit;

namespace tapcet_api.Services.Interfaces
{
    public interface IUnitService
    {
        Task<UnitResponseDto?> CreateUnitAsync(CreateUnitDto createDto);
        Task<UnitResponseDto?> GetUnitByIdAsync(int unitId);
        Task<UnitWithQuizzesDto?> GetUnitWithQuizzesAsync(int unitId);
        Task<List<UnitResponseDto>> GetUnitsByCourseAsync(int courseId);
        Task<UnitResponseDto?> UpdateUnitAsync(int unitId, UpdateUnitDto updateDto);
        Task<UnitResponseDto?> ReorderUnitAsync(int unitId, int newOrderIndex);
        Task<bool> DeleteUnitAsync(int unitId);
        Task<bool> UnitExistsAsync(int unitId);
        Task<bool> HasQuizzesAsync(int unitId);
        Task<bool> ValidateOrderIndexAsync(int courseId, int orderIndex, int? excludeUnitId = null);
        Task<List<QuizSummaryDto>> GetQuizzesByUnitAsync(int unitId);
    }
}
