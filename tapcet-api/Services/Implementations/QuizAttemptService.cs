using AutoMapper;
using Microsoft.EntityFrameworkCore;
using tapcet_api.Data;
using tapcet_api.DTO.Attempt;
using tapcet_api.Models;
using tapcet_api.Services.Interfaces;

namespace tapcet_api.Services.Implementations
{
    public class QuizAttemptService : IQuizAttemptService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<QuizAttemptService> _logger;

        public QuizAttemptService(
            ApplicationDbContext context,
            IMapper mapper,
            ILogger<QuizAttemptService> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<QuizAttemptResponseDto?> StartQuizAttemptAsync(int quizId, string userId)
        {
            try
            {
                var quiz = await _context.Quizzes
                    .Include(q => q.Questions)
                    .FirstOrDefaultAsync(q => q.Id == quizId && q.IsActive);

                if (quiz == null)
                {
                    _logger.LogWarning("Quiz not found or inactive: {QuizId}", quizId);
                    return null;
                }

                if (!quiz.Questions.Any())
                {
                    _logger.LogWarning("Quiz {QuizId} has no questions", quizId);
                    return null;
                }

                var attempt = new QuizAttempt
                {
                    QuizId = quizId,
                    UserId = userId,
                    StartedAt = DateTimeOffset.UtcNow,
                    Score = 0
                };

                _context.QuizAttempts.Add(attempt);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Quiz attempt started: {AttemptId} for quiz {QuizId} by user {UserId}", 
                    attempt.Id, quizId, userId);

                // Load navigation properties for response
                await _context.Entry(attempt)
                    .Reference(a => a.Quiz)
                    .LoadAsync();
                await _context.Entry(attempt)
                    .Reference(a => a.User)
                    .LoadAsync();

                return _mapper.Map<QuizAttemptResponseDto>(attempt);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error starting quiz attempt for quiz {QuizId}", quizId);
                return null;
            }
        }

        public async Task<QuizResultDto?> SubmitQuizAsync(SubmitQuizDto submitDto, string userId)
        {
            try
            {
                var attempt = await _context.QuizAttempts
                    .Include(a => a.Quiz)
                        .ThenInclude(q => q.Questions)
                            .ThenInclude(q => q.Choices)
                    .Include(a => a.User)
                    .FirstOrDefaultAsync(a => a.Id == submitDto.QuizAttemptId && a.UserId == userId);

                if (attempt == null)
                {
                    _logger.LogWarning("Quiz attempt not found or user mismatch: {AttemptId}", submitDto.QuizAttemptId);
                    return null;
                }

                if (attempt.CompletedAt.HasValue)
                {
                    _logger.LogWarning("Quiz attempt already completed: {AttemptId}", submitDto.QuizAttemptId);
                    return null;
                }

                // Validate answers count
                var totalQuestions = attempt.Quiz.Questions.Count;
                if (submitDto.Answers.Count != totalQuestions)
                {
                    _logger.LogWarning("Answer count mismatch for attempt {AttemptId}. Expected {Expected}, got {Actual}",
                        submitDto.QuizAttemptId, totalQuestions, submitDto.Answers.Count);
                    return null;
                }

                // Process answers and calculate score
                int correctAnswers = 0;
                var questionResults = new List<QuestionResultDto>();

                foreach (var answerDto in submitDto.Answers)
                {
                    var question = attempt.Quiz.Questions.FirstOrDefault(q => q.Id == answerDto.QuestionId);
                    if (question == null)
                    {
                        _logger.LogWarning("Question {QuestionId} not found in quiz", answerDto.QuestionId);
                        continue;
                    }

                    var selectedChoice = question.Choices.FirstOrDefault(c => c.Id == answerDto.ChoiceId);
                    var correctChoice = question.Choices.FirstOrDefault(c => c.IsCorrect);

                    if (selectedChoice == null || correctChoice == null)
                    {
                        _logger.LogWarning("Invalid choice for question {QuestionId}", answerDto.QuestionId);
                        continue;
                    }

                    bool isCorrect = selectedChoice.IsCorrect;
                    if (isCorrect)
                    {
                        correctAnswers++;
                    }

                    // Save user answer
                    var userAnswer = new UserAnswer
                    {
                        QuizAttemptId = attempt.Id,
                        QuestionId = answerDto.QuestionId,
                        ChoiceId = answerDto.ChoiceId,
                        AnsweredAt = DateTimeOffset.UtcNow
                    };
                    _context.UserAnswers.Add(userAnswer);

                    // Add to results
                    questionResults.Add(new QuestionResultDto
                    {
                        QuestionId = question.Id,
                        QuestionText = question.Text,
                        Explanation = question.Explanation,
                        SelectedChoiceId = selectedChoice.Id,
                        SelectedChoiceText = selectedChoice.Text,
                        CorrectChoiceId = correctChoice.Id,
                        CorrectChoiceText = correctChoice.Text,
                        IsCorrect = isCorrect
                    });
                }

                // Calculate score (percentage)
                int score = (int)Math.Round((double)correctAnswers / totalQuestions * 100);

                // Update attempt
                attempt.Score = score;
                attempt.CompletedAt = DateTimeOffset.UtcNow;

                await _context.SaveChangesAsync();

                // Update user statistics
                await UpdateUserStatisticsAsync(userId);

                _logger.LogInformation("Quiz attempt completed: {AttemptId} with score {Score}%", attempt.Id, score);

                // Build result DTO
                var result = new QuizResultDto
                {
                    QuizAttemptId = attempt.Id,
                    QuizTitle = attempt.Quiz.Title,
                    TotalQuestions = totalQuestions,
                    CorrectAnswers = correctAnswers,
                    IncorrectAnswers = totalQuestions - correctAnswers,
                    Score = score,
                    Percentage = score, 
                    StartedAt = attempt.StartedAt,
                    CompletedAt = attempt.CompletedAt.Value,
                    Duration = attempt.CompletedAt.Value - attempt.StartedAt,
                    QuestionResults = questionResults
                };

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error submitting quiz attempt {AttemptId}", submitDto.QuizAttemptId);
                return null;
            }
        }

        public async Task<QuizAttemptResponseDto?> GetAttemptByIdAsync(int attemptId, string userId)
        {
            try
            {
                var attempt = await _context.QuizAttempts
                    .Include(a => a.Quiz)
                    .Include(a => a.User)
                    .FirstOrDefaultAsync(a => a.Id == attemptId && a.UserId == userId);

                if (attempt == null)
                {
                    _logger.LogWarning("Quiz attempt not found or user mismatch: {AttemptId}", attemptId);
                    return null;
                }

                return _mapper.Map<QuizAttemptResponseDto>(attempt);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving quiz attempt {AttemptId}", attemptId);
                return null;
            }
        }

        public async Task<List<QuizAttemptResponseDto>> GetUserAttemptsAsync(string userId)
        {
            try
            {
                var attempts = await _context.QuizAttempts
                    .Include(a => a.Quiz)
                    .Include(a => a.User)
                    .Where(a => a.UserId == userId)
                    .OrderByDescending(a => a.StartedAt)
                    .ToListAsync();

                return _mapper.Map<List<QuizAttemptResponseDto>>(attempts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving attempts for user {UserId}", userId);
                return new List<QuizAttemptResponseDto>();
            }
        }

        public async Task<List<QuizAttemptResponseDto>> GetQuizAttemptsAsync(int quizId)
        {
            try
            {
                var attempts = await _context.QuizAttempts
                    .Include(a => a.Quiz)
                    .Include(a => a.User)
                    .Where(a => a.QuizId == quizId && a.CompletedAt.HasValue)
                    .OrderByDescending(a => a.Score)
                    .ThenBy(a => a.CompletedAt!.Value - a.StartedAt)
                    .ToListAsync();

                return _mapper.Map<List<QuizAttemptResponseDto>>(attempts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving attempts for quiz {QuizId}", quizId);
                return new List<QuizAttemptResponseDto>();
            }
        }

        public async Task<QuizResultDto?> GetAttemptResultAsync(int attemptId, string userId)
        {
            try
            {
                var attempt = await _context.QuizAttempts
                    .Include(a => a.Quiz)
                        .ThenInclude(q => q.Questions)
                            .ThenInclude(q => q.Choices)
                    .Include(a => a.UserAnswers)
                        .ThenInclude(ua => ua.Choice)
                    .Include(a => a.UserAnswers)
                        .ThenInclude(ua => ua.Question)
                    .FirstOrDefaultAsync(a => a.Id == attemptId && a.UserId == userId);

                if (attempt == null || !attempt.CompletedAt.HasValue)
                {
                    _logger.LogWarning("Quiz attempt not found, incomplete, or user mismatch: {AttemptId}", attemptId);
                    return null;
                }

                var questionResults = new List<QuestionResultDto>();
                int correctAnswers = 0;

                foreach (var question in attempt.Quiz.Questions)
                {
                    var userAnswer = attempt.UserAnswers.FirstOrDefault(ua => ua.QuestionId == question.Id);
                    var correctChoice = question.Choices.FirstOrDefault(c => c.IsCorrect);

                    if (userAnswer != null && correctChoice != null)
                    {
                        bool isCorrect = userAnswer.Choice.IsCorrect;
                        if (isCorrect) correctAnswers++;

                        questionResults.Add(new QuestionResultDto
                        {
                            QuestionId = question.Id,
                            QuestionText = question.Text,
                            Explanation = question.Explanation,
                            SelectedChoiceId = userAnswer.ChoiceId,
                            SelectedChoiceText = userAnswer.Choice.Text,
                            CorrectChoiceId = correctChoice.Id,
                            CorrectChoiceText = correctChoice.Text,
                            IsCorrect = isCorrect
                        });
                    }
                }

                return new QuizResultDto
                {
                    QuizAttemptId = attempt.Id,
                    QuizTitle = attempt.Quiz.Title,
                    TotalQuestions = attempt.Quiz.Questions.Count,
                    CorrectAnswers = correctAnswers,
                    IncorrectAnswers = attempt.Quiz.Questions.Count - correctAnswers,
                    Score = attempt.Score,
                    Percentage = attempt.Score,
                    StartedAt = attempt.StartedAt,
                    CompletedAt = attempt.CompletedAt.Value,
                    Duration = attempt.CompletedAt.Value - attempt.StartedAt,
                    QuestionResults = questionResults
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving result for attempt {AttemptId}", attemptId);
                return null;
            }
        }

        public async Task<List<QuizAttemptResponseDto>> GetQuizLeaderboardAsync(int quizId, int topCount = 10)
        {
            try
            {
                var attempts = await _context.QuizAttempts
                    .Include(a => a.Quiz)
                    .Include(a => a.User)
                    .Where(a => a.QuizId == quizId && a.CompletedAt.HasValue)
                    .OrderByDescending(a => a.Score)
                    .ThenBy(a => a.CompletedAt!.Value - a.StartedAt)
                    .Take(topCount)
                    .ToListAsync();

                return _mapper.Map<List<QuizAttemptResponseDto>>(attempts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving leaderboard for quiz {QuizId}", quizId);
                return new List<QuizAttemptResponseDto>();
            }
        }

        private async Task UpdateUserStatisticsAsync(string userId)
        {
            try
            {
                var user = await _context.Users.FindAsync(userId);
                if (user == null) return;

                var completedAttempts = await _context.QuizAttempts
                    .Where(a => a.UserId == userId && a.CompletedAt.HasValue)
                    .ToListAsync();

                user.TotalQuizAttempts = completedAttempts.Count;
                user.AverageScore = completedAttempts.Any() 
                    ? completedAttempts.Average(a => a.Score) 
                    : 0;

                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating statistics for user {UserId}", userId);
            }
        }
    }
}
