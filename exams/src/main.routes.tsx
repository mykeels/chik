import { Outlet } from 'react-router';
import { AppContexts } from './main.context';
import { Login } from './auth/Login';
import { AuthProvider } from './auth';

export const routes = [
  {
    path: '/',
    element: (
      <AppContexts>
        <AuthProvider>
          <Outlet />
        </AuthProvider>
      </AppContexts>
    ),
    children: [
      {
        path: '/',
        element: <div>Home</div>,
      },
    ],
  },
  {
    path: '/login',
    element: <Login />,
  },
];
