using tapcet_api.DTO.Course;

namespace tapcet_api.Services.Interfaces
{
    public interface ICourseService
    {
        Task<CourseResponseDto?> CreateCourseAsync(CreateCourseDto createDto);
        Task<CourseResponseDto?> GetCourseByIdAsync(int courseId);
        Task<CourseWithUnitsDto?> GetCourseWithUnitsAsync(int courseId);
        Task<List<CourseResponseDto>> GetAllCoursesAsync();
        Task<List<CourseResponseDto>> GetCoursesBySubjectAsync(int subjectId);
        Task<CourseResponseDto?> UpdateCourseAsync(int courseId, UpdateCourseDto updateDto);
        Task<bool> DeleteCourseAsync(int courseId);
        Task<bool> CourseExistsAsync(int courseId);
        Task<bool> HasUnitsAsync(int courseId);
    }
}
