import type { Meta, StoryObj } from '@storybook/react';
import { MyExams } from './MyExams';
import { MemoryRouter } from 'react-router';
import { QueryClient, QueryClientProvider } from 'react-query';

const queryClient = new QueryClient();

const mockPendingExams = [
  {
    id: 1,
    userId: 4,
    quizId: 1,
    creatorId: 2,
    isStarted: false,
    isEnded: false,
    isMarked: false,
    score: null,
    startedAt: null,
    endedAt: null,
    createdAt: '2026-03-04T00:00:00Z',
    quiz: { id: 1, title: 'Math Quiz 1', description: null, creatorId: 2, createdAt: '2026-03-01T00:00:00Z' },
    creator: { id: 2, username: 'Mr. Smith', roles: [2], createdAt: '2026-01-01T00:00:00Z' },
  },
] as any[];

const mockHistory = [
  {
    id: 2,
    userId: 4,
    quizId: 2,
    creatorId: 2,
    isStarted: true,
    isEnded: true,
    isMarked: true,
    score: 80,
    startedAt: '2026-03-01T10:00:00Z',
    endedAt: '2026-03-01T10:30:00Z',
    createdAt: '2026-03-01T00:00:00Z',
    quiz: { id: 2, title: 'History Quiz', description: null, creatorId: 2, createdAt: '2026-02-20T00:00:00Z' },
  },
] as any[];

const meta: Meta<typeof MyExams> = {
  title: 'Exams/MyExams',
  component: MyExams,
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
type Story = StoryObj<typeof MyExams>;

export const Default: Story = {
  args: {
    getPendingExams: async () => mockPendingExams,
    getExamHistory: async () => mockHistory,
    startExam: async (id) => ({ ...mockPendingExams[0], id, isStarted: true } as any),
  },
};
