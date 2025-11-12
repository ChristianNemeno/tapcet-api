using AutoMapper;
using tapcet_api.Data;
using tapcet_api.DTO.Quiz;
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

        public Task<QuizResponseDto?> CreateQuizAsync(CreateQuizDto createDto, string userId)
        {
            throw new NotImplementedException();
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
