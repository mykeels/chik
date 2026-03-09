import { Outlet } from 'react-router';
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
      },
      {
        path: '/audit-logs',
        element: <AuditLogs />,
      },
      {
        path: '/quizzes',
        element: <Quizzes />,
      },
      {
        path: '/quizzes/new',
        element: <QuizEditor />,
      },
      {
        path: '/quizzes/:id/edit',
        element: <QuizEditor />,
      },
      {
        path: '/exams',
        element: <Exams />,
      },
      {
        path: '/exams/:id',
        element: <ExamReview />,
      },
      {
        path: '/my-exams',
        element: <MyExams />,
      },
      {
        path: '/exams/:id/take',
        element: <TakeExam />,
      },
      {
        path: '/exams/:id/review',
        element: <StudentExamReview />,
      },
      {
        path: '/settings/password',
        element: <ChangePassword />,
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
  },
];
