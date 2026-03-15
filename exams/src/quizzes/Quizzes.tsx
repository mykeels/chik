import { useState } from 'react';
import { useNavigate } from 'react-router';
import { useMutation } from 'react-query';
import { useCacheUpdate } from '@/hooks/useCacheUpdate';
import { toast } from 'react-toastify';
import { ioc } from '@/utils/ioc';
import * as chikexamsService from '@/services/chikexams.service';
import { types } from '@/services/chikexams.service';
import { useQuizzes } from '@/services/chikexams.hooks';
import { CacheKeys } from '@/utils/cache-keys.utils';
import { Search, Plus, Edit, Trash2 } from 'lucide-react';
import {
  Button,
  CircularProgress,
  Dialog,
  DialogActions,
  DialogContent,
  DialogTitle,
} from '@mui/material';

const parseDuration = (duration: string | null | undefined): string => {
  if (!duration) return '—';
  // Parse "HH:MM:SS" or ISO 8601 duration
  const match = duration.match(/(\d+):(\d+):(\d+)/);
  if (match) {
    const hours = parseInt(match[1]);
    const minutes = parseInt(match[2]);
    if (hours > 0 && minutes > 0) return `${hours}h ${minutes}m`;
    if (hours > 0) return `${hours}h`;
    if (minutes > 0) return `${minutes}m`;
  }
  return duration;
};

export const Quizzes = ({
  searchQuizzes = ioc((keys) => keys.searchQuizzes) || chikexamsService.searchQuizzes,
  deleteQuiz = ioc((keys) => keys.deleteQuiz) || chikexamsService.deleteQuiz,
}: {
  searchQuizzes?: typeof chikexamsService.searchQuizzes;
  deleteQuiz?: typeof chikexamsService.deleteQuiz;
}) => {
  const navigate = useNavigate();
  const quizzesCache = useCacheUpdate(CacheKeys.searchQuizzes);
  const [search, setSearch] = useState('');
  const [deleteTarget, setDeleteTarget] = useState<types.Quiz | null>(null);

  const { data: quizzes, isLoading } = useQuizzes({
    params: { Title: search || undefined, IncludeQuestions: true },
    searchQuizzes,
  });

  const deleteMutation = useMutation({
    mutationFn: async (id: number) => await deleteQuiz(id),
    onSuccess: () => {
      toast.success('Quiz deleted');
      quizzesCache.invalidateAndRefetch();
      setDeleteTarget(null);
    },
    onError: () => {
      toast.error('Failed to delete quiz');
    },
  });

  return (
    <div>
      <div className="flex items-center justify-between mb-6">
        <h1 className="text-2xl font-bold" style={{ color: '#211A1E' }}>
          Quizzes
        </h1>
        <Button
          variant="contained"
          startIcon={<Plus size={16} />}
          onClick={() => navigate('/quizzes/new')}
          sx={{ backgroundColor: '#314CB6', '&:hover': { backgroundColor: '#2a3f9e' } }}
        >
          New Quiz
        </Button>
      </div>

      <div
        className="flex items-center gap-2 px-3 py-1.5 rounded-lg bg-white mb-4"
        style={{ border: '1px solid #E5E7EB', width: 'fit-content' }}
      >
        <Search size={16} style={{ color: '#6B7280' }} />
        <input
          type="text"
          placeholder="Search quizzes..."
          value={search}
          onChange={(e) => setSearch(e.target.value)}
          className="text-sm outline-none bg-transparent"
          style={{ color: '#211A1E', width: 220 }}
        />
      </div>

      <div className="bg-white rounded-xl shadow-sm" style={{ border: '1px solid #E5E7EB' }}>
        <div className="overflow-x-auto">
          <table className="w-full">
            <thead>
              <tr style={{ borderBottom: '1px solid #E5E7EB' }}>
                <th className="px-4 py-3 text-left text-xs font-semibold uppercase" style={{ color: '#6B7280' }}>Title</th>
                <th className="px-4 py-3 text-left text-xs font-semibold uppercase" style={{ color: '#6B7280' }}>Questions</th>
                <th className="px-4 py-3 text-left text-xs font-semibold uppercase" style={{ color: '#6B7280' }}>Duration</th>
                <th className="px-4 py-3 text-left text-xs font-semibold uppercase" style={{ color: '#6B7280' }}>Created</th>
                <th className="px-4 py-3 text-left text-xs font-semibold uppercase" style={{ color: '#6B7280' }}>Actions</th>
              </tr>
            </thead>
            <tbody>
              {isLoading ? (
                <tr>
                  <td colSpan={5} className="px-4 py-8 text-center">
                    <CircularProgress size={24} />
                  </td>
                </tr>
              ) : (quizzes ?? []).length === 0 ? (
                <tr>
                  <td colSpan={5} className="px-4 py-8 text-center text-sm" style={{ color: '#6B7280' }}>
                    No quizzes found
                  </td>
                </tr>
              ) : (
                (quizzes ?? []).map((quiz) => (
                  <tr
                    key={quiz.id}
                    style={{ borderBottom: '1px solid #F3F4F6' }}
                    className="hover:bg-slate-50 transition-colors"
                  >
                    <td className="px-4 py-3 text-sm font-medium" style={{ color: '#211A1E' }}>
                      {quiz.title}
                    </td>
                    <td className="px-4 py-3 text-sm" style={{ color: '#6B7280' }}>
                      {quiz.questions?.length ?? '—'}
                    </td>
                    <td className="px-4 py-3 text-sm" style={{ color: '#6B7280' }}>
                      {parseDuration(quiz.duration)}
                    </td>
                    <td className="px-4 py-3 text-sm" style={{ color: '#6B7280' }}>
                      {new Date(quiz.createdAt).toLocaleDateString()}
                    </td>
                    <td className="px-4 py-3">
                      <div className="flex items-center gap-2">
                        <button
                          className="flex items-center gap-1 text-xs px-2 py-1 rounded"
                          style={{ color: '#314CB6', border: '1px solid #314CB6' }}
                          onClick={() => navigate(`/quizzes/${quiz.id}/edit`)}
                        >
                          <Edit size={12} />
                          Edit
                        </button>
                        <button
                          className="flex items-center gap-1 text-xs px-2 py-1 rounded"
                          style={{ color: '#EF4444', border: '1px solid #EF4444' }}
                          onClick={() => setDeleteTarget(quiz)}
                        >
                          <Trash2 size={12} />
                          Delete
                        </button>
                      </div>
                    </td>
                  </tr>
                ))
              )}
            </tbody>
          </table>
        </div>
      </div>

      <Dialog open={!!deleteTarget} onClose={() => setDeleteTarget(null)} maxWidth="xs" fullWidth>
        <DialogTitle>Delete Quiz</DialogTitle>
        <DialogContent>
          <p className="text-sm" style={{ color: '#211A1E' }}>
            Are you sure you want to delete <strong>{deleteTarget?.title}</strong>? This action cannot be undone.
          </p>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setDeleteTarget(null)} color="inherit">Cancel</Button>
          <Button
            variant="contained"
            color="error"
            disabled={deleteMutation.isLoading}
            onClick={() => deleteTarget && deleteMutation.mutate(deleteTarget.id)}
          >
            {deleteMutation.isLoading ? 'Deleting...' : 'Delete'}
          </Button>
        </DialogActions>
      </Dialog>
    </div>
  );
};
