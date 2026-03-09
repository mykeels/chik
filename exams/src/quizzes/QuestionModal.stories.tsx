import type { Meta, StoryObj } from '@storybook/react';
import { QuestionModal } from './QuestionModal';
import { MemoryRouter } from 'react-router';
import { QueryClient, QueryClientProvider } from 'react-query';
import { useState } from 'react';

const queryClient = new QueryClient();

const meta: Meta<typeof QuestionModal> = {
  title: 'Quizzes/QuestionModal',
  component: QuestionModal,
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
type Story = StoryObj<typeof QuestionModal>;

export const NewQuestion: Story = {
  render: () => {
    const [open, setOpen] = useState(true);
    return (
      <QuestionModal
        open={open}
        onClose={() => setOpen(false)}
        onSave={(data) => {
          console.log('Saved:', data);
          setOpen(false);
        }}
        existingCount={2}
      />
    );
  },
};

export const EditQuestion: Story = {
  render: () => {
    const [open, setOpen] = useState(true);
    return (
      <QuestionModal
        open={open}
        onClose={() => setOpen(false)}
        onSave={(data) => {
          console.log('Saved:', data);
          setOpen(false);
        }}
        initialData={{
          id: 1,
          quizId: 1,
          prompt: 'What is 2 + 2?',
          typeId: 1,
          score: 5,
          order: 1,
          isActive: true,
          createdAt: '2026-01-01T00:00:00Z',
          properties: { type: null },
        }}
      />
    );
  },
};
