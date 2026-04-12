# Plan: Introducing classes (students, teachers, exams)

## Goals

- **Students** belong to **exactly one** class at a time.
- **Teachers** may be associated with **many** classes.
- **Assigning an exam to a class** (from the frontend) creates an exam assignment for **each student** in that class (same quiz, one `Exam` row per student).
- **`ExamDbo` stores `StudentClassId`** so we always know **which class the student was in** when that exam row was created (historical snapshot; survives later class changes or student moves).

## Current state in the repo

- Partial types exist: `ClassDbo`, `UserClassDbo`, `Class` model, `Student` / `Teacher` records on `User`, `IClassService` stub, and `ExamDbo` already declares `StudentClassId` plus `StudentClass` navigation.
- **`ChikExamsDbContext` does not yet** register `classes` / `user_classes` or map `exams.student_class_id`; `AddExam` fluent config has no `StudentClassId` column.
- **`Exam` domain model and `ExamDbo.ToModel()`** do not yet surface `StudentClassId` / class metadata; `ExamRepository.Create` does not set it.
- **`UserClassDbo.UserId` is `int`** in the scratch file — it should align with **`UserDbo.Id` (`long`)** everywhere.
- **Users API** (`CreateUserRequest` / `UpdateUserRequest`) has no class fields; **User** GET/search responses do not hydrate `Student` / `Teacher` with class data.
- **Frontend** (`exams/src/users/Users.tsx`): create/edit modal is username/password only; **no class UI**.
- **Assign exam** (`AssignExamModal`, `Exams.tsx`): multi-select **students** + quiz; no **assign by class** path.

This plan assumes we implement the missing wiring and APIs rather than only documenting intent.

---

## 1. Data model & migrations

### 1.1 Tables

| Table | Purpose |
|--------|---------|
| `classes` | `id`, `name`, timestamps (matches `ClassDbo` / `Class`). |
| `user_classes` | Join table: `user_id` (FK → `users.id`), `class_id` (FK → `classes.id`), optional `id` PK, timestamps. **Teachers:** many rows per user. **Students:** exactly one row** enforcing one class (unique index on `user_id` where role is student, or enforce in application + optional partial unique constraint if the DB supports it). |

**Recommendation:** Enforce “student has one class” in **service layer** (on create/update, replace single `user_classes` row for students). Optionally add a **partial unique index** on `user_id` for student users if you add a `role` or `is_primary` flag to the join table; otherwise app-level validation is enough for v1.

### 1.2 `exams` table

- Add nullable or required **`student_class_id`** FK → `classes.id` (nullable only if you must support legacy rows before backfill; prefer **required for new exams** once rollout is complete).
- Configure EF: map `ExamDbo.StudentClassId`, relationship `Exam` → `Class` (`StudentClass`), `OnDelete` behavior (typically **Restrict**).

### 1.3 EF Core

- Add `DbSet<ClassDbo>`, `DbSet<UserClassDbo>` (or name consistent with existing snake_case tables).
- Implement `AddClass`, `AddUserClass`, extend `AddExam` and relationship methods (`AddUserRelationships` for `User` ↔ `UserClass`, `Class` ↔ `UserClass`, `Exam` → `StudentClass`).
- **Fix** `UserClassDbo.UserId` to **`long`** and shadow properties if needed.

### 1.4 Migration strategy

- Create migration adding tables + `student_class_id` on `exams`.
- **Backfill:** for existing exams, set `student_class_id` from the student’s **current** class if available, else leave null (if column nullable) or assign a default “Unassigned” class (product decision).

---

## 2. Backend: class management

### 2.1 `IClassService` / `ClassService`

- **CRUD** (at minimum **list** + **create**; **update**/**delete** if admins need rename/remove).
- **Authorization:** e.g. Admin full access; Teachers may only read classes they are linked to (if that matches product rules).

### 2.2 API surface

- **`GET /api/classes`** — list/search for dropdowns and Autocomplete.
- **`POST /api/classes`** (and optional `PUT`/`DELETE`) — admin-only unless PRD says otherwise.

Wire controller + OpenAPI; regenerate **`chikexams-client`** / `chikexams.service.ts` patterns to match existing hooks.

---

## 3. Backend: users and class membership

### 3.1 `User.Create` / `User.Update`

Extend domain records (not only API DTOs) so the service can persist membership in one transaction:

- **Student:** `ClassId` (single `int`, required when role includes Student for create; optional on update to move class).
- **Teacher:** `ClassIds` (`List<int>`, optional empty).

**Validation:**

- If roles include **Student**, require **exactly one** class and write **one** `UserClassDbo` row (or update it).
- If roles include **Teacher**, sync **many** `UserClassDbo` rows from `ClassIds` (replace set or diff — **replace** is simpler).
- If a user is **both** teacher and student (if allowed), define behavior (likely **disallowed** for v1 — validate mutually exclusive roles for class fields).

### 3.2 `UserRepository` / `UserDbo`

- On **Get** (and Search when needed), **Include** `UserClasses` → `Class` so `UserDbo.ToModel()` can populate `Student` / `Teacher`.
- Implement **`UserDbo.ToModel()`** to set `Student` / `Teacher` from loaded classes (Student: single class; Teacher: list).

### 3.3 API requests

Extend **`CreateUserRequest`** / **`UpdateUserRequest`**:

- `ClassId` (optional `int?`) — when creating/updating a **student**.
- `ClassIds` (optional `List<int>?`) — when creating/updating a **teacher**.

Controllers map into `User.Create` / `User.Update`.

### 3.4 Search / list users

- Ensure **search results** used by the admin UI include enough class data for the Edit modal (either embed in `User` or document that **GET user by id** is called on edit — prefer **consistent shape** on list + get).

---

## 4. Backend: exams and `StudentClassId`

### 4.1 `Exam.Create`

- Add **`StudentClassId`** (or derive it inside the service).
- When creating an exam for a **student**, set `StudentClassId` to that student’s **current** class from `user_classes` (or dedicated student class field). **Fail** with a clear error if the student has **no** class when the product requires it.

### 4.2 Assign exam to a class (bulk)

Two equivalent approaches; pick one for consistency:

**A. New endpoint (recommended for clarity)**

- `POST /api/exams/assign-to-class` with body `{ classId, quizId }` (creator from auth).
- Server loads all **student user IDs** in that class, creates one `Exam` per student, each with **`StudentClassId = classId`**.

**B. Extend existing create**

- Keep `POST /api/exams` for single student; frontend loops for class assignment (more round-trips, easier to get inconsistent `StudentClassId` if client bugs).

**Recommendation:** implement **A** plus keep single-create for edge cases; both paths must set `StudentClassId`.

### 4.3 `Exam` model & OpenAPI

- Add **`StudentClassId`** (and optional nested **`Class`** for display) to **`Exam`** record and JSON responses.
- Update **`ExamDbo` implicit operator / `ToModel()`** to map `StudentClassId` and optional `StudentClass` navigation.
- Regenerate OpenAPI client types for the SPA.

### 4.4 Authorization

- Reuse existing rules: only Admin/Teacher create exams; if teachers are scoped to **their** classes, enforce **class membership** on `assign-to-class` and single-create (teacher can only assign to students in classes they teach).

---

## 5. Frontend: Users (create + Edit User modal)

**File:** `exams/src/users/Users.tsx` (and Storybook / tests if present).

### 5.1 Data loading

- Fetch **class list** (`useClasses` or reuse React Query pattern in `chikexams.hooks.ts`) when opening create/edit modals.

### 5.2 Create student

- Add **`Select`** (MUI) for **class** — required before submit.
- Pass **`classId`** in `createUser` payload (types updated after OpenAPI regen).

### 5.3 Create teacher

- Add **`Autocomplete`** (MUI) **multiple** for **classes** — optional or required per product; map to **`classIds`** in API.

### 5.4 Edit user

- **Student:** show current class in a **`Select`**; on save, call **`updateUser`** with **`classId`**.
- **Teacher:** show classes in **`Autocomplete` multiple**; on save, send **`classIds`**.
- Non-student/non-teacher: hide class controls (same as today).

### 5.5 Forms

- Extend **react-hook-form** types (`UserFormData`) with `classId` / `classIds`.
- Reset/load defaults from `editingUser.student` / `editingUser.teacher` when API returns them.

---

## 6. Frontend: Assign exam by class

**Files:** `exams/src/exams/AssignExamModal.tsx`, `exams/src/exams/Exams.tsx`, `exams/src/services/chikexams.service.ts`.

### 6.1 UX

- Add a **mode** or second path: **assign by class** vs **assign by students** (tabs or radio).
- **By class:** `Select` or single-select Autocomplete for **class** + existing **Quiz** `Select`; submit calls **`assignExamToClass(classId, quizId)`** (new service method).

### 6.2 Behavior

- On success, invalidate **`searchExams`** cache (already done for assign).
- Optional: filter student Autocomplete by selected class when using combined UI (nice-to-have).

---

## 7. OpenAPI & client

- **`openapi/ChikExams.openapi.json`** and **`chikexams-client`** will be automatically updated on dotnet build

---

## 8. Tests & quality

- **Unit/integration:** `UserService` create/update with class rows; `ExamService` assign-to-class; repository queries with Includes.
- **E2E** (`exams/e2e/*.spec.ts`): extend **assign exam** flows to cover **class assignment**; user flows for class on create/edit.
- **Seed data** (`Seeder.cs`): create classes, attach users, set `StudentClassId` on seeded exams.

---

## 9. Suggested implementation order

1. DB + EF models (`classes`, `user_classes`, `exams.student_class_id`) and fix `UserClassId` types.
2. Class list/create API + client.
3. User create/update + hydration of `Student` / `Teacher` on GET/search.
4. Exam create sets `StudentClassId`; extend `Exam` model + API.
5. `assign-to-class` endpoint + frontend Assign modal.
6. Users modal: **Select** (student) + **Autocomplete** (teacher).
7. Add a ClassSeeder to create classes:
  - PREPARATORY
  - KINDERGARTEN 1
  - KINDERGARTEN 2
  - NURSERY
  - BASIC 1
  - BASIC 2
  - BASIC 3
  - BASIC 4
  - BASIC 5
  - JSS 1
  - JSS 2
  - JSS 3
  - SSS 1
  - SSS 2
  - SSS 3

---

## 10. Open decisions (resolve before or during implementation)

- **Nullable `student_class_id` on legacy exams** vs mandatory + backfill class.
  - Answer: mandatory. There is no production data that needs to be migrated.
- **Teacher-only class scope:** may a teacher assign exams to any class or only classes they belong to?
  - Answer: only classes they belong to.
- **Renaming/deleting a class** when `user_classes` and `exams` reference it (Restrict vs soft-delete).
  - Answer: The UI should not allow adding/renaming/deleting a class. Creating classes is handled by the ClassSeeder.
- **Student moves class** after exams assigned: historical **`StudentClassId`** on `Exam` remains the source of truth for reporting.
  - Answer: The historical `StudentClassId` on `Exam` remains the source of truth for reporting.
