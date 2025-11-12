using tapcet_api.DTO.Quiz;

namespace tapcet_api.Services.Interfaces
{
    public interface IQuizService
    {
        Task<QuizResponseDto?> CreateQuizAsync(CreateQuizDto createDto, string userId);
        Task<QuizResponseDto?> GetQuizByIdAsync(int quizId);
        Task<List<QuizSummaryDto>> GetAllQuizzesAsync();
        Task<List<QuizSummaryDto>> GetActiveQuizzesAsync();
        Task<QuizResponseDto?> UpdateQuizAsync(int quizId, UpdateQuizDto updateDto, string userId);
        Task<bool> DeleteQuizAsync(int quizId, string userId);
        Task<bool> ToggleQuizStatusAsync(int quizId, string userId);

        Task<QuizResponseDto?> AddQuestionToQuizAsync(int quizId, CreateQuestionDto questionDto, string userId);
        Task<bool> RemoveQuestionFromQuizAsync(int quizId, int questionId, string userId);

        Task<List<QuizSummaryDto>> GetUserCreatedQuizzesAsync(string userId);
    }
}