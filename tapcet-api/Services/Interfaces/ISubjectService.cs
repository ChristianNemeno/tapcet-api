using tapcet_api.DTO.Subject;

namespace tapcet_api.Services.Interfaces
{
    public interface ISubjectService
    {
        Task<SubjectResponseDto?> CreateSubjectAsync(CreateSubjectDto createDto);
        Task<SubjectResponseDto?> GetSubjectByIdAsync(int subjectId);
        Task<SubjectWithCoursesDto?> GetSubjectWithCoursesAsync(int subjectId);
        Task<List<SubjectResponseDto>> GetAllSubjectsAsync();
        Task<SubjectResponseDto?> UpdateSubjectAsync(int subjectId, UpdateSubjectDto updateDto);
        Task<bool> DeleteSubjectAsync(int subjectId);
        Task<bool> SubjectExistsAsync(string name);
        Task<bool> SubjectExistsByIdAsync(int subjectId);
    }
}
