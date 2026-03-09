# E2E Tests

Url: <http://localhost:5174>

---

## Admin Flow

**Credentials:** `admin` / `admin123`

### 1. Login & Navigation

- [ ] Navigate to `/login`
- [ ] Enter username "admin" and password "admin123"
- [ ] Click "Log In" button
- [ ] Verify redirect to `/users` (Admin default route)
- [ ] Verify sidebar shows: Users, Quizzes, Exams, Audit Logs

### 2. User Management

- [ ] Click "Users" in sidebar → verify `/users` route
- [ ] Verify tabs: All | Teachers | Students
- [ ] Verify table shows columns: Username, Roles, Created, Actions

#### Create Teacher

- [ ] Click "+ New User" dropdown → select "New Teacher"
- [ ] Verify modal opens with title "Create User"
- [ ] Enter username (e.g., "teacher_e2e")
- [ ] Enter password
- [ ] Verify Role field shows "Teacher" (read-only)
- [ ] Click "Save" → verify toast success
- [ ] Verify new teacher appears in "Teachers" tab

#### Create Student

- [ ] Click "+ New User" dropdown → select "New Student"
- [ ] Enter username (e.g., "student_e2e") and password
- [ ] Verify Role field shows "Student" (read-only)
- [ ] Click "Save" → verify toast success
- [ ] Verify new student appears in "Students" tab

#### Edit User

- [ ] Click "Edit" on any user row
- [ ] Verify modal opens with pre-filled data
- [ ] Modify username → click "Save"
- [ ] Verify changes reflected in table

### 3. Quiz Management

- [ ] Click "Quizzes" in sidebar → verify `/quizzes` route
- [ ] Verify table shows: Title, Questions, Duration, Created, Actions

#### Create Quiz

- [ ] Click "+ New Quiz" → verify `/quizzes/new` route
- [ ] Enter Title: "Admin E2E Quiz"
- [ ] Enter Description: "Test quiz created by admin"
- [ ] Set Duration: 1 hr 30 mins
- [ ] Select Examiner (dropdown of teachers) — Admin-only field
- [ ] Click "Save Quiz" → verify success
- [ ] Verify redirected to edit page with quiz ID

#### Add Questions

- [ ] Click "+ Add Question"
- [ ] Enter Prompt: "What is 2 + 2?"
- [ ] Select Type: "Single Choice"
- [ ] Enter Score: 5
- [ ] Add options: "3", "4" (mark correct), "5", "6"
- [ ] Click "Save Question" → verify question appears in table

- [ ] Add another question (Multiple Choice)
- [ ] Prompt: "Select all prime numbers"
- [ ] Type: "Multiple Choice"
- [ ] Score: 10
- [ ] Options: "2" (correct), "3" (correct), "4", "5" (correct)
- [ ] Save → verify in table

- [ ] Add Essay question
- [ ] Prompt: "Explain the theory of relativity"
- [ ] Type: "Essay"
- [ ] Score: 20
- [ ] Save → verify in table

#### Deactivate Question

- [ ] Click ⊗ (deactivate) on a question
- [ ] Verify question is visually marked as deactivated
- [ ] Click ⊗ again to reactivate

### 4. Exam Management

- [ ] Click "Exams" in sidebar → verify `/exams` route
- [ ] Verify table shows: Student, Quiz, Status, Score, Started, Actions
- [ ] Verify status filter dropdown

#### Assign Exam

- [ ] Click "+ Assign Exam"
- [ ] Select Student from dropdown
- [ ] Select Quiz from dropdown
- [ ] Click "Assign" → verify success toast
- [ ] Verify new exam appears with status "Pending"

#### Review Exam (after student submits)

- [ ] Find exam with status "Submitted"
- [ ] Click "Review" → verify `/exams/:id` route
- [ ] Verify exam details: quiz title, student, timestamps
- [ ] Verify auto-scored questions show read-only scores
- [ ] For Essay/Short Answer questions:
  - [ ] Enter examiner score
  - [ ] Enter examiner comment
  - [ ] Click "Save Score"
- [ ] Add overall examiner comment
- [ ] Click "Save Comment"
- [ ] Verify status changes to "Examined"

### 5. Audit Logs

- [ ] Click "Audit Logs" in sidebar → verify `/audit-logs` route
- [ ] Verify table shows: User, Service, Entity ID, Date
- [ ] Test search by user
- [ ] Test date range filter
- [ ] Verify data is read-only (no edit actions)

### 6. Change Password

- [ ] Click User menu → "Change Password"
- [ ] Verify `/settings/password` route
- [ ] Enter current password
- [ ] Enter new password + confirm
- [ ] Click "Save" → verify success toast

### 7. Logout

- [ ] Click "Logout" button
- [ ] Verify redirect to `/login`
- [ ] Verify cannot access protected routes

---

## Teacher Flow

**Credentials:** `teacher1` / `teacher123`

### 1. Login & Navigation

- [ ] Navigate to `/login`
- [ ] Enter username "teacher1" and password "teacher123"
- [ ] Click "Log In"
- [ ] Verify redirect to `/quizzes` (Teacher default route)
- [ ] Verify sidebar shows: Quizzes, Exams, Students
- [ ] Verify NO access to: Users (admin tab), Audit Logs

### 2. Quiz Management (Own Quizzes Only)

- [ ] Verify on `/quizzes` page
- [ ] Verify only own quizzes are visible (created by teacher1)

#### Create Quiz

- [ ] Click "+ New Quiz"
- [ ] Enter Title: "Teacher E2E Quiz"
- [ ] Enter Description: "Quiz for testing teacher flow"
- [ ] Set Duration: 45 mins
- [ ] Verify NO Examiner field (Admin-only)
- [ ] Click "Save Quiz"

#### Add Questions (All Types)

- [ ] Add Single Choice question
  - Prompt: "Capital of France?"
  - Options: "London", "Paris" (correct), "Berlin", "Madrid"
  - Score: 5

- [ ] Add Multiple Choice question
  - Prompt: "Which are programming languages?"
  - Options: "Python" (correct), "HTML", "JavaScript" (correct), "CSS"
  - Score: 10

- [ ] Add True/False question
  - Prompt: "The Earth is flat"
  - Verify options auto-populate: "True", "False" (correct)
  - Score: 5

- [ ] Add Fill in the Blank question
  - Prompt: "The chemical symbol for water is ___"
  - Score: 5

- [ ] Add Short Answer question
  - Prompt: "What is the capital of Japan?"
  - Score: 5

- [ ] Add Essay question
  - Prompt: "Describe the water cycle"
  - Score: 20

#### Edit Quiz

- [ ] Navigate to quiz list
- [ ] Click "Edit" on own quiz
- [ ] Modify title/description
- [ ] Save → verify changes

#### Delete Quiz

- [ ] Click "Delete" on own quiz
- [ ] Verify confirmation dialog
- [ ] Confirm → verify quiz removed from list

#### Reorder Questions

- [ ] Open quiz editor
- [ ] Drag question to new position
- [ ] Verify order changes saved

### 3. Student Management

- [ ] Click "Students" in sidebar
- [ ] Verify can only see/create students (not teachers/admins)

#### Create Student

- [ ] Click "+ New Student"
- [ ] Enter username and password
- [ ] Save → verify student created

### 4. Exam Management

- [ ] Click "Exams" in sidebar → verify `/exams` route
- [ ] Verify only sees exams for quizzes they created/examine

#### Assign Exam to Student

- [ ] Click "+ Assign Exam"
- [ ] Select a student
- [ ] Select one of own quizzes
- [ ] Click "Assign" → verify success
- [ ] Verify exam appears with "Pending" status

#### Cancel Pending Exam

- [ ] Find exam with "Pending" status
- [ ] Click "Cancel"
- [ ] Verify confirmation dialog
- [ ] Confirm → verify exam removed or marked cancelled

#### Examine Submitted Exam

- [ ] Find exam with "Submitted" status
- [ ] Click "Review"
- [ ] Verify exam review page loads

##### Auto-Scored Questions

- [ ] Verify Single Choice shows auto-score (read-only)
- [ ] Verify Multiple Choice shows auto-score (read-only)
- [ ] Verify True/False shows auto-score (read-only)

##### Manual Scoring

- [ ] For Fill in the Blank:
  - [ ] View student answer
  - [ ] Enter examiner score (0 to max)
  - [ ] Enter comment
  - [ ] Save

- [ ] For Short Answer:
  - [ ] View student answer
  - [ ] Enter examiner score
  - [ ] Enter comment
  - [ ] Save

- [ ] For Essay:
  - [ ] View student answer
  - [ ] Enter examiner score
  - [ ] Enter detailed comment
  - [ ] Save

##### Finalize Examination

- [ ] Add overall examiner comment
- [ ] Save → verify status changes to "Examined"
- [ ] Verify total score calculated correctly

### 5. Change Password

- [ ] Navigate to `/settings/password`
- [ ] Change password successfully

### 6. Logout

- [ ] Click "Logout"
- [ ] Verify redirect to login

---

## Student Flow

**Credentials:** `student1` / `student123`

### 1. Login & Navigation

- [ ] Navigate to `/login`
- [ ] Enter username "student1" and password "student123"
- [ ] Click "Log In"
- [ ] Verify redirect to `/my-exams` (Student default route)
- [ ] Verify sidebar shows ONLY: My Exams
- [ ] Verify NO access to: Users, Quizzes, Exams, Audit Logs

### 2. My Exams Dashboard

- [ ] Verify on `/my-exams` page
- [ ] Verify tabs: Pending | History

#### Pending Tab

- [ ] Click "Pending" tab
- [ ] Verify table shows: Quiz, Assigned By, Due, Actions
- [ ] Verify pending exams have "Start Exam" or "Continue" button
- [ ] Verify "Continue" appears for in-progress exams

#### History Tab

- [ ] Click "History" tab
- [ ] Verify table shows: Quiz, Score, Submitted, Actions
- [ ] Verify completed exams have "Review" button

### 3. Take Exam (Full Flow)

#### Start Exam

- [ ] Go to "Pending" tab
- [ ] Click "Start Exam" on a pending exam
- [ ] Verify redirect to `/exams/:id/take`
- [ ] Verify exam title displayed
- [ ] Verify timer shown (if quiz has duration)
- [ ] Verify question counter "Question 1 of X"

#### Answer Single Choice Question

- [ ] Read question prompt
- [ ] Select one radio option
- [ ] Verify selection is highlighted
- [ ] Click "Next →"

#### Answer Multiple Choice Question

- [ ] Read question prompt
- [ ] Select multiple checkbox options
- [ ] Verify selections are highlighted
- [ ] Click "Next →"

#### Answer True/False Question

- [ ] Read question prompt
- [ ] Select "True" or "False"
- [ ] Click "Next →"

#### Answer Fill in the Blank Question

- [ ] Read question prompt
- [ ] Enter text answer in input field
- [ ] Click "Next →"

#### Answer Short Answer Question

- [ ] Read question prompt
- [ ] Enter brief text answer
- [ ] Click "Next →"

#### Answer Essay Question

- [ ] Read question prompt
- [ ] Enter detailed essay response in textarea
- [ ] Click "Next →"

#### Navigation

- [ ] Use "← Previous" to go back
- [ ] Verify previous answer is preserved
- [ ] Use question nav dots [1][2][3]... to jump to specific question
- [ ] Verify filled dots = answered, empty = unanswered
- [ ] Verify current question is highlighted

#### Submit Exam

- [ ] Navigate to last question
- [ ] Answer if not answered
- [ ] Click "Submit Exam"
- [ ] Verify confirmation dialog: "Are you sure? You cannot change your answers after submitting."
- [ ] Click "Cancel" → verify stays on exam
- [ ] Click "Submit Exam" again → Click "Confirm"
- [ ] Verify redirect to exam review page
- [ ] Verify EndedAt is set

### 4. Resume In-Progress Exam

- [ ] Start an exam but don't submit
- [ ] Navigate away or logout
- [ ] Log back in
- [ ] Go to "Pending" tab
- [ ] Verify exam shows "Continue" button
- [ ] Click "Continue"
- [ ] Verify returns to exam with previous answers preserved
- [ ] Verify timer continues from where it left off (if timed)

### 5. Exam Review (After Submission)

#### Before Examination

- [ ] After submitting, verify on `/exams/:id/review`
- [ ] Verify status shows "Submitted" (awaiting examination)
- [ ] Verify can see own answers
- [ ] Verify auto-scores shown for choice questions
- [ ] Verify Essay/Short Answer show "Awaiting examiner" or no score yet

#### After Examination

- [ ] Navigate to "History" tab
- [ ] Find "Examined" exam
- [ ] Click "Review"
- [ ] Verify final score displayed
- [ ] Verify examiner comment displayed (if any)

##### Per-Question Review

- [ ] Verify each question shows:
  - Question number and prompt
  - Question type
  - Your answer
  - Score (X/Y format)
- [ ] For choice questions:
  - [ ] Verify correct answer shown
  - [ ] Verify ✓ if correct, ✗ if incorrect
- [ ] For Essay/Short Answer:
  - [ ] Verify examiner score shown
  - [ ] Verify examiner comment shown (if any)

##### Color Coding

- [ ] Verify green = full marks
- [ ] Verify yellow = partial marks
- [ ] Verify red = zero marks

### 6. Timed Exam Behavior

- [ ] Start a timed exam
- [ ] Verify countdown timer displayed
- [ ] Wait or simulate time passing
- [ ] Verify timer updates in real-time
- [ ] (Optional) Verify auto-submit when timer expires

### 7. Change Password

- [ ] Click user menu → "Change Password"
- [ ] Enter current password: "student123"
- [ ] Enter new password and confirm
- [ ] Click "Save"
- [ ] Verify success toast
- [ ] Logout and login with new password

### 8. Logout

- [ ] Click "Logout"
- [ ] Verify redirect to `/login`
- [ ] Verify cannot access `/my-exams` without login

---

## Cross-Role Scenarios

### Role-Based Access Control

- [ ] Login as Student → try to navigate to `/users` → verify access denied/redirect
- [ ] Login as Teacher → try to navigate to `/audit-logs` → verify access denied/redirect
- [ ] Login as Student → try to navigate to `/quizzes` → verify access denied/redirect

### Exam Lifecycle (End-to-End)

1. [ ] Admin creates quiz with examiner assigned
2. [ ] Teacher (examiner) assigns exam to student
3. [ ] Student takes and submits exam
4. [ ] Teacher examines and scores exam
5. [ ] Student reviews final scores and comments
6. [ ] Admin verifies audit logs show all actions

### Error Handling

- [ ] Login with wrong credentials → verify error message
- [ ] Submit exam with unanswered questions → verify warning/handling
- [ ] Try to access exam not assigned to you → verify access denied
- [ ] Network error during exam → verify graceful handling

---

## Test Data Cleanup Notes

- Clean up test users: `teacher_e2e`, `student_e2e`
- Clean up test quizzes: "Admin E2E Quiz", "Teacher E2E Quiz"
- Or use database seeding/reset between test runs
