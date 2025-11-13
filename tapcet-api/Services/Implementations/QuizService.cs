using AutoMapper;
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
        public Task<QuizResponseDto?> AddQuestionToQuizAsync(int quizId, CreateQuestionDto questionDto, string userId)
        {
            
            throw new NotImplementedException();
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

        public Task<QuizResponseDto?> GetQuizByIdAsync(int quizId)
        {
            throw new NotImplementedException();
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
