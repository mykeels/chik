import type { Meta, StoryObj } from '@storybook/react';
import { expect, userEvent, within } from '@storybook/test';
import { Quizzes } from './Quizzes';
import { MemoryRouter } from 'react-router';
import { QueryClient, QueryClientProvider } from 'react-query';

const makeQueryClient = () => new QueryClient({ defaultOptions: { queries: { retry: false } } });

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
      <QueryClientProvider client={makeQueryClient()}>
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
    exportQuiz: async () => {},
    importQuiz: async () => mockQuizzes[0],
  },
};

export const Empty: Story = {
  args: {
    searchQuizzes: async () => [],
    deleteQuiz: async () => {},
    exportQuiz: async () => {},
    importQuiz: async () => mockQuizzes[0],
  },
};

export const ImportDialogOpen: Story = {
  args: {
    ...Default.args,
  },
  play: async ({ canvasElement }) => {
    const canvas = within(canvasElement);
    const importBtn = await canvas.findByRole('button', { name: /import/i });
    await userEvent.click(importBtn);
    await expect(canvas.getByText(/index\.yaml/i)).toBeInTheDocument();
  },
};

export const DeleteDialogOpen: Story = {
  args: {
    ...Default.args,
  },
  play: async ({ canvasElement }) => {
    const canvas = within(canvasElement);
    const deleteButtons = await canvas.findAllByRole('button', { name: /delete/i });
    await userEvent.click(deleteButtons[0]);
    await expect(canvas.getByText(/cannot be undone/i)).toBeInTheDocument();
  },
};
