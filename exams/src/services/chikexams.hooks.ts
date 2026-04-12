import * as chikexamsService from './chikexams.service';
import { useQuery } from 'react-query';
import { CacheKeys } from '@/utils/cache-keys.utils';
import { ioc } from '@/utils/ioc';

export const TWO_MINUTES = 2 * 60 * 1000;
export const ONE_HOUR = 60 * 60 * 1000;
export const EIGHT_HOURS = 8 * 60 * 60 * 1000;

// Search quizzes
export const useQuizzes = ({
  params,
  searchQuizzes = ioc((keys) => keys.searchQuizzes) || chikexamsService.searchQuizzes,
}: {
  params?: chikexamsService.PropsOf<typeof chikexamsService.searchQuizzes>;
  searchQuizzes?: typeof chikexamsService.searchQuizzes;
} = {}) => {
  return useQuery({
    queryKey: [CacheKeys.searchQuizzes, params],
    queryFn: async () => await searchQuizzes(params),
    staleTime: TWO_MINUTES,
    cacheTime: TWO_MINUTES,
  });
};

// Get single quiz
export const useQuiz = (
  id: number,
  {
    getQuiz = ioc((keys) => keys.getQuiz) || chikexamsService.getQuiz,
  }: {
    getQuiz?: typeof chikexamsService.getQuiz;
  } = {}
) => {
  return useQuery({
    queryKey: [CacheKeys.getQuiz, id],
    queryFn: async () => await getQuiz(id),
    staleTime: TWO_MINUTES,
    cacheTime: TWO_MINUTES,
    enabled: !!id,
  });
};

// Get quiz questions
export const useQuizQuestions = (
  quizId: number,
  {
    getQuizQuestions = ioc((keys) => keys.getQuizQuestions) || chikexamsService.getQuizQuestions,
  }: {
    getQuizQuestions?: typeof chikexamsService.getQuizQuestions;
  } = {}
) => {
  return useQuery({
    queryKey: [CacheKeys.getQuizQuestions, quizId],
    queryFn: async () => await getQuizQuestions(quizId),
    staleTime: TWO_MINUTES,
    cacheTime: TWO_MINUTES,
    enabled: !!quizId,
  });
};

// Search users
export const useUsers = ({
  params,
  searchUsers = ioc((keys) => keys.searchUsers) || chikexamsService.searchUsers,
}: {
  params?: chikexamsService.PropsOf<typeof chikexamsService.searchUsers>;
  searchUsers?: typeof chikexamsService.searchUsers;
} = {}) => {
  return useQuery({
    queryKey: [CacheKeys.searchUsers, params],
    queryFn: async () => await searchUsers(params),
    staleTime: TWO_MINUTES,
    cacheTime: TWO_MINUTES,
  });
};

// Search audit logs
export const useAuditLogs = ({
  params,
  searchAuditLogs = ioc((keys) => keys.searchAuditLogs) || chikexamsService.searchAuditLogs,
}: {
  params?: chikexamsService.PropsOf<typeof chikexamsService.searchAuditLogs>;
  searchAuditLogs?: typeof chikexamsService.searchAuditLogs;
} = {}) => {
  return useQuery({
    queryKey: [CacheKeys.searchAuditLogs, params],
    queryFn: async () => await searchAuditLogs(params),
    staleTime: TWO_MINUTES,
    cacheTime: TWO_MINUTES,
  });
};

// Search exams
export const useExams = ({
  params,
  searchExams = ioc((keys) => keys.searchExams) || chikexamsService.searchExams,
}: {
  params?: chikexamsService.PropsOf<typeof chikexamsService.searchExams>;
  searchExams?: typeof chikexamsService.searchExams;
} = {}) => {
  return useQuery({
    queryKey: [CacheKeys.searchExams, params],
    queryFn: async () => await searchExams(params),
    staleTime: TWO_MINUTES,
    cacheTime: TWO_MINUTES,
  });
};

// Get single exam
export const useExam = (
  id: number,
  {
    getExam = ioc((keys) => keys.getExam) || chikexamsService.getExam,
  }: {
    getExam?: typeof chikexamsService.getExam;
  } = {}
) => {
  return useQuery({
    queryKey: [CacheKeys.getExam, id],
    queryFn: async () => await getExam(id),
    staleTime: TWO_MINUTES,
    cacheTime: TWO_MINUTES,
    enabled: !!id,
  });
};

// Get exam answers
export const useExamAnswers = (
  examId: number,
  {
    getExamAnswers = ioc((keys) => keys.getExamAnswers) || chikexamsService.getExamAnswers,
  }: {
    getExamAnswers?: typeof chikexamsService.getExamAnswers;
  } = {}
) => {
  return useQuery({
    queryKey: [CacheKeys.getExamAnswers, examId],
    queryFn: async () => await getExamAnswers(examId),
    staleTime: TWO_MINUTES,
    cacheTime: TWO_MINUTES,
    enabled: !!examId,
  });
};

// Get exam scores
export const useExamScores = (
  examId: number,
  {
    getExamScores = ioc((keys) => keys.getExamScores) || chikexamsService.getExamScores,
  }: {
    getExamScores?: typeof chikexamsService.getExamScores;
  } = {}
) => {
  return useQuery({
    queryKey: [CacheKeys.getExamScores, examId],
    queryFn: async () => await getExamScores(examId),
    staleTime: TWO_MINUTES,
    cacheTime: TWO_MINUTES,
    enabled: !!examId,
  });
};

// Get pending exams (student)
export const usePendingExams = ({
  getPendingExams = ioc((keys) => keys.getPendingExams) || chikexamsService.getPendingExams,
}: {
  getPendingExams?: typeof chikexamsService.getPendingExams;
} = {}) => {
  return useQuery({
    queryKey: [CacheKeys.getPendingExams],
    queryFn: async () => await getPendingExams(),
    staleTime: TWO_MINUTES,
    cacheTime: TWO_MINUTES,
  });
};

// List classes
export const useClasses = ({
  listClasses = ioc((keys) => keys.listClasses) || chikexamsService.listClasses,
}: {
  listClasses?: typeof chikexamsService.listClasses;
} = {}) => {
  return useQuery({
    queryKey: [CacheKeys.listClasses],
    queryFn: async () => await listClasses(),
    staleTime: ONE_HOUR,
    cacheTime: ONE_HOUR,
  });
};

// Get exam history (student)
export const useExamHistory = ({
  getExamHistory = ioc((keys) => keys.getExamHistory) || chikexamsService.getExamHistory,
}: {
  getExamHistory?: typeof chikexamsService.getExamHistory;
} = {}) => {
  return useQuery({
    queryKey: [CacheKeys.getExamHistory],
    queryFn: async () => await getExamHistory(),
    staleTime: TWO_MINUTES,
    cacheTime: TWO_MINUTES,
  });
};
