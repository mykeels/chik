import type { Meta, StoryObj } from '@storybook/react';
import { TakeExam } from './TakeExam';
import { MemoryRouter, Route, Routes } from 'react-router';
import { QueryClient, QueryClientProvider } from 'react-query';

const queryClient = new QueryClient();

const mockExam = {
  id: 1,
  userId: 4,
  quizId: 1,
  creatorId: 2,
  isStarted: true,
  isEnded: false,
  isMarked: false,
  score: null,
  startedAt: new Date(Date.now() - 5 * 60 * 1000).toISOString(),
  endedAt: null,
  createdAt: '2026-03-04T00:00:00Z',
  quiz: { id: 1, title: 'Math Quiz 1', description: null, creatorId: 2, duration: '00:30:00', createdAt: '2026-03-01T00:00:00Z' },
} as any;

const mockQuestions = [
  {
    id: 1,
    quizId: 1,
    prompt: 'What is 2+2?',
    typeId: 1,
    score: 5,
    order: 1,
    isActive: true,
    createdAt: '2026-03-01T00:00:00Z',
    properties: JSON.stringify({ options: [{ text: '3', isCorrect: false }, { text: '4', isCorrect: true }, { text: '5', isCorrect: false }] }),
  },
  {
    id: 2,
    quizId: 1,
    prompt: 'Is the Earth flat?',
    typeId: 3,
    score: 5,
    order: 2,
    isActive: true,
    createdAt: '2026-03-01T00:00:00Z',
    properties: JSON.stringify({ options: [{ text: 'True', isCorrect: false }, { text: 'False', isCorrect: true }] }),
  },
  {
    id: 3,
    quizId: 1,
    prompt: 'Explain the theory of relativity.',
    typeId: 6,
    score: 10,
    order: 3,
    isActive: true,
    createdAt: '2026-03-01T00:00:00Z',
    properties: null,
  },
] as any[];

const meta: Meta<typeof TakeExam> = {
  title: 'Exams/TakeExam',
  component: TakeExam,
  decorators: [
    (Story) => (
      <QueryClientProvider client={queryClient}>
        <MemoryRouter initialEntries={['/exams/1/take']}>
          <Routes>
            <Route path="/exams/:id/take" element={<Story />} />
            <Route path="/exams/:id/review" element={<div>Review page</div>} />
          </Routes>
        </MemoryRouter>
      </QueryClientProvider>
    ),
  ],
};

export default meta;
type Story = StoryObj<typeof TakeExam>;

export const Default: Story = {
  args: {
    getExam: async () => mockExam,
    getQuizQuestions: async () => mockQuestions,
    getExamAnswers: async () => [],
    submitAnswer: async (examId, questionId, answer) => ({
      id: 1, examId, questionId, answer, autoScore: null, examinerScore: null, examinerComment: null, finalScore: null,
    } as any),
    submitExam: async () => ({ ...mockExam, isEnded: true, endedAt: new Date().toISOString() }),
  },
};
