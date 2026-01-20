# Frontend Development Guide

## Overview

This guide provides frontend developers with everything needed to integrate with the Tapcet Quiz API, including the educational hierarchy system.

---

## Table of Contents

1. [Quick Start](#quick-start)
2. [Authentication](#authentication)
3. [Educational Hierarchy](#educational-hierarchy)
4. [Quiz Management](#quiz-management)
5. [Quiz Taking](#quiz-taking)
6. [State Management](#state-management)
7. [UI Components](#ui-components)
8. [Error Handling](#error-handling)
9. [TypeScript Types](#typescript-types)

---

## Quick Start

### Base URL

```typescript
const API_BASE_URL = import.meta.env.VITE_API_URL || 'https://localhost:7237';
```

### API Client Setup

```typescript
// src/lib/api-client.ts
export class ApiClient {
  private baseUrl: string;
  
  constructor(baseUrl: string) {
    this.baseUrl = baseUrl;
  }
  
  private getHeaders(): HeadersInit {
    const token = localStorage.getItem('authToken');
    return {
      'Content-Type': 'application/json',
      ...(token && { 'Authorization': `Bearer ${token}` })
    };
  }
  
  async get<T>(endpoint: string): Promise<T> {
    const response = await fetch(`${this.baseUrl}${endpoint}`, {
      headers: this.getHeaders()
    });
    
    if (!response.ok) {
      throw await this.handleError(response);
    }
    
    return response.json();
  }
  
  async post<T>(endpoint: string, data: any): Promise<T> {
    const response = await fetch(`${this.baseUrl}${endpoint}`, {
      method: 'POST',
      headers: this.getHeaders(),
      body: JSON.stringify(data)
    });
    
    if (!response.ok) {
      throw await this.handleError(response);
    }
    
    return response.json();
  }
  
  async put<T>(endpoint: string, data: any): Promise<T> {
    const response = await fetch(`${this.baseUrl}${endpoint}`, {
      method: 'PUT',
      headers: this.getHeaders(),
      body: JSON.stringify(data)
    });
    
    if (!response.ok) {
      throw await this.handleError(response);
    }
    
    return response.json();
  }
  
  async patch<T>(endpoint: string, data: any): Promise<T> {
    const response = await fetch(`${this.baseUrl}${endpoint}`, {
      method: 'PATCH',
      headers: this.getHeaders(),
      body: JSON.stringify(data)
    });
    
    if (!response.ok) {
      throw await this.handleError(response);
    }
    
    return response.json();
  }
  
  async delete(endpoint: string): Promise<void> {
    const response = await fetch(`${this.baseUrl}${endpoint}`, {
      method: 'DELETE',
      headers: this.getHeaders()
    });
    
    if (!response.ok) {
      throw await this.handleError(response);
    }
  }
  
  private async handleError(response: Response) {
    const error = await response.json().catch(() => ({ 
      message: 'An unexpected error occurred' 
    }));
    return new Error(error.message || `HTTP ${response.status}`);
  }
}

export const apiClient = new ApiClient(API_BASE_URL);
```

---

## Authentication

### Registration

```typescript
// src/services/auth.service.ts
export interface RegisterData {
  userName: string;
  email: string;
  password: string;
  confirmPassword: string;
}

export interface LoginData {
  email: string;
  password: string;
}

export interface AuthResponse {
  token: string;
  email: string;
  userName: string;
  expiresAt: string;
}

export const authService = {
  async register(data: RegisterData): Promise<{ message: string }> {
    return apiClient.post('/api/auth/register', data);
  },
  
  async login(data: LoginData): Promise<AuthResponse> {
    const response = await apiClient.post<AuthResponse>('/api/auth/login', data);
    localStorage.setItem('authToken', response.token);
    localStorage.setItem('userName', response.userName);
    return response;
  },
  
  logout() {
    localStorage.removeItem('authToken');
    localStorage.removeItem('userName');
  },
  
  isAuthenticated(): boolean {
    return !!localStorage.getItem('authToken');
  },
  
  getCurrentUser(): string | null {
    return localStorage.getItem('userName');
  }
};
```

### React Hook Example

```typescript
// src/hooks/useAuth.ts
import { useState, useEffect } from 'react';
import { authService } from '../services/auth.service';

export function useAuth() {
  const [isAuthenticated, setIsAuthenticated] = useState(authService.isAuthenticated());
  const [userName, setUserName] = useState(authService.getCurrentUser());
  
  const login = async (email: string, password: string) => {
    const response = await authService.login({ email, password });
    setIsAuthenticated(true);
    setUserName(response.userName);
  };
  
  const logout = () => {
    authService.logout();
    setIsAuthenticated(false);
    setUserName(null);
  };
  
  return { isAuthenticated, userName, login, logout };
}
```

---

## Educational Hierarchy

### Subject Service

```typescript
// src/services/subject.service.ts
export interface Subject {
  id: number;
  name: string;
  description?: string;
  courseCount: number;
}

export interface SubjectWithCourses extends Subject {
  courses: Course[];
}

export const subjectService = {
  async getAll(): Promise<Subject[]> {
    return apiClient.get('/api/subject');
  },
  
  async getById(id: number): Promise<SubjectWithCourses> {
    return apiClient.get(`/api/subject/${id}`);
  },
  
  async create(data: { name: string; description?: string }): Promise<Subject> {
    return apiClient.post('/api/subject', data);
  },
  
  async update(id: number, data: { name: string; description?: string }): Promise<Subject> {
    return apiClient.put(`/api/subject/${id}`, data);
  },
  
  async delete(id: number): Promise<void> {
    return apiClient.delete(`/api/subject/${id}`);
  }
};
```

### Course Service

```typescript
// src/services/course.service.ts
export interface Course {
  id: number;
  title: string;
  description?: string;
  subjectId: number;
  subjectName: string;
  unitCount: number;
}

export interface CourseWithUnits extends Course {
  units: Unit[];
}

export const courseService = {
  async getAll(): Promise<Course[]> {
    return apiClient.get('/api/course');
  },
  
  async getById(id: number): Promise<CourseWithUnits> {
    return apiClient.get(`/api/course/${id}`);
  },
  
  async getBySubject(subjectId: number): Promise<Course[]> {
    return apiClient.get(`/api/course/subject/${subjectId}`);
  },
  
  async create(data: { 
    title: string; 
    description?: string; 
    subjectId: number 
  }): Promise<Course> {
    return apiClient.post('/api/course', data);
  },
  
  async update(id: number, data: { 
    title: string; 
    description?: string; 
    subjectId: number 
  }): Promise<Course> {
    return apiClient.put(`/api/course/${id}`, data);
  },
  
  async delete(id: number): Promise<void> {
    return apiClient.delete(`/api/course/${id}`);
  }
};
```

### Unit Service

```typescript
// src/services/unit.service.ts
export interface Unit {
  id: number;
  title: string;
  orderIndex: number;
  courseId: number;
  courseTitle: string;
  quizCount: number;
}

export interface UnitWithQuizzes extends Unit {
  quizzes: QuizSummary[];
}

export const unitService = {
  async getById(id: number): Promise<UnitWithQuizzes> {
    return apiClient.get(`/api/unit/${id}`);
  },
  
  async getByCourse(courseId: number): Promise<Unit[]> {
    return apiClient.get(`/api/unit/course/${courseId}`);
  },
  
  async getQuizzes(unitId: number): Promise<QuizSummary[]> {
    return apiClient.get(`/api/unit/${unitId}/quizzes`);
  },
  
  async create(data: { 
    title: string; 
    orderIndex: number; 
    courseId: number 
  }): Promise<Unit> {
    return apiClient.post('/api/unit', data);
  },
  
  async update(id: number, data: { 
    title: string; 
    orderIndex: number; 
    courseId: number 
  }): Promise<Unit> {
    return apiClient.put(`/api/unit/${id}`, data);
  },
  
  async reorder(id: number, orderIndex: number): Promise<Unit> {
    return apiClient.patch(`/api/unit/${id}/reorder`, { orderIndex });
  },
  
  async delete(id: number): Promise<void> {
    return apiClient.delete(`/api/unit/${id}`);
  }
};
```

---

## Quiz Management

### Quiz Service

```typescript
// src/services/quiz.service.ts
export interface QuizSummary {
  id: number;
  title: string;
  description?: string;
  unitId?: number;
  unitTitle?: string;
  orderIndex: number;
  createdAt: string;
  createdByName: string;
  isActive: boolean;
  questionCount: number;
  attemptCount: number;
}

export interface Quiz {
  id: number;
  title: string;
  description?: string;
  unitId?: number;
  unitTitle?: string;
  orderIndex: number;
  createdAt: string;
  createdById: string;
  createdByName: string;
  isActive: boolean;
  questionCount: number;
  questions: Question[];
}

export interface Question {
  id: number;
  text: string;
  explanation?: string;
  imageUrl?: string;
  choices: Choice[];
}

export interface Choice {
  id: number;
  text: string;
  isCorrect: boolean;
}

export const quizService = {
  async getAll(unitId?: number): Promise<QuizSummary[]> {
    const endpoint = unitId ? `/api/quiz?unitId=${unitId}` : '/api/quiz';
    return apiClient.get(endpoint);
  },
  
  async getStandalone(): Promise<QuizSummary[]> {
    return apiClient.get('/api/quiz/standalone');
  },
  
  async getActive(): Promise<QuizSummary[]> {
    return apiClient.get('/api/quiz/active');
  },
  
  async getMyQuizzes(): Promise<QuizSummary[]> {
    return apiClient.get('/api/quiz/user/me');
  },
  
  async getById(id: number): Promise<Quiz> {
    return apiClient.get(`/api/quiz/${id}`);
  },
  
  async create(data: {
    title: string;
    description?: string;
    unitId?: number;
    orderIndex?: number;
    questions: Array<{
      text: string;
      explanation?: string;
      choices: Array<{ text: string; isCorrect: boolean }>;
    }>;
  }): Promise<Quiz> {
    return apiClient.post('/api/quiz', data);
  },
  
  async update(id: number, data: {
    title: string;
    description?: string;
    unitId?: number;
    orderIndex?: number;
    isActive: boolean;
  }): Promise<Quiz> {
    return apiClient.put(`/api/quiz/${id}`, data);
  },
  
  async assignToUnit(id: number, unitId: number, orderIndex: number): Promise<Quiz> {
    return apiClient.patch(`/api/quiz/${id}/assign-unit`, { unitId, orderIndex });
  },
  
  async reorder(id: number, orderIndex: number): Promise<Quiz> {
    return apiClient.patch(`/api/quiz/${id}/reorder`, { orderIndex });
  },
  
  async toggleStatus(id: number): Promise<{ message: string }> {
    return apiClient.patch(`/api/quiz/${id}/toggle`, {});
  },
  
  async delete(id: number): Promise<void> {
    return apiClient.delete(`/api/quiz/${id}`);
  }
};
```

---

## Quiz Taking

### Quiz Attempt Service

```typescript
// src/services/quiz-attempt.service.ts
export interface QuizAttempt {
  id: number;
  quizId: number;
  quizTitle: string;
  userId: string;
  userName: string;
  startedAt: string;
  completedAt?: string;
  score: number;
  isCompleted: boolean;
}

export interface QuizResult {
  quizAttemptId: number;
  quizTitle: string;
  totalQuestions: number;
  correctAnswers: number;
  incorrectAnswers: number;
  score: number;
  percentage: number;
  startedAt: string;
  completedAt: string;
  duration: string;
  questionResults: QuestionResult[];
}

export interface QuestionResult {
  questionId: number;
  questionText: string;
  explanation?: string;
  selectedChoiceId: number;
  selectedChoiceText: string;
  correctChoiceId: number;
  correctChoiceText: string;
  isCorrect: boolean;
}

export const quizAttemptService = {
  async start(quizId: number): Promise<QuizAttempt> {
    return apiClient.post('/api/quiz-attempt/start', { quizId });
  },
  
  async submit(attemptId: number, answers: Array<{
    questionId: number;
    choiceId: number;
  }>): Promise<QuizResult> {
    return apiClient.post('/api/quiz-attempt/submit', {
      quizAttemptId: attemptId,
      answers
    });
  },
  
  async getById(id: number): Promise<QuizAttempt> {
    return apiClient.get(`/api/quiz-attempt/${id}`);
  },
  
  async getResult(id: number): Promise<QuizResult> {
    return apiClient.get(`/api/quiz-attempt/${id}/result`);
  },
  
  async getMyAttempts(): Promise<QuizAttempt[]> {
    return apiClient.get('/api/quiz-attempt/user/me');
  },
  
  async getQuizAttempts(quizId: number): Promise<QuizAttempt[]> {
    return apiClient.get(`/api/quiz-attempt/quiz/${quizId}`);
  },
  
  async getLeaderboard(quizId: number, topCount = 10): Promise<Array<{
    id: number;
    userName: string;
    score: number;
    completedAt: string;
  }>> {
    return apiClient.get(`/api/quiz-attempt/quiz/${quizId}/leaderboard?topCount=${topCount}`);
  }
};
```

---

## State Management

### React Context Example

```typescript
// src/context/HierarchyContext.tsx
import React, { createContext, useContext, useState, useCallback } from 'react';
import { subjectService, courseService, unitService } from '../services';

interface HierarchyContextType {
  subjects: Subject[];
  loadSubjects: () => Promise<void>;
  selectedSubject: SubjectWithCourses | null;
  selectSubject: (id: number) => Promise<void>;
  selectedCourse: CourseWithUnits | null;
  selectCourse: (id: number) => Promise<void>;
  selectedUnit: UnitWithQuizzes | null;
  selectUnit: (id: number) => Promise<void>;
}

const HierarchyContext = createContext<HierarchyContextType | undefined>(undefined);

export function HierarchyProvider({ children }: { children: React.ReactNode }) {
  const [subjects, setSubjects] = useState<Subject[]>([]);
  const [selectedSubject, setSelectedSubject] = useState<SubjectWithCourses | null>(null);
  const [selectedCourse, setSelectedCourse] = useState<CourseWithUnits | null>(null);
  const [selectedUnit, setSelectedUnit] = useState<UnitWithQuizzes | null>(null);
  
  const loadSubjects = useCallback(async () => {
    const data = await subjectService.getAll();
    setSubjects(data);
  }, []);
  
  const selectSubject = useCallback(async (id: number) => {
    const data = await subjectService.getById(id);
    setSelectedSubject(data);
    setSelectedCourse(null);
    setSelectedUnit(null);
  }, []);
  
  const selectCourse = useCallback(async (id: number) => {
    const data = await courseService.getById(id);
    setSelectedCourse(data);
    setSelectedUnit(null);
  }, []);
  
  const selectUnit = useCallback(async (id: number) => {
    const data = await unitService.getById(id);
    setSelectedUnit(data);
  }, []);
  
  return (
    <HierarchyContext.Provider value={{
      subjects,
      loadSubjects,
      selectedSubject,
      selectSubject,
      selectedCourse,
      selectCourse,
      selectedUnit,
      selectUnit
    }}>
      {children}
    </HierarchyContext.Provider>
  );
}

export function useHierarchy() {
  const context = useContext(HierarchyContext);
  if (!context) {
    throw new Error('useHierarchy must be used within HierarchyProvider');
  }
  return context;
}
```

---

## UI Components

### Breadcrumb Component

```typescript
// src/components/Breadcrumb.tsx
import React from 'react';
import { Link } from 'react-router-dom';

interface BreadcrumbItem {
  label: string;
  path?: string;
}

interface BreadcrumbProps {
  items: BreadcrumbItem[];
}

export function Breadcrumb({ items }: BreadcrumbProps) {
  return (
    <nav aria-label="Breadcrumb">
      <ol className="flex items-center space-x-2">
        {items.map((item, index) => (
          <li key={index} className="flex items-center">
            {index > 0 && <span className="mx-2">/</span>}
            {item.path ? (
              <Link to={item.path} className="text-blue-600 hover:underline">
                {item.label}
              </Link>
            ) : (
              <span className="text-gray-600">{item.label}</span>
            )}
          </li>
        ))}
      </ol>
    </nav>
  );
}

// Usage example with quiz
function QuizBreadcrumb({ quiz }: { quiz: Quiz }) {
  const [breadcrumbs, setBreadcrumbs] = useState<BreadcrumbItem[]>([]);
  
  useEffect(() => {
    async function loadBreadcrumbs() {
      const items: BreadcrumbItem[] = [{ label: 'Home', path: '/' }];
      
      if (quiz.unitId) {
        const unit = await unitService.getById(quiz.unitId);
        const course = await courseService.getById(unit.courseId);
        const subject = await subjectService.getById(course.subjectId);
        
        items.push(
          { label: subject.name, path: `/subjects/${subject.id}` },
          { label: course.title, path: `/courses/${course.id}` },
          { label: unit.title, path: `/units/${unit.id}` },
          { label: quiz.title }
        );
      } else {
        items.push({ label: quiz.title });
      }
      
      setBreadcrumbs(items);
    }
    
    loadBreadcrumbs();
  }, [quiz]);
  
  return <Breadcrumb items={breadcrumbs} />;
}
```

### Subject List Component

```typescript
// src/components/SubjectList.tsx
import React, { useEffect, useState } from 'react';
import { subjectService } from '../services';
import { useNavigate } from 'react-router-dom';

export function SubjectList() {
  const [subjects, setSubjects] = useState<Subject[]>([]);
  const [loading, setLoading] = useState(true);
  const navigate = useNavigate();
  
  useEffect(() => {
    async function loadSubjects() {
      try {
        const data = await subjectService.getAll();
        setSubjects(data);
      } catch (error) {
        console.error('Failed to load subjects:', error);
      } finally {
        setLoading(false);
      }
    }
    
    loadSubjects();
  }, []);
  
  if (loading) return <div>Loading...</div>;
  
  return (
    <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
      {subjects.map(subject => (
        <div 
          key={subject.id}
          onClick={() => navigate(`/subjects/${subject.id}`)}
          className="p-6 border rounded-lg cursor-pointer hover:shadow-lg transition"
        >
          <h3 className="text-xl font-bold mb-2">{subject.name}</h3>
          <p className="text-gray-600 mb-4">{subject.description}</p>
          <p className="text-sm text-blue-600">
            {subject.courseCount} {subject.courseCount === 1 ? 'course' : 'courses'}
          </p>
        </div>
      ))}
    </div>
  );
}
```

### Quiz Taking Component

```typescript
// src/components/QuizTaker.tsx
import React, { useState, useEffect } from 'react';
import { quizService, quizAttemptService } from '../services';
import { useParams, useNavigate } from 'react-router-dom';

export function QuizTaker() {
  const { quizId } = useParams<{ quizId: string }>();
  const navigate = useNavigate();
  const [quiz, setQuiz] = useState<Quiz | null>(null);
  const [attemptId, setAttemptId] = useState<number | null>(null);
  const [answers, setAnswers] = useState<Record<number, number>>({});
  const [loading, setLoading] = useState(true);
  
  useEffect(() => {
    async function startQuiz() {
      try {
        const quizData = await quizService.getById(Number(quizId));
        const attempt = await quizAttemptService.start(Number(quizId));
        setQuiz(quizData);
        setAttemptId(attempt.id);
      } catch (error) {
        console.error('Failed to start quiz:', error);
      } finally {
        setLoading(false);
      }
    }
    
    startQuiz();
  }, [quizId]);
  
  const handleAnswerSelect = (questionId: number, choiceId: number) => {
    setAnswers(prev => ({ ...prev, [questionId]: choiceId }));
  };
  
  const handleSubmit = async () => {
    if (!attemptId) return;
    
    const answerArray = Object.entries(answers).map(([questionId, choiceId]) => ({
      questionId: Number(questionId),
      choiceId
    }));
    
    try {
      const result = await quizAttemptService.submit(attemptId, answerArray);
      navigate(`/quiz-results/${attemptId}`, { state: { result } });
    } catch (error) {
      console.error('Failed to submit quiz:', error);
    }
  };
  
  if (loading) return <div>Loading quiz...</div>;
  if (!quiz) return <div>Quiz not found</div>;
  
  const allAnswered = quiz.questions.every(q => answers[q.id] !== undefined);
  
  return (
    <div className="max-w-4xl mx-auto p-6">
      <h1 className="text-3xl font-bold mb-6">{quiz.title}</h1>
      
      {quiz.questions.map((question, index) => (
        <div key={question.id} className="mb-8 p-6 border rounded-lg">
          <h3 className="text-lg font-semibold mb-4">
            {index + 1}. {question.text}
          </h3>
          
          <div className="space-y-3">
            {question.choices.map(choice => (
              <label 
                key={choice.id}
                className="flex items-center p-3 border rounded cursor-pointer hover:bg-gray-50"
              >
                <input
                  type="radio"
                  name={`question-${question.id}`}
                  value={choice.id}
                  checked={answers[question.id] === choice.id}
                  onChange={() => handleAnswerSelect(question.id, choice.id)}
                  className="mr-3"
                />
                <span>{choice.text}</span>
              </label>
            ))}
          </div>
        </div>
      ))}
      
      <button
        onClick={handleSubmit}
        disabled={!allAnswered}
        className="w-full py-3 px-6 bg-blue-600 text-white rounded-lg disabled:bg-gray-400 disabled:cursor-not-allowed hover:bg-blue-700 transition"
      >
        Submit Quiz
      </button>
    </div>
  );
}
```

---

## Error Handling

### Error Boundary Component

```typescript
// src/components/ErrorBoundary.tsx
import React, { Component, ErrorInfo, ReactNode } from 'react';

interface Props {
  children: ReactNode;
  fallback?: ReactNode;
}

interface State {
  hasError: boolean;
  error?: Error;
}

export class ErrorBoundary extends Component<Props, State> {
  constructor(props: Props) {
    super(props);
    this.state = { hasError: false };
  }
  
  static getDerivedStateFromError(error: Error): State {
    return { hasError: true, error };
  }
  
  componentDidCatch(error: Error, errorInfo: ErrorInfo) {
    console.error('Error caught by boundary:', error, errorInfo);
  }
  
  render() {
    if (this.state.hasError) {
      return this.props.fallback || (
        <div className="p-6 text-center">
          <h2 className="text-2xl font-bold text-red-600 mb-4">Something went wrong</h2>
          <p className="text-gray-600">{this.state.error?.message}</p>
          <button 
            onClick={() => window.location.reload()}
            className="mt-4 px-6 py-2 bg-blue-600 text-white rounded"
          >
            Reload Page
          </button>
        </div>
      );
    }
    
    return this.props.children;
  }
}
```

### Toast Notifications

```typescript
// src/hooks/useToast.ts
import { useState, useCallback } from 'react';

type ToastType = 'success' | 'error' | 'info';

interface Toast {
  id: number;
  message: string;
  type: ToastType;
}

export function useToast() {
  const [toasts, setToasts] = useState<Toast[]>([]);
  
  const showToast = useCallback((message: string, type: ToastType = 'info') => {
    const id = Date.now();
    setToasts(prev => [...prev, { id, message, type }]);
    
    setTimeout(() => {
      setToasts(prev => prev.filter(t => t.id !== id));
    }, 5000);
  }, []);
  
  const removeToast = useCallback((id: number) => {
    setToasts(prev => prev.filter(t => t.id !== id));
  }, []);
  
  return { toasts, showToast, removeToast };
}
```

---

## TypeScript Types

### Complete Type Definitions

```typescript
// src/types/api.ts

// Auth
export interface RegisterData {
  userName: string;
  email: string;
  password: string;
  confirmPassword: string;
}

export interface LoginData {
  email: string;
  password: string;
}

export interface AuthResponse {
  token: string;
  email: string;
  userName: string;
  expiresAt: string;
}

// Hierarchy
export interface Subject {
  id: number;
  name: string;
  description?: string;
  courseCount: number;
}

export interface SubjectWithCourses extends Subject {
  courses: Course[];
}

export interface Course {
  id: number;
  title: string;
  description?: string;
  subjectId: number;
  subjectName: string;
  unitCount: number;
}

export interface CourseWithUnits extends Course {
  units: Unit[];
}

export interface Unit {
  id: number;
  title: string;
  orderIndex: number;
  courseId: number;
  courseTitle: string;
  quizCount: number;
}

export interface UnitWithQuizzes extends Unit {
  quizzes: QuizSummary[];
}

// Quiz
export interface QuizSummary {
  id: number;
  title: string;
  description?: string;
  unitId?: number;
  unitTitle?: string;
  orderIndex: number;
  createdAt: string;
  createdByName: string;
  isActive: boolean;
  questionCount: number;
  attemptCount: number;
}

export interface Quiz {
  id: number;
  title: string;
  description?: string;
  unitId?: number;
  unitTitle?: string;
  orderIndex: number;
  createdAt: string;
  createdById: string;
  createdByName: string;
  isActive: boolean;
  questionCount: number;
  questions: Question[];
}

export interface Question {
  id: number;
  text: string;
  explanation?: string;
  imageUrl?: string;
  choices: Choice[];
}

export interface Choice {
  id: number;
  text: string;
  isCorrect: boolean;
}

// Quiz Attempt
export interface QuizAttempt {
  id: number;
  quizId: number;
  quizTitle: string;
  userId: string;
  userName: string;
  startedAt: string;
  completedAt?: string;
  score: number;
  isCompleted: boolean;
}

export interface QuizResult {
  quizAttemptId: number;
  quizTitle: string;
  totalQuestions: number;
  correctAnswers: number;
  incorrectAnswers: number;
  score: number;
  percentage: number;
  startedAt: string;
  completedAt: string;
  duration: string;
  questionResults: QuestionResult[];
}

export interface QuestionResult {
  questionId: number;
  questionText: string;
  explanation?: string;
  selectedChoiceId: number;
  selectedChoiceText: string;
  correctChoiceId: number;
  correctChoiceText: string;
  isCorrect: boolean;
}

// API Errors
export interface ApiError {
  message: string;
  errors?: string[];
}
```

---

## Best Practices

### 1. **Always Handle Loading States**

```typescript
function MyComponent() {
  const [data, setData] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  
  useEffect(() => {
    async function load() {
      try {
        setLoading(true);
        const result = await someService.getData();
        setData(result);
      } catch (err) {
        setError(err.message);
      } finally {
        setLoading(false);
      }
    }
    load();
  }, []);
  
  if (loading) return <LoadingSpinner />;
  if (error) return <ErrorMessage message={error} />;
  return <DataDisplay data={data} />;
}
```

### 2. **Cache Frequently Accessed Data**

```typescript
// Use React Query or similar library
import { useQuery } from '@tanstack/react-query';

function useSubjects() {
  return useQuery({
    queryKey: ['subjects'],
    queryFn: () => subjectService.getAll(),
    staleTime: 5 * 60 * 1000, // 5 minutes
  });
}
```

### 3. **Optimistic Updates**

```typescript
async function deleteQuiz(id: number) {
  // Optimistically remove from UI
  setQuizzes(prev => prev.filter(q => q.id !== id));
  
  try {
    await quizService.delete(id);
    showToast('Quiz deleted successfully', 'success');
  } catch (error) {
    // Revert on error
    await loadQuizzes();
    showToast('Failed to delete quiz', 'error');
  }
}
```

### 4. **Debounce Search Inputs**

```typescript
import { useDebouncedCallback } from 'use-debounce';

function SearchBox() {
  const [searchTerm, setSearchTerm] = useState('');
  
  const debouncedSearch = useDebouncedCallback(
    async (value: string) => {
      const results = await searchService.search(value);
      setResults(results);
    },
    300
  );
  
  return (
    <input
      value={searchTerm}
      onChange={(e) => {
        setSearchTerm(e.target.value);
        debouncedSearch(e.target.value);
      }}
    />
  );
}
```

---

## Testing

### API Client Tests

```typescript
// src/lib/__tests__/api-client.test.ts
import { apiClient } from '../api-client';

describe('ApiClient', () => {
  it('should add auth token to requests', async () => {
    localStorage.setItem('authToken', 'test-token');
    
    const fetchSpy = jest.spyOn(global, 'fetch').mockResolvedValue(
      new Response(JSON.stringify({ data: 'test' }))
    );
    
    await apiClient.get('/test');
    
    expect(fetchSpy).toHaveBeenCalledWith(
      expect.any(String),
      expect.objectContaining({
        headers: expect.objectContaining({
          'Authorization': 'Bearer test-token'
        })
      })
    );
  });
});
```

---

**Last Updated**: January 2025  
**Status**: Production Ready