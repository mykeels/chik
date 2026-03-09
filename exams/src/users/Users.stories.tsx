import type { Meta, StoryObj } from '@storybook/react';
import { Users } from './Users';
import { MemoryRouter } from 'react-router';
import { QueryClient, QueryClientProvider } from 'react-query';

const queryClient = new QueryClient();

const mockUsers = [
  { id: 1, username: 'jdoe', roles: [2], createdAt: '2026-01-05T00:00:00Z' },
  { id: 2, username: 'asmith', roles: [4], createdAt: '2026-01-06T00:00:00Z' },
  { id: 3, username: 'admin', roles: [1], createdAt: '2026-01-01T00:00:00Z' },
] as any[];

const meta: Meta<typeof Users> = {
  title: 'Users/Users',
  component: Users,
  decorators: [
    (Story) => (
      <QueryClientProvider client={queryClient}>
        <MemoryRouter>
          <Story />
        </MemoryRouter>
      </QueryClientProvider>
    ),
  ],
};

export default meta;
type Story = StoryObj<typeof Users>;

export const Default: Story = {
  args: {
    searchUsers: async () => mockUsers,
    createUser: async (body) => ({ id: 99, username: body.username, roles: body.roles, createdAt: new Date().toISOString() }),
    updateUser: async (id, body) => ({ id, username: body.username ?? '', roles: [2], createdAt: new Date().toISOString() }),
  },
};

export const Empty: Story = {
  args: {
    searchUsers: async () => [],
    createUser: async () => ({ id: 99, username: 'new', roles: [2], createdAt: new Date().toISOString() }),
    updateUser: async (id, body) => ({ id, username: body.username ?? '', roles: [2], createdAt: new Date().toISOString() }),
  },
};
