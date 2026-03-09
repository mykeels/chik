import './index.css';
import React, { StrictMode } from 'react';
import { QueryClient, QueryClientProvider } from 'react-query';
import { ToastContainer } from 'react-toastify';

const RenderMode = ({ children }: { children: React.ReactNode }) => {
  const isProduction = process.env.NODE_ENV === 'production';
  return isProduction ? <StrictMode>{children}</StrictMode> : children;
};

type AppContextsProps = {
  children: React.ReactNode;
};

const queryClient = new QueryClient();

export const AppContexts = ({ children }: AppContextsProps) => {
  return (
    <RenderMode>
      <QueryClientProvider client={queryClient}>
        <div className="mfe daily-bible-comics">{children}</div>
        <ToastContainer position="bottom-center" />
      </QueryClientProvider>
    </RenderMode>
  );
};
