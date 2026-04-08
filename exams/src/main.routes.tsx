import { AppContexts } from './main.context';
import { Login } from './auth/Login';
import { AuthProvider } from './auth';
import { ShellLayout } from './shell/Shell';
import { Users } from './users/Users';
import { AuditLogs } from './audit-logs/AuditLogs';
import { Quizzes } from './quizzes/Quizzes';
import { QuizEditor } from './quizzes/QuizEditor';
import { Exams } from './exams/Exams';
import { ExamReview } from './exams/ExamReview';
import { MyExams } from './exams/MyExams';
import { TakeExam } from './exams/TakeExam';
import { StudentExamReview } from './exams/StudentExamReview';
import { ChangePassword } from './settings/ChangePassword';

export const routes = [
  {
    path: '/',
    element: (
      <AppContexts>
        <AuthProvider>
          <ShellLayout />
        </AuthProvider>
      </AppContexts>
    ),
    children: [
      {
        path: '/users',
        element: <Users />,
        handle: { title: 'Users' },
      },
      {
        path: '/audit-logs',
        element: <AuditLogs />,
        handle: { title: 'Audit Logs' },
      },
      {
        path: '/quizzes',
        element: <Quizzes />,
        handle: { title: 'Quizzes' },
      },
      {
        path: '/quizzes/new',
        element: <QuizEditor />,
        handle: { title: 'New Quiz' },
      },
      {
        path: '/quizzes/:id/edit',
        element: <QuizEditor />,
        handle: { title: 'Edit Quiz' },
      },
      {
        path: '/exams',
        element: <Exams />,
        handle: { title: 'Exams' },
      },
      {
        path: '/exams/:id',
        element: <ExamReview />,
        handle: { title: 'Exam Review' },
      },
      {
        path: '/my-exams',
        element: <MyExams />,
        handle: { title: 'My Exams' },
      },
      {
        path: '/exams/:id/take',
        element: <TakeExam />,
        handle: { title: 'Take Exam' },
      },
      {
        path: '/exams/:id/review',
        element: <StudentExamReview />,
        handle: { title: 'Student Exam Review' },
      },
      {
        path: '/settings/password',
        element: <ChangePassword />,
        handle: { title: 'Change Password' },
      },
    ],
  },
  {
    path: '/login',
    element: (
      <AppContexts>
        <Login />
      </AppContexts>
    ),
    handle: { title: 'Login' },
  },
];
