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

export const loginWithCredentials = async (username: string, password: string) => {
  return await api.Auth_Login({ username, password });
}

export const changePassword = async (currentPassword: string, newPassword: string) => {
  return await api.Auth_ChangePassword({ currentPassword, newPassword });
}

export const searchUsers = async (queries?: QueryParamsOf<typeof api.Users_Search>) => {
  return await api.Users_Search({ queries });
}

export const createUser = async (body: { username: string; password: string; roles: enums.UserRole_Values[]; classId?: number | null; classIds?: number[] | null }) => {
  return await api.Users_Create(body);
}

export const updateUser = async (id: number, body: { username?: string; password?: string; roles?: enums.UserRole_Values[]; classId?: number | null; classIds?: number[] | null }) => {
  return await api.Users_Update(body, { params: { id } });
}

export const listClasses = async () => {
  return await api.Classes_List();
}

export const assignExamToClass = async (classId: number, quizId: number) => {
  return await api.Exams_AssignToClass({ classId, quizId });
}

export const deleteUser = async (id: number) => {
  return await api.Users_Delete(undefined, { params: { id } });
}

export const searchAuditLogs = async (queries?: QueryParamsOf<typeof api.AuditLogs_Search>) => {
  return await api.AuditLogs_Search({ queries });
}

export const searchQuizzes = async (queries?: QueryParamsOf<typeof api.Quizzes_Search>) => {
  return await api.Quizzes_Search({
    queries,
  });
}

export const createQuiz = async (body: { title: string; description?: string; examinerId?: number; duration?: string }) => {
  return await api.Quizzes_Create(body);
}

export const getQuiz = async (id: number) => {
  return await api.Quizzes_Get({ params: { id } });
}

export const updateQuiz = async (id: number, body: { title?: string; description?: string; examinerId?: number; duration?: string }) => {
  return await api.Quizzes_Update(body, { params: { id } });
}

export const deleteQuiz = async (id: number) => {
  return await api.Quizzes_Delete(undefined, { params: { id } });
}

export const getQuizQuestions = async (quizId: number) => {
  return await api.Quizzes_GetQuestions({ params: { quizId } });
}

export const createQuizQuestion = async (
  quizId: number,
  body: { prompt: string; typeId: number; properties?: string; score: number; order: number }
) => {
  return await api.Quizzes_CreateQuestion(body, { params: { quizId } });
}

export const updateQuizQuestion = async (
  id: number,
  body: { prompt?: string; typeId?: number; properties?: string; score?: number; order?: number }
) => {
  return await api.QuizQuestions_Update(body, { params: { id } });
}

export const deactivateQuestion = async (id: number) => {
  return await api.QuizQuestions_Deactivate(undefined, { params: { id } });
}

export const reactivateQuestion = async (id: number) => {
  return await api.QuizQuestions_Reactivate(undefined, { params: { id } });
}

export const reorderQuestions = async (quizId: number, questionIdsInOrder: number[]) => {
  return await api.Quizzes_ReorderQuestions({ questionIdsInOrder }, { params: { quizId } });
}

export const searchExams = async (queries?: QueryParamsOf<typeof api.Exams_Search>) => {
  return await api.Exams_Search({ queries });
}

export const createExam = async (userId: number, quizId: number) => {
  return await api.Exams_Create({ userId, quizId });
}

export const getExam = async (id: number) => {
  return await api.Exams_Get({ params: { id } });
}

export const startExam = async (id: number) => {
  return await api.Exams_Start(undefined, { params: { id } });
}

export const submitExam = async (id: number) => {
  return await api.Exams_Submit(undefined, { params: { id } });
}

export const cancelExam = async (id: number) => {
  return await api.Exams_Cancel(undefined, { params: { id } });
}

export const submitAnswer = async (examId: number, questionId: number, answer: string | null) => {
  return await api.Exams_SubmitAnswer({ questionId, answer: answer ?? undefined }, { params: { examId } });
}

export const getExamAnswers = async (examId: number) => {
  return await api.Exams_GetAnswers({ params: { examId } });
}

export const getExamScores = async (examId: number) => {
  return await api.Exams_GetScores({ params: { id: examId } });
}

export const getPendingExams = async () => {
  return await api.Exams_GetPendingExams();
}

export const getExamHistory = async () => {
  return await api.Exams_GetExamHistory();
}

export const updateExam = async (
  id: number,
  body: { startedAt?: string; endedAt?: string; score?: number; examinerId?: number; examinerComment?: string }
) => {
  return await api.Exams_Update(body, { params: { id } });
}

export const scoreAnswer = async (answerId: number, score: number, comment?: string) => {
  return await api.ExamAnswers_ExaminerScore({ score, comment: comment ?? undefined }, { params: { id: answerId } });
}

export const exportQuiz = async (id: number): Promise<void> => {
  const response = await api.axios.get(`${apiRootUrl}/api/quizzes/${id}/export`, {
    responseType: 'blob',
  });
  const disposition: string = response.headers['content-disposition'] ?? '';
  const filename = disposition.match(/filename="?([^";\n]+)"?/i)?.[1] ?? `quiz-${id}.zip`;
  const url = URL.createObjectURL(new Blob([response.data], { type: 'application/zip' }));
  const link = document.createElement('a');
  link.href = url;
  link.download = filename;
  document.body.appendChild(link);
  link.click();
  document.body.removeChild(link);
  URL.revokeObjectURL(url);
}

export const importQuiz = async (file: File, examinerId?: number) => {
  const formData = new FormData();
  formData.append('file', file, file.name);
  if (examinerId !== undefined) formData.append('examinerId', String(examinerId));
  const response = await api.axios.post<types.Quiz>(`${apiRootUrl}/api/quizzes/import`, formData);
  return response.data;
}
