# Entity Relationship Diagram

```mermaid
erDiagram
    users {
        bigint id PK
        varchar username UK
        varchar password
        int roles
        timestamp created_at
        timestamp updated_at
    }

    quizzes {
        bigint id PK
        varchar title
        text description
        bigint creator_id FK
        interval duration
        timestamp created_at
        timestamp updated_at
    }

    quiz_question_types {
        bigint id PK
        varchar name UK
        varchar description
        timestamp created_at
        timestamp updated_at
    }

    quiz_questions {
        bigint id PK
        bigint quiz_id FK
        text prompt
        bigint type_id FK
        jsonb properties
        int score
        int order
        timestamp created_at
        timestamp updated_at
        timestamp deactivated_at
    }

    exams {
        bigint id PK
        bigint user_id FK
        bigint quiz_id FK
        bigint creator_id FK
        timestamp started_at
        timestamp ended_at
        int score
        bigint examiner_id FK
        text examiner_comment
        timestamp created_at
        timestamp updated_at
    }

    exam_answers {
        bigint id PK
        bigint exam_id FK
        bigint question_id FK
        jsonb answer
        int auto_score
        int examiner_score
        bigint examiner_id FK
        text examiner_comment
        timestamp created_at
        timestamp updated_at
    }

    audit_logs {
        bigint id PK
        bigint user_id FK
        varchar entity
        bigint entity_id
        jsonb application_context
        jsonb old_value
        jsonb new_value
        timestamp created_at
    }

    ip_address_locations {
        uuid id PK
        varchar ip_address UK
        varchar country_code
    }

    logins {
        uuid id PK
        bigint user_id FK
        uuid ip_address_location_id FK
        timestamp created_at
    }

    %% User relationships
    users ||--o{ quizzes : "creates"
    users ||--o{ exams : "creates"
    users ||--o{ exams : "takes"
    users ||--o{ exams : "examines"
    users ||--o{ exam_answers : "examines"
    users ||--o{ audit_logs : "performs"

    %% Quiz relationships
    quizzes ||--o{ quiz_questions : "contains"
    quizzes ||--o{ exams : "has"

    %% Question relationships
    quiz_question_types ||--o{ quiz_questions : "categorizes"
    quiz_questions ||--o{ exam_answers : "answered_in"

    %% Exam relationships
    exams ||--o{ exam_answers : "contains"

    %% Login relationships
    users ||--o{ logins : "has"
    ip_address_locations ||--o{ logins : "tracks"
```

## Relationships Summary

| From | To | Relationship | Foreign Key | On Delete |
|------|-----|--------------|-------------|-----------|
| `users` | `quizzes` | One-to-Many | `creator_id` | Restrict |
| `users` | `exams` | One-to-Many | `creator_id` | Restrict |
| `users` | `exams` | One-to-Many | `user_id` | Restrict |
| `users` | `exams` | One-to-Many | `examiner_id` | Restrict |
| `users` | `exam_answers` | One-to-Many | `examiner_id` | Restrict |
| `users` | `audit_logs` | One-to-Many | `user_id` | Restrict |
| `quizzes` | `quiz_questions` | One-to-Many | `quiz_id` | **Cascade** |
| `quizzes` | `exams` | One-to-Many | `quiz_id` | Restrict |
| `quiz_question_types` | `quiz_questions` | One-to-Many | `type_id` | Restrict |
| `quiz_questions` | `exam_answers` | One-to-Many | `question_id` | Restrict |
| `exams` | `exam_answers` | One-to-Many | `exam_id` | **Cascade** |
| `users` | `logins` | One-to-Many | `user_id` | **Cascade** |
| `ip_address_locations` | `logins` | One-to-Many | `ip_address_location_id` | Restrict |

## Indexes

| Table | Columns | Unique |
|-------|---------|--------|
| `users` | `username` | ✓ |
| `quiz_question_types` | `name` | ✓ |
| `quiz_questions` | `quiz_id`, `order` | |
| `exams` | `user_id`, `quiz_id` | |
| `exams` | `creator_id` | |
| `exam_answers` | `exam_id`, `question_id` | ✓ |
| `audit_logs` | `entity`, `entity_id` | |
| `audit_logs` | `user_id` | |
| `audit_logs` | `created_at` | |
| `ip_address_locations` | `ip_address` | ✓ |
| `logins` | `user_id` | |
| `logins` | `created_at` | |
