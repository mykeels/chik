import { useEffect, useState } from 'react';
import { useForm, useFieldArray, Controller } from 'react-hook-form';
import { types } from '@/services/chikexams.service';
import {
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  Button,
  TextField,
  Select,
  MenuItem,
  FormControl,
  InputLabel,
  Checkbox,
  Radio,
  IconButton,
} from '@mui/material';
import { Plus, X } from 'lucide-react';

export const QUESTION_TYPES = {
  SINGLE_CHOICE: 1,
  MULTIPLE_CHOICE: 2,
  TRUE_OR_FALSE: 3,
  FILL_IN_THE_BLANK: 4,
  SHORT_ANSWER: 5,
  ESSAY: 6,
} as const;

const QUESTION_TYPE_LABELS: Record<number, string> = {
  1: 'Single Choice',
  2: 'Multiple Choice',
  3: 'True or False',
  4: 'Fill in the Blank',
  5: 'Short Answer',
  6: 'Essay',
};

const isChoiceBased = (typeId: number) =>
  [QUESTION_TYPES.SINGLE_CHOICE, QUESTION_TYPES.MULTIPLE_CHOICE, QUESTION_TYPES.TRUE_OR_FALSE].includes(typeId as never);

type Option = { text: string; isCorrect: boolean };

type QuestionFormData = {
  prompt: string;
  typeId: number;
  score: number;
  options: Option[];
};

type Props = {
  open: boolean;
  onClose: () => void;
  onSave: (data: {
    prompt: string;
    typeId: number;
    score: number;
    properties: string | undefined;
    order?: number;
  }) => void;
  initialData?: Partial<types['QuizQuestion']>;
  existingCount?: number;
  isLoading?: boolean;
};

export const QuestionModal = ({ open, onClose, onSave, initialData, existingCount = 0, isLoading }: Props) => {
  const parseOptions = (): Option[] => {
    if (initialData?.properties) {
      try {
        const parsed = typeof initialData.properties === 'string'
          ? JSON.parse(initialData.properties as string)
          : initialData.properties;
        if (parsed?.options) return parsed.options;
      } catch {
        // ignore
      }
    }
    return [];
  };

  const { register, handleSubmit, watch, control, reset, setValue, formState: { errors } } = useForm<QuestionFormData>({
    defaultValues: {
      prompt: initialData?.prompt ?? '',
      typeId: initialData?.typeId ?? QUESTION_TYPES.SINGLE_CHOICE,
      score: initialData?.score ?? 5,
      options: parseOptions(),
    },
  });

  const { fields, append, remove, replace } = useFieldArray({ control, name: 'options' });
  const typeId = watch('typeId');

  useEffect(() => {
    if (!open) return;
    const opts = parseOptions();
    reset({
      prompt: initialData?.prompt ?? '',
      typeId: initialData?.typeId ?? QUESTION_TYPES.SINGLE_CHOICE,
      score: initialData?.score ?? 5,
      options: opts,
    });
  }, [open, initialData]);

  // Auto-populate True/False options when type changes
  useEffect(() => {
    if (typeId === QUESTION_TYPES.TRUE_OR_FALSE) {
      replace([
        { text: 'True', isCorrect: false },
        { text: 'False', isCorrect: false },
      ]);
    } else if (!isChoiceBased(typeId)) {
      replace([]);
    }
  }, [typeId]);

  const handleCorrectChange = (index: number, checked: boolean) => {
    if (typeId === QUESTION_TYPES.SINGLE_CHOICE || typeId === QUESTION_TYPES.TRUE_OR_FALSE) {
      // single correct
      fields.forEach((_, i) => {
        setValue(`options.${i}.isCorrect`, i === index ? checked : false);
      });
    } else {
      setValue(`options.${index}.isCorrect`, checked);
    }
  };

  const onSubmit = (data: QuestionFormData) => {
    let properties: string | undefined;
    if (isChoiceBased(data.typeId)) {
      properties = JSON.stringify({ options: data.options });
    }
    onSave({
      prompt: data.prompt,
      typeId: data.typeId,
      score: Number(data.score),
      properties,
      order: existingCount + 1,
    });
  };

  return (
    <Dialog open={open} onClose={onClose} maxWidth="sm" fullWidth>
      <form onSubmit={handleSubmit(onSubmit)}>
        <DialogTitle>{initialData?.id ? 'Edit Question' : 'Add Question'}</DialogTitle>
        <DialogContent className="flex flex-col gap-4" sx={{ pt: 2 }}>
          <TextField
            label="Question Prompt"
            multiline
            rows={3}
            fullWidth
            {...register('prompt', { required: 'Prompt is required' })}
            error={!!errors.prompt}
            helperText={errors.prompt?.message}
            sx={{ mt: 1 }}
          />

          <div className="flex gap-4">
            <FormControl size="small" sx={{ flex: 1 }}>
              <InputLabel>Question Type</InputLabel>
              <Controller
                control={control}
                name="typeId"
                render={({ field }) => (
                  <Select label="Question Type" {...field}>
                    {Object.entries(QUESTION_TYPE_LABELS).map(([id, label]) => (
                      <MenuItem key={id} value={Number(id)}>{label}</MenuItem>
                    ))}
                  </Select>
                )}
              />
            </FormControl>

            <TextField
              label="Score"
              type="number"
              size="small"
              sx={{ width: 100 }}
              {...register('score', { required: true, min: 0 })}
              error={!!errors.score}
            />
          </div>

          {isChoiceBased(typeId) && (
            <div>
              <div className="flex items-center justify-between mb-2">
                <span className="text-sm font-medium" style={{ color: '#211A1E' }}>Options</span>
                {typeId !== QUESTION_TYPES.TRUE_OR_FALSE && (
                  <button
                    type="button"
                    className="flex items-center gap-1 text-xs px-2 py-1 rounded"
                    style={{ color: '#314CB6', border: '1px solid #314CB6' }}
                    onClick={() => append({ text: '', isCorrect: false })}
                  >
                    <Plus size={12} />
                    Add Option
                  </button>
                )}
              </div>
              <div className="flex flex-col gap-2">
                {fields.map((field, index) => (
                  <div key={field.id} className="flex items-center gap-2">
                    {typeId === QUESTION_TYPES.MULTIPLE_CHOICE ? (
                      <Controller
                        control={control}
                        name={`options.${index}.isCorrect`}
                        render={({ field: f }) => (
                          <Checkbox
                            size="small"
                            checked={f.value}
                            onChange={(e) => handleCorrectChange(index, e.target.checked)}
                          />
                        )}
                      />
                    ) : (
                      <Controller
                        control={control}
                        name={`options.${index}.isCorrect`}
                        render={({ field: f }) => (
                          <Radio
                            size="small"
                            checked={f.value}
                            onChange={(e) => handleCorrectChange(index, e.target.checked)}
                          />
                        )}
                      />
                    )}
                    <TextField
                      size="small"
                      fullWidth
                      placeholder={`Option ${index + 1}`}
                      {...register(`options.${index}.text`, { required: true })}
                      disabled={typeId === QUESTION_TYPES.TRUE_OR_FALSE}
                    />
                    {typeId !== QUESTION_TYPES.TRUE_OR_FALSE && (
                      <IconButton size="small" onClick={() => remove(index)}>
                        <X size={14} />
                      </IconButton>
                    )}
                  </div>
                ))}
                {fields.length === 0 && isChoiceBased(typeId) && typeId !== QUESTION_TYPES.TRUE_OR_FALSE && (
                  <p className="text-xs" style={{ color: '#6B7280' }}>
                    Click "+ Add Option" to add answer choices.
                  </p>
                )}
              </div>
            </div>
          )}
        </DialogContent>
        <DialogActions>
          <Button onClick={onClose} color="inherit">Cancel</Button>
          <Button
            type="submit"
            variant="contained"
            disabled={isLoading}
            sx={{ backgroundColor: '#314CB6', '&:hover': { backgroundColor: '#2a3f9e' } }}
          >
            {isLoading ? 'Saving...' : 'Save Question'}
          </Button>
        </DialogActions>
      </form>
    </Dialog>
  );
};
