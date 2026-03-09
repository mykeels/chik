import { AxiosError } from 'axios';
import { ZodiosError } from '@zodios/core';
import { ZodError } from 'zod';

// eslint-disable-next-line @typescript-eslint/no-explicit-any
export const handleApiError = async (error: any) => {
  console.error(
    error?.cause
      ? {
          ...(error?.config ? error?.config : {}),
          cause: error?.cause,
        }
      : error
  );
  if (error instanceof ZodiosError || error.cause instanceof ZodError) {
    console.log({
      data: error.data,
      config: error.config,
      // eslint-disable-next-line @typescript-eslint/no-explicit-any
      status: (error as any).status,
      error,
    });
    return error.data as never;
  } else if (error instanceof AxiosError) {
    const axiosError = error as AxiosError;
    if (axiosError.response?.status === 401) {
      error.message += ' (401 Unauthorized)';
    }
  }
  return Promise.reject(error);
};
