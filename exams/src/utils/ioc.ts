// eslint-disable-next-line @typescript-eslint/no-explicit-any
type IocValue = (...args: any[]) => any;

const IocKeys = {
  getMe: 'getMe',
  logout: 'logout',
  login: 'login',
  searchQuizzes: 'searchQuizzes',
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
