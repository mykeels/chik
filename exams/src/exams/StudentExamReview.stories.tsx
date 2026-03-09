import type { Meta, StoryObj } from '@storybook/react';
import { StudentExamReview } from './StudentExamReview';
import { MemoryRouter, Route, Routes } from 'react-router';
import { QueryClient, QueryClientProvider } from 'react-query';

const queryClient = new QueryClient();

const mockExam = {
  id: 1,
  userId: 4,
  quizId: 1,
  creatorId: 2,
  isStarted: true,
  isEnded: true,
  isMarked: true,
  score: 80,
  examinerComment: 'Good work overall, keep it up!',
  startedAt: '2026-03-05T10:00:00Z',
  endedAt: '2026-03-05T10:28:00Z',
  createdAt: '2026-03-04T00:00:00Z',
  quiz: { id: 1, title: 'Math Quiz 1', description: null, creatorId: 2, createdAt: '2026-03-01T00:00:00Z' },
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
    answer: 'Gravity pulls objects toward each other.',
    autoScore: null,
    examinerScore: 8,
    examinerComment: 'Well explained, could add more detail.',
    finalScore: 8,
    question: { id: 2, quizId: 1, prompt: 'Explain gravity.', typeId: 6, score: 10, order: 2, isActive: true, createdAt: '2026-03-01T00:00:00Z' },
  },
  {
    id: 3,
    examId: 1,
    questionId: 3,
    answer: '4, 7',
    autoScore: 0,
    examinerScore: null,
    examinerComment: null,
    finalScore: 0,
    question: { id: 3, quizId: 1, prompt: 'Select all prime numbers.', typeId: 2, score: 5, order: 3, isActive: true, createdAt: '2026-03-01T00:00:00Z' },
  },
] as any[];

const mockScores = {
  examId: 1,
  totalScore: 13,
  maxPossibleScore: 20,
  answeredQuestions: 3,
  totalQuestions: 3,
  answerScores: [
    { questionId: 1, autoScore: 5, examinerScore: null, finalScore: 5, maxScore: 5 },
    { questionId: 2, autoScore: null, examinerScore: 8, finalScore: 8, maxScore: 10 },
    { questionId: 3, autoScore: 0, examinerScore: null, finalScore: 0, maxScore: 5 },
  ],
};

const meta: Meta<typeof StudentExamReview> = {
  title: 'Exams/StudentExamReview',
  component: StudentExamReview,
  decorators: [
    (Story) => (
      <QueryClientProvider client={queryClient}>
        <MemoryRouter initialEntries={['/exams/1/review']}>
          <Routes>
            <Route path="/exams/:id/review" element={<Story />} />
            <Route path="/my-exams" element={<div>My Exams</div>} />
          </Routes>
        </MemoryRouter>
      </QueryClientProvider>
    ),
  ],
};

export default meta;
type Story = StoryObj<typeof StudentExamReview>;

export const Default: Story = {
  args: {
    getExam: async () => mockExam,
    getExamAnswers: async () => mockAnswers,
    getExamScores: async () => mockScores,
  },
};
