import { createBrowserRouter, RouterProvider } from 'react-router';
import { routes } from './main.routes.tsx';

export const App = ({ basename = import.meta.env.BASE_URL || '/' }) => {
  // Create a new router instance
  const router = createBrowserRouter(routes, {
    basename,
  });

  return <RouterProvider router={router} />;
};
