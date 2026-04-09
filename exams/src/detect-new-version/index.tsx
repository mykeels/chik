import { useCallback, useEffect, useRef, useState } from 'react';
import {
  Button,
  Dialog,
  DialogActions,
  DialogContent,
  DialogTitle,
  Typography,
} from '@mui/material';
import { fetchLatestVersion, isNewVersionAvailable } from '@/utils/fetch-latest-version';
import { useWindowFocus } from '@/hooks/useWindowFocus';
import { useMutation } from 'react-query';

export const DetectNewVersion = ({
  hasNewVersion = async () => {
    return isNewVersionAvailable();
  },
  getLatestVersion = async () => {
    await fetchLatestVersion();
  },
}: {
  hasNewVersion?: () => Promise<boolean>;
  getLatestVersion?: () => Promise<void>;
}) => {
  const [open, setOpen] = useState(false);
  const intervalRef = useRef<NodeJS.Timeout | null>(null);
  const isCheckingRef = useRef(false);

  const checkForUpdate = useCallback(async () => {
    if (isCheckingRef.current) return;
    isCheckingRef.current = true;
    const hasNew = await hasNewVersion();
    if (hasNew) {
      setOpen(true);
    }
    isCheckingRef.current = false;
  }, [hasNewVersion]);

  // Handle window focus
  useWindowFocus({
    onFocus: () => {
      checkForUpdate();
      // Start interval when focused
      if (!intervalRef.current) {
        intervalRef.current = setInterval(() => {
          checkForUpdate();
        }, 60_000);
      }
    },
    onBlur: () => {
      // Stop interval when blurred
      if (intervalRef.current) {
        clearInterval(intervalRef.current);
        intervalRef.current = null;
      }
    },
  });

  // Clean up interval on unmount
  useEffect(() => {
    checkForUpdate();
    return () => {
      if (intervalRef.current) {
        clearInterval(intervalRef.current);
      }
    };
  }, [checkForUpdate]);

  const updateMutation = useMutation({
    mutationFn: async () => {
      await getLatestVersion();
      window.location.reload();
    },
  });

  return (
    <Dialog open={open} onClose={() => setOpen(false)} maxWidth="sm" fullWidth>
      <DialogTitle>New Version Available</DialogTitle>
      <DialogContent>
        <Typography variant="body2" color="text.secondary">
          A new version of this app is available. Please update to get the latest features and fixes.
        </Typography>
      </DialogContent>
      <DialogActions sx={{ px: 3, pb: 2 }}>
        <Button
          variant="contained"
          onClick={() => updateMutation.mutate()}
          disabled={updateMutation.isLoading}
        >
          {updateMutation.isLoading ? 'Please wait...' : 'Update Now'}
        </Button>
      </DialogActions>
    </Dialog>
  );
};
