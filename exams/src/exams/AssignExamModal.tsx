import { useForm, Controller } from 'react-hook-form';
import { ioc } from '@/utils/ioc';
import * as chikexamsService from '@/services/chikexams.service';
import { enums } from '@/services/chikexams.service';
import { useUsers, useQuizzes } from '@/services/chikexams.hooks';
import {
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  Button,
  FormControl,
  InputLabel,
  Select,
  MenuItem,
  Autocomplete,
  TextField,
} from '@mui/material';

type AssignExamFormData = {
  userIds: number[];
  quizId: number | '';
};

type Props = {
  open: boolean;
  onClose: () => void;
  onAssign: (userIds: number[], quizId: number) => void;
  isLoading?: boolean;
  searchUsers?: typeof chikexamsService.searchUsers;
  searchQuizzes?: typeof chikexamsService.searchQuizzes;
};

export const AssignExamModal = ({
  open,
  onClose,
  onAssign,
  isLoading,
  searchUsers = ioc((keys) => keys.searchUsers) || chikexamsService.searchUsers,
  searchQuizzes = ioc((keys) => keys.searchQuizzes) || chikexamsService.searchQuizzes,
}: Props) => {
  const { control, handleSubmit, reset, formState: { errors } } = useForm<AssignExamFormData>({
    defaultValues: { userIds: [], quizId: '' },
  });

  const { data: users } = useUsers({ params: { Roles: enums.UserRole.Student }, searchUsers });
  const { data: quizzes } = useQuizzes({ searchQuizzes });

  const students = (users ?? []).filter((u) => u.roles?.includes(enums.UserRole.Student));

  const onSubmit = (data: AssignExamFormData) => {
    if (data.userIds.length > 0 && data.quizId) {
      onAssign(data.userIds, Number(data.quizId));
    }
  };

  const handleClose = () => {
    reset();
    onClose();
  };

  return (
    <Dialog open={open} onClose={handleClose} maxWidth="xs" fullWidth>
      <form onSubmit={handleSubmit(onSubmit)}>
        <DialogTitle>Assign Exam</DialogTitle>
        <DialogContent className="flex flex-col gap-4" sx={{ pt: 2 }}>
          <Controller
            control={control}
            name="userIds"
            rules={{
              validate: (v: number[]) =>
                Array.isArray(v) && v.length > 0 ? true : 'Select at least one student',
            }}
            render={({ field: { onChange, value: selectedIds, ref, onBlur, name } }) => (
              <Autocomplete
                multiple
                id="assign-exam-students"
                options={students}
                getOptionLabel={(option) => option.username ?? ''}
                isOptionEqualToValue={(a, b) => a.id === b.id}
                value={selectedIds
                  .map((id) => students.find((s) => s.id === id))
                  .filter((s): s is (typeof students)[number] => s != null)}
                onChange={(_, selected) => {
                  onChange(selected.map((s) => s.id));
                }}
                renderInput={(params) => (
                  <TextField
                    {...params}
                    name={name}
                    label="Students"
                    error={!!errors.userIds}
                    helperText={errors.userIds?.message as string | undefined}
                    inputRef={ref}
                    onBlur={onBlur}
                  />
                )}
                size="small"
                sx={{ mt: 1 }}
              />
            )}
          />

          <FormControl fullWidth size="small" error={!!errors.quizId}>
            <InputLabel>Quiz</InputLabel>
            <Controller
              control={control}
              name="quizId"
              rules={{ required: 'Quiz is required' }}
              render={({ field }) => (
                <Select label="Quiz" {...field}>
                  <MenuItem value="" disabled>Select quiz</MenuItem>
                  {(quizzes ?? []).map((q) => (
                    <MenuItem key={q.id} value={q.id}>{q.title}</MenuItem>
                  ))}
                </Select>
              )}
            />
          </FormControl>
        </DialogContent>
        <DialogActions>
          <Button onClick={handleClose} color="inherit">Cancel</Button>
          <Button
            type="submit"
            variant="contained"
            disabled={isLoading}
            sx={{ backgroundColor: '#314CB6', '&:hover': { backgroundColor: '#2a3f9e' } }}
          >
            {isLoading ? 'Assigning...' : 'Assign'}
          </Button>
        </DialogActions>
      </form>
    </Dialog>
  );
};
