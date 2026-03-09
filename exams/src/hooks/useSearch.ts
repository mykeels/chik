import { URLSearchParamsInit, useSearchParams } from 'react-router';
import { z } from 'zod';

export function useSearch<T extends z.AnyZodObject>(schema: T, defaultInit?: URLSearchParamsInit) {
  const [searchParams] = useSearchParams(defaultInit);
  const search = schema.parse(Object.fromEntries(searchParams));
  return search as z.infer<T>;
}
