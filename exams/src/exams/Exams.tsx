import { useState } from 'react';
import { useNavigate } from 'react-router';
import { useMutation } from 'react-query';
import { useCacheUpdate } from '@/hooks/useCacheUpdate';
import { toast } from 'react-toastify';
import { ioc } from '@/utils/ioc';
import * as chikexamsService from '@/services/chikexams.service';
import { types } from '@/services/chikexams.service';
import { useExams } from '@/services/chikexams.hooks';
import { CacheKeys } from '@/utils/cache-keys.utils';
import { AssignExamModal } from './AssignExamModal';
import { Search, Plus } from 'lucide-react';
import {
  Button,
  CircularProgress,
  FormControl,
  InputLabel,
  Select,
  MenuItem,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
} from '@mui/material';

type ExamStatus = 'all' | 'pending' | 'in-progress' | 'submitted' | 'examined';

const getExamStatus = (exam: types['Exam']): ExamStatus => {
  if (!exam.isStarted) return 'pending';
  if (exam.isStarted && !exam.isEnded) return 'in-progress';
  if (exam.isEnded && !exam.isMarked) return 'submitted';
  if (exam.isMarked) return 'examined';
  return 'pending';
};

const StatusBadge = ({ status }: { status: ExamStatus }) => {
  const styles: Record<ExamStatus, { bg: string; text: string; label: string }> = {
    pending: { bg: '#F3F4F6', text: '#6B7280', label: 'Pending' },
    'in-progress': { bg: '#EFF6FF', text: '#3B82F6', label: 'In Progress' },
    submitted: { bg: '#FFFBEB', text: '#F59E0B', label: 'Submitted' },
    examined: { bg: '#ECFDF5', text: '#10B981', label: 'Examined' },
    all: { bg: '#F3F4F6', text: '#6B7280', label: 'All' },
  };
  const s = styles[status];
  return (
    <span
      className="inline-block px-2 py-0.5 rounded-full text-xs font-medium"
      style={{ backgroundColor: s.bg, color: s.text }}
    >
      {s.label}
    </span>
  );
};

export const Exams = ({
  searchExams = ioc((keys) => keys.searchExams) || chikexamsService.searchExams,
  createExam = ioc((keys) => keys.createExam) || chikexamsService.createExam,
  cancelExam = ioc((keys) => keys.cancelExam) || chikexamsService.cancelExam,
  searchUsers = ioc((keys) => keys.searchUsers) || chikexamsService.searchUsers,
  searchQuizzes = ioc((keys) => keys.searchQuizzes) || chikexamsService.searchQuizzes,
}: {
  searchExams?: typeof chikexamsService.searchExams;
  createExam?: typeof chikexamsService.createExam;
  cancelExam?: typeof chikexamsService.cancelExam;
  searchUsers?: typeof chikexamsService.searchUsers;
  searchQuizzes?: typeof chikexamsService.searchQuizzes;
}) => {
  const navigate = useNavigate();
  const examsCache = useCacheUpdate(CacheKeys.searchExams);
  const [statusFilter, setStatusFilter] = useState<ExamStatus>('all');
  const [search, setSearch] = useState('');
  const [assignOpen, setAssignOpen] = useState(false);
  const [cancelTarget, setCancelTarget] = useState<types['Exam'] | null>(null);

  const { data: exams, isLoading } = useExams({
    params: {
      IncludeUser: true,
      IncludeQuiz: true,
    },
    searchExams,
  });

  const filteredExams = (exams ?? []).filter((exam) => {
    if (search) {
      const q = search.toLowerCase();
      if (!(exam.user?.username?.toLowerCase().includes(q) ?? false)) return false;
    }
    if (statusFilter !== 'all') {
      return getExamStatus(exam) === statusFilter;
    }
    return true;
  });

  const createMutation = useMutation({
    mutationFn: async ({ userIds, quizId }: { userIds: number[]; quizId: number }) => {
      await Promise.all(userIds.map((userId) => createExam(userId, quizId)));
    },
    onSuccess: (_data, { userIds }) => {
      const n = userIds.length;
      toast.success(n === 1 ? 'Exam assigned' : `Exam assigned to ${n} students`);
      examsCache.invalidateAndRefetch();
      setAssignOpen(false);
    },
    onError: () => {
      toast.error('Failed to assign exam');
    },
  });

  const cancelMutation = useMutation({
    mutationFn: async (id: number) => await cancelExam(id),
    onSuccess: () => {
      toast.success('Exam cancelled');
      examsCache.invalidateAndRefetch();
      setCancelTarget(null);
    },
    onError: () => {
      toast.error('Failed to cancel exam');
    },
  });

  return (
    <div>
      <div className="flex items-center justify-between mb-6">
        <h1 className="text-2xl font-bold" style={{ color: '#211A1E' }}>Exams</h1>
        <Button
          variant="contained"
          startIcon={<Plus size={16} />}
          onClick={() => setAssignOpen(true)}
          sx={{ backgroundColor: '#314CB6', '&:hover': { backgroundColor: '#2a3f9e' } }}
        >
          Assign Exam
        </Button>
      </div>

      <div className="flex items-center gap-3 mb-4">
        <FormControl size="small" sx={{ minWidth: 160 }}>
          <InputLabel>Filter by status</InputLabel>
          <Select
            label="Filter by status"
            value={statusFilter}
            onChange={(e) => setStatusFilter(e.target.value as ExamStatus)}
          >
            <MenuItem value="all">All</MenuItem>
            <MenuItem value="pending">Pending</MenuItem>
            <MenuItem value="in-progress">In Progress</MenuItem>
            <MenuItem value="submitted">Submitted</MenuItem>
            <MenuItem value="examined">Examined</MenuItem>
          </Select>
        </FormControl>

        <div
          className="flex items-center gap-2 px-3 py-1.5 rounded-lg bg-white"
          style={{ border: '1px solid #E5E7EB' }}
        >
          <Search size={16} style={{ color: '#6B7280' }} />
          <input
            type="text"
            placeholder="Search student..."
            value={search}
            onChange={(e) => setSearch(e.target.value)}
            className="text-sm outline-none bg-transparent"
            style={{ color: '#211A1E', width: 180 }}
          />
        </div>
      </div>

      <div className="bg-white rounded-xl shadow-sm" style={{ border: '1px solid #E5E7EB' }}>
        <div className="overflow-x-auto">
          <table className="w-full">
            <thead>
              <tr style={{ borderBottom: '1px solid #E5E7EB' }}>
                <th className="px-4 py-3 text-left text-xs font-semibold uppercase" style={{ color: '#6B7280' }}>Student</th>
                <th className="px-4 py-3 text-left text-xs font-semibold uppercase" style={{ color: '#6B7280' }}>Quiz</th>
                <th className="px-4 py-3 text-left text-xs font-semibold uppercase" style={{ color: '#6B7280' }}>Status</th>
                <th className="px-4 py-3 text-left text-xs font-semibold uppercase" style={{ color: '#6B7280' }}>Score</th>
                <th className="px-4 py-3 text-left text-xs font-semibold uppercase" style={{ color: '#6B7280' }}>Started</th>
                <th className="px-4 py-3 text-left text-xs font-semibold uppercase" style={{ color: '#6B7280' }}>Actions</th>
              </tr>
            </thead>
            <tbody>
              {isLoading ? (
                <tr>
                  <td colSpan={6} className="px-4 py-8 text-center">
                    <CircularProgress size={24} />
                  </td>
                </tr>
              ) : filteredExams.length === 0 ? (
                <tr>
                  <td colSpan={6} className="px-4 py-8 text-center text-sm" style={{ color: '#6B7280' }}>
                    No exams found
                  </td>
                </tr>
              ) : (
                filteredExams.map((exam) => {
                  const status = getExamStatus(exam);
                  return (
                    <tr
                      key={exam.id}
                      style={{ borderBottom: '1px solid #F3F4F6' }}
                      className="hover:bg-slate-50 transition-colors"
                    >
                      <td className="px-4 py-3 text-sm font-medium" style={{ color: '#211A1E' }}>
                        {exam.user?.username ?? `User ${exam.userId}`}
                      </td>
                      <td className="px-4 py-3 text-sm" style={{ color: '#6B7280' }}>
                        {exam.quiz?.title ?? `Quiz ${exam.quizId}`}
                      </td>
                      <td className="px-4 py-3">
                        <StatusBadge status={status} />
                      </td>
                      <td className="px-4 py-3 text-sm" style={{ color: '#6B7280' }}>
                        {exam.score != null ? `${exam.score}` : '—'}
                      </td>
                      <td className="px-4 py-3 text-sm" style={{ color: '#6B7280' }}>
                        {exam.startedAt ? new Date(exam.startedAt).toLocaleDateString() : '—'}
                      </td>
                      <td className="px-4 py-3">
                        <div className="flex items-center gap-2">
                          {(status === 'submitted' || status === 'examined' || status === 'in-progress') && (
                            <button
                              className="text-xs px-2 py-1 rounded"
                              style={{ color: '#314CB6', border: '1px solid #314CB6' }}
                              onClick={() => navigate(`/exams/${exam.id}`)}
                            >
                              Review
                            </button>
                          )}
                          {status === 'pending' && (
                            <button
                              className="text-xs px-2 py-1 rounded"
                              style={{ color: '#EF4444', border: '1px solid #EF4444' }}
                              onClick={() => setCancelTarget(exam)}
                            >
                              Cancel
                            </button>
                          )}
                        </div>
                      </td>
                    </tr>
                  );
                })
              )}
            </tbody>
          </table>
        </div>
      </div>

      <AssignExamModal
        open={assignOpen}
        onClose={() => setAssignOpen(false)}
        onAssign={(userIds, quizId) => createMutation.mutate({ userIds, quizId })}
        isLoading={createMutation.isLoading}
        searchUsers={searchUsers}
        searchQuizzes={searchQuizzes}
      />

      <Dialog open={!!cancelTarget} onClose={() => setCancelTarget(null)} maxWidth="xs" fullWidth>
        <DialogTitle>Cancel Exam</DialogTitle>
        <DialogContent>
          <p className="text-sm" style={{ color: '#211A1E' }}>
            Are you sure you want to cancel this exam for{' '}
            <strong>{cancelTarget?.user?.username ?? 'this student'}</strong>?
          </p>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setCancelTarget(null)} color="inherit">No</Button>
          <Button
            variant="contained"
            color="error"
            disabled={cancelMutation.isLoading}
            onClick={() => cancelTarget && cancelMutation.mutate(cancelTarget.id)}
          >
            {cancelMutation.isLoading ? 'Cancelling...' : 'Yes, Cancel'}
          </Button>
        </DialogActions>
      </Dialog>
    </div>
  );
};
