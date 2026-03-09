import * as chikexamsService from './chikexams.service';
import { useQuery } from 'react-query';
import { CacheKeys } from '@/utils/cache-keys.utils';
import { ioc } from '@/utils/ioc';

export const TWO_MINUTES = 2 * 60 * 1000;
export const ONE_HOUR = 60 * 60 * 1000;
export const EIGHT_HOURS = 8 * 60 * 60 * 1000;

// Search comics
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
