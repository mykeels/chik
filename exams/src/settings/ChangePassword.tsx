import { useForm } from 'react-hook-form';
import { useMutation } from 'react-query';
import { toast } from 'react-toastify';
import { ioc } from '@/utils/ioc';
import * as chikexamsService from '@/services/chikexams.service';
import { Button, TextField } from '@mui/material';

type ChangePasswordFormData = {
  currentPassword: string;
  newPassword: string;
  confirmPassword: string;
};

export const ChangePassword = ({
  changePassword = ioc((keys) => keys.changePassword) || chikexamsService.changePassword,
}: {
  changePassword?: typeof chikexamsService.changePassword;
}) => {
  const { register, handleSubmit, reset, watch, formState: { errors } } = useForm<ChangePasswordFormData>();
  const newPassword = watch('newPassword');

  const mutation = useMutation({
    mutationFn: async (data: ChangePasswordFormData) => {
      return await changePassword(data.currentPassword, data.newPassword);
    },
    onSuccess: () => {
      toast.success('Password changed successfully!');
      reset();
    },
    onError: () => {
      toast.error('Failed to change password. Please check your current password.');
    },
  });

  const onSubmit = (data: ChangePasswordFormData) => {
    mutation.mutate(data);
  };

  return (
    <div className="max-w-md">
      <h1 className="text-2xl font-bold mb-6" style={{ color: '#211A1E' }}>Change Password</h1>

      <div className="bg-white rounded-xl shadow-sm p-6" style={{ border: '1px solid #E5E7EB' }}>
        <form onSubmit={handleSubmit(onSubmit)} className="flex flex-col gap-4">
          <TextField
            label="Current Password"
            type="password"
            fullWidth
            size="small"
            {...register('currentPassword', { required: 'Current password is required' })}
            error={!!errors.currentPassword}
            helperText={errors.currentPassword?.message}
          />

          <TextField
            label="New Password"
            type="password"
            fullWidth
            size="small"
            {...register('newPassword', {
              required: 'New password is required',
              minLength: { value: 8, message: 'Password must be at least 8 characters' },
            })}
            error={!!errors.newPassword}
            helperText={errors.newPassword?.message}
          />

          <TextField
            label="Confirm New Password"
            type="password"
            fullWidth
            size="small"
            {...register('confirmPassword', {
              required: 'Please confirm your new password',
              validate: (value) => value === newPassword || 'Passwords do not match',
            })}
            error={!!errors.confirmPassword}
            helperText={errors.confirmPassword?.message}
          />

          <div className="flex justify-end">
            <Button
              type="submit"
              variant="contained"
              disabled={mutation.isLoading}
              sx={{ backgroundColor: '#314CB6', '&:hover': { backgroundColor: '#2a3f9e' } }}
            >
              {mutation.isLoading ? 'Saving...' : 'Save Password'}
            </Button>
          </div>
        </form>
      </div>
    </div>
  );
};
