import { useState, useEffect } from 'react';
import { useNavigate, useParams, Link } from 'react-router';
import { useForm, Controller } from 'react-hook-form';
import { useMutation, useQueryClient } from 'react-query';
import { toast } from 'react-toastify';
import { ioc } from '@/utils/ioc';
import * as chikexamsService from '@/services/chikexams.service';
import { enums } from '@/services/chikexams.service';
import { useQuiz, useQuizQuestions, useUsers } from '@/services/chikexams.hooks';
import { CacheKeys } from '@/utils/cache-keys.utils';
import { useAuth } from '@/auth';
import { QuestionModal, QUESTION_TYPES } from './QuestionModal';
import { ChevronLeft, Plus, Edit, ArrowUp, ArrowDown } from 'lucide-react';
import {
  Button,
  TextField,
  FormControl,
  InputLabel,
  Select,
  MenuItem,
  CircularProgress,
  Tooltip,
  Switch,
} from '@mui/material';

type QuizFormData = {
  title: string;
  description: string;
  durationHrs: number;
  durationMins: number;
  examinerId: number | '';
};

const QUESTION_TYPE_LABELS: Record<number, string> = {
  1: 'Multiple Choice',
  2: 'Single Choice',
  3: 'Fill in the Blank',
  4: 'Essay',
  5: 'Short Answer',
  6: 'True or False',
};

const parseDurationToFields = (duration: string | null | undefined) => {
  if (!duration) return { hrs: 0, mins: 0 };
  const match = duration.match(/(\d+):(\d+):(\d+)/);
  if (match) {
    return { hrs: parseInt(match[1]), mins: parseInt(match[2]) };
  }
  return { hrs: 0, mins: 0 };
};

const fieldsToDuration = (hrs: number, mins: number): string | undefined => {
  if (!hrs && !mins) return undefined;
  return `${String(hrs).padStart(2, '0')}:${String(mins).padStart(2, '0')}:00`;
};

export const QuizEditor = ({
  searchQuizzes = ioc((keys) => keys.searchQuizzes) || chikexamsService.searchQuizzes,
  createQuiz = ioc((keys) => keys.createQuiz) || chikexamsService.createQuiz,
  updateQuiz = ioc((keys) => keys.updateQuiz) || chikexamsService.updateQuiz,
  getQuizQuestions: getQuizQuestionsFn = ioc((keys) => keys.getQuizQuestions) || chikexamsService.getQuizQuestions,
  createQuizQuestion = ioc((keys) => keys.createQuizQuestion) || chikexamsService.createQuizQuestion,
  updateQuizQuestion = ioc((keys) => keys.updateQuizQuestion) || chikexamsService.updateQuizQuestion,
  deactivateQuestion = ioc((keys) => keys.deactivateQuestion) || chikexamsService.deactivateQuestion,
  reactivateQuestion = ioc((keys) => keys.reactivateQuestion) || chikexamsService.reactivateQuestion,
  reorderQuestions = ioc((keys) => keys.reorderQuestions) || chikexamsService.reorderQuestions,
  searchUsers = ioc((keys) => keys.searchUsers) || chikexamsService.searchUsers,
}: {
  searchQuizzes?: typeof chikexamsService.searchQuizzes;
  createQuiz?: typeof chikexamsService.createQuiz;
  updateQuiz?: typeof chikexamsService.updateQuiz;
  getQuizQuestions?: typeof chikexamsService.getQuizQuestions;
  createQuizQuestion?: typeof chikexamsService.createQuizQuestion;
  updateQuizQuestion?: typeof chikexamsService.updateQuizQuestion;
  deactivateQuestion?: typeof chikexamsService.deactivateQuestion;
  reactivateQuestion?: typeof chikexamsService.reactivateQuestion;
  reorderQuestions?: typeof chikexamsService.reorderQuestions;
  searchUsers?: typeof chikexamsService.searchUsers;
}) => {
  const { id } = useParams<{ id?: string }>();
  const quizId = id ? parseInt(id) : undefined;
  const navigate = useNavigate();
  const queryClient = useQueryClient();
  const { profile } = useAuth();

  const isAdmin = profile?.roles?.includes(enums.UserRole.Admin) ?? false;
  const isNew = !quizId;

  const { data: quiz, isLoading: quizLoading } = useQuiz(quizId ?? 0);
  const { data: questions, isLoading: questionsLoading } = useQuizQuestions(quizId ?? 0, {
    getQuizQuestions: getQuizQuestionsFn,
  });

  const { data: teachers } = useUsers({
    params: { Roles: enums.UserRole.Teacher },
    searchUsers,
  });

  const teacherList = (teachers ?? []).filter((u) => u.roles?.includes(enums.UserRole.Teacher));

  const [questionModalOpen, setQuestionModalOpen] = useState(false);
  const [editingQuestion, setEditingQuestion] = useState<NonNullable<typeof questions>[number] | null>(null);

  const durationParsed = parseDurationToFields(quiz?.duration);

  const { register, handleSubmit, control, reset, formState: { errors } } = useForm<QuizFormData>({
    defaultValues: {
      title: '',
      description: '',
      durationHrs: 0,
      durationMins: 0,
      examinerId: '',
    },
  });

  useEffect(() => {
    if (quiz) {
      const { hrs, mins } = parseDurationToFields(quiz.duration);
      reset({
        title: quiz.title ?? '',
        description: quiz.description ?? '',
        durationHrs: hrs,
        durationMins: mins,
        examinerId: quiz.examinerId ?? '',
      });
    }
  }, [quiz]);

  const saveMutation = useMutation({
    mutationFn: async (data: QuizFormData) => {
      const duration = fieldsToDuration(Number(data.durationHrs), Number(data.durationMins));
      const body = {
        title: data.title,
        description: data.description ?? null,
        duration,
        examinerId: data.examinerId ? Number(data.examinerId) : undefined,
      };
      if (isNew) {
        return await createQuiz(body);
      } else {
        return await updateQuiz(quizId!, body);
      }
    },
    onSuccess: (result) => {
      toast.success(isNew ? 'Quiz created!' : 'Quiz saved!');
      queryClient.invalidateQueries(CacheKeys.searchQuizzes);
      if (isNew && result) {
        navigate(`/quizzes/${result.id}/edit`);
      } else {
        queryClient.invalidateQueries([CacheKeys.getQuiz, quizId]);
      }
    },
    onError: () => {
      toast.error('Failed to save quiz');
    },
  });

  const createQuestionMutation = useMutation({
    mutationFn: async (data: Parameters<typeof createQuizQuestion>[1]) => {
      return await createQuizQuestion(quizId!, data);
    },
    onSuccess: () => {
      toast.success('Question added');
      queryClient.invalidateQueries([CacheKeys.getQuizQuestions, quizId]);
      setQuestionModalOpen(false);
      setEditingQuestion(null);
    },
    onError: () => {
      toast.error('Failed to add question');
    },
  });

  const updateQuestionMutation = useMutation({
    mutationFn: async ({ id, data }: { id: number; data: Parameters<typeof updateQuizQuestion>[1] }) => {
      return await updateQuizQuestion(id, data);
    },
    onSuccess: () => {
      toast.success('Question updated');
      queryClient.invalidateQueries([CacheKeys.getQuizQuestions, quizId]);
      setQuestionModalOpen(false);
      setEditingQuestion(null);
    },
    onError: () => {
      toast.error('Failed to update question');
    },
  });

  const toggleMutation = useMutation({
    mutationFn: async ({ id, isActive }: { id: number; isActive: boolean }) => {
      if (isActive) {
        return await deactivateQuestion(id);
      } else {
        return await reactivateQuestion(id);
      }
    },
    onSuccess: () => {
      queryClient.invalidateQueries([CacheKeys.getQuizQuestions, quizId]);
    },
    onError: () => {
      toast.error('Failed to toggle question');
    },
  });

  const reorderMutation = useMutation({
    mutationFn: async (ids: number[]) => {
      return await reorderQuestions(quizId!, ids);
    },
    onSuccess: () => {
      queryClient.invalidateQueries([CacheKeys.getQuizQuestions, quizId]);
    },
  });

  const activeQuestions = (questions ?? []).filter((q) => q.isActive);
  const sortedQuestions = [...(questions ?? [])].sort((a, b) => a.order - b.order);

  const moveQuestion = (index: number, direction: 'up' | 'down') => {
    const sorted = [...sortedQuestions];
    const newIndex = direction === 'up' ? index - 1 : index + 1;
    if (newIndex < 0 || newIndex >= sorted.length) return;
    [sorted[index], sorted[newIndex]] = [sorted[newIndex], sorted[index]];
    reorderMutation.mutate(sorted.map((q) => q.id));
  };

  const onSaveQuestion = (data: {
    prompt: string;
    typeId: number;
    score: number;
    properties: string;
    order?: number;
  }) => {
    const body = {
      prompt: data.prompt,
      typeId: data.typeId,
      score: data.score,
      properties: data.properties,
      order: data.order ?? (questions?.length ?? 0) + 1,
    };
    if (editingQuestion) {
      updateQuestionMutation.mutate({ id: editingQuestion.id, data: body });
    } else {
      createQuestionMutation.mutate(body);
    }
  };

  if (quizLoading && !isNew) {
    return (
      <div className="flex items-center justify-center py-16">
        <CircularProgress />
      </div>
    );
  }

  return (
    <div>
      <div className="mb-4">
        <Link
          to="/quizzes"
          className="flex items-center gap-1 text-sm hover:underline"
          style={{ color: '#314CB6' }}
        >
          <ChevronLeft size={16} />
          Back to Quizzes
        </Link>
      </div>

      <div className="bg-white rounded-xl shadow-sm p-6 mb-6" style={{ border: '1px solid #E5E7EB' }}>
        <h2 className="text-lg font-bold mb-4" style={{ color: '#211A1E' }}>
          Quiz Details
        </h2>
        <form onSubmit={handleSubmit((data) => saveMutation.mutate(data))} className="flex flex-col gap-4">
          <TextField
            label="Title"
            fullWidth
            size="small"
            {...register('title', { required: 'Title is required' })}
            error={!!errors.title}
            helperText={errors.title?.message}
          />
          <TextField
            label="Description"
            fullWidth
            size="small"
            multiline
            rows={3}
            {...register('description', { required: 'Description is required' })}
          />
          <div className="flex gap-3 items-center">
            <TextField
              label="Duration (hrs)"
              type="number"
              size="small"
              sx={{ width: 140 }}
              {...register('durationHrs')}
              inputProps={{ min: 0, max: 24 }}
            />
            <TextField
              label="Duration (mins)"
              type="number"
              size="small"
              sx={{ width: 140 }}
              {...register('durationMins')}
              inputProps={{ min: 0, max: 59 }}
            />
            <span className="text-sm" style={{ color: '#6B7280' }}>(optional)</span>
          </div>
          {isAdmin && (
            <FormControl size="small" sx={{ maxWidth: 300 }}>
              <InputLabel>Examiner (Teacher)</InputLabel>
              <Controller
                control={control}
                name="examinerId"
                render={({ field }) => (
                  <Select label="Examiner (Teacher)" {...field}>
                    <MenuItem value="">None</MenuItem>
                    {teacherList.map((t) => (
                      <MenuItem key={t.id} value={t.id}>{t.username}</MenuItem>
                    ))}
                  </Select>
                )}
              />
            </FormControl>
          )}
          <div className="flex justify-end">
            <Button
              type="submit"
              variant="contained"
              disabled={saveMutation.isLoading}
              sx={{ backgroundColor: '#314CB6', '&:hover': { backgroundColor: '#2a3f9e' } }}
            >
              {saveMutation.isLoading ? 'Saving...' : 'Save Quiz'}
            </Button>
          </div>
        </form>
      </div>

      {!isNew && (
        <div className="bg-white rounded-xl shadow-sm p-6" style={{ border: '1px solid #E5E7EB' }}>
          <div className="flex items-center justify-between mb-4">
            <h2 className="text-lg font-bold" style={{ color: '#211A1E' }}>Questions</h2>
            <Button
              variant="outlined"
              startIcon={<Plus size={16} />}
              onClick={() => {
                setEditingQuestion(null);
                setQuestionModalOpen(true);
              }}
              sx={{ borderColor: '#314CB6', color: '#314CB6', '&:hover': { borderColor: '#2a3f9e' } }}
            >
              Add Question
            </Button>
          </div>

          {questionsLoading ? (
            <div className="flex items-center justify-center py-8">
              <CircularProgress size={24} />
            </div>
          ) : sortedQuestions.length === 0 ? (
            <p className="text-sm text-center py-8" style={{ color: '#6B7280' }}>
              No questions yet. Click "+ Add Question" to get started.
            </p>
          ) : (
            <div className="overflow-x-auto">
              <table className="w-full">
                <thead>
                  <tr style={{ borderBottom: '1px solid #E5E7EB' }}>
                    <th className="px-3 py-2 text-left text-xs font-semibold uppercase" style={{ color: '#6B7280' }}>#</th>
                    <th className="px-3 py-2 text-left text-xs font-semibold uppercase" style={{ color: '#6B7280' }}>Prompt</th>
                    <th className="px-3 py-2 text-left text-xs font-semibold uppercase" style={{ color: '#6B7280' }}>Type</th>
                    <th className="px-3 py-2 text-left text-xs font-semibold uppercase" style={{ color: '#6B7280' }}>Score</th>
                    <th className="px-3 py-2 text-left text-xs font-semibold uppercase" style={{ color: '#6B7280' }}>Actions</th>
                  </tr>
                </thead>
                <tbody>
                  {sortedQuestions.map((q, index) => (
                    <tr
                      key={q.id}
                      style={{
                        borderBottom: '1px solid #F3F4F6',
                        opacity: q.isActive ? 1 : 0.5,
                      }}
                      className="hover:bg-slate-50 transition-colors"
                    >
                      <td className="px-3 py-2 text-sm" style={{ color: '#6B7280' }}>{q.order}</td>
                      <td className="px-3 py-2 text-sm" style={{ color: '#211A1E', maxWidth: 300 }}>
                        <span className="line-clamp-2">{q.prompt}</span>
                      </td>
                      <td className="px-3 py-2 text-sm" style={{ color: '#6B7280' }}>
                        {QUESTION_TYPE_LABELS[q.typeId] ?? q.typeId}
                      </td>
                      <td className="px-3 py-2 text-sm" style={{ color: '#6B7280' }}>{q.score}</td>
                      <td className="px-3 py-2">
                        <div className="flex items-center gap-1">
                          <button
                            className="p-1 rounded hover:bg-slate-100"
                            onClick={() => moveQuestion(index, 'up')}
                            disabled={index === 0}
                            title="Move up"
                          >
                            <ArrowUp size={14} style={{ color: '#6B7280' }} />
                          </button>
                          <button
                            className="p-1 rounded hover:bg-slate-100"
                            onClick={() => moveQuestion(index, 'down')}
                            disabled={index === sortedQuestions.length - 1}
                            title="Move down"
                          >
                            <ArrowDown size={14} style={{ color: '#6B7280' }} />
                          </button>
                          <button
                            className="flex items-center gap-1 text-xs px-2 py-1 rounded"
                            style={{ color: '#314CB6', border: '1px solid #314CB6' }}
                            onClick={() => {
                              setEditingQuestion(q);
                              setQuestionModalOpen(true);
                            }}
                          >
                            <Edit size={12} />
                            Edit
                          </button>
                          <Tooltip title={q.isActive ? 'Deactivate' : 'Reactivate'}>
                            <Switch
                              size="small"
                              checked={q.isActive}
                              onChange={() => toggleMutation.mutate({ id: q.id, isActive: q.isActive })}
                            />
                          </Tooltip>
                        </div>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          )}
        </div>
      )}

      <QuestionModal
        open={questionModalOpen}
        onClose={() => {
          setQuestionModalOpen(false);
          setEditingQuestion(null);
        }}
        onSave={onSaveQuestion}
        initialData={editingQuestion ?? undefined}
        existingCount={questions?.length ?? 0}
        isLoading={createQuestionMutation.isLoading || updateQuestionMutation.isLoading}
      />
    </div>
  );
};
