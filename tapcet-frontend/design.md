# TAPCET Frontend — Design Document

## Overview

A React SPA that consumes the TAPCET REST API. It serves two audiences:
- **Students (User role):** browse the content hierarchy, take quizzes, view results and leaderboards.
- **Admins:** manage subjects, and have the same capabilities as students.
- **Quiz authors (any authenticated user):** create and manage their own quizzes.

---

## Tech Stack

| Concern | Choice | Reason |
|---|---|---|
| Framework | React 18 + TypeScript | Type safety aligns well with the strongly-typed API |
| Routing | React Router v6 | Nested routes map cleanly to the Subject → Course → Unit → Quiz hierarchy |
| Server state | TanStack Query (React Query) | Caching, refetching, and loading states without boilerplate |
| Client state | Zustand | Lightweight; only JWT token + current user need global state |
| Forms | React Hook Form + Zod | Validation mirrors server-side rules (min/max lengths, password rules) |
| Styling | Tailwind CSS | Utility-first; fast prototyping |
| UI primitives | shadcn/ui | Accessible, unstyled-by-default components on top of Radix UI |
| HTTP client | Axios | Interceptor attaches `Authorization: Bearer` header globally |
| Build | Vite | Fast HMR, minimal config |

---

## Project Structure

```
src/
├── api/              # Axios instance + typed API functions (one file per domain)
│   ├── auth.ts
│   ├── subjects.ts
│   ├── courses.ts
│   ├── units.ts
│   ├── quizzes.ts
│   └── attempts.ts
├── components/
│   ├── ui/           # shadcn/ui wrappers (Button, Input, Card, Badge, etc.)
│   ├── layout/       # AppShell, Navbar, Sidebar, ProtectedRoute
│   └── shared/       # BreadcrumbNav, LoadingSpinner, ErrorBoundary, EmptyState
├── features/         # Feature-scoped components (co-located with their hooks)
│   ├── auth/
│   ├── subjects/
│   ├── courses/
│   ├── units/
│   ├── quizzes/
│   └── attempts/
├── hooks/            # useAuth, useCurrentUser
├── store/            # Zustand auth store
├── types/            # TypeScript types mirroring all API DTOs
├── lib/              # Zod schemas, axios config, utils
└── pages/            # Route-level components (thin; delegate to features)
```

---

## Auth Store (Zustand)

```ts
interface AuthState {
  token: string | null
  user: { id: string; userName: string; email: string; roles: string[] } | null
  login: (response: AuthResponseDto) => void
  logout: () => void
  isAdmin: () => boolean
}
```

Token is persisted to `localStorage`. The Axios instance reads `token` on each request via an interceptor. On 401 response, the interceptor calls `logout()` and redirects to `/login`.

---

## Routes

```
/                         → redirect to /subjects
/login                    → LoginPage
/register                 → RegisterPage

/subjects                 → SubjectListPage
/subjects/:subjectId      → SubjectDetailPage  (lists courses)

/courses/:courseId        → CourseDetailPage   (lists units in order)

/units/:unitId            → UnitDetailPage     (lists quizzes in order)

/quizzes                  → QuizBrowsePage     (active quizzes, filterable)
/quizzes/new              → QuizCreatePage     [auth required]
/quizzes/me               → MyQuizzesPage      [auth required]
/quizzes/:quizId          → QuizDetailPage     (question preview + start button)
/quizzes/:quizId/edit     → QuizEditPage       [auth + ownership required]

/attempts/:attemptId/take → QuizTakePage       [auth required]
/attempts/:attemptId/result → ResultPage       [auth required]

/profile                  → ProfilePage        [auth required]
```

Protected routes render a `<ProtectedRoute>` wrapper that redirects to `/login` if `token` is null.

---

## Pages & Key Features

### SubjectListPage
- Grid of subject cards; each shows name and course count.
- Admins see an "Add Subject" button and per-card edit/delete controls.
- Uses `GET /api/subject`.

### SubjectDetailPage
- Subject header with description.
- List of `CourseCard` components.
- Admins see subject edit controls.
- Uses `GET /api/subject/:id` (returns subject with courses).

### CourseDetailPage
- Course header.
- Ordered list of `UnitCard` components (`OrderIndex` drives sort).
- Authenticated users see "Add Quiz" shortcut.
- Uses `GET /api/course/:id` (returns course with units).

### UnitDetailPage
- Unit header.
- Ordered list of `QuizCard` components.
- Quiz creator sees toggle active/inactive and edit/delete per card.
- Uses `GET /api/unit/:id` (returns unit with quizzes).

### QuizDetailPage
- Quiz title, description, author, question count, active badge.
- Preview of question count only (choices hidden before attempt).
- "Start Quiz" button → `POST /api/quiz-attempt/start` → navigate to `/attempts/:id/take`.
- Shows leaderboard table (top 10): rank, username, score, duration.
- Uses `GET /api/quiz/:id` and `GET /api/quiz-attempt/quiz/:quizId/leaderboard`.

### QuizCreatePage / QuizEditPage
- **Step 1 — Quiz metadata:** title, description, optional unit assignment (searchable select).
- **Step 2 — Questions:** dynamic list of question forms. Each question has:
  - Text input (required)
  - Optional explanation
  - 2–6 choice rows; radio group selects the one correct choice
  - Add/remove choice buttons (capped at 6)
  - Add/remove question buttons
- Validation via Zod mirrors server constraints.
- Submit sends `POST /api/quiz` or `PUT /api/quiz/:id`.

### QuizTakePage
- One question displayed at a time with a progress indicator (e.g. "3 / 10").
- Choice list rendered as a radio group; selection is stored in local state.
- "Next" / "Previous" navigation; last question shows "Submit" button.
- On submit → `POST /api/quiz-attempt/submit` → navigate to `/attempts/:id/result`.
- Timer displayed (elapsed seconds since `startedAt` returned by start response).
- No ability to revisit after submit.

### ResultPage
- Score summary: `X / Y correct — Z%`. Visual score ring.
- Per-question breakdown table: question text, your answer, correct answer, correct/incorrect icon, explanation if present.
- "Try Again" button (starts a new attempt), "Back to Quiz" link.
- Uses `GET /api/quiz-attempt/:id/result`.

### MyQuizzesPage
- Table of quizzes the current user created: title, question count, status (active/inactive), attempts count.
- Inline toggle status, edit, delete actions.
- "New Quiz" button.
- Uses `GET /api/quiz/user/me`.

### ProfilePage
- Display name, email, total attempts, average score.
- Change password form (currently not in API — placeholder / future).

---

## Component Breakdown

### Layout
- **AppShell** — Navbar + optional sidebar + `<Outlet />`.
- **Navbar** — Logo, breadcrumb, user menu (avatar + dropdown: My Quizzes, Profile, Logout). Shows "Admin" badge when role is Admin.
- **BreadcrumbNav** — Derived from the current route: `Subjects > Math > Algebra > Unit 1`.
- **ProtectedRoute** — Renders children or `<Navigate to="/login" />`.

### Shared
- **QuizCard** — Title, question count, active badge, author, action buttons (contextual by role/ownership).
- **UnitCard** — Title, order index, quiz count, link.
- **CourseCard** — Title, unit count, link.
- **SubjectCard** — Name, description, course count, admin actions.
- **ScoreRing** — SVG circle progress for percentage display.
- **LeaderboardTable** — Rank, username, score, duration columns.
- **ChoiceInput** — Single row: text input + "Correct?" radio + remove button; used in quiz builder.
- **QuestionForm** — Question text + explanation + image URL + list of `ChoiceInput`s.

---

## State & Data Flow

```
User action
    │
    ▼
React Hook Form (local form state)
    │ onSubmit
    ▼
API function (axios)
    │ mutationFn
    ▼
TanStack Query useMutation
    │ onSuccess → invalidate relevant queries
    ▼
TanStack Query useQuery (re-fetches)
    │
    ▼
Component re-renders with fresh data
```

Query keys follow a consistent pattern:
```ts
['subjects']
['subjects', id]
['courses', id]
['units', id]
['quizzes', { unitId }]
['quizzes', id]
['attempts', 'me']
['attempts', id, 'result']
['leaderboard', quizId]
```

---

## Form Validation (Zod schemas mirror API)

```ts
// Example: CreateQuiz
const createQuizSchema = z.object({
  title: z.string().min(3).max(200),
  description: z.string().max(2000).optional(),
  unitId: z.number().int().positive().optional(),
  orderIndex: z.number().int().min(1).max(999),
  questions: z.array(z.object({
    text: z.string().min(5).max(1000),
    explanation: z.string().max(1000).optional(),
    choices: z.array(z.object({
      text: z.string().min(1).max(500),
      isCorrect: z.boolean()
    })).min(2).max(6)
  })).min(1)
}).refine(
  (data) => data.questions.every(
    q => q.choices.filter(c => c.isCorrect).length === 1
  ),
  { message: 'Each question must have exactly one correct choice' }
)
```

---

## Error Handling

- **API 401** — Axios interceptor clears auth store and redirects to `/login`.
- **API 403** — Toast: "You don't have permission to do that."
- **API 404** — `<NotFoundState>` component inline in the page.
- **API 422 / 400** — Field-level errors from the response body surfaced back into React Hook Form via `setError`.
- **Network error** — Generic toast with retry option.
- All async data states (loading, error, empty) handled explicitly per component; no silent failures.

---

## Permissions Matrix

| Action | Guest | User | Quiz Owner | Admin |
|---|---|---|---|---|
| Browse subjects / courses / units | ✓ | ✓ | ✓ | ✓ |
| View quiz detail & leaderboard | ✓ | ✓ | ✓ | ✓ |
| Take quiz | — | ✓ | ✓ | ✓ |
| View own results | — | ✓ | ✓ | ✓ |
| Create quiz | — | ✓ | ✓ | ✓ |
| Edit / delete own quiz | — | — | ✓ | ✓ |
| Toggle own quiz active | — | — | ✓ | ✓ |
| Create / edit / delete subject | — | — | — | ✓ |

Ownership checks are enforced server-side; the frontend hides controls as a UX convenience only.

---

## Key UX Decisions

- **No page reload on quiz submit** — TanStack Query mutation + optimistic invalidation keeps the experience fluid.
- **Quiz builder is multi-step but single-page** — Avoids navigation loss; questions built up before any API call.
- **Breadcrumb always visible** — Users navigating deep into Subject → Course → Unit → Quiz can always orient themselves.
- **Leaderboard on quiz detail page** — Motivates students without requiring a separate page.
- **Active/inactive toggle is immediate** — PATCH endpoint is fast; no confirmation modal unless deleting.
- **Standalone quizzes browsable from `/quizzes`** — Students who know what they want skip the hierarchy entirely.

---

## Future Considerations (out of scope for v1)

- Pagination or infinite scroll on list pages (API currently returns all records)
- Image upload for question `imageUrl` field
- Real-time score updates via WebSocket or SSE
- Dark mode toggle
- Change password endpoint (not in current API)
- Admin user management panel
