import { useRef, useState } from 'react';
import { useNavigate } from 'react-router';
import { useMutation } from 'react-query';
import { useCacheUpdate } from '@/hooks/useCacheUpdate';
import { toast } from 'react-toastify';
import { ioc } from '@/utils/ioc';
import * as chikexamsService from '@/services/chikexams.service';
import { types } from '@/services/chikexams.service';
import { useQuizzes } from '@/services/chikexams.hooks';
import { CacheKeys } from '@/utils/cache-keys.utils';
import { Search, Plus, Edit, Trash2, Download, Upload } from 'lucide-react';
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
  exportQuiz = ioc((keys) => keys.exportQuiz) || chikexamsService.exportQuiz,
  importQuiz = ioc((keys) => keys.importQuiz) || chikexamsService.importQuiz,
}: {
  searchQuizzes?: typeof chikexamsService.searchQuizzes;
  deleteQuiz?: typeof chikexamsService.deleteQuiz;
  exportQuiz?: typeof chikexamsService.exportQuiz;
  importQuiz?: typeof chikexamsService.importQuiz;
}) => {
  const navigate = useNavigate();
  const quizzesCache = useCacheUpdate(CacheKeys.searchQuizzes);
  const [search, setSearch] = useState('');
  const [deleteTarget, setDeleteTarget] = useState<types.Quiz | null>(null);
  const [importOpen, setImportOpen] = useState(false);
  const [importFile, setImportFile] = useState<File | null>(null);
  const fileInputRef = useRef<HTMLInputElement>(null);

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

  const exportMutation = useMutation({
    mutationFn: async (id: number) => await exportQuiz(id),
    onError: () => {
      toast.error('Failed to export quiz');
    },
  });

  const importMutation = useMutation({
    mutationFn: async (file: File) => await importQuiz(file),
    onSuccess: (quiz) => {
      toast.success(`Quiz "${quiz.title}" imported successfully`);
      quizzesCache.invalidateAndRefetch();
      setImportOpen(false);
      setImportFile(null);
    },
    onError: () => {
      toast.error('Failed to import quiz — check that the ZIP contains a valid index.yaml');
    },
  });

  const handleImportFileChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0] ?? null;
    setImportFile(file);
  };

  const handleImportClose = () => {
    setImportOpen(false);
    setImportFile(null);
    if (fileInputRef.current) fileInputRef.current.value = '';
  };

  return (
    <div>
      <div className="flex items-center justify-between mb-6">
        <h1 className="text-2xl font-bold" style={{ color: '#211A1E' }}>
          Quizzes
        </h1>
        <div className="flex items-center gap-2">
          <Button
            variant="outlined"
            startIcon={<Upload size={16} />}
            onClick={() => setImportOpen(true)}
            sx={{ borderColor: '#314CB6', color: '#314CB6', '&:hover': { borderColor: '#2a3f9e', color: '#2a3f9e' } }}
          >
            Import
          </Button>
          <Button
            variant="contained"
            startIcon={<Plus size={16} />}
            onClick={() => navigate('/quizzes/new')}
            sx={{ backgroundColor: '#314CB6', '&:hover': { backgroundColor: '#2a3f9e' } }}
          >
            New Quiz
          </Button>
        </div>
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
                          className="flex items-center gap-1 text-xs px-2 py-1 rounded disabled:opacity-50"
                          style={{ color: '#6B7280', border: '1px solid #6B7280' }}
                          disabled={exportMutation.isLoading}
                          onClick={() => exportMutation.mutate(quiz.id)}
                        >
                          <Download size={12} />
                          Export
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

      {/* Delete dialog */}
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

      {/* Import dialog */}
      <Dialog open={importOpen} onClose={handleImportClose} maxWidth="xs" fullWidth>
        <DialogTitle>Import Quiz</DialogTitle>
        <DialogContent>
          <p className="text-sm mb-4" style={{ color: '#6B7280' }}>
            Upload a <code>.zip</code> file containing an <code>index.yaml</code>, or upload a <code>.yaml</code> file directly.
          </p>
          <div
            className="flex flex-col items-center justify-center gap-3 rounded-lg p-6 cursor-pointer"
            style={{ border: '2px dashed #E5E7EB', background: importFile ? '#F0F4FF' : '#FAFAFA' }}
            onClick={() => fileInputRef.current?.click()}
          >
            <Upload size={24} style={{ color: importFile ? '#314CB6' : '#6B7280' }} />
            {importFile ? (
              <span className="text-sm font-medium" style={{ color: '#314CB6' }}>{importFile.name}</span>
            ) : (
              <span className="text-sm" style={{ color: '#6B7280' }}>Click to select a .zip or .yaml file</span>
            )}
          </div>
          <input
            ref={fileInputRef}
            type="file"
            accept=".zip,.yaml,.yml,application/zip,application/octet-stream,application/x-yaml,text/yaml"
            className="hidden"
            onChange={handleImportFileChange}
          />
        </DialogContent>
        <DialogActions>
          <Button onClick={handleImportClose} color="inherit" disabled={importMutation.isLoading}>
            Cancel
          </Button>
          <Button
            variant="contained"
            disabled={!importFile || importMutation.isLoading}
            onClick={() => importFile && importMutation.mutate(importFile)}
            sx={{ backgroundColor: '#314CB6', '&:hover': { backgroundColor: '#2a3f9e' } }}
          >
            {importMutation.isLoading ? <CircularProgress size={16} sx={{ color: 'white' }} /> : 'Import'}
          </Button>
        </DialogActions>
      </Dialog>
    </div>
  );
};
