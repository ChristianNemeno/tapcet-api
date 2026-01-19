using AutoMapper;
using Microsoft.EntityFrameworkCore;
using tapcet_api.Data;
using tapcet_api.DTO.Quiz;
using tapcet_api.DTO.Unit;
using tapcet_api.Models;
using tapcet_api.Services.Interfaces;

namespace tapcet_api.Services.Implementations
{
    public class UnitService : IUnitService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<UnitService> _logger;

        public UnitService(
            ApplicationDbContext context,
            IMapper mapper,
            ILogger<UnitService> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<UnitResponseDto?> CreateUnitAsync(CreateUnitDto createDto)
        {
            try
            {
                var courseExists = await _context.Courses
                    .AnyAsync(c => c.Id == createDto.CourseId);

                if (!courseExists)
                {
                    _logger.LogWarning("Course not found: {CourseId}", createDto.CourseId);
                    return null;
                }

                if (!await ValidateOrderIndexAsync(createDto.CourseId, createDto.OrderIndex))
                {
                    _logger.LogWarning("Order index {OrderIndex} already exists for course {CourseId}", 
                        createDto.OrderIndex, createDto.CourseId);
                    return null;
                }

                var unit = _mapper.Map<Unit>(createDto);

                _context.Units.Add(unit);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Unit created successfully: {UnitId}", unit.Id);

                return await GetUnitByIdAsync(unit.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating unit");
                return null;
            }
        }

        public async Task<UnitResponseDto?> GetUnitByIdAsync(int unitId)
        {
            try
            {
                var unit = await _context.Units
                    .Include(u => u.Course)
                    .Include(u => u.Quizzes)
                    .FirstOrDefaultAsync(u => u.Id == unitId);

                if (unit == null)
                {
                    _logger.LogWarning("Unit not found: {UnitId}", unitId);
                    return null;
                }

                return _mapper.Map<UnitResponseDto>(unit);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving unit {UnitId}", unitId);
                return null;
            }
        }

        public async Task<UnitWithQuizzesDto?> GetUnitWithQuizzesAsync(int unitId)
        {
            try
            {
                var unit = await _context.Units
                    .Include(u => u.Course)
                    .Include(u => u.Quizzes)
                        .ThenInclude(q => q.Questions)
                    .Include(u => u.Quizzes)
                        .ThenInclude(q => q.CreatedBy)
                    .Include(u => u.Quizzes)
                        .ThenInclude(q => q.QuizAttempts)
                    .FirstOrDefaultAsync(u => u.Id == unitId);

                if (unit == null)
                {
                    _logger.LogWarning("Unit not found: {UnitId}", unitId);
                    return null;
                }

                return _mapper.Map<UnitWithQuizzesDto>(unit);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving unit with quizzes {UnitId}", unitId);
                return null;
            }
        }

        public async Task<List<UnitResponseDto>> GetUnitsByCourseAsync(int courseId)
        {
            try
            {
                var units = await _context.Units
                    .Include(u => u.Course)
                    .Include(u => u.Quizzes)
                    .Where(u => u.CourseId == courseId)
                    .OrderBy(u => u.OrderIndex)
                    .ToListAsync();

                return _mapper.Map<List<UnitResponseDto>>(units);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving units for course {CourseId}", courseId);
                return new List<UnitResponseDto>();
            }
        }

        public async Task<UnitResponseDto?> UpdateUnitAsync(int unitId, UpdateUnitDto updateDto)
        {
            try
            {
                var unit = await _context.Units
                    .Include(u => u.Course)
                    .Include(u => u.Quizzes)
                    .FirstOrDefaultAsync(u => u.Id == unitId);

                if (unit == null)
                {
                    _logger.LogWarning("Unit not found: {UnitId}", unitId);
                    return null;
                }

                var courseExists = await _context.Courses
                    .AnyAsync(c => c.Id == updateDto.CourseId);

                if (!courseExists)
                {
                    _logger.LogWarning("Course not found: {CourseId}", updateDto.CourseId);
                    return null;
                }

                if (unit.OrderIndex != updateDto.OrderIndex && 
                    !await ValidateOrderIndexAsync(updateDto.CourseId, updateDto.OrderIndex, unitId))
                {
                    _logger.LogWarning("Order index {OrderIndex} already exists for course {CourseId}", 
                        updateDto.OrderIndex, updateDto.CourseId);
                    return null;
                }

                unit.Title = updateDto.Title;
                unit.OrderIndex = updateDto.OrderIndex;
                unit.CourseId = updateDto.CourseId;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Unit updated: {UnitId}", unitId);

                return _mapper.Map<UnitResponseDto>(unit);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating unit {UnitId}", unitId);
                return null;
            }
        }

        public async Task<UnitResponseDto?> ReorderUnitAsync(int unitId, int newOrderIndex)
        {
            try
            {
                var unit = await _context.Units
                    .Include(u => u.Course)
                    .Include(u => u.Quizzes)
                    .FirstOrDefaultAsync(u => u.Id == unitId);

                if (unit == null)
                {
                    _logger.LogWarning("Unit not found: {UnitId}", unitId);
                    return null;
                }

                if (!await ValidateOrderIndexAsync(unit.CourseId, newOrderIndex, unitId))
                {
                    _logger.LogWarning("Order index {OrderIndex} already exists for course {CourseId}", 
                        newOrderIndex, unit.CourseId);
                    return null;
                }

                unit.OrderIndex = newOrderIndex;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Unit reordered: {UnitId} to index {OrderIndex}", unitId, newOrderIndex);

                return _mapper.Map<UnitResponseDto>(unit);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reordering unit {UnitId}", unitId);
                return null;
            }
        }

        public async Task<bool> DeleteUnitAsync(int unitId)
        {
            try
            {
                var unit = await _context.Units
                    .Include(u => u.Quizzes)
                    .FirstOrDefaultAsync(u => u.Id == unitId);

                if (unit == null)
                {
                    _logger.LogWarning("Unit not found: {UnitId}", unitId);
                    return false;
                }

                if (unit.Quizzes.Any())
                {
                    _logger.LogWarning("Cannot delete unit {UnitId} with existing quizzes. " +
                        "Quizzes will be orphaned (UnitId set to null)", unitId);
                }

                _context.Units.Remove(unit);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Unit deleted: {UnitId}", unitId);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting unit {UnitId}", unitId);
                return false;
            }
        }

        public async Task<bool> UnitExistsAsync(int unitId)
        {
            try
            {
                return await _context.Units
                    .AnyAsync(u => u.Id == unitId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if unit exists: {UnitId}", unitId);
                return false;
            }
        }

        public async Task<bool> HasQuizzesAsync(int unitId)
        {
            try
            {
                return await _context.Quizzes
                    .AnyAsync(q => q.UnitId == unitId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if unit has quizzes: {UnitId}", unitId);
                return false;
            }
        }

        public async Task<bool> ValidateOrderIndexAsync(int courseId, int orderIndex, int? excludeUnitId = null)
        {
            try
            {
                var query = _context.Units
                    .Where(u => u.CourseId == courseId && u.OrderIndex == orderIndex);

                if (excludeUnitId.HasValue)
                {
                    query = query.Where(u => u.Id != excludeUnitId.Value);
                }

                return !await query.AnyAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating order index for course {CourseId}", courseId);
                return false;
            }
        }

        public async Task<List<QuizSummaryDto>> GetQuizzesByUnitAsync(int unitId)
        {
            try
            {
                var quizzes = await _context.Quizzes
                    .Include(q => q.CreatedBy)
                    .Include(q => q.Questions)
                    .Include(q => q.QuizAttempts)
                    .Where(q => q.UnitId == unitId)
                    .OrderBy(q => q.OrderIndex)
                    .ToListAsync();

                return _mapper.Map<List<QuizSummaryDto>>(quizzes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving quizzes for unit {UnitId}", unitId);
                return new List<QuizSummaryDto>();
            }
        }
    }
}
