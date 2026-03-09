import type { Meta, StoryObj } from '@storybook/react';
import { ExamReview } from './ExamReview';
import { MemoryRouter, Route, Routes } from 'react-router';
import { QueryClient, QueryClientProvider } from 'react-query';

const queryClient = new QueryClient();

const mockExam = {
  id: 1,
  userId: 2,
  quizId: 1,
  creatorId: 1,
  isStarted: true,
  isEnded: true,
  isMarked: false,
  score: null,
  examinerComment: '',
  startedAt: '2026-03-05T10:00:00Z',
  endedAt: '2026-03-05T10:28:00Z',
  createdAt: '2026-03-04T00:00:00Z',
  user: { id: 2, username: 'asmith', roles: [4], createdAt: '2026-01-06T00:00:00Z' },
  quiz: { id: 1, title: 'Math Quiz 1', description: null, creatorId: 1, createdAt: '2026-03-01T00:00:00Z' },
} as any;

const mockAnswers = [
  {
    id: 1,
    examId: 1,
    questionId: 1,
    answer: '4',
    autoScore: 5,
    examinerScore: null,
    examinerComment: null,
    finalScore: 5,
    question: { id: 1, quizId: 1, prompt: 'What is 2+2?', typeId: 1, score: 5, order: 1, isActive: true, createdAt: '2026-03-01T00:00:00Z' },
  },
  {
    id: 2,
    examId: 1,
    questionId: 2,
    answer: 'Gravity is a force...',
    autoScore: null,
    examinerScore: null,
    examinerComment: null,
    finalScore: null,
    question: { id: 2, quizId: 1, prompt: 'Explain gravity.', typeId: 6, score: 10, order: 2, isActive: true, createdAt: '2026-03-01T00:00:00Z' },
  },
] as any[];

const mockScores = {
  examId: 1,
  totalScore: 5,
  maxPossibleScore: 15,
  answeredQuestions: 2,
  totalQuestions: 2,
  answerScores: [
    { questionId: 1, autoScore: 5, examinerScore: null, finalScore: 5, maxScore: 5 },
    { questionId: 2, autoScore: null, examinerScore: null, finalScore: 0, maxScore: 10 },
  ],
};

const meta: Meta<typeof ExamReview> = {
  title: 'Exams/ExamReview',
  component: ExamReview,
  decorators: [
    (Story) => (
      <QueryClientProvider client={queryClient}>
        <MemoryRouter initialEntries={['/exams/1']}>
          <Routes>
            <Route path="/exams/:id" element={<Story />} />
            <Route path="/exams" element={<div>Exams list</div>} />
          </Routes>
        </MemoryRouter>
      </QueryClientProvider>
    ),
  ],
};

export default meta;
type Story = StoryObj<typeof ExamReview>;

export const Default: Story = {
  args: {
    getExam: async () => mockExam,
    getExamAnswers: async () => mockAnswers,
    getExamScores: async () => mockScores,
    updateExam: async () => mockExam,
    scoreAnswer: async () => mockAnswers[0],
  },
};
