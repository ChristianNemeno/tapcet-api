using tapcet_api.DTO.Attempt;

namespace tapcet_api.Services.Interfaces
{
    public interface IQuizAttemptService
    {
        Task<QuizAttemptResponseDto?> StartQuizAttemptAsync(int quizId, string userId);
        Task<QuizResultDto?> SubmitQuizAsync(SubmitQuizDto submitDto, string userId);
        

        Task<QuizAttemptResponseDto?> GetAttemptByIdAsync(int attemptId, string userId);
        Task<List<QuizAttemptResponseDto>> GetUserAttemptsAsync(string userId);
        Task<List<QuizAttemptResponseDto>> GetQuizAttemptsAsync(int quizId);
        

        Task<QuizResultDto?> GetAttemptResultAsync(int attemptId, string userId);
        Task<List<QuizAttemptResponseDto>> GetQuizLeaderboardAsync(int quizId, int topCount = 10);
    }
}