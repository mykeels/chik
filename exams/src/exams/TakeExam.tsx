import { useState, useEffect, useCallback } from 'react';
import { useParams, useNavigate } from 'react-router';
import { useMutation, useQueryClient } from 'react-query';
import { toast } from 'react-toastify';
import { ioc } from '@/utils/ioc';
import * as chikexamsService from '@/services/chikexams.service';
import { useExam, useQuizQuestions, useExamAnswers } from '@/services/chikexams.hooks';
import { CacheKeys } from '@/utils/cache-keys.utils';
import { ChevronLeft, ChevronRight } from 'lucide-react';
import {
  Button,
  CircularProgress,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
} from '@mui/material';

const QUESTION_TYPES = {
  SINGLE_CHOICE: 1,
  MULTIPLE_CHOICE: 2,
  TRUE_OR_FALSE: 3,
  FILL_IN_THE_BLANK: 4,
  SHORT_ANSWER: 5,
  ESSAY: 6,
};

type Option = { text: string; isCorrect: boolean };

const parseOptions = (properties: unknown): Option[] => {
  try {
    const p = typeof properties === 'string' ? JSON.parse(properties) : properties;
    return p?.options ?? [];
  } catch {
    return [];
  }
};

const parseDurationToSeconds = (duration: string | null | undefined): number | null => {
  if (!duration) return null;
  const match = duration.match(/(\d+):(\d+):(\d+)/);
  if (match) {
    return parseInt(match[1]) * 3600 + parseInt(match[2]) * 60 + parseInt(match[3]);
  }
  return null;
};

const formatTime = (seconds: number): string => {
  const h = Math.floor(seconds / 3600);
  const m = Math.floor((seconds % 3600) / 60);
  const s = seconds % 60;
  if (h > 0) return `${h}:${String(m).padStart(2, '0')}:${String(s).padStart(2, '0')}`;
  return `${m}:${String(s).padStart(2, '0')}`;
};

export const TakeExam = ({
  getExam: getExamFn = ioc((keys) => keys.getExam) || chikexamsService.getExam,
  getQuizQuestions: getQuizQuestionsFn = ioc((keys) => keys.getQuizQuestions) || chikexamsService.getQuizQuestions,
  getExamAnswers: getExamAnswersFn = ioc((keys) => keys.getExamAnswers) || chikexamsService.getExamAnswers,
  submitAnswer = ioc((keys) => keys.submitAnswer) || chikexamsService.submitAnswer,
  submitExam = ioc((keys) => keys.submitExam) || chikexamsService.submitExam,
}: {
  getExam?: typeof chikexamsService.getExam;
  getQuizQuestions?: typeof chikexamsService.getQuizQuestions;
  getExamAnswers?: typeof chikexamsService.getExamAnswers;
  submitAnswer?: typeof chikexamsService.submitAnswer;
  submitExam?: typeof chikexamsService.submitExam;
}) => {
  const { id } = useParams<{ id: string }>();
  const examId = parseInt(id!);
  const navigate = useNavigate();
  const queryClient = useQueryClient();

  const [currentIndex, setCurrentIndex] = useState(0);
  const [localAnswers, setLocalAnswers] = useState<Record<number, string>>({});
  const [confirmOpen, setConfirmOpen] = useState(false);
  const [timeLeft, setTimeLeft] = useState<number | null>(null);

  const { data: exam, isLoading: examLoading } = useExam(examId, { getExam: getExamFn });
  const { data: questions, isLoading: questionsLoading } = useQuizQuestions(exam?.quizId ?? 0, {
    getQuizQuestions: getQuizQuestionsFn,
  });
  const { data: savedAnswers } = useExamAnswers(examId, { getExamAnswers: getExamAnswersFn });

  const sortedQuestions = [...(questions ?? [])].filter((q) => q.isActive).sort((a, b) => a.order - b.order);
  const currentQuestion = sortedQuestions[currentIndex];

  // Initialize timer
  useEffect(() => {
    if (exam?.quiz?.duration) {
      const total = parseDurationToSeconds(exam.quiz.duration);
      if (total && exam.startedAt) {
        const elapsed = Math.floor((Date.now() - new Date(exam.startedAt).getTime()) / 1000);
        setTimeLeft(Math.max(0, total - elapsed));
      }
    }
  }, [exam]);

  // Timer tick
  useEffect(() => {
    if (timeLeft === null) return;
    if (timeLeft <= 0) {
      handleSubmit();
      return;
    }
    const timer = setInterval(() => setTimeLeft((t) => (t !== null ? t - 1 : null)), 1000);
    return () => clearInterval(timer);
  }, [timeLeft]);

  // Load saved answers into local state
  useEffect(() => {
    if (savedAnswers) {
      const mapped: Record<number, string> = {};
      savedAnswers.forEach((a) => {
        if (a.questionId && a.answer) {
          mapped[a.questionId] = a.answer;
        }
      });
      setLocalAnswers((prev) => ({ ...mapped, ...prev }));
    }
  }, [savedAnswers]);

  const submitAnswerMutation = useMutation({
    mutationFn: async ({ questionId, answer }: { questionId: number; answer: string | null }) => {
      return await submitAnswer(examId, questionId, answer);
    },
    onError: () => {
      toast.error('Failed to save answer');
    },
  });

  const submitExamMutation = useMutation({
    mutationFn: async () => {
      return await submitExam(examId);
    },
    onSuccess: (result) => {
      toast.success('Exam submitted!');
      queryClient.invalidateQueries(CacheKeys.getPendingExams);
      navigate(`/exams/${result?.id ?? examId}/review`);
    },
    onError: () => {
      toast.error('Failed to submit exam');
    },
  });

  const handleAnswerChange = (questionId: number, answer: string) => {
    setLocalAnswers((prev) => ({ ...prev, [questionId]: answer }));
    submitAnswerMutation.mutate({ questionId, answer });
  };

  const handleMultipleChoiceChange = (questionId: number, optionText: string, checked: boolean) => {
    const current = localAnswers[questionId] ?? '';
    const currentSelections = current ? current.split(',').map((s) => s.trim()).filter(Boolean) : [];
    let newSelections: string[];
    if (checked) {
      newSelections = [...currentSelections, optionText];
    } else {
      newSelections = currentSelections.filter((s) => s !== optionText);
    }
    const answer = newSelections.join(', ');
    handleAnswerChange(questionId, answer);
  };

  const handleSubmit = useCallback(() => {
    setConfirmOpen(false);
    submitExamMutation.mutate();
  }, []);

  if (examLoading || questionsLoading) {
    return (
      <div className="flex items-center justify-center py-16">
        <CircularProgress />
      </div>
    );
  }

  if (!exam || sortedQuestions.length === 0) {
    return (
      <div className="text-center py-16 text-sm" style={{ color: '#6B7280' }}>
        {!exam ? 'Exam not found.' : 'No questions available.'}
      </div>
    );
  }

  const quizTitle = exam.quiz?.title ?? `Quiz ${exam.quizId}`;
  const totalQuestions = sortedQuestions.length;
  const answeredCount = Object.keys(localAnswers).filter((id) =>
    sortedQuestions.some((q) => q.id === parseInt(id)) && localAnswers[parseInt(id)]
  ).length;

  return (
    <div className="max-w-3xl mx-auto">
      {/* Header */}
      <div className="flex items-center justify-between mb-4">
        <h1 className="text-xl font-bold" style={{ color: '#211A1E' }}>{quizTitle}</h1>
        {timeLeft !== null && (
          <div
            className="flex items-center gap-2 px-3 py-1.5 rounded-lg font-mono text-sm font-semibold"
            style={{
              backgroundColor: timeLeft < 300 ? '#FEF2F2' : '#EBF2FA',
              color: timeLeft < 300 ? '#EF4444' : '#314CB6',
              border: `1px solid ${timeLeft < 300 ? '#FECACA' : '#314CB6'}`,
            }}
          >
            Time remaining: {formatTime(timeLeft)}
          </div>
        )}
      </div>

      {/* Question card */}
      {currentQuestion && (
        <div className="bg-white rounded-xl shadow-sm p-6 mb-4" style={{ border: '1px solid #E5E7EB' }}>
          <p className="text-sm font-medium mb-4" style={{ color: '#6B7280' }}>
            Question {currentIndex + 1} of {totalQuestions}
          </p>
          <p className="text-base font-semibold mb-6" style={{ color: '#211A1E' }}>
            {currentQuestion.prompt}
          </p>

          {/* Single choice / True or False */}
          {(currentQuestion.typeId === QUESTION_TYPES.SINGLE_CHOICE ||
            currentQuestion.typeId === QUESTION_TYPES.TRUE_OR_FALSE) && (
            <div className="flex flex-col gap-2">
              {parseOptions(currentQuestion.properties).map((opt, idx) => {
                const isSelected = localAnswers[currentQuestion.id] === opt.text;
                return (
                  <label
                    key={idx}
                    className="flex items-center gap-3 p-3 rounded-lg cursor-pointer transition-colors"
                    style={{
                      border: `1px solid ${isSelected ? '#314CB6' : '#E5E7EB'}`,
                      backgroundColor: isSelected ? '#EBF2FA' : '#fff',
                    }}
                  >
                    <input
                      type="radio"
                      name={`q-${currentQuestion.id}`}
                      value={opt.text}
                      checked={isSelected}
                      onChange={() => handleAnswerChange(currentQuestion.id, opt.text)}
                      className="accent-blue-600"
                    />
                    <span className="text-sm" style={{ color: '#211A1E' }}>{opt.text}</span>
                  </label>
                );
              })}
            </div>
          )}

          {/* Multiple choice */}
          {currentQuestion.typeId === QUESTION_TYPES.MULTIPLE_CHOICE && (
            <div className="flex flex-col gap-2">
              {parseOptions(currentQuestion.properties).map((opt, idx) => {
                const current = localAnswers[currentQuestion.id] ?? '';
                const isChecked = current.split(',').map((s) => s.trim()).includes(opt.text);
                return (
                  <label
                    key={idx}
                    className="flex items-center gap-3 p-3 rounded-lg cursor-pointer transition-colors"
                    style={{
                      border: `1px solid ${isChecked ? '#314CB6' : '#E5E7EB'}`,
                      backgroundColor: isChecked ? '#EBF2FA' : '#fff',
                    }}
                  >
                    <input
                      type="checkbox"
                      value={opt.text}
                      checked={isChecked}
                      onChange={(e) =>
                        handleMultipleChoiceChange(currentQuestion.id, opt.text, e.target.checked)
                      }
                      className="accent-blue-600"
                    />
                    <span className="text-sm" style={{ color: '#211A1E' }}>{opt.text}</span>
                  </label>
                );
              })}
            </div>
          )}

          {/* Text-based answers */}
          {[QUESTION_TYPES.FILL_IN_THE_BLANK, QUESTION_TYPES.SHORT_ANSWER, QUESTION_TYPES.ESSAY].includes(
            currentQuestion.typeId
          ) && (
            <textarea
              className="w-full p-3 rounded-lg text-sm resize-none outline-none focus:ring-2"
              style={{
                border: '1px solid #E5E7EB',
                color: '#211A1E',
                minHeight: currentQuestion.typeId === QUESTION_TYPES.ESSAY ? 160 : 80,
              }}
              placeholder="Type your answer here..."
              value={localAnswers[currentQuestion.id] ?? ''}
              onChange={(e) => handleAnswerChange(currentQuestion.id, e.target.value)}
            />
          )}
        </div>
      )}

      {/* Navigation */}
      <div className="flex items-center justify-between mb-4">
        <Button
          variant="outlined"
          startIcon={<ChevronLeft size={16} />}
          disabled={currentIndex === 0}
          onClick={() => setCurrentIndex((i) => i - 1)}
          sx={{ borderColor: '#E5E7EB', color: '#211A1E' }}
        >
          Previous
        </Button>

        {/* Dot navigation */}
        <div className="flex items-center gap-1 flex-wrap justify-center max-w-sm">
          {sortedQuestions.map((q, idx) => {
            const isAnswered = !!(localAnswers[q.id]);
            const isCurrent = idx === currentIndex;
            return (
              <button
                key={q.id}
                onClick={() => setCurrentIndex(idx)}
                className="w-7 h-7 rounded-full text-xs font-semibold transition-colors"
                style={{
                  backgroundColor: isCurrent ? '#314CB6' : isAnswered ? '#10B981' : '#E5E7EB',
                  color: isCurrent || isAnswered ? '#fff' : '#6B7280',
                  border: isCurrent ? '2px solid #314CB6' : '2px solid transparent',
                }}
              >
                {idx + 1}
              </button>
            );
          })}
        </div>

        <Button
          variant="outlined"
          endIcon={<ChevronRight size={16} />}
          disabled={currentIndex === totalQuestions - 1}
          onClick={() => setCurrentIndex((i) => i + 1)}
          sx={{ borderColor: '#E5E7EB', color: '#211A1E' }}
        >
          Next
        </Button>
      </div>

      <div className="flex justify-end">
        <Button
          variant="contained"
          color="error"
          onClick={() => setConfirmOpen(true)}
          disabled={submitExamMutation.isLoading}
        >
          Submit Exam
        </Button>
      </div>

      <Dialog open={confirmOpen} onClose={() => setConfirmOpen(false)} maxWidth="xs" fullWidth>
        <DialogTitle>Submit Exam?</DialogTitle>
        <DialogContent>
          <p className="text-sm" style={{ color: '#211A1E' }}>
            You have answered {answeredCount} of {totalQuestions} questions. Are you sure you want to submit?
            You cannot change your answers after submitting.
          </p>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setConfirmOpen(false)} color="inherit">Cancel</Button>
          <Button
            variant="contained"
            color="error"
            disabled={submitExamMutation.isLoading}
            onClick={handleSubmit}
          >
            {submitExamMutation.isLoading ? 'Submitting...' : 'Yes, Submit'}
          </Button>
        </DialogActions>
      </Dialog>
    </div>
  );
};
