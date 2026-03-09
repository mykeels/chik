import type { Meta, StoryObj } from '@storybook/react';
import { Quizzes } from './Quizzes';
import { MemoryRouter } from 'react-router';
import { QueryClient, QueryClientProvider } from 'react-query';

const queryClient = new QueryClient();

const mockQuizzes = [
  {
    id: 1,
    title: 'Math Quiz 1',
    description: 'Basic arithmetic',
    creatorId: 1,
    duration: '00:30:00',
    createdAt: '2026-03-01T00:00:00Z',
    questions: [{ id: 1 }, { id: 2 }, { id: 3 }],
  },
  {
    id: 2,
    title: 'History Quiz',
    description: 'World history',
    creatorId: 1,
    duration: null,
    createdAt: '2026-03-02T00:00:00Z',
    questions: [],
  },
] as any[];

const meta: Meta<typeof Quizzes> = {
  title: 'Quizzes/Quizzes',
  component: Quizzes,
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
type Story = StoryObj<typeof Quizzes>;

export const Default: Story = {
  args: {
    searchQuizzes: async () => mockQuizzes,
    deleteQuiz: async () => {},
  },
};

export const Empty: Story = {
  args: {
    searchQuizzes: async () => [],
    deleteQuiz: async () => {},
  },
};
