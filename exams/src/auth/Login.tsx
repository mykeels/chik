import { useState } from 'react';
import { useNavigate } from 'react-router';
import { DocumentTitle } from '@/shell/DocumentTitle';
import { useForm } from 'react-hook-form';
import { ioc } from '@/utils/ioc';
import * as chikexamsService from '@/services/chikexams.service';
import { enums } from '@/services/chikexams.service';
import { BookOpen } from 'lucide-react';

type LoginForm = {
  username: string;
  password: string;
};

export const Login = ({
  loginWithCredentials = ioc((keys) => keys.loginWithCredentials) || chikexamsService.loginWithCredentials,
}: {
  loginWithCredentials?: typeof chikexamsService.loginWithCredentials;
}) => {
  const navigate = useNavigate();
  const [error, setError] = useState<string | null>(null);
  const [isLoading, setIsLoading] = useState(false);

  const { register, handleSubmit, formState: { errors } } = useForm<LoginForm>();

  const onSubmit = async (data: LoginForm) => {
    setError(null);
    setIsLoading(true);
    try {
      const result = await loginWithCredentials(data.username, data.password);
      const roles = result?.roles ?? [];
      if (roles.includes(enums.UserRole.Admin)) {
        navigate('/users');
      } else if (roles.includes(enums.UserRole.Teacher)) {
        navigate('/quizzes');
      } else if (roles.includes(enums.UserRole.Student)) {
        navigate('/my-exams');
      } else {
        navigate('/');
      }
    } catch {
      setError('Invalid username or password. Please try again.');
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <div className="min-h-screen flex items-center justify-center" style={{ backgroundColor: '#EBF2FA' }}>
      <DocumentTitle />
      <div className="bg-white rounded-2xl shadow-lg p-10 w-full max-w-sm">
        <div className="flex flex-col items-center mb-8">
          <div
            className="w-14 h-14 rounded-full flex items-center justify-center mb-3"
            style={{ backgroundColor: '#314CB6' }}
          >
            <BookOpen className="text-white" size={28} />
          </div>
          <h1 className="text-2xl font-bold" style={{ color: '#211A1E' }}>
            Chik.Exams
          </h1>
          <p className="text-sm mt-1" style={{ color: '#6B7280' }}>
            Sign in to your account
          </p>
        </div>

        <form onSubmit={handleSubmit(onSubmit)} className="flex flex-col gap-5">
          <div>
            <label className="block text-sm font-medium mb-1" style={{ color: '#211A1E' }}>
              Username
            </label>
            <input
              {...register('username', { required: 'Username is required' })}
              type="text"
              autoComplete="username"
              className="w-full border rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2"
              style={{
                borderColor: errors.username ? '#EF4444' : '#E5E7EB',
                color: '#211A1E',
              }}
              placeholder="Enter your username"
            />
            {errors.username && (
              <p className="text-xs mt-1" style={{ color: '#EF4444' }}>
                {errors.username.message}
              </p>
            )}
          </div>

          <div>
            <label className="block text-sm font-medium mb-1" style={{ color: '#211A1E' }}>
              Password
            </label>
            <input
              {...register('password', { required: 'Password is required' })}
              type="password"
              autoComplete="current-password"
              className="w-full border rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2"
              style={{
                borderColor: errors.password ? '#EF4444' : '#E5E7EB',
                color: '#211A1E',
              }}
              placeholder="Enter your password"
            />
            {errors.password && (
              <p className="text-xs mt-1" style={{ color: '#EF4444' }}>
                {errors.password.message}
              </p>
            )}
          </div>

          {error && (
            <div
              className="rounded-lg px-3 py-2 text-sm"
              style={{ backgroundColor: '#FEF2F2', color: '#EF4444', border: '1px solid #FECACA' }}
            >
              {error}
            </div>
          )}

          <button
            type="submit"
            disabled={isLoading}
            className="w-full py-2 rounded-lg text-white font-semibold text-sm transition-opacity"
            style={{ backgroundColor: '#314CB6', opacity: isLoading ? 0.7 : 1 }}
          >
            {isLoading ? 'Signing in...' : 'Log In'}
          </button>
        </form>
      </div>
    </div>
  );
};
