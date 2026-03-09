import { ioc } from '@/utils/ioc';
import { useEffect } from 'react';
import { toast } from 'react-toastify';
import * as chikexamsService from '@/services/chikexams.service';
import { useCacheUpdate } from '@/hooks/useCacheUpdate';
import { CacheKeys } from '@/utils/cache-keys.utils';
import { useMutation, useQuery } from 'react-query';
import { EIGHT_HOURS } from '@/services/chikexams.hooks';

export const AuthProvider = ({
  children,
  login = ioc((keys) => keys.login) || (async () => chikexamsService.login()),
}: {
  children: React.ReactNode;
  login?: typeof chikexamsService.login;
}) => {
  const { isAuthenticated, isAuthenticationChallenge, isError, isLoading } = useAuth();

  useEffect(() => {
    if (isError && !location.search.includes('noServerDownWarning')) {
      toast.warn('Could not reach the server. You can keep playing while we fix this!');
    }
  }, [isError]);

  const shouldRedirectToLogin =
    !isLoading && (!isAuthenticated || isAuthenticationChallenge || isError);

  useEffect(() => {
    if (shouldRedirectToLogin) {
      login();
    }
  }, [shouldRedirectToLogin]);

  console.log({
    shouldRedirectToLogin,
    isLoading,
    isAuthenticated,
    isAuthenticationChallenge,
    isError,
  });

  return <>{isAuthenticated && !isAuthenticationChallenge && !isLoading ? <>{children}</> : null}</>;
};

type UseAuthProps = {
  getMe?: typeof chikexamsService.getMe;
  logout?: typeof chikexamsService.logout;
};

export const useAuth = ({
  getMe = ioc((keys) => keys.getMe) || (async () => chikexamsService.getMe()),
  logout = ioc((keys) => keys.logout) || (async () => chikexamsService.logout()),
}: UseAuthProps = {}) => {
  const meCache = useCacheUpdate(CacheKeys.me);
  const { data: profile, ...meQuery } = useQuery({
    queryKey: CacheKeys.me,
    queryFn: () => {
      return getMe();
    },
    cacheTime: EIGHT_HOURS,
    staleTime: EIGHT_HOURS,
    retry: false,
    enabled: !useAuth.hasAuthError,
    onError: () => {
      useAuth.hasAuthError = true;
    },
  });
  const { isLoading: isLoggingOut, ...logoutMutation } = useMutation({
    mutationFn: async () => {
      await logout();
      meCache.invalidate();
      location.reload();
    },
  });

  const isAuthenticated = meQuery.isSuccess && !!profile;
  const isAuthenticationChallenge = meQuery.isError && meQuery.error?.toString()?.includes('401');
  const isError = meQuery.isError && !isAuthenticationChallenge;

  return {
    isAuthenticated:  isAuthenticated,
    isAuthenticationChallenge,
    isError,
    isLoading: meQuery.isLoading || isLoggingOut,
    profile: profile,
    logout: logoutMutation.mutate,
    isLoggingOut,
  };
};

useAuth.hasAuthError = false;
