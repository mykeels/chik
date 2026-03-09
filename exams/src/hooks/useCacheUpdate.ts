import { useQueryClient } from 'react-query';

export const useCacheUpdate = <TData, TKey extends string = string>(key: TKey) => {
  const queryClient = useQueryClient();
  const cacheUpdate = {
    update: (transform: (data?: TData | undefined) => TData | undefined) => {
      queryClient.setQueryData<TData | undefined>(key, (data) => {
        return transform(data);
      });
    },
    invalidate: () => {
      queryClient.invalidateQueries(
        key,
        {
          exact: true,
          refetchActive: false,
          refetchInactive: false,
        },
        {
          cancelRefetch: true,
        }
      );
    },
    refetch: () => {
      queryClient.refetchQueries(key, { active: true, inactive: true });
    },
    invalidateAndRefetch: () => {
      cacheUpdate.invalidate();
      cacheUpdate.refetch();
    },
  };
  return cacheUpdate;
};
