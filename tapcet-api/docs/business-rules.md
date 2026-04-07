# Business Rules

All rules below are enforced in the service layer unless noted otherwise.

---

## Registration & Passwords

| Rule | Detail |
|------|--------|
| Username length | 3–50 characters |
| Email | Valid format, must be unique (case-insensitive, enforced by Identity) |
| Password | 6–100 chars, must include at least one uppercase, lowercase, and digit |
| Confirm password | Must match password (DTO validation, controller layer) |
| Roles on register | All new users get the `User` role; `Admin` is seeded only |
| Account lockout | 5 failed logins → locked for 5 minutes (Identity) |

---

## Quiz Rules

| Rule | Detail |
|------|--------|
| Title | 3–200 characters, required |
| Description | Optional, max 2000 characters |
| Questions on create | At least 1 question required (`[MinLength(1)]` on `CreateQuizDto.Questions`) |
| Ownership | Only the creator (`CreatedById`) can update, delete, toggle, add questions, assign to unit, or reorder |
| Active flag | `IsActive` defaults to `true`. Can be toggled without deleting |
| Unit assignment | `UnitId` is optional. Quizzes without a unit are "standalone" |
| Order index | 1–999. Gaps are allowed; no automatic re-sequencing |

---

## Question & Choice Rules

| Rule | Detail |
|------|--------|
| Question text | 5–1000 characters, required |
| Explanation | Optional, max 1000 characters |
| Image URL | Optional, must be a valid URL if provided |
| Choice count | Minimum 2, maximum 6 per question |
| Correct choices | Exactly 1 choice per question must have `IsCorrect = true` |
| Choice text | 1–500 characters, required |

These rules are validated in `QuizService` before saving to the database (not just at the DTO level).

---

## Quiz Attempt Rules

| Rule | Detail |
|------|--------|
| Start | Creates a `QuizAttempt` with `CompletedAt = null`. Quiz must be active and have questions. |
| Submit — answer count | Must submit exactly one answer per question in the quiz. Partial submissions are rejected. |
| Submit — already submitted | Rejected if `CompletedAt` is already set on the attempt. |
| Score calculation | `score = Round(correctAnswers / totalQuestions * 100)` → integer 0–100 |
| Leaderboard | Ranked by score descending, then by duration ascending (faster = better tiebreak) |
| `topCount` (leaderboard) | Must be 1–100, defaults to 10 |

---

## Educational Hierarchy Rules

| Action | Rule |
|--------|------|
| Delete Subject | **Blocked** if it has any Courses |
| Delete Course | **Blocked** if it has any Units |
| Delete Unit | **Allowed** — associated Quizzes have `UnitId` set to `null` (become standalone) |
| Delete Quiz | **Blocked** (at DB level via RESTRICT) if it has any QuizAttempts |

---

## User Statistics

After each successful quiz submission, the user's stats are recalculated:

- `TotalQuizAttempts` → incremented by 1
- `AverageScore` → recalculated by summing all completed attempt scores and dividing by count

This is a full recalculation on each submission (reads all completed attempts for the user). Acceptable at low volume.

---

## Authorization Summary

| Action | Required |
|--------|---------|
| Read subjects, courses, units, quizzes | None (anonymous) |
| Register / login / check email | None (anonymous) |
| Create course, unit | Any authenticated user |
| Update / delete course, unit | Any authenticated user (no ownership check) |
| Create quiz | Any authenticated user |
| Update / delete / toggle quiz | Authenticated + must be quiz owner |
| Add question to quiz | Authenticated + must be quiz owner |
| Assign / reorder quiz | Authenticated + must be quiz owner |
| Start / submit quiz attempt | Any authenticated user |
| View own attempts, attempt results | Authenticated (own data enforced in service) |
| View all attempts for a quiz | Any authenticated user |
| Create / update / delete Subject | `Admin` role only |
