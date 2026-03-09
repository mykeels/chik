import type { Meta, StoryObj } from '@storybook/react';
import { Login } from './Login';
import { MemoryRouter } from 'react-router';
import { QueryClient, QueryClientProvider } from 'react-query';

const queryClient = new QueryClient();

const meta: Meta<typeof Login> = {
  title: 'Auth/Login',
  component: Login,
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
type Story = StoryObj<typeof Login>;

export const Default: Story = {
  args: {
    loginWithCredentials: async () => {
      await new Promise((r) => setTimeout(r, 500));
      return { id: 1, username: 'admin', roles: [1], message: 'ok' } as any;
    },
  },
};

export const WithError: Story = {
  args: {
    loginWithCredentials: async () => {
      await new Promise((r) => setTimeout(r, 500));
      throw new Error('Invalid credentials');
    },
  },
};
