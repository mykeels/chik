import type { Meta, StoryObj } from '@storybook/react';
import { AuditLogs } from './AuditLogs';
import { MemoryRouter } from 'react-router';
import { QueryClient, QueryClientProvider } from 'react-query';

const queryClient = new QueryClient();

const mockLogs = [
  { id: 1, userId: 1, service: 'UserService', entityId: 12, properties: null, createdAt: '2026-03-08T10:00:00Z', user: { id: 1, username: 'admin', roles: [1], createdAt: '2026-01-01T00:00:00Z' } },
  { id: 2, userId: 2, service: 'QuizService', entityId: 5, properties: null, createdAt: '2026-03-07T09:00:00Z', user: { id: 2, username: 'jdoe', roles: [2], createdAt: '2026-01-05T00:00:00Z' } },
] as any[];

const meta: Meta<typeof AuditLogs> = {
  title: 'AuditLogs/AuditLogs',
  component: AuditLogs,
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
type Story = StoryObj<typeof AuditLogs>;

export const Default: Story = {
  args: {
    searchAuditLogs: async () => mockLogs,
  },
};

export const Empty: Story = {
  args: {
    searchAuditLogs: async () => [],
  },
};
