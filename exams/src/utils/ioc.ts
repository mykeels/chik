// eslint-disable-next-line @typescript-eslint/no-explicit-any
type IocValue = (...args: any[]) => any;

const IocKeys = {
  getMe: 'getMe',
  logout: 'logout',
  login: 'login',
  loginWithCredentials: 'loginWithCredentials',
  changePassword: 'changePassword',
  searchQuizzes: 'searchQuizzes',
  createQuiz: 'createQuiz',
  getQuiz: 'getQuiz',
  updateQuiz: 'updateQuiz',
  deleteQuiz: 'deleteQuiz',
  getQuizQuestions: 'getQuizQuestions',
  createQuizQuestion: 'createQuizQuestion',
  updateQuizQuestion: 'updateQuizQuestion',
  deactivateQuestion: 'deactivateQuestion',
  reactivateQuestion: 'reactivateQuestion',
  reorderQuestions: 'reorderQuestions',
  searchUsers: 'searchUsers',
  createUser: 'createUser',
  updateUser: 'updateUser',
  deleteUser: 'deleteUser',
  searchAuditLogs: 'searchAuditLogs',
  searchExams: 'searchExams',
  createExam: 'createExam',
  getExam: 'getExam',
  startExam: 'startExam',
  submitExam: 'submitExam',
  cancelExam: 'cancelExam',
  submitAnswer: 'submitAnswer',
  getExamAnswers: 'getExamAnswers',
  getExamScores: 'getExamScores',
  getPendingExams: 'getPendingExams',
  getExamHistory: 'getExamHistory',
  updateExam: 'updateExam',
  scoreAnswer: 'scoreAnswer',
} as const;
const IocValues = {} as Record<keyof typeof IocKeys, IocValue>;

/**
 * Simple IOC container for the application
 *
 * Useful for injecting dependencies into components
 *
 * Usage:
 *
 * ```tsx
 * const Foo = ({
 *   getItems = ioc(keys => keys.getFoo) || (() => Promise.resolve([])),
 * }: FooProps) => {
 *   return <div>Foo</div>;
 * }
 * ```
 *
 * Feel free to add more keys as needed
 */
export const ioc = (getKey: (keys: typeof IocKeys) => keyof typeof IocKeys): IocValue | undefined => {
  const key = getKey(IocKeys);
  return IocValues[key as keyof typeof IocKeys];
};

const add = (getKey: (keys: typeof IocKeys) => keyof typeof IocKeys, value: IocValue) => {
  const key = getKey(IocKeys);
  IocValues[key as keyof typeof IocKeys] = value;
  return ioc;
};

ioc.add = add;
ioc.keys = IocKeys;
