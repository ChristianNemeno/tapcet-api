# API Reference

## Base URL
- **Development**: `https://localhost:7237`
- **Production**: TBD

## Authentication
```
Authorization: Bearer <your-jwt-token>
```

---

## Endpoints Summary

### Authentication
| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| POST | `/api/auth/register` | No | Register new user |
| POST | `/api/auth/login` | No | Login and get JWT token |

### Educational Hierarchy
| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| **Subjects** |
| GET | `/api/subject` | No | List all subjects |
| GET | `/api/subject/{id}` | No | Get subject with courses |
| POST | `/api/subject` | Admin | Create subject |
| PUT | `/api/subject/{id}` | Admin | Update subject |
| DELETE | `/api/subject/{id}` | Admin | Delete subject |
| **Courses** |
| GET | `/api/course` | No | List all courses |
| GET | `/api/course/{id}` | No | Get course with units |
| GET | `/api/course/subject/{subjectId}` | No | Courses by subject |
| POST | `/api/course` | Yes | Create course |
| PUT | `/api/course/{id}` | Owner | Update course |
| DELETE | `/api/course/{id}` | Owner | Delete course |
| **Units** |
| GET | `/api/unit/{id}` | No | Get unit with quizzes |
| GET | `/api/unit/course/{courseId}` | No | Units by course (ordered) |
| GET | `/api/unit/{unitId}/quizzes` | No | Quizzes by unit (ordered) |
| POST | `/api/unit` | Yes | Create unit |
| PUT | `/api/unit/{id}` | Owner | Update unit |
| PATCH | `/api/unit/{id}/reorder` | Owner | Reorder unit |
| DELETE | `/api/unit/{id}` | Owner | Delete unit |

### Quiz Management
| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| GET | `/api/quiz/{id}` | No | Get quiz details |
| GET | `/api/quiz?unitId={id}` | Yes | Get quizzes by unit |
| GET | `/api/quiz/standalone` | No | Get standalone quizzes |
| GET | `/api/quiz/active` | No | Get active quizzes |
| GET | `/api/quiz/user/me` | Yes | Get my quizzes |
| POST | `/api/quiz` | Yes | Create quiz |
| PUT | `/api/quiz/{id}` | Owner | Update quiz |
| PATCH | `/api/quiz/{id}/assign-unit` | Owner | Assign to unit |
| PATCH | `/api/quiz/{id}/reorder` | Owner | Reorder in unit |
| PATCH | `/api/quiz/{id}/toggle` | Owner | Toggle active status |
| DELETE | `/api/quiz/{id}` | Owner | Delete quiz |
| POST | `/api/quiz/{id}/questions` | Owner | Add question |

### Quiz Attempts
| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| POST | `/api/quiz-attempt/start` | Yes | Start quiz attempt |
| POST | `/api/quiz-attempt/submit` | Yes | Submit quiz answers |
| GET | `/api/quiz-attempt/{id}` | Owner | Get attempt by ID |
| GET | `/api/quiz-attempt/{id}/result` | Owner | Get attempt results |
| GET | `/api/quiz-attempt/user/me` | Yes | Get user's attempts |
| GET | `/api/quiz-attempt/quiz/{quizId}/leaderboard?topCount={n}` | Yes | Get leaderboard |

---

## Data Models

### Dart Classes

```dart
// Authentication Models
class LoginRequest {
  final String email;
  final String password;

  LoginRequest({required this.email, required this.password});

  Map<String, dynamic> toJson() => {
    'email': email,
    'password': password,
  };
}

class RegisterRequest {
  final String userName;
  final String email;
  final String password;
  final String confirmPassword;

  RegisterRequest({
    required this.userName,
    required this.email,
    required this.password,
    required this.confirmPassword,
  });

  Map<String, dynamic> toJson() => {
    'userName': userName,
    'email': email,
    'password': password,
    'confirmPassword': confirmPassword,
  };
}

class AuthResponse {
  final String token;
  final String email;
  final String userName;
  final String expiresAt;

  AuthResponse({
    required this.token,
    required this.email,
    required this.userName,
    required this.expiresAt,
  });

  factory AuthResponse.fromJson(Map<String, dynamic> json) => AuthResponse(
    token: json['token'],
    email: json['email'],
    userName: json['userName'],
    expiresAt: json['expiresAt'],
  );
}

// Hierarchy Models
class Subject {
  final int id;
  final String name;
  final String? description;
  final int courseCount;
  final List<Course>? courses;

  Subject({
    required this.id,
    required this.name,
    this.description,
    required this.courseCount,
    this.courses,
  });

  factory Subject.fromJson(Map<String, dynamic> json) => Subject(
    id: json['id'],
    name: json['name'],
    description: json['description'],
    courseCount: json['courseCount'],
    courses: json['courses'] != null
        ? (json['courses'] as List).map((e) => Course.fromJson(e)).toList()
        : null,
  );
}

class Course {
  final int id;
  final String title;
  final String? description;
  final int subjectId;
  final String subjectName;
  final int unitCount;
  final List<Unit>? units;

  Course({
    required this.id,
    required this.title,
    this.description,
    required this.subjectId,
    required this.subjectName,
    required this.unitCount,
    this.units,
  });

  factory Course.fromJson(Map<String, dynamic> json) => Course(
    id: json['id'],
    title: json['title'],
    description: json['description'],
    subjectId: json['subjectId'],
    subjectName: json['subjectName'],
    unitCount: json['unitCount'],
    units: json['units'] != null
        ? (json['units'] as List).map((e) => Unit.fromJson(e)).toList()
        : null,
  );
}

class Unit {
  final int id;
  final String title;
  final int orderIndex;
  final int courseId;
  final String courseTitle;
  final int quizCount;
  final List<QuizSummary>? quizzes;

  Unit({
    required this.id,
    required this.title,
    required this.orderIndex,
    required this.courseId,
    required this.courseTitle,
    required this.quizCount,
    this.quizzes,
  });

  factory Unit.fromJson(Map<String, dynamic> json) => Unit(
    id: json['id'],
    title: json['title'],
    orderIndex: json['orderIndex'],
    courseId: json['courseId'],
    courseTitle: json['courseTitle'],
    quizCount: json['quizCount'],
    quizzes: json['quizzes'] != null
        ? (json['quizzes'] as List).map((e) => QuizSummary.fromJson(e)).toList()
        : null,
  );
}

// Quiz Models
class QuizSummary {
  final int id;
  final String title;
  final String? description;
  final int? unitId;
  final String? unitTitle;
  final int orderIndex;
  final String createdAt;
  final String createdByName;
  final bool isActive;
  final int questionCount;
  final int attemptCount;

  QuizSummary({
    required this.id,
    required this.title,
    this.description,
    this.unitId,
    this.unitTitle,
    required this.orderIndex,
    required this.createdAt,
    required this.createdByName,
    required this.isActive,
    required this.questionCount,
    required this.attemptCount,
  });

  factory QuizSummary.fromJson(Map<String, dynamic> json) => QuizSummary(
    id: json['id'],
    title: json['title'],
    description: json['description'],
    unitId: json['unitId'],
    unitTitle: json['unitTitle'],
    orderIndex: json['orderIndex'],
    createdAt: json['createdAt'],
    createdByName: json['createdByName'],
    isActive: json['isActive'],
    questionCount: json['questionCount'],
    attemptCount: json['attemptCount'],
  );
}

class Quiz {
  final int id;
  final String title;
  final String? description;
  final int? unitId;
  final String? unitTitle;
  final int orderIndex;
  final String createdAt;
  final String createdById;
  final String createdByName;
  final bool isActive;
  final int questionCount;
  final List<Question> questions;

  Quiz({
    required this.id,
    required this.title,
    this.description,
    this.unitId,
    this.unitTitle,
    required this.orderIndex,
    required this.createdAt,
    required this.createdById,
    required this.createdByName,
    required this.isActive,
    required this.questionCount,
    required this.questions,
  });

  factory Quiz.fromJson(Map<String, dynamic> json) => Quiz(
    id: json['id'],
    title: json['title'],
    description: json['description'],
    unitId: json['unitId'],
    unitTitle: json['unitTitle'],
    orderIndex: json['orderIndex'],
    createdAt: json['createdAt'],
    createdById: json['createdById'],
    createdByName: json['createdByName'],
    isActive: json['isActive'],
    questionCount: json['questionCount'],
    questions: (json['questions'] as List).map((e) => Question.fromJson(e)).toList(),
  );
}

class Question {
  final int id;
  final String text;
  final String? explanation;
  final String? imageUrl;
  final List<Choice> choices;

  Question({
    required this.id,
    required this.text,
    this.explanation,
    this.imageUrl,
    required this.choices,
  });

  factory Question.fromJson(Map<String, dynamic> json) => Question(
    id: json['id'],
    text: json['text'],
    explanation: json['explanation'],
    imageUrl: json['imageUrl'],
    choices: (json['choices'] as List).map((e) => Choice.fromJson(e)).toList(),
  );
}

class Choice {
  final int id;
  final String text;
  final bool isCorrect;

  Choice({required this.id, required this.text, required this.isCorrect});

  factory Choice.fromJson(Map<String, dynamic> json) => Choice(
    id: json['id'],
    text: json['text'],
    isCorrect: json['isCorrect'],
  );
}

// Quiz Attempt Models
class QuizAttempt {
  final int id;
  final int quizId;
  final String quizTitle;
  final String userId;
  final String userName;
  final String startedAt;
  final String? completedAt;
  final int score;
  final bool isCompleted;

  QuizAttempt({
    required this.id,
    required this.quizId,
    required this.quizTitle,
    required this.userId,
    required this.userName,
    required this.startedAt,
    this.completedAt,
    required this.score,
    required this.isCompleted,
  });

  factory QuizAttempt.fromJson(Map<String, dynamic> json) => QuizAttempt(
    id: json['id'],
    quizId: json['quizId'],
    quizTitle: json['quizTitle'],
    userId: json['userId'],
    userName: json['userName'],
    startedAt: json['startedAt'],
    completedAt: json['completedAt'],
    score: json['score'],
    isCompleted: json['isCompleted'],
  );
}

class QuizResult {
  final int quizAttemptId;
  final String quizTitle;
  final int totalQuestions;
  final int correctAnswers;
  final int incorrectAnswers;
  final int score;
  final double percentage;
  final String startedAt;
  final String completedAt;
  final String duration;
  final List<QuestionResult> questionResults;

  QuizResult({
    required this.quizAttemptId,
    required this.quizTitle,
    required this.totalQuestions,
    required this.correctAnswers,
    required this.incorrectAnswers,
    required this.score,
    required this.percentage,
    required this.startedAt,
    required this.completedAt,
    required this.duration,
    required this.questionResults,
  });

  factory QuizResult.fromJson(Map<String, dynamic> json) => QuizResult(
    quizAttemptId: json['quizAttemptId'],
    quizTitle: json['quizTitle'],
    totalQuestions: json['totalQuestions'],
    correctAnswers: json['correctAnswers'],
    incorrectAnswers: json['incorrectAnswers'],
    score: json['score'],
    percentage: json['percentage'].toDouble(),
    startedAt: json['startedAt'],
    completedAt: json['completedAt'],
    duration: json['duration'],
    questionResults: (json['questionResults'] as List)
        .map((e) => QuestionResult.fromJson(e))
        .toList(),
  );
}

class QuestionResult {
  final int questionId;
  final String questionText;
  final String? explanation;
  final int selectedChoiceId;
  final String selectedChoiceText;
  final int correctChoiceId;
  final String correctChoiceText;
  final bool isCorrect;

  QuestionResult({
    required this.questionId,
    required this.questionText,
    this.explanation,
    required this.selectedChoiceId,
    required this.selectedChoiceText,
    required this.correctChoiceId,
    required this.correctChoiceText,
    required this.isCorrect,
  });

  factory QuestionResult.fromJson(Map<String, dynamic> json) => QuestionResult(
    questionId: json['questionId'],
    questionText: json['questionText'],
    explanation: json['explanation'],
    selectedChoiceId: json['selectedChoiceId'],
    selectedChoiceText: json['selectedChoiceText'],
    correctChoiceId: json['correctChoiceId'],
    correctChoiceText: json['correctChoiceText'],
    isCorrect: json['isCorrect'],
  );
}
```

---

## Key Request/Response Examples

### Authentication

```dart
// Login
final response = await http.post(
  Uri.parse('$baseUrl/api/auth/login'),
  headers: {'Content-Type': 'application/json'},
  body: jsonEncode({
    'email': 'user@example.com',
    'password': 'Password123!',
  }),
);

// Response
{
  "token": "eyJhbGci...",
  "email": "user@example.com",
  "userName": "johndoe",
  "expiresAt": "2024-01-15T15:30:00Z"
}
```

### Create Complete Hierarchy

```dart
// 1. Create Subject (Admin only)
await http.post(
  Uri.parse('$baseUrl/api/subject'),
  headers: headers,
  body: jsonEncode({'name': 'Science', 'description': 'Natural sciences'}),
);

// 2. Create Course
await http.post(
  Uri.parse('$baseUrl/api/course'),
  headers: headers,
  body: jsonEncode({'title': 'High School Physics', 'subjectId': 1}),
);

// 3. Create Unit
await http.post(
  Uri.parse('$baseUrl/api/unit'),
  headers: headers,
  body: jsonEncode({
    'title': 'Forces and Newton\'s Laws',
    'orderIndex': 1,
    'courseId': 1
  }),
);

// 4. Create Quiz
await http.post(
  Uri.parse('$baseUrl/api/quiz'),
  headers: headers,
  body: jsonEncode({
    'title': 'Newton\'s Laws Quiz',
    'unitId': 1,
    'orderIndex': 1,
    'questions': [
      {
        'text': 'What is Newton\'s First Law?',
        'choices': [
          {'text': 'F = ma', 'isCorrect': false},
          {'text': 'An object at rest stays at rest', 'isCorrect': true}
        ]
      }
    ]
  }),
);
```

### Quiz Taking Flow

```dart
// 1. Start Quiz
final startResponse = await http.post(
  Uri.parse('$baseUrl/api/quiz-attempt/start'),
  headers: headers,
  body: jsonEncode({'quizId': 1}),
);
final attempt = QuizAttempt.fromJson(jsonDecode(startResponse.body));

// 2. Submit Answers
final submitResponse = await http.post(
  Uri.parse('$baseUrl/api/quiz-attempt/submit'),
  headers: headers,
  body: jsonEncode({
    'quizAttemptId': attempt.id,
    'answers': [
      {'questionId': 1, 'choiceId': 2},
      {'questionId': 2, 'choiceId': 5}
    ]
  }),
);
final result = QuizResult.fromJson(jsonDecode(submitResponse.body));
```

### Hierarchy Navigation

```dart
// Browse path: Subject ? Course ? Unit ? Quiz
final subjectsResponse = await http.get(Uri.parse('$baseUrl/api/subject'));
final subjects = (jsonDecode(subjectsResponse.body) as List)
    .map((e) => Subject.fromJson(e))
    .toList();

final subjectResponse = await http.get(Uri.parse('$baseUrl/api/subject/$id'));
final subject = Subject.fromJson(jsonDecode(subjectResponse.body)); // includes courses

final courseResponse = await http.get(Uri.parse('$baseUrl/api/course/$id'));
final course = Course.fromJson(jsonDecode(courseResponse.body)); // includes units

final quizzesResponse = await http.get(Uri.parse('$baseUrl/api/unit/$unitId/quizzes'));
final quizzes = (jsonDecode(quizzesResponse.body) as List)
    .map((e) => QuizSummary.fromJson(e))
    .toList();
```

### Breadcrumb Builder

```dart
class BreadcrumbItem {
  final String name;
  final String? path;

  BreadcrumbItem({required this.name, this.path});
}

Future<List<BreadcrumbItem>> getBreadcrumbs(int quizId) async {
  final quizResponse = await http.get(Uri.parse('$baseUrl/api/quiz/$quizId'));
  final quiz = Quiz.fromJson(jsonDecode(quizResponse.body));
  
  if (quiz.unitId == null) {
    return [
      BreadcrumbItem(name: 'Home', path: '/'),
      BreadcrumbItem(name: quiz.title),
    ];
  }
  
  final unitResponse = await http.get(Uri.parse('$baseUrl/api/unit/${quiz.unitId}'));
  final unit = Unit.fromJson(jsonDecode(unitResponse.body));
  
  final courseResponse = await http.get(Uri.parse('$baseUrl/api/course/${unit.courseId}'));
  final course = Course.fromJson(jsonDecode(courseResponse.body));
  
  final subjectResponse = await http.get(Uri.parse('$baseUrl/api/subject/${course.subjectId}'));
  final subject = Subject.fromJson(jsonDecode(subjectResponse.body));
  
  return [
    BreadcrumbItem(name: 'Home', path: '/'),
    BreadcrumbItem(name: subject.name, path: '/subjects/${subject.id}'),
    BreadcrumbItem(name: course.title, path: '/courses/${course.id}'),
    BreadcrumbItem(name: unit.title, path: '/units/${unit.id}'),
    BreadcrumbItem(name: quiz.title),
  ];
}
```

---

## Validation Rules

### Subject
- Name: Required, 2-100 chars, unique (case-insensitive)
- Description: Optional, max 500 chars

### Course
- Title: Required, 3-100 chars
- Description: Optional, max 500 chars
- SubjectId: Required, must exist

### Unit
- Title: Required, 3-100 chars
- OrderIndex: Required, 1-999, unique within course
- CourseId: Required, must exist

### Quiz
- Title: Required, 3-200 chars
- Description: Optional, max 2000 chars
- UnitId: Optional (null for standalone)
- OrderIndex: Optional, 1-999, defaults to 1
- Questions: Required, min 1
- Each Question: 2-6 choices, exactly 1 correct

---

## HTTP Status Codes

| Code | Meaning |
|------|---------|
| 200 | OK - Successful request |
| 201 | Created - Resource created |
| 204 | No Content - Successful delete |
| 400 | Bad Request - Validation error |
| 401 | Unauthorized - Missing/invalid token |
| 403 | Forbidden - Not authorized |
| 404 | Not Found - Resource doesn't exist |
| 500 | Server Error - Unexpected error |

## Error Response Format

```json
{
  "message": "Human-readable error message",
  "errors": ["Optional array of details"]
}
```

---

## Quick Tips

### Efficient Data Loading
```dart
// Single call loads nested data
final subject = await apiClient.getSubjectById(id); // includes courses
final course = await apiClient.getCourseById(id);   // includes units
final unit = await apiClient.getUnitById(id);       // includes quizzes
```

### Filtering Options
```dart
GET /api/quiz                  // All quizzes (authenticated)
GET /api/quiz?unitId=5        // Quizzes in unit 5
GET /api/quiz/standalone      // Quizzes without unit
GET /api/quiz/active          // Only active quizzes
GET /api/quiz/user/me         // My created quizzes
```

### Managing Order
```dart
// Reorder unit within course
await http.patch(
  Uri.parse('$baseUrl/api/unit/$id/reorder'),
  headers: headers,
  body: jsonEncode({'orderIndex': 3}),
);

// Reorder quiz within unit
await http.patch(
  Uri.parse('$baseUrl/api/quiz/$id/reorder'),
  headers: headers,
  body: jsonEncode({'orderIndex': 5}),
);

// Assign quiz to unit
await http.patch(
  Uri.parse('$baseUrl/api/quiz/$id/assign-unit'),
  headers: headers,
  body: jsonEncode({'unitId': 1, 'orderIndex': 2}),
);
```

---

## Flutter/Dart Integration

### API Client Setup

```dart
// lib/services/api_client.dart
import 'dart:convert';
import 'package:http/http.dart' as http;
import 'package:shared_preferences/shared_preferences.dart';

class ApiClient {
  static const String baseUrl = 'https://localhost:7237';
  
  Future<Map<String, String>> _getHeaders() async {
    final prefs = await SharedPreferences.getInstance();
    final token = prefs.getString('authToken');
    
    return {
      'Content-Type': 'application/json',
      if (token != null) 'Authorization': 'Bearer $token',
    };
  }
  
  Future<T> get<T>(
    String endpoint,
    T Function(Map<String, dynamic>) fromJson,
  ) async {
    final response = await http.get(
      Uri.parse('$baseUrl$endpoint'),
      headers: await _getHeaders(),
    );
    
    if (response.statusCode >= 200 && response.statusCode < 300) {
      return fromJson(jsonDecode(response.body));
    } else {
      throw Exception(jsonDecode(response.body)['message']);
    }
  }
  
  Future<List<T>> getList<T>(
    String endpoint,
    T Function(Map<String, dynamic>) fromJson,
  ) async {
    final response = await http.get(
      Uri.parse('$baseUrl$endpoint'),
      headers: await _getHeaders(),
    );
    
    if (response.statusCode >= 200 && response.statusCode < 300) {
      return (jsonDecode(response.body) as List)
          .map((e) => fromJson(e))
          .toList();
    } else {
      throw Exception(jsonDecode(response.body)['message']);
    }
  }
  
  Future<T> post<T>(
    String endpoint,
    Map<String, dynamic> data,
    T Function(Map<String, dynamic>) fromJson,
  ) async {
    final response = await http.post(
      Uri.parse('$baseUrl$endpoint'),
      headers: await _getHeaders(),
      body: jsonEncode(data),
    );
    
    if (response.statusCode >= 200 && response.statusCode < 300) {
      return fromJson(jsonDecode(response.body));
    } else {
      throw Exception(jsonDecode(response.body)['message']);
    }
  }
  
  Future<void> delete(String endpoint) async {
    final response = await http.delete(
      Uri.parse('$baseUrl$endpoint'),
      headers: await _getHeaders(),
    );
    
    if (response.statusCode < 200 || response.statusCode >= 300) {
      throw Exception(jsonDecode(response.body)['message']);
    }
  }
}
```

### Service Examples

```dart
// lib/services/subject_service.dart
class SubjectService {
  final ApiClient _client = ApiClient();
  
  Future<List<Subject>> getAll() async {
    return _client.getList('/api/subject', Subject.fromJson);
  }
  
  Future<Subject> getById(int id) async {
    return _client.get('/api/subject/$id', Subject.fromJson);
  }
  
  Future<Subject> create(String name, String? description) async {
    return _client.post(
      '/api/subject',
      {'name': name, 'description': description},
      Subject.fromJson,
    );
  }
  
  Future<void> delete(int id) async {
    return _client.delete('/api/subject/$id');
  }
}

// lib/services/course_service.dart
class CourseService {
  final ApiClient _client = ApiClient();
  
  Future<List<Course>> getAll() async {
    return _client.getList('/api/course', Course.fromJson);
  }
  
  Future<Course> getById(int id) async {
    return _client.get('/api/course/$id', Course.fromJson);
  }
  
  Future<List<Course>> getBySubject(int subjectId) async {
    return _client.getList('/api/course/subject/$subjectId', Course.fromJson);
  }
}

// lib/services/quiz_service.dart
class QuizService {
  final ApiClient _client = ApiClient();
  
  Future<Quiz> getById(int id) async {
    return _client.get('/api/quiz/$id', Quiz.fromJson);
  }
  
  Future<List<QuizSummary>> getByUnit(int unitId) async {
    return _client.getList('/api/quiz?unitId=$unitId', QuizSummary.fromJson);
  }
  
  Future<List<QuizSummary>> getActive() async {
    return _client.getList('/api/quiz/active', QuizSummary.fromJson);
  }
}

// lib/services/quiz_attempt_service.dart
class QuizAttemptService {
  final ApiClient _client = ApiClient();
  
  Future<QuizAttempt> start(int quizId) async {
    return _client.post(
      '/api/quiz-attempt/start',
      {'quizId': quizId},
      QuizAttempt.fromJson,
    );
  }
  
  Future<QuizResult> submit(int attemptId, List<Map<String, int>> answers) async {
    return _client.post(
      '/api/quiz-attempt/submit',
      {'quizAttemptId': attemptId, 'answers': answers},
      QuizResult.fromJson,
    );
  }
  
  Future<QuizResult> getResult(int id) async {
    return _client.get('/api/quiz-attempt/$id/result', QuizResult.fromJson);
  }
}
```

### Provider/State Management Example (using Riverpod)

```dart
// lib/providers/subject_provider.dart
import 'package:flutter_riverpod/flutter_riverpod.dart';

final subjectServiceProvider = Provider((ref) => SubjectService());

final subjectsProvider = FutureProvider<List<Subject>>((ref) async {
  final service = ref.watch(subjectServiceProvider);
  return service.getAll();
});

final subjectDetailProvider = FutureProvider.family<Subject, int>((ref, id) async {
  final service = ref.watch(subjectServiceProvider);
  return service.getById(id);
});
```

---

**Last Updated**: January 2025  
**API Version**: 1.0  
**Total Endpoints**: 40  
**Language**: Dart/Flutter

For complete Flutter integration guide, see `frontend-guide.md`  
For quick reference with workflow examples, see `api-endpoints-quick-reference.md`
