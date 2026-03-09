import type { Meta, StoryObj } from '@storybook/react';
import { ChangePassword } from './ChangePassword';
import { MemoryRouter } from 'react-router';
import { QueryClient, QueryClientProvider } from 'react-query';

const queryClient = new QueryClient();

const meta: Meta<typeof ChangePassword> = {
  title: 'Settings/ChangePassword',
  component: ChangePassword,
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
type Story = StoryObj<typeof ChangePassword>;

export const Default: Story = {
  args: {
    changePassword: async () => {},
  },
};

export const WithError: Story = {
  args: {
    changePassword: async () => {
      throw new Error('Invalid current password');
    },
  },
};
