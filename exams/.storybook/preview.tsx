import React from 'react';
import type { Preview } from '@storybook/react';
import { QueryClient, QueryClientProvider } from 'react-query';
import { ThemeProvider, createTheme, CssBaseline } from '@mui/material';
import '../src/index.css';

const theme = createTheme({
  palette: {
    primary: {
      main: '#314CB6',
      light: '#427AA1',
      dark: '#0A81D1',
    },
    secondary: {
      main: '#0A81D1',
      light: '#427AA1',
      dark: '#0A81D1',
    },
    background: {
      default: '#EBF2FA',
      paper: '#EBF2FA',
    },
  },
  typography: {
    fontFamily: '"Nunito", "Segoe UI", "San Francisco", "Helvetica Neue", "Arial", sans-serif',
  },
  shape: {
    borderRadius: 8,
  },
  components: {
    MuiButton: {
      styleOverrides: {
        root: {
          textTransform: 'none',
          fontWeight: 500,
        },
      },
    },
  },
});

const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      refetchOnWindowFocus: false,
      retry: false,
    },
  },
});

const preview: Preview = {
  parameters: {
    controls: {
      matchers: {
        color: /(background|color)$/i,
        date: /Date$/i,
      },
    },
    layout: 'padded',
  },
  decorators: [
    (Story) => (
      <QueryClientProvider client={queryClient}>
        <ThemeProvider theme={theme}>
          <style>{`
          body {
            padding: 0;
            margin: 0;
          }
          `}</style>
          <div className="mfe chik-exams">
            <CssBaseline />
            <Story />
          </div>
        </ThemeProvider>
      </QueryClientProvider>
    ),
  ],
};

export default preview;
