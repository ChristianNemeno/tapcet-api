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



        /**
         * Notes: Naay validation sa service too , so three validation, sa DTO , sa Models and this 
         */
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

                // validation checks if user is the creator
                if (quiz.CreatedById != userId)
                {
                    _logger.LogWarning("User {UserId} attempted to add question to quiz {QuizId} without permission", userId, quizId);
                    return null;
                }

                // validate question, given my wants 
                if (questionDto.Choices == null || questionDto.Choices.Count < 2)
                {
                    _logger.LogWarning("Question must have at least 2 choices");
                    return null;
                }

                //learn lambda
                var correctCount = questionDto.Choices.Count(c => c.IsCorrect);
                if (correctCount != 1)
                {
                    _logger.LogWarning("Question must have exactly one correct answer");
                    return null;
                }


                // always use mapper
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

        public async Task<QuizResponseDto?> CreateQuizAsync(CreateQuizDto createDto, string userId)
        {
            try
            {

                if (createDto.Questions == null | !createDto.Questions.Any())
                {
                    _logger.LogWarning("Attemp to create quiz without questions");
                    return null;
                }

                foreach (var question in createDto.Questions)
                {

                    if(question.Choices == null || !question.Choices.Any())
                    {
                        _logger.LogWarning("Quesiton must have atleast 2 choices");
                        return null;
                    }

                    var correctCount = question.Choices.Count(c => c.IsCorrect);

                    if (correctCount != 1)
                    {
                        _logger.LogWarning("Question must only have 1 correct answer");
                        return null;
                    }

                }

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

        public Task<bool> DeleteQuizAsync(int quizId, string userId)
        {
            throw new NotImplementedException();
        }

        public Task<List<QuizSummaryDto>> GetActiveQuizzesAsync()
        {
            throw new NotImplementedException();
        }

        public Task<List<QuizSummaryDto>> GetAllQuizzesAsync()
        {
            throw new NotImplementedException();
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

        public Task<List<QuizSummaryDto>> GetUserCreatedQuizzesAsync(string userId)
        {
            throw new NotImplementedException();
        }

        public Task<bool> RemoveQuestionFromQuizAsync(int quizId, int questionId, string userId)
        {
            throw new NotImplementedException();
        }

        public Task<bool> ToggleQuizStatusAsync(int quizId, string userId)
        {
            throw new NotImplementedException();
        }

        public Task<QuizResponseDto?> UpdateQuizAsync(int quizId, UpdateQuizDto updateDto, string userId)
        {
            throw new NotImplementedException();
        }



    }
}
