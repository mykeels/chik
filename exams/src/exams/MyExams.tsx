import { useState } from 'react';
import { useNavigate } from 'react-router';
import { useMutation } from 'react-query';
import { useCacheUpdate } from '@/hooks/useCacheUpdate';
import { toast } from 'react-toastify';
import { ioc } from '@/utils/ioc';
import * as chikexamsService from '@/services/chikexams.service';
import { usePendingExams, useExamHistory } from '@/services/chikexams.hooks';
import { CacheKeys } from '@/utils/cache-keys.utils';
import { Tabs, Tab, CircularProgress, Button } from '@mui/material';

export const MyExams = ({
  getPendingExams: getPendingExamsFn = ioc((keys) => keys.getPendingExams) || chikexamsService.getPendingExams,
  getExamHistory: getExamHistoryFn = ioc((keys) => keys.getExamHistory) || chikexamsService.getExamHistory,
  startExam = ioc((keys) => keys.startExam) || chikexamsService.startExam,
}: {
  getPendingExams?: typeof chikexamsService.getPendingExams;
  getExamHistory?: typeof chikexamsService.getExamHistory;
  startExam?: typeof chikexamsService.startExam;
}) => {
  const navigate = useNavigate();
  const pendingExamsCache = useCacheUpdate(CacheKeys.getPendingExams);
  const [tab, setTab] = useState<'pending' | 'history'>('pending');

  const { data: pendingExams, isLoading: pendingLoading } = usePendingExams({
    getPendingExams: getPendingExamsFn,
  });
  const { data: historyExams, isLoading: historyLoading } = useExamHistory({
    getExamHistory: getExamHistoryFn,
  });

  const startMutation = useMutation({
    mutationFn: async (examId: number) => {
      return await startExam(examId);
    },
    onSuccess: (exam) => {
      toast.success('Exam started!');
      pendingExamsCache.invalidateAndRefetch();
      navigate(`/exams/${exam?.id}/take`);
    },
    onError: () => {
      toast.error('Failed to start exam');
    },
  });

  return (
    <div>
      <h1 className="text-2xl font-bold mb-6" style={{ color: '#211A1E' }}>My Exams</h1>

      <Tabs
        value={tab}
        onChange={(_, val) => setTab(val)}
        sx={{ mb: 3, '& .MuiTab-root': { textTransform: 'none', fontSize: '0.875rem' } }}
      >
        <Tab label="Pending" value="pending" />
        <Tab label="History" value="history" />
      </Tabs>

      {tab === 'pending' && (
        <div className="bg-white rounded-xl shadow-sm" style={{ border: '1px solid #E5E7EB' }}>
          <div className="overflow-x-auto">
            <table className="w-full">
              <thead>
                <tr style={{ borderBottom: '1px solid #E5E7EB' }}>
                  <th className="px-4 py-3 text-left text-xs font-semibold uppercase" style={{ color: '#6B7280' }}>Quiz</th>
                  <th className="px-4 py-3 text-left text-xs font-semibold uppercase" style={{ color: '#6B7280' }}>Assigned By</th>
                  <th className="px-4 py-3 text-left text-xs font-semibold uppercase" style={{ color: '#6B7280' }}>Due</th>
                  <th className="px-4 py-3 text-left text-xs font-semibold uppercase" style={{ color: '#6B7280' }}>Actions</th>
                </tr>
              </thead>
              <tbody>
                {pendingLoading ? (
                  <tr>
                    <td colSpan={4} className="px-4 py-8 text-center">
                      <CircularProgress size={24} />
                    </td>
                  </tr>
                ) : (pendingExams ?? []).length === 0 ? (
                  <tr>
                    <td colSpan={4} className="px-4 py-8 text-center text-sm" style={{ color: '#6B7280' }}>
                      No pending exams
                    </td>
                  </tr>
                ) : (
                  (pendingExams ?? []).map((exam) => (
                    <tr
                      key={exam.id}
                      style={{ borderBottom: '1px solid #F3F4F6' }}
                      className="hover:bg-slate-50 transition-colors"
                    >
                      <td className="px-4 py-3 text-sm font-medium" style={{ color: '#211A1E' }}>
                        {exam.quiz?.title ?? `Quiz ${exam.quizId}`}
                      </td>
                      <td className="px-4 py-3 text-sm" style={{ color: '#6B7280' }}>
                        {exam.creator?.username ?? `User ${exam.creatorId}`}
                      </td>
                      <td className="px-4 py-3 text-sm" style={{ color: '#6B7280' }}>—</td>
                      <td className="px-4 py-3">
                        <Button
                          variant="contained"
                          size="small"
                          disabled={startMutation.isLoading}
                          onClick={() => {
                            if (exam.isStarted) {
                              navigate(`/exams/${exam.id}/take`);
                            } else {
                              startMutation.mutate(exam.id);
                            }
                          }}
                          sx={{
                            backgroundColor: '#314CB6',
                            '&:hover': { backgroundColor: '#2a3f9e' },
                            fontSize: '0.75rem',
                          }}
                        >
                          {exam.isStarted ? 'Continue' : 'Start Exam'}
                        </Button>
                      </td>
                    </tr>
                  ))
                )}
              </tbody>
            </table>
          </div>
        </div>
      )}

      {tab === 'history' && (
        <div className="bg-white rounded-xl shadow-sm" style={{ border: '1px solid #E5E7EB' }}>
          <div className="overflow-x-auto">
            <table className="w-full">
              <thead>
                <tr style={{ borderBottom: '1px solid #E5E7EB' }}>
                  <th className="px-4 py-3 text-left text-xs font-semibold uppercase" style={{ color: '#6B7280' }}>Quiz</th>
                  <th className="px-4 py-3 text-left text-xs font-semibold uppercase" style={{ color: '#6B7280' }}>Score</th>
                  <th className="px-4 py-3 text-left text-xs font-semibold uppercase" style={{ color: '#6B7280' }}>Submitted</th>
                  <th className="px-4 py-3 text-left text-xs font-semibold uppercase" style={{ color: '#6B7280' }}>Actions</th>
                </tr>
              </thead>
              <tbody>
                {historyLoading ? (
                  <tr>
                    <td colSpan={4} className="px-4 py-8 text-center">
                      <CircularProgress size={24} />
                    </td>
                  </tr>
                ) : (historyExams ?? []).length === 0 ? (
                  <tr>
                    <td colSpan={4} className="px-4 py-8 text-center text-sm" style={{ color: '#6B7280' }}>
                      No exam history
                    </td>
                  </tr>
                ) : (
                  (historyExams ?? []).map((exam) => (
                    <tr
                      key={exam.id}
                      style={{ borderBottom: '1px solid #F3F4F6' }}
                      className="hover:bg-slate-50 transition-colors"
                    >
                      <td className="px-4 py-3 text-sm font-medium" style={{ color: '#211A1E' }}>
                        {exam.quiz?.title ?? `Quiz ${exam.quizId}`}
                      </td>
                      <td className="px-4 py-3 text-sm" style={{ color: '#6B7280' }}>
                        {exam.score != null ? `${exam.score}` : '—'}
                      </td>
                      <td className="px-4 py-3 text-sm" style={{ color: '#6B7280' }}>
                        {exam.endedAt ? new Date(exam.endedAt).toLocaleDateString() : '—'}
                      </td>
                      <td className="px-4 py-3">
                        <button
                          className="text-xs px-2 py-1 rounded"
                          style={{ color: '#314CB6', border: '1px solid #314CB6' }}
                          onClick={() => navigate(`/exams/${exam.id}/review`)}
                        >
                          Review
                        </button>
                      </td>
                    </tr>
                  ))
                )}
              </tbody>
            </table>
          </div>
        </div>
      )}
    </div>
  );
};
