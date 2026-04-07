# Product Requirements

This is an ASP.NET Core MVC application for managing online exams. It consists of modules:

## 1. Quiz

A Quiz has properties:

- Id (long)
- Title (string)
- Description (string)
- CreatorId (long)
- ExaminerId (long?) - the teacher responsible for examining this quiz
- Duration (timespan?)
- CreatedAt (DateTime)
- UpdatedAt (DateTime)

### How it works

- A quiz is created by a user
- A quiz can have multiple questions
- When an admin creates a quiz, they can assign an examiner (teacher) who will be responsible for the quiz

## 2. User

A User has properties:

- Id (long)
- Username (string)
- Password (string)
- Roles (bitmask)
- CreatedAt (DateTime)
- UpdatedAt (DateTime)

The roles are:

| Role | Description |
|------|-------------|
| 1 | Admin (can create and manage users, exams, and questions) |
| 2 | Teacher (can create and manage exams) |
| 4 | Student (can take exams) |

## 3. Quiz Question

A Quiz Question has properties:

- Id (long)
- QuizId (long)
- Prompt (string)
- Type (QuestionType)
- Properties (string)
- Score (int)
- Order (int)
- CreatedAt (DateTime)
- UpdatedAt (DateTime)
- DeactivatedAt (DateTime?)

## 4. Quiz Question Types

- Id (long)
- Name (string)
- Description (string)
- CreatedAt (DateTime)
- UpdatedAt (DateTime)

The question types are:

| Type | Description |
|------|-------------|
| 1 | Multiple Choice |
| 2 | Single Choice |
| 3 | Fill in the Blank |
| 4 | Essay |
| 5 | Short Answer |
| 6 | True or False |

The properties are a JSON string that contains the properties of the question type. e.g.

For multiple choice:

```json
{
    "type": "multiple-choice",
    "options": [
        {
            "text": "Option 1",
            "isCorrect": true
        },
        {
            "text": "Option 2",
            "isCorrect": false
        },
        {
            "text": "Option 3",
            "isCorrect": true
        },
        {
            "text": "Option 4",
            "isCorrect": false
        }
    ]
}
```

For single choice:

```json
{
    "type": "single-choice",
    "options": [
        {
            "text": "Option 1",
            "isCorrect": true
        },
        {
            "text": "Option 2",
            "isCorrect": false
        }
        {
            "text": "Option 3",
            "isCorrect": false
        }
        {
            "text": "Option 4",
            "isCorrect": false
        }
    ]
}
```

For fill in the blank:

```json
{
    "type": "fill-in-the-blank"
}
```

For essay:

```json
{}
```

For short answer:

```json
{
    "type": "short-answer"
}
```

For true or false:

```json
{
    "type": "true-or-false",
    "options": [true, false]
}
```

## 5. Exam

An Exam represents a quiz that can be or has been taken by a user.

- Id (long)
- UserId (long)
- QuizId (long)
- CreatorId (long)
- StartedAt (DateTime)
- EndedAt (DateTime)
- Score (int?)
- ExaminerId (long?)
- ExaminerComment (string?)
- CreatedAt (DateTime)
- UpdatedAt (DateTime?)

The exam is created by a creator and can be taken by a user. The exam is started when the user starts the exam and ended when the user completes the exam. The exam is marked when the user completes the exam.

## 6. Exam Answers

An Exam Answer represents the answers given by a user to a quiz.

- Id (long)
- ExamId (long)
- QuestionId (long)
- Answer (string)
- AutoScore (int?)
- ExaminerScore (int?)
- ExaminerId (long?)
- ExaminerComment (string?)
- CreatedAt (DateTime)
- UpdatedAt (DateTime?)

The exam answer is a JSON string that contains the answers given by the user to the quiz question. It must contain the original question properties e.g.

For multiple choice:

```json
{
    "question": {
        "type": "multiple-choice",
        "options": [
            {
                "text": "Option 1",
                "isCorrect": true
            },
            {
                "text": "Option 2",
                "isCorrect": false
            },
            {
                "text": "Option 3",
                "isCorrect": true
            },
            {
                "text": "Option 4",
                "isCorrect": false
            }
        ]
    },
    "answer": ["Option 3", "Option 1"]
}
```

## 7. Audit Logs

An Audit Log represents a log of an action taken by a user.

- Id (long)
- UserId (long)
- Service (string)
- EntityId (long)
- CreatedAt (DateTime)

The audit log is a JSON string that contains the audit log properties.

## User Stories

### 1. Admin User

- We create an admin user on startup
- An admin user can create teacher users and student users
- can do everything a teacher user can do
- can update/delete any user
- can view audit logs
- can view all quizzes and exams in the system

### 2. Teacher User

- can create student users
- can create quizzes and quiz questions
- can create exams i.e. assign a quiz to a student
- can examine exams i.e. review the answers of a student
- can score exams i.e. manually score the answers of a student
- can view the scores of:
  - a student
  - an exam
- can update/delete quizzes they created
- can deactivate/reactivate quiz questions
- can view all quizzes they created
- can update/cancel exams they created

### 3. Student User

- A student user can take exams
- can view the scores of:
  - an exam they have taken
  - can review the answers of an exam they have taken, including the auto-scored and examiner-scored answers, where examiner-scored answers override auto-scored answers
- can view pending exams assigned to them
- can view their exam history
- can start an exam (sets StartedAt)
- can submit an exam (sets EndedAt)

### 4. All Users

- can log in with username/password
- can change their password
- can log out

## 8. Quiz Export

A Quiz can be exported from the `/quizzes/{quizId}/edit` page by teachers and admins who own the quiz. The export is a `.zip` file containing:

- `index.yaml` — the quiz definition (metadata + questions)
- `assets/` — optional folder for referenced media files (images, etc.)

The exported zip filename should be `{quiz-title-slug}.zip`.

### Export Format

The `index.yaml` follows the schema defined in `quiz.schema.json`. It includes quiz metadata and all active questions with their properties.

Asset files (e.g. images embedded in question prompts) are referenced using relative paths like `assets/image.png` within the YAML.

### Export User Story

- Teacher or Admin visits `/quizzes/{quizId}/edit`
- Clicks "Export Quiz" button
- Browser downloads `{quiz-title-slug}.zip`
- Only active questions (not deactivated) are included in the export

## 9. Quiz Import

A Quiz can be imported from the `/quizzes/new` page. Three input methods are supported:

| Method | Description |
|--------|-------------|
| Plain text YAML | Paste YAML directly into a code-editor field |
| YAML file upload | Upload a `.yaml` or `.yml` file |
| Zip file upload | Upload a `.zip` containing `index.yaml` and an optional `assets/` folder |

### Import Behaviour

- On successful import, a new Quiz is created (does not overwrite existing quizzes)
- The importing user becomes the `CreatorId`
- Questions are created in the order specified in the YAML
- Assets referenced in the YAML are extracted from the zip and stored
- Validation errors (schema violations, missing assets) are shown inline before saving

### Import User Story

- Teacher or Admin visits `/quizzes/new`
- Selects an import method (YAML text, YAML file, or Zip file)
- Reviews a preview of the quiz (title, question count, duration)
- Confirms import — quiz and questions are persisted
- User is redirected to `/quizzes/{newQuizId}/edit`

## Implementation Notes

- Quiz question types are seeded on startup
- When `ExamService.AutoScore(auth, examId)` is called, it will auto-score exam answers where possible
- When `ExamService.GetScores(auth, examId)` is called, it will return the score of the exam, which is an aggregate of the auto-scored and examiner-scored answers, where examiner-scored answers override auto-scored answers
- Every service method should have `Auth auth` as its first parameter. This is used to determine whether the user has the required roles to perform the action.
- Every service method that mutates data should create an audit log e.g.

```cs
await auditLogService.Create(
    auth, 
    new AuditLog.Create<User.Create>(
        $"{nameof(UserService)}.{nameof(Create)}", 
        userDbo.Id, 
        user
    )
);
```
