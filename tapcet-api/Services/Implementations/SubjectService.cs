using AutoMapper;
using Microsoft.EntityFrameworkCore;
using tapcet_api.Data;
using tapcet_api.DTO.Subject;
using tapcet_api.Models;
using tapcet_api.Services.Interfaces;

namespace tapcet_api.Services.Implementations
{
    public class SubjectService : ISubjectService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<SubjectService> _logger;

        public SubjectService(
            ApplicationDbContext context,
            IMapper mapper,
            ILogger<SubjectService> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<SubjectResponseDto?> CreateSubjectAsync(CreateSubjectDto createDto)
        {
            try
            {
                if (await SubjectExistsAsync(createDto.Name))
                {
                    _logger.LogWarning("Subject with name '{Name}' already exists", createDto.Name);
                    return null;
                }

                var subject = _mapper.Map<Subject>(createDto);

                _context.Subjects.Add(subject);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Subject created successfully: {SubjectId}", subject.Id);

                return await GetSubjectByIdAsync(subject.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating subject");
                return null;
            }
        }

        public async Task<SubjectResponseDto?> GetSubjectByIdAsync(int subjectId)
        {
            try
            {
                var subject = await _context.Subjects
                    .Include(s => s.Courses)
                    .FirstOrDefaultAsync(s => s.Id == subjectId);

                if (subject == null)
                {
                    _logger.LogWarning("Subject not found: {SubjectId}", subjectId);
                    return null;
                }

                return _mapper.Map<SubjectResponseDto>(subject);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving subject {SubjectId}", subjectId);
                return null;
            }
        }

        public async Task<SubjectWithCoursesDto?> GetSubjectWithCoursesAsync(int subjectId)
        {
            try
            {
                var subject = await _context.Subjects
                    .Include(s => s.Courses)
                        .ThenInclude(c => c.Units)
                    .FirstOrDefaultAsync(s => s.Id == subjectId);

                if (subject == null)
                {
                    _logger.LogWarning("Subject not found: {SubjectId}", subjectId);
                    return null;
                }

                return _mapper.Map<SubjectWithCoursesDto>(subject);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving subject with courses {SubjectId}", subjectId);
                return null;
            }
        }

        public async Task<List<SubjectResponseDto>> GetAllSubjectsAsync()
        {
            try
            {
                var subjects = await _context.Subjects
                    .Include(s => s.Courses)
                    .OrderBy(s => s.Name)
                    .ToListAsync();

                return _mapper.Map<List<SubjectResponseDto>>(subjects);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all subjects");
                return new List<SubjectResponseDto>();
            }
        }

        public async Task<SubjectResponseDto?> UpdateSubjectAsync(int subjectId, UpdateSubjectDto updateDto)
        {
            try
            {
                var subject = await _context.Subjects
                    .Include(s => s.Courses)
                    .FirstOrDefaultAsync(s => s.Id == subjectId);

                if (subject == null)
                {
                    _logger.LogWarning("Subject not found: {SubjectId}", subjectId);
                    return null;
                }

                if (subject.Name.ToLower() != updateDto.Name.ToLower() && 
                    await SubjectExistsAsync(updateDto.Name))
                {
                    _logger.LogWarning("Subject with name '{Name}' already exists", updateDto.Name);
                    return null;
                }

                subject.Name = updateDto.Name;
                subject.Description = updateDto.Description;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Subject updated: {SubjectId}", subjectId);

                return _mapper.Map<SubjectResponseDto>(subject);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating subject {SubjectId}", subjectId);
                return null;
            }
        }

        public async Task<bool> DeleteSubjectAsync(int subjectId)
        {
            try
            {
                var subject = await _context.Subjects
                    .Include(s => s.Courses)
                    .FirstOrDefaultAsync(s => s.Id == subjectId);

                if (subject == null)
                {
                    _logger.LogWarning("Subject not found: {SubjectId}", subjectId);
                    return false;
                }

                if (subject.Courses.Any())
                {
                    _logger.LogWarning("Cannot delete subject {SubjectId} with existing courses", subjectId);
                    return false;
                }

                _context.Subjects.Remove(subject);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Subject deleted: {SubjectId}", subjectId);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting subject {SubjectId}", subjectId);
                return false;
            }
        }

        public async Task<bool> SubjectExistsAsync(string name)
        {
            try
            {
                return await _context.Subjects
                    .AnyAsync(s => s.Name.ToLower() == name.ToLower());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if subject exists: {Name}", name);
                return false;
            }
        }

        public async Task<bool> SubjectExistsByIdAsync(int subjectId)
        {
            try
            {
                return await _context.Subjects
                    .AnyAsync(s => s.Id == subjectId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if subject exists: {SubjectId}", subjectId);
                return false;
            }
        }
    }
}
