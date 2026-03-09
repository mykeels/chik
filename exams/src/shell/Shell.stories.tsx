import type { Meta, StoryObj } from '@storybook/react';
import { ShellLayout } from './Shell';
import { MemoryRouter, Route, Routes } from 'react-router';
import { QueryClient, QueryClientProvider } from 'react-query';
import { ioc } from '@/utils/ioc';

const queryClient = new QueryClient();

ioc.add(
  (keys) => keys.getMe,
  async () => ({ id: 1, username: 'admin', roles: [1], createdAt: '2026-01-01' })
);

const meta: Meta<typeof ShellLayout> = {
  title: 'Shell/Shell',
  component: ShellLayout,
  decorators: [
    (Story) => (
      <QueryClientProvider client={queryClient}>
        <MemoryRouter initialEntries={['/quizzes']}>
          <Routes>
            <Route path="/*" element={<Story />} />
          </Routes>
        </MemoryRouter>
      </QueryClientProvider>
    ),
  ],
};

export default meta;
type Story = StoryObj<typeof ShellLayout>;

export const AdminShell: Story = {};
