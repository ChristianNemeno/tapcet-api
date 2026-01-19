using tapcet_api.DTO.Quiz;

namespace tapcet_api.Services.Interfaces
{
    public interface IQuizService
    {
        // Quiz CRUD operations
        Task<QuizResponseDto?> CreateQuizAsync(CreateQuizDto createDto, string userId);
        Task<QuizResponseDto?> GetQuizByIdAsync(int quizId);
        Task<List<QuizSummaryDto>> GetAllQuizzesAsync();
        Task<List<QuizSummaryDto>> GetActiveQuizzesAsync();
        Task<QuizResponseDto?> UpdateQuizAsync(int quizId, UpdateQuizDto updateDto, string userId);
        Task<bool> DeleteQuizAsync(int quizId, string userId);
        Task<bool> ToggleQuizStatusAsync(int quizId, string userId);

        // Quiz questions
        Task<QuizResponseDto?> AddQuestionToQuizAsync(int quizId, CreateQuestionDto questionDto, string userId);
        Task<bool> RemoveQuestionFromQuizAsync(int quizId, int questionId, string userId);

        // User specific
        Task<List<QuizSummaryDto>> GetUserCreatedQuizzesAsync(string userId);

        // Unit hierarchy support
        Task<List<QuizSummaryDto>> GetQuizzesByUnitAsync(int? unitId);
        Task<QuizResponseDto?> AssignQuizToUnitAsync(int quizId, int unitId, int orderIndex, string userId);
        Task<QuizResponseDto?> ReorderQuizAsync(int quizId, int newOrderIndex, string userId);
    }
}