using AutoMapper;
using Microsoft.EntityFrameworkCore;
using tapcet_api.Data;
using tapcet_api.DTO.Course;
using tapcet_api.Models;
using tapcet_api.Services.Interfaces;

namespace tapcet_api.Services.Implementations
{
    public class CourseService : ICourseService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<CourseService> _logger;

        public CourseService(
            ApplicationDbContext context,
            IMapper mapper,
            ILogger<CourseService> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<CourseResponseDto?> CreateCourseAsync(CreateCourseDto createDto)
        {
            try
            {
                var subjectExists = await _context.Subjects
                    .AnyAsync(s => s.Id == createDto.SubjectId);

                if (!subjectExists)
                {
                    _logger.LogWarning("Subject not found: {SubjectId}", createDto.SubjectId);
                    return null;
                }

                var course = _mapper.Map<Course>(createDto);

                _context.Courses.Add(course);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Course created successfully: {CourseId}", course.Id);

                return await GetCourseByIdAsync(course.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating course");
                return null;
            }
        }

        public async Task<CourseResponseDto?> GetCourseByIdAsync(int courseId)
        {
            try
            {
                var course = await _context.Courses
                    .Include(c => c.Subject)
                    .Include(c => c.Units)
                    .FirstOrDefaultAsync(c => c.Id == courseId);

                if (course == null)
                {
                    _logger.LogWarning("Course not found: {CourseId}", courseId);
                    return null;
                }

                return _mapper.Map<CourseResponseDto>(course);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving course {CourseId}", courseId);
                return null;
            }
        }

        public async Task<CourseWithUnitsDto?> GetCourseWithUnitsAsync(int courseId)
        {
            try
            {
                var course = await _context.Courses
                    .Include(c => c.Subject)
                    .Include(c => c.Units)
                        .ThenInclude(u => u.Quizzes)
                    .FirstOrDefaultAsync(c => c.Id == courseId);

                if (course == null)
                {
                    _logger.LogWarning("Course not found: {CourseId}", courseId);
                    return null;
                }

                return _mapper.Map<CourseWithUnitsDto>(course);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving course with units {CourseId}", courseId);
                return null;
            }
        }

        public async Task<List<CourseResponseDto>> GetAllCoursesAsync()
        {
            try
            {
                var courses = await _context.Courses
                    .Include(c => c.Subject)
                    .Include(c => c.Units)
                    .OrderBy(c => c.Title)
                    .ToListAsync();

                return _mapper.Map<List<CourseResponseDto>>(courses);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all courses");
                return new List<CourseResponseDto>();
            }
        }

        public async Task<List<CourseResponseDto>> GetCoursesBySubjectAsync(int subjectId)
        {
            try
            {
                var courses = await _context.Courses
                    .Include(c => c.Subject)
                    .Include(c => c.Units)
                    .Where(c => c.SubjectId == subjectId)
                    .OrderBy(c => c.Title)
                    .ToListAsync();

                return _mapper.Map<List<CourseResponseDto>>(courses);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving courses for subject {SubjectId}", subjectId);
                return new List<CourseResponseDto>();
            }
        }

        public async Task<CourseResponseDto?> UpdateCourseAsync(int courseId, UpdateCourseDto updateDto)
        {
            try
            {
                var course = await _context.Courses
                    .Include(c => c.Subject)
                    .Include(c => c.Units)
                    .FirstOrDefaultAsync(c => c.Id == courseId);

                if (course == null)
                {
                    _logger.LogWarning("Course not found: {CourseId}", courseId);
                    return null;
                }

                var subjectExists = await _context.Subjects
                    .AnyAsync(s => s.Id == updateDto.SubjectId);

                if (!subjectExists)
                {
                    _logger.LogWarning("Subject not found: {SubjectId}", updateDto.SubjectId);
                    return null;
                }

                course.Title = updateDto.Title;
                course.Description = updateDto.Description;
                course.SubjectId = updateDto.SubjectId;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Course updated: {CourseId}", courseId);

                return _mapper.Map<CourseResponseDto>(course);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating course {CourseId}", courseId);
                return null;
            }
        }

        public async Task<bool> DeleteCourseAsync(int courseId)
        {
            try
            {
                var course = await _context.Courses
                    .Include(c => c.Units)
                    .FirstOrDefaultAsync(c => c.Id == courseId);

                if (course == null)
                {
                    _logger.LogWarning("Course not found: {CourseId}", courseId);
                    return false;
                }

                if (course.Units.Any())
                {
                    _logger.LogWarning("Cannot delete course {CourseId} with existing units", courseId);
                    return false;
                }

                _context.Courses.Remove(course);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Course deleted: {CourseId}", courseId);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting course {CourseId}", courseId);
                return false;
            }
        }

        public async Task<bool> CourseExistsAsync(int courseId)
        {
            try
            {
                return await _context.Courses
                    .AnyAsync(c => c.Id == courseId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if course exists: {CourseId}", courseId);
                return false;
            }
        }

        public async Task<bool> HasUnitsAsync(int courseId)
        {
            try
            {
                return await _context.Units
                    .AnyAsync(u => u.CourseId == courseId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if course has units: {CourseId}", courseId);
                return false;
            }
        }
    }
}
