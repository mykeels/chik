# Product Requirements

This is an ASP.NET Core MVC application for managing online exams. It consists of modules:

## 1. Quiz

A Quiz has properties:

- Id (long)
- Title (string)
- Description (string)
- CreatorId (long)
- Duration (timespan?)
- CreatedAt (DateTime)
- UpdatedAt (DateTime)

### How it works

- A quiz is created by a user
- A quiz can have multiple questions

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
- Entity (string)
- EntityId (long)
- ApplicationContext (JSON string)
- OldValue (string)
- NewValue (string)
- CreatedAt (DateTime)

The audit log is a JSON string that contains the audit log properties.