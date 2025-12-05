using AutoMapper;
using Microsoft.EntityFrameworkCore;
using tapcet_api.Data;
using tapcet_api.DTO.Quiz;
using tapcet_api.Models;
using tapcet_api.Services.Interfaces;

namespace tapcet_api.Services.Implementations
{
    public class QuizService : IQuizService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<QuizService> _logger;

        public QuizService(
            ApplicationDbContext context,
            IMapper mapper,
            ILogger<QuizService> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<QuizResponseDto?> CreateQuizAsync(CreateQuizDto createDto, string userId)
        {
            try
            {
                // Validate that quiz has at least one question
                if (createDto.Questions == null || !createDto.Questions.Any())
                {
                    _logger.LogWarning("Attempt to create quiz without questions");
                    return null;
                }

                // Validate each question has at least 2 choices and exactly one correct answer
                foreach (var question in createDto.Questions)
                {
                    if (question.Choices == null || question.Choices.Count < 2)
                    {
                        _logger.LogWarning("Question must have at least 2 choices");
                        return null;
                    }

                    var correctCount = question.Choices.Count(c => c.IsCorrect);
                    if (correctCount != 1)
                    {
                        _logger.LogWarning("Question must have exactly one correct answer");
                        return null;
                    }
                }

                // Map DTO to entity
                var quiz = _mapper.Map<Quiz>(createDto);
                quiz.CreatedById = userId;
                quiz.CreatedAt = DateTimeOffset.UtcNow;
                quiz.IsActive = true;

                _context.Quizzes.Add(quiz);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Quiz created successfully: {QuizId} by user {UserId}", quiz.Id, userId);

                return await GetQuizByIdAsync(quiz.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating quiz for user {UserId}", userId);
                return null;
            }
        }

        public async Task<QuizResponseDto?> GetQuizByIdAsync(int quizId)
        {
            try
            {
                var quiz = await _context.Quizzes
                    .Include(q => q.CreatedBy)
                    .Include(q => q.Questions)
                        .ThenInclude(q => q.Choices)
                    .FirstOrDefaultAsync(q => q.Id == quizId);

                if (quiz == null)
                {
                    _logger.LogWarning("Quiz not found: {QuizId}", quizId);
                    return null;
                }

                return _mapper.Map<QuizResponseDto>(quiz);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving quiz {QuizId}", quizId);
                return null;
            }
        }

        public async Task<List<QuizSummaryDto>> GetAllQuizzesAsync()
        {
            try
            {
                var quizzes = await _context.Quizzes
                    .Include(q => q.CreatedBy)
                    .Include(q => q.Questions)
                    .Include(q => q.QuizAttempts)
                    .OrderByDescending(q => q.CreatedAt)
                    .ToListAsync();

                return _mapper.Map<List<QuizSummaryDto>>(quizzes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all quizzes");
                return new List<QuizSummaryDto>();
            }
        }

        public async Task<List<QuizSummaryDto>> GetActiveQuizzesAsync()
        {
            try
            {
                var quizzes = await _context.Quizzes
                    .Include(q => q.CreatedBy)
                    .Include(q => q.Questions)
                    .Include(q => q.QuizAttempts)
                    .Where(q => q.IsActive)
                    .OrderByDescending(q => q.CreatedAt)
                    .ToListAsync();

                return _mapper.Map<List<QuizSummaryDto>>(quizzes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving active quizzes");
                return new List<QuizSummaryDto>();
            }
        }

        public async Task<QuizResponseDto?> UpdateQuizAsync(int quizId, UpdateQuizDto updateDto, string userId)
        {
            try
            {
                var quiz = await _context.Quizzes
                    .Include(q => q.CreatedBy)
                    .Include(q => q.Questions)
                        .ThenInclude(q => q.Choices)
                    .FirstOrDefaultAsync(q => q.Id == quizId);

                if (quiz == null)
                {
                    _logger.LogWarning("Quiz not found: {QuizId}", quizId);
                    return null;
                }

                // Check if user is the creator or admin
                if (quiz.CreatedById != userId)
                {
                    _logger.LogWarning("User {UserId} attempted to update quiz {QuizId} without permission", userId, quizId);
                    return null;
                }

                // Update properties
                quiz.Title = updateDto.Title;
                quiz.Description = updateDto.Description;
                quiz.IsActive = updateDto.IsActive;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Quiz updated: {QuizId} by user {UserId}", quizId, userId);

                return _mapper.Map<QuizResponseDto>(quiz);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating quiz {QuizId}", quizId);
                return null;
            }
        }

        public async Task<bool> DeleteQuizAsync(int quizId, string userId)
        {
            try
            {
                var quiz = await _context.Quizzes
                    .FirstOrDefaultAsync(q => q.Id == quizId);

                if (quiz == null)
                {
                    _logger.LogWarning("Quiz not found: {QuizId}", quizId);
                    return false;
                }

                // Check if user is the creator or admin
                if (quiz.CreatedById != userId)
                {
                    _logger.LogWarning("User {UserId} attempted to delete quiz {QuizId} without permission", userId, quizId);
                    return false;
                }

                _context.Quizzes.Remove(quiz);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Quiz deleted: {QuizId} by user {UserId}", quizId, userId);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting quiz {QuizId}", quizId);
                return false;
            }
        }

        public async Task<bool> ToggleQuizStatusAsync(int quizId, string userId)
        {
            try
            {
                var quiz = await _context.Quizzes
                    .FirstOrDefaultAsync(q => q.Id == quizId);

                if (quiz == null)
                {
                    _logger.LogWarning("Quiz not found: {QuizId}", quizId);
                    return false;
                }

                // Check if user is the creator or admin
                if (quiz.CreatedById != userId)
                {
                    _logger.LogWarning("User {UserId} attempted to toggle quiz {QuizId} without permission", userId, quizId);
                    return false;
                }

                quiz.IsActive = !quiz.IsActive;
                await _context.SaveChangesAsync();

                _logger.LogInformation("Quiz status toggled: {QuizId} to {Status}", quizId, quiz.IsActive);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling quiz status {QuizId}", quizId);
                return false;
            }
        }

        public async Task<QuizResponseDto?> AddQuestionToQuizAsync(int quizId, CreateQuestionDto questionDto, string userId)
        {
            try
            {
                var quiz = await _context.Quizzes
                    .Include(q => q.Questions)
                    .FirstOrDefaultAsync(q => q.Id == quizId);

                if (quiz == null)
                {
                    _logger.LogWarning("Quiz not found: {QuizId}", quizId);
                    return null;
                }

                // Check if user is the creator
                if (quiz.CreatedById != userId)
                {
                    _logger.LogWarning("User {UserId} attempted to add question to quiz {QuizId} without permission", userId, quizId);
                    return null;
                }

                // Validate question
                if (questionDto.Choices == null || questionDto.Choices.Count < 2)
                {
                    _logger.LogWarning("Question must have at least 2 choices");
                    return null;
                }

                var correctCount = questionDto.Choices.Count(c => c.IsCorrect);
                if (correctCount != 1)
                {
                    _logger.LogWarning("Question must have exactly one correct answer");
                    return null;
                }

                var question = _mapper.Map<Question>(questionDto);
                question.QuizId = quizId;

                _context.Questions.Add(question);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Question added to quiz {QuizId}", quizId);

                return await GetQuizByIdAsync(quizId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding question to quiz {QuizId}", quizId);
                return null;
            }
        }

        public async Task<bool> RemoveQuestionFromQuizAsync(int quizId, int questionId, string userId)
        {
            try
            {
                var quiz = await _context.Quizzes
                    .Include(q => q.Questions)
                    .FirstOrDefaultAsync(q => q.Id == quizId);

                if (quiz == null)
                {
                    _logger.LogWarning("Quiz not found: {QuizId}", quizId);
                    return false;
                }

                // Check if user is the creator
                if (quiz.CreatedById != userId)
                {
                    _logger.LogWarning("User {UserId} attempted to remove question from quiz {QuizId} without permission", userId, quizId);
                    return false;
                }

                var question = await _context.Questions
                    .FirstOrDefaultAsync(q => q.Id == questionId && q.QuizId == quizId);

                if (question == null)
                {
                    _logger.LogWarning("Question {QuestionId} not found in quiz {QuizId}", questionId, quizId);
                    return false;
                }

                _context.Questions.Remove(question);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Question {QuestionId} removed from quiz {QuizId}", questionId, quizId);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing question {QuestionId} from quiz {QuizId}", questionId, quizId);
                return false;
            }
        }

        public async Task<List<QuizSummaryDto>> GetUserCreatedQuizzesAsync(string userId)
        {
            try
            {
                var quizzes = await _context.Quizzes
                    .Include(q => q.CreatedBy)
                    .Include(q => q.Questions)
                    .Include(q => q.QuizAttempts)
                    .Where(q => q.CreatedById == userId)
                    .OrderByDescending(q => q.CreatedAt)
                    .ToListAsync();

                return _mapper.Map<List<QuizSummaryDto>>(quizzes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving quizzes for user {UserId}", userId);
                return new List<QuizSummaryDto>();
            }
        }
    }
}
