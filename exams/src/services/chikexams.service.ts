import { getAppVersion } from '@/utils/version';
import { createApiClient, types, enums } from 'chikexams-client';

type ChikExamsApi = ReturnType<typeof createApiClient>;
type QueryParamsOf<T> = T extends (params?: infer P) => any ? P extends { readonly queries?: infer Q } ? Q : never : never;  
export type PropsOf<T> = T extends (params?: infer P) => any ? P : never;

export const apiRootUrl = `${import.meta.env.VITE_API_ROOT_URL || 'http://localhost:5000'}`;

const api: ChikExamsApi = createApiClient(apiRootUrl, 'web-app');

api.axios.defaults.withCredentials = true;
api.axios.defaults.headers['X-App-Version'] = getAppVersion();

// Re-export types and enums for convenience
export type { types };
export { enums };

// Helper to convert null values to undefined for API compatibility
// eslint-disable-next-line @typescript-eslint/no-explicit-any
const nullToUndefined = <T extends Record<string, any>>(obj: T): T => {
  return Object.fromEntries(
    Object.entries(obj).map(([key, value]) => [key, value === null ? undefined : value])
  ) as T;
};

export const getMe = async () => {
  return await api.Auth_GetCurrentUser();
}

export const logout = async () => {
  await api.Auth_Logout(undefined);
  login()
}

export const login = async () => {
  location.replace(`${apiRootUrl}/api/auth/login`);
}
export const searchQuizzes = async (queries?: QueryParamsOf<typeof api.Quizzes_Search>) => {
  return await api.Quizzes_Search({
    queries,
  });
}