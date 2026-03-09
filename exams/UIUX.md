# Chik.Exams UI/UX

## Overview

The Chik.Exams UI/UX is a web application that allows users to create and manage online exams. It is built with React and Tailwind CSS.

## Brand Identity

- Logo: A stylized "C" with a "K" inside it
- Colors: Black and white, minimal and clean
  - Primary: #314CB6
  - Secondary: #0A81D1
  - Accent: #427AA1
  - Background: #EBF2FA
  - Text: #211A1E
  - Warning: #F59E0B
  - Error: #EF4444
  - Success: #10B981
  - Info: #3B82F6
  - Border: #E5E7EB
  - Muted: #6B7280

## Layout

### Shell
A persistent sidebar + main content area layout used by all authenticated screens.

```
┌─────────────────────────────────────────────────────────┐
│  [Logo] Chik.Exams            [User menu ▾]  [Logout]   │  ← Top bar
├──────────────┬──────────────────────────────────────────┤
│              │                                          │
│  Sidebar     │  Main Content Area                       │
│  (nav links  │                                          │
│   per role)  │                                          │
│              │                                          │
└──────────────┴──────────────────────────────────────────┘
```

**Sidebar nav by role:**

| Admin | Teacher | Student |
|-------|---------|---------|
| Users | Quizzes | My Exams |
| Quizzes | Exams | |
| Exams | Students | |
| Audit Logs | | |

---

## Screens

### 1. Login

**Route:** `/login`
**Access:** Public

```
┌─────────────────────────────┐
│       [Logo]                │
│    Chik.Exams               │
│                             │
│  Username                   │
│  ┌─────────────────────┐    │
│  │                     │    │
│  └─────────────────────┘    │
│                             │
│  Password                   │
│  ┌─────────────────────┐    │
│  │                     │    │
│  └─────────────────────┘    │
│                             │
│  ┌─────────────────────┐    │
│  │       Log In        │    │
│  └─────────────────────┘    │
└─────────────────────────────┘
```

- On success: redirect to role-appropriate dashboard
- Show inline error on bad credentials

---

### 2. Change Password

**Route:** `/settings/password`
**Access:** All authenticated users (via User menu)

Fields: Current Password, New Password, Confirm New Password
Action: Save — shows success toast on completion.

---

### 3. Admin — Users

**Route:** `/users`
**Access:** Admin

Three tabs: **All** | **Teachers** | **Students**

```
Users                            [+ New User ▾]
─────────────────────────────────────────────
[All] [Teachers] [Students]      [Search...]

┌──────────────────────────────────────────────────────┐
│ Username    │ Roles           │ Created     │ Actions │
├─────────────┼─────────────────┼─────────────┼─────────┤
│ jdoe        │ Teacher         │ Jan 5, 2026 │ Edit    │
│ asmith      │ Student         │ Jan 6, 2026 │ Edit    │
└─────────────┴─────────────────┴─────────────┴─────────┘
```

**+ New User** dropdown: "New Teacher" | "New Student"

**Create/Edit User modal:**
- Username (text)
- Password (text, only shown on create)
- Role (read-only, set by which action was triggered)
- Save / Cancel

---

### 4. Admin — Audit Logs

**Route:** `/audit-logs`
**Access:** Admin

```
Audit Logs
──────────────────────────────────────────────
[Search by user or service...]   [Date range ▾]

┌────────────────────────────────────────────────────┐
│ User     │ Service          │ Entity ID │ Date      │
├──────────┼──────────────────┼───────────┼───────────┤
│ admin    │ UserService      │ 12        │ Mar 8 … │
└──────────┴──────────────────┴───────────┴───────────┘
```

Read-only, paginated.

---

### 5. Quizzes List

**Route:** `/quizzes`
**Access:** Admin (all quizzes), Teacher (own quizzes)

```
Quizzes                                 [+ New Quiz]
──────────────────────────────────────────────────
[Search...]

┌────────────────────────────────────────────────────────────┐
│ Title        │ Questions │ Duration   │ Created    │ Actions │
├──────────────┼───────────┼────────────┼────────────┼─────────┤
│ Math Quiz 1  │ 10        │ 30 min     │ Mar 1 …  │ Edit  Delete │
└──────────────┴───────────┴────────────┴────────────┴─────────┘
```

---

### 6. Create / Edit Quiz

**Route:** `/quizzes/new` | `/quizzes/:id/edit`
**Access:** Admin, Teacher

```
← Back to Quizzes

Quiz Details
──────────────
Title         [                          ]
Description   [                          ]
Duration      [  ] hrs  [  ] mins  (optional)
Examiner      [ Select teacher ▾ ]  (Admin only)

                                    [Save Quiz]

Questions                                [+ Add Question]
──────────────────────────────────────────────────────────
┌───────────────────────────────────────────────────────────┐
│ #  │ Prompt              │ Type           │ Score │ Actions │
├────┼─────────────────────┼────────────────┼───────┼─────────┤
│ 1  │ What is 2+2?        │ Single Choice  │  5    │ Edit  ⊗ │
│ 2  │ Explain gravity     │ Essay          │ 10    │ Edit  ⊗ │
└────┴─────────────────────┴────────────────┴───────┴─────────┘
```

- ⊗ = deactivate/reactivate toggle (soft delete)
- Reordering via drag-and-drop

---

### 7. Add / Edit Quiz Question

**Route:** Modal or `/quizzes/:id/questions/new`
**Access:** Admin, Teacher

```
Question Prompt
───────────────
[                                        ]

Question Type    [ Single Choice ▾ ]

Score            [    ]

─── Options (for choice-based types) ───

  ○  Option 1   [           ]  [✓ Correct]  [x]
  ○  Option 2   [           ]  [ ] Correct  [x]
  ○  Option 3   [           ]  [ ] Correct  [x]
                                   [+ Add Option]

                               [Cancel]  [Save Question]
```

- For **Multiple Choice**: checkboxes, multiple can be marked correct
- For **Single Choice / True or False**: radio buttons, only one correct
- For **Fill in the Blank / Short Answer / Essay**: no options, just prompt + score
- **True or False** auto-populates options: "True", "False"

---

### 8. Exams List

**Route:** `/exams`
**Access:** Admin (all exams), Teacher (exams they created)

```
Exams                                [+ Assign Exam]
──────────────────────────────────────────────────────
[Filter by status ▾]  [Search student...]

┌──────────────────────────────────────────────────────────────┐
│ Student  │ Quiz         │ Status    │ Score │ Started  │ Actions │
├──────────┼──────────────┼───────────┼───────┼──────────┼─────────┤
│ asmith   │ Math Quiz 1  │ Submitted │ 80/100│ Mar 5 …│ Review  │
│ bjones   │ History Q2   │ Pending   │ —     │ —       │ Cancel  │
└──────────┴──────────────┴───────────┴───────┴──────────┴─────────┘
```

**Status badges:**
- `Pending` — gray
- `In Progress` — blue
- `Submitted` — yellow (awaiting examination)
- `Examined` — green

---

### 9. Assign Exam (Create Exam)

**Route:** Modal triggered from `/exams`
**Access:** Admin, Teacher

```
Assign Exam
──────────────────────────────
Student     [ Select student ▾ ]
Quiz        [ Select quiz ▾    ]

                  [Cancel]  [Assign]
```

---

### 10. Exam Review (Teacher — Examine & Score)

**Route:** `/exams/:id`
**Access:** Admin, Teacher (examiner or creator)

```
← Back to Exams

Exam: Math Quiz 1 — asmith
Status: Submitted  │  Started: Mar 5, 10:00  │  Ended: Mar 5, 10:28

Overall Score: 80 / 100

Examiner Comment
┌────────────────────────────────────────────┐
│                                            │
└────────────────────────────────────────────┘
                              [Save Comment]

─── Questions ───────────────────────────────

  Q1. What is 2+2?  [Single Choice]  Score: 5/5 ✓
  Student answered: "4"  (correct)

  Q2. Explain gravity.  [Essay]  Score: ___/10
  Student answered: "Gravity is a force that..."
  Auto-score: —
  Examiner score: [ 8 ]
  Comment: [                            ]
                              [Save Score]
```

- Auto-scored questions (choice types) show read-only scores
- Essay / Short Answer / Fill-in-the-blank show editable examiner score + comment fields

---

### 11. Student — My Exams (Dashboard)

**Route:** `/my-exams`
**Access:** Student

Two tabs: **Pending** | **History**

```
My Exams
──────────────────────────────────
[Pending]  [History]

┌──────────────────────────────────────────────────┐
│ Quiz          │ Assigned By │ Due  │ Actions      │
├───────────────┼─────────────┼──────┼──────────────┤
│ Math Quiz 1   │ Mr. Smith   │ —    │ [Start Exam] │
│ History Quiz  │ Ms. Lee     │ —    │ [Continue]   │
└───────────────┴─────────────┴──────┴──────────────┘
```

**History tab:**
```
┌──────────────────────────────────────────────────────┐
│ Quiz          │ Score    │ Submitted  │ Actions       │
├───────────────┼──────────┼────────────┼───────────────┤
│ Math Quiz 1   │ 80/100   │ Mar 5 …  │ [Review]      │
└───────────────┴──────────┴────────────┴───────────────┘
```

---

### 12. Take Exam

**Route:** `/exams/:id/take`
**Access:** Student (only for their own assigned exams)

```
Math Quiz 1                        Time remaining: 28:42
────────────────────────────────────────────────────────
Question 3 of 10

What is the capital of France?

  ○  London
  ○  Berlin
  ● Paris                   ← selected
  ○  Madrid

──────────────────────────────────────────
[← Previous]   [1][2][✓3][4]…[10]   [Next →]

                              [Submit Exam]
```

- Timer shown if quiz has a Duration
- Question nav dots: filled = answered, empty = unanswered, current = highlighted
- **Submit Exam** prompts a confirmation dialog: "Are you sure? You cannot change your answers after submitting."
- On submit: calls EndedAt, redirects to exam review

---

### 13. Student — Exam Review

**Route:** `/exams/:id/review`
**Access:** Student (own exams only, after submission)

```
← Back to My Exams

Math Quiz 1 — Review
Submitted: Mar 5, 10:28  │  Final Score: 80 / 100

Examiner Comment: "Good work overall."

─── Questions ───────────────────────────────────

  Q1. What is 2+2?  [Single Choice]  5/5 ✓
  Your answer: "4"  ✓

  Q2. Explain gravity.  [Essay]  8/10
  Your answer: "Gravity is a force that pulls objects toward..."
  Examiner score: 8  │  "Well explained, could add more detail."

  Q3. Select all prime numbers.  [Multiple Choice]  0/5 ✗
  Your answer: "4, 7"
  Correct: "2, 3, 5, 7"
```

- Color-coded per question: green = full marks, yellow = partial, red = zero
- Examiner comments shown where present

---

## Component Library

| Component | Usage |
|-----------|-------|
| `Button` | Primary (primary color bg), Secondary (secondary color bg), Danger (red) |
| `Input` | Text, Password, Number — all with label + error state |
| `Select` | Dropdown with search |
| `Modal` | Centered overlay with backdrop |
| `Badge` | Status indicators (Pending, In Progress, Submitted, Examined) |
| `Table` | Sortable, paginated data table |
| `Toast` | Success / error notifications, auto-dismiss |
| `Tabs` | Horizontal tab switcher |
| `ConfirmDialog` | "Are you sure?" modal for destructive actions |
| `QuestionForm` | Type-aware question editor (switches form based on question type) |
| `Timer` | Countdown display for timed exams |

---

## Navigation & Role Routing

After login, redirect based on role:

| Role | Default Route |
|------|--------------|
| Admin | `/users` |
| Teacher | `/quizzes` |
| Student | `/my-exams` |

Users with multiple roles see merged nav and land on the highest-privilege dashboard.
