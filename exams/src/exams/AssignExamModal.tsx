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
} from '@mui/material';

type AssignExamFormData = {
  userId: number | '';
  quizId: number | '';
};

type Props = {
  open: boolean;
  onClose: () => void;
  onAssign: (userId: number, quizId: number) => void;
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
    defaultValues: { userId: '', quizId: '' },
  });

  const { data: users } = useUsers({ params: { IncludeRoles: true }, searchUsers });
  const { data: quizzes } = useQuizzes({ searchQuizzes });

  const students = (users ?? []).filter((u) => u.roles?.includes(enums.UserRole.Student));

  const onSubmit = (data: AssignExamFormData) => {
    if (data.userId && data.quizId) {
      onAssign(Number(data.userId), Number(data.quizId));
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
          <FormControl fullWidth size="small" error={!!errors.userId} sx={{ mt: 1 }}>
            <InputLabel>Student</InputLabel>
            <Controller
              control={control}
              name="userId"
              rules={{ required: 'Student is required' }}
              render={({ field }) => (
                <Select label="Student" {...field}>
                  <MenuItem value="" disabled>Select student</MenuItem>
                  {students.map((s) => (
                    <MenuItem key={s.id} value={s.id}>{s.username}</MenuItem>
                  ))}
                </Select>
              )}
            />
          </FormControl>

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
