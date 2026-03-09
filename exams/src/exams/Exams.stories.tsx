import type { Meta, StoryObj } from '@storybook/react';
import { Exams } from './Exams';
import { MemoryRouter } from 'react-router';
import { QueryClient, QueryClientProvider } from 'react-query';

const queryClient = new QueryClient();

const mockExams = [
  {
    id: 1,
    userId: 2,
    quizId: 1,
    creatorId: 1,
    isStarted: true,
    isEnded: true,
    isMarked: false,
    score: null,
    startedAt: '2026-03-05T10:00:00Z',
    endedAt: '2026-03-05T10:28:00Z',
    createdAt: '2026-03-04T00:00:00Z',
    user: { id: 2, username: 'asmith', roles: [4], createdAt: '2026-01-06T00:00:00Z' },
    quiz: { id: 1, title: 'Math Quiz 1', description: null, creatorId: 1, createdAt: '2026-03-01T00:00:00Z' },
  },
  {
    id: 2,
    userId: 3,
    quizId: 2,
    creatorId: 1,
    isStarted: false,
    isEnded: false,
    isMarked: false,
    score: null,
    startedAt: null,
    endedAt: null,
    createdAt: '2026-03-05T00:00:00Z',
    user: { id: 3, username: 'bjones', roles: [4], createdAt: '2026-01-07T00:00:00Z' },
    quiz: { id: 2, title: 'History Quiz', description: null, creatorId: 1, createdAt: '2026-03-02T00:00:00Z' },
  },
] as any[];

const meta: Meta<typeof Exams> = {
  title: 'Exams/Exams',
  component: Exams,
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
type Story = StoryObj<typeof Exams>;

export const Default: Story = {
  args: {
    searchExams: async () => mockExams,
    createExam: async (userId, quizId) => ({ id: 99, userId, quizId, creatorId: 1, isStarted: false, isEnded: false, isMarked: false, createdAt: new Date().toISOString() } as any),
    cancelExam: async () => {},
    searchUsers: async () => [{ id: 2, username: 'asmith', roles: [4], createdAt: '2026-01-06T00:00:00Z' }] as any,
    searchQuizzes: async () => [{ id: 1, title: 'Math Quiz 1', description: null, creatorId: 1, createdAt: '2026-03-01T00:00:00Z' }] as any,
  },
};
