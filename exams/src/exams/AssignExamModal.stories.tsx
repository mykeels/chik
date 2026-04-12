import type { Meta } from '@storybook/react';
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

export const Index = () => {
  const [open, setOpen] = useState(true);
  return (
    <AssignExamModal
      open={open}
      onClose={() => setOpen(false)}
      onAssignToClass={(classId, quizId) => {
        console.log('Assigned to class:', { classId, quizId });
        setOpen(false);
      }}
      onAssignToStudents={(userIds, quizId) => {
        console.log('Assigned to students:', { userIds, quizId });
        setOpen(false);
      }}
      searchUsers={async () => [
        { id: 1, username: 'asmith', roles: [4], createdAt: '2026-01-01T00:00:00Z' },
      ]}
      searchQuizzes={async () => [
        { id: 1, title: 'Math Quiz 1', description: null, creatorId: 1, createdAt: '2026-03-01T00:00:00Z' },
      ]}
      listClasses={async () => [
        { id: 1, name: 'Class A', createdAt: '2026-01-01T00:00:00Z' },
      ]}
    />
  );
}