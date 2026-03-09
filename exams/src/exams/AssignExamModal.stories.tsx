import type { Meta, StoryObj } from '@storybook/react';
import { AssignExamModal } from './AssignExamModal';
import { MemoryRouter } from 'react-router';
import { QueryClient, QueryClientProvider } from 'react-query';
import { useState } from 'react';

const queryClient = new QueryClient();

const meta: Meta<typeof AssignExamModal> = {
  title: 'Exams/AssignExamModal',
  component: AssignExamModal,
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
type Story = StoryObj<typeof AssignExamModal>;

export const Default: Story = {
  render: () => {
    const [open, setOpen] = useState(true);
    return (
      <AssignExamModal
        open={open}
        onClose={() => setOpen(false)}
        onAssign={(userId, quizId) => {
          console.log('Assigned:', { userId, quizId });
          setOpen(false);
        }}
        searchUsers={async () => [
          { id: 1, username: 'asmith', roles: [4], createdAt: '2026-01-01T00:00:00Z' },
        ] as any}
        searchQuizzes={async () => [
          { id: 1, title: 'Math Quiz 1', description: null, creatorId: 1, createdAt: '2026-03-01T00:00:00Z' },
        ] as any}
      />
    );
  },
};
