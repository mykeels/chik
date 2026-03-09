import type { Meta, StoryObj } from '@storybook/react';
import { QuizEditor } from './QuizEditor';
import { MemoryRouter, Route, Routes } from 'react-router';
import { QueryClient, QueryClientProvider } from 'react-query';
import { ioc } from '@/utils/ioc';

const queryClient = new QueryClient();

ioc.add((keys) => keys.getMe, async () => ({
  id: 1,
  username: 'admin',
  roles: [1],
  createdAt: '2026-01-01T00:00:00Z',
}));

const mockQuiz = {
  id: 1,
  title: 'Math Quiz 1',
  description: 'Basic arithmetic',
  creatorId: 1,
  duration: '00:30:00',
  createdAt: '2026-03-01T00:00:00Z',
  questions: [],
} as any;

const mockQuestions = [
  { id: 1, quizId: 1, prompt: 'What is 2+2?', typeId: 1, score: 5, order: 1, isActive: true, createdAt: '2026-03-01T00:00:00Z' },
  { id: 2, quizId: 1, prompt: 'Explain gravity.', typeId: 6, score: 10, order: 2, isActive: true, createdAt: '2026-03-01T00:00:00Z' },
] as any[];

const meta: Meta<typeof QuizEditor> = {
  title: 'Quizzes/QuizEditor',
  component: QuizEditor,
  decorators: [
    (Story) => (
      <QueryClientProvider client={queryClient}>
        <MemoryRouter initialEntries={['/quizzes/1/edit']}>
          <Routes>
            <Route path="/quizzes/:id/edit" element={<Story />} />
            <Route path="/quizzes" element={<div>Quizzes list</div>} />
          </Routes>
        </MemoryRouter>
      </QueryClientProvider>
    ),
  ],
};

export default meta;
type Story = StoryObj<typeof QuizEditor>;

export const EditExisting: Story = {
  args: {
    createQuiz: async (body) => ({ ...mockQuiz, ...body }),
    updateQuiz: async (id, body) => ({ ...mockQuiz, ...body }),
    getQuizQuestions: async () => mockQuestions,
    createQuizQuestion: async (quizId, body) => ({ id: 99, quizId, ...body, order: 3, isActive: true, createdAt: new Date().toISOString() }),
    updateQuizQuestion: async (id, body) => ({ id, ...body, quizId: 1, order: 1, isActive: true, createdAt: new Date().toISOString() }),
    deactivateQuestion: async () => {},
    reactivateQuestion: async () => {},
    reorderQuestions: async () => {},
    searchUsers: async () => [],
  },
};
