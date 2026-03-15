import { useState } from 'react';
import { Link, useParams } from 'react-router';
import { useMutation } from 'react-query';
import { useCacheUpdate } from '@/hooks/useCacheUpdate';
import { toast } from 'react-toastify';
import { ioc } from '@/utils/ioc';
import * as chikexamsService from '@/services/chikexams.service';
import { useExam, useExamAnswers, useExamScores } from '@/services/chikexams.hooks';
import { CacheKeys } from '@/utils/cache-keys.utils';
import { ChevronLeft } from 'lucide-react';
import { Button, CircularProgress, TextField } from '@mui/material';

const QUESTION_TYPE_LABELS: Record<number, string> = {
  1: 'Multiple Choice',
  2: 'Single Choice',
  3: 'Fill in the Blank',
  4: 'Essay',
  5: 'Short Answer',
  6: 'True or False',
};

const isManualScored = (typeId: number) => [4, 5, 6].includes(typeId);

type AnswerScoreState = {
  score: number | '';
  comment: string;
};

export const ExamReview = ({
  getExam: getExamFn = ioc((keys) => keys.getExam) || chikexamsService.getExam,
  getExamAnswers: getExamAnswersFn = ioc((keys) => keys.getExamAnswers) || chikexamsService.getExamAnswers,
  getExamScores: getExamScoresFn = ioc((keys) => keys.getExamScores) || chikexamsService.getExamScores,
  updateExam = ioc((keys) => keys.updateExam) || chikexamsService.updateExam,
  scoreAnswer = ioc((keys) => keys.scoreAnswer) || chikexamsService.scoreAnswer,
}: {
  getExam?: typeof chikexamsService.getExam;
  getExamAnswers?: typeof chikexamsService.getExamAnswers;
  getExamScores?: typeof chikexamsService.getExamScores;
  updateExam?: typeof chikexamsService.updateExam;
  scoreAnswer?: typeof chikexamsService.scoreAnswer;
}) => {
  const { id } = useParams<{ id: string }>();
  const examId = parseInt(id!);
  const examCache = useCacheUpdate(CacheKeys.getExam);
  const examAnswersCache = useCacheUpdate(CacheKeys.getExamAnswers);
  const examScoresCache = useCacheUpdate(CacheKeys.getExamScores);

  const [examinerComment, setExaminerComment] = useState('');
  const [answerScores, setAnswerScores] = useState<Record<number, AnswerScoreState>>({});

  const { data: exam, isLoading: examLoading } = useExam(examId, { getExam: getExamFn });
  const { data: answers, isLoading: answersLoading } = useExamAnswers(examId, { getExamAnswers: getExamAnswersFn });
  const { data: scores } = useExamScores(examId, { getExamScores: getExamScoresFn });

  const saveCommentMutation = useMutation({
    mutationFn: async () => {
      return await updateExam(examId, { examinerComment });
    },
    onSuccess: () => {
      toast.success('Comment saved');
      examCache.invalidateAndRefetch();
    },
    onError: () => {
      toast.error('Failed to save comment');
    },
  });

  const scoreAnswerMutation = useMutation({
    mutationFn: async ({ answerId, score, comment }: { answerId: number; score: number; comment?: string }) => {
      return await scoreAnswer(answerId, score, comment);
    },
    onSuccess: () => {
      toast.success('Score saved');
      examAnswersCache.invalidateAndRefetch();
      examScoresCache.invalidateAndRefetch();
    },
    onError: () => {
      toast.error('Failed to save score');
    },
  });

  if (examLoading) {
    return (
      <div className="flex items-center justify-center py-16">
        <CircularProgress />
      </div>
    );
  }

  if (!exam) {
    return <div className="text-center py-16 text-sm" style={{ color: '#6B7280' }}>Exam not found.</div>;
  }

  const getAnswerForQuestion = (questionId: number) =>
    (answers ?? []).find((a) => a.questionId === questionId);

  const getScoreForQuestion = (questionId: number) =>
    scores?.answerScores?.find((s) => s.questionId === questionId);

  return (
    <div>
      <div className="mb-4">
        <Link
          to="/exams"
          className="flex items-center gap-1 text-sm hover:underline"
          style={{ color: '#314CB6' }}
        >
          <ChevronLeft size={16} />
          Back to Exams
        </Link>
      </div>

      <div className="bg-white rounded-xl shadow-sm p-6 mb-6" style={{ border: '1px solid #E5E7EB' }}>
        <h1 className="text-xl font-bold mb-2" style={{ color: '#211A1E' }}>
          Exam: {exam.quiz?.title ?? `Quiz ${exam.quizId}`} — {exam.user?.username ?? `User ${exam.userId}`}
        </h1>
        <div className="flex flex-wrap gap-4 text-sm mb-4" style={{ color: '#6B7280' }}>
          <span>
            Status:{' '}
            <strong style={{ color: '#211A1E' }}>
              {!exam.isStarted ? 'Pending' : exam.isStarted && !exam.isEnded ? 'In Progress' : exam.isEnded && !exam.isMarked ? 'Submitted' : 'Examined'}
            </strong>
          </span>
          {exam.startedAt && (
            <span>Started: <strong style={{ color: '#211A1E' }}>{new Date(exam.startedAt).toLocaleString()}</strong></span>
          )}
          {exam.endedAt && (
            <span>Ended: <strong style={{ color: '#211A1E' }}>{new Date(exam.endedAt).toLocaleString()}</strong></span>
          )}
        </div>

        {scores && (
          <div className="mb-4 p-4 rounded-lg" style={{ backgroundColor: '#EBF2FA' }}>
            <p className="text-lg font-bold" style={{ color: '#211A1E' }}>
              Overall Score: {scores.totalScore} / {scores.maxPossibleScore}
            </p>
            <p className="text-sm mt-1" style={{ color: '#6B7280' }}>
              Answered: {scores.answeredQuestions} / {scores.totalQuestions} questions
            </p>
          </div>
        )}

        <div className="flex flex-col gap-2">
          <label className="text-sm font-medium" style={{ color: '#211A1E' }}>Examiner Comment</label>
          <TextField
            multiline
            rows={3}
            fullWidth
            size="small"
            value={examinerComment || exam.examinerComment || ''}
            onChange={(e) => setExaminerComment(e.target.value)}
            placeholder="Add an overall comment..."
          />
          <div className="flex justify-end">
            <Button
              variant="outlined"
              size="small"
              disabled={saveCommentMutation.isLoading}
              onClick={() => saveCommentMutation.mutate()}
              sx={{ borderColor: '#314CB6', color: '#314CB6' }}
            >
              {saveCommentMutation.isLoading ? 'Saving...' : 'Save Comment'}
            </Button>
          </div>
        </div>
      </div>

      {/* Questions */}
      {answersLoading ? (
        <div className="flex items-center justify-center py-8">
          <CircularProgress size={24} />
        </div>
      ) : (
        <div className="flex flex-col gap-4">
          {(answers ?? []).map((answer, idx) => {
            const question = answer.question;
            const scoreInfo = getScoreForQuestion(answer.questionId);
            const manual = question ? isManualScored(question.typeId) : false;
            const localState = answerScores[answer.id] ?? {
              score: answer.examinerScore ?? '',
              comment: answer.examinerComment ?? '',
            };

            return (
              <div
                key={answer.id}
                className="bg-white rounded-xl shadow-sm p-5"
                style={{ border: '1px solid #E5E7EB' }}
              >
                <div className="flex items-start justify-between mb-2">
                  <div>
                    <span className="text-sm font-semibold" style={{ color: '#211A1E' }}>
                      Q{idx + 1}. {question?.prompt ?? `Question ${answer.questionId}`}
                    </span>
                    {question && (
                      <span
                        className="ml-2 text-xs px-2 py-0.5 rounded"
                        style={{ backgroundColor: '#F3F4F6', color: '#6B7280' }}
                      >
                        {QUESTION_TYPE_LABELS[question.typeId] ?? 'Unknown'}
                      </span>
                    )}
                  </div>
                  <span className="text-sm font-medium" style={{ color: '#314CB6' }}>
                    {scoreInfo
                      ? `${scoreInfo.finalScore} / ${scoreInfo.maxScore}`
                      : question
                        ? `— / ${question.score}`
                        : '— / —'}
                  </span>
                </div>

                <div className="mb-3">
                  <p className="text-xs font-medium mb-1" style={{ color: '#6B7280' }}>Student's Answer:</p>
                  <p className="text-sm p-2 rounded" style={{ backgroundColor: '#F9FAFB', color: '#211A1E' }}>
                    {answer.answer ?? <em style={{ color: '#6B7280' }}>No answer</em>}
                  </p>
                </div>

                {!manual && (
                  <div className="text-sm" style={{ color: '#6B7280' }}>
                    Auto-score: <strong style={{ color: '#211A1E' }}>{answer.autoScore ?? '—'}</strong>
                  </div>
                )}

                {manual && (
                  <div className="flex flex-col gap-2 mt-2">
                    <div className="flex gap-3 items-end">
                      <TextField
                        label="Examiner Score"
                        type="number"
                        size="small"
                        sx={{ width: 140 }}
                        value={localState.score}
                        onChange={(e) =>
                          setAnswerScores((prev) => ({
                            ...prev,
                            [answer.id]: { ...localState, score: e.target.value === '' ? '' : Number(e.target.value) },
                          }))
                        }
                        inputProps={{ min: 0, max: question?.score }}
                      />
                      {question && (
                        <span className="text-sm mb-1" style={{ color: '#6B7280' }}>/ {question.score}</span>
                      )}
                    </div>
                    <TextField
                      label="Comment"
                      multiline
                      rows={2}
                      fullWidth
                      size="small"
                      value={localState.comment}
                      onChange={(e) =>
                        setAnswerScores((prev) => ({
                          ...prev,
                          [answer.id]: { ...localState, comment: e.target.value },
                        }))
                      }
                      placeholder="Optional comment for this answer..."
                    />
                    <div className="flex justify-end">
                      <Button
                        variant="outlined"
                        size="small"
                        disabled={scoreAnswerMutation.isLoading || localState.score === ''}
                        onClick={() =>
                          scoreAnswerMutation.mutate({
                            answerId: answer.id,
                            score: Number(localState.score),
                            comment: localState.comment || undefined,
                          })
                        }
                        sx={{ borderColor: '#314CB6', color: '#314CB6' }}
                      >
                        Save Score
                      </Button>
                    </div>
                  </div>
                )}
              </div>
            );
          })}
        </div>
      )}
    </div>
  );
};
