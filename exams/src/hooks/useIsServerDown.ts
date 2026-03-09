import { ioc } from '@/utils/ioc';
import { useQuery } from 'react-query';

const defaultIsServerDown = async () => {
  if (localStorage.getItem('tg:isServerDown') === 'false') {
    return false;
  }
  return false;
};

export const useIsServerDown = ({ isServerDown = ioc((keys) => keys.isServerDown) || defaultIsServerDown } = {}) => {
  const isServerDownQuery = useQuery({
    queryKey: ['isServerDown'],
    queryFn: () => isServerDown(),
    staleTime: Infinity,
    cacheTime: Infinity,
    refetchOnMount: false,
    refetchOnWindowFocus: false,
    refetchOnReconnect: false,
  });
  return isServerDownQuery.isLoading || isServerDownQuery.isError ? true : isServerDownQuery.data;
};
