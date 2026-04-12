import { useState } from 'react';
import { useForm, Controller } from 'react-hook-form';
import { ioc } from '@/utils/ioc';
import * as chikexamsService from '@/services/chikexams.service';
import { enums } from '@/services/chikexams.service';
import { useUsers, useQuizzes, useClasses } from '@/services/chikexams.hooks';
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
  Tab,
  Tabs,
  FormHelperText,
} from '@mui/material';

type AssignMode = 'by-class' | 'by-students';

type ByClassFormData = {
  classId: number | '';
  quizId: number | '';
};

type ByStudentsFormData = {
  userIds: number[];
  quizId: number | '';
};

type Props = {
  open: boolean;
  onClose: () => void;
  onAssignToClass: (classId: number, quizId: number) => void;
  onAssignToStudents: (userIds: number[], quizId: number) => void;
  isLoading?: boolean;
  searchUsers?: typeof chikexamsService.searchUsers;
  searchQuizzes?: typeof chikexamsService.searchQuizzes;
  listClasses?: typeof chikexamsService.listClasses;
};

export const AssignExamModal = ({
  open,
  onClose,
  onAssignToClass,
  onAssignToStudents,
  isLoading,
  searchUsers = ioc((keys) => keys.searchUsers) || chikexamsService.searchUsers,
  searchQuizzes = ioc((keys) => keys.searchQuizzes) || chikexamsService.searchQuizzes,
  listClasses = ioc((keys) => keys.listClasses) || chikexamsService.listClasses,
}: Props) => {
  const [mode, setMode] = useState<AssignMode>('by-class');

  const byClassForm = useForm<ByClassFormData>({
    defaultValues: { classId: '', quizId: '' },
  });

  const byStudentsForm = useForm<ByStudentsFormData>({
    defaultValues: { userIds: [], quizId: '' },
  });

  const { data: users } = useUsers({ params: { Roles: enums.UserRole.Student }, searchUsers });
  const { data: quizzes } = useQuizzes({ searchQuizzes });
  const { data: classes = [] } = useClasses({ listClasses });

  const students = (users ?? []).filter((u) => u.roles?.includes(enums.UserRole.Student));

  const onSubmitByClass = (data: ByClassFormData) => {
    if (data.classId && data.quizId) {
      onAssignToClass(Number(data.classId), Number(data.quizId));
    }
  };

  const onSubmitByStudents = (data: ByStudentsFormData) => {
    if (data.userIds.length > 0 && data.quizId) {
      onAssignToStudents(data.userIds, Number(data.quizId));
    }
  };

  const handleClose = () => {
    byClassForm.reset();
    byStudentsForm.reset();
    onClose();
  };

  const QuizSelect = ({ control, errors }: { control: any; errors: any }) => (
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
      {errors.quizId && <FormHelperText>{errors.quizId.message}</FormHelperText>}
    </FormControl>
  );

  return (
    <Dialog open={open} onClose={handleClose} maxWidth="xs" fullWidth>
      <DialogTitle>Assign Exam</DialogTitle>

      <Tabs
        value={mode}
        onChange={(_, val) => setMode(val)}
        sx={{ px: 3, borderBottom: '1px solid #E5E7EB', '& .MuiTab-root': { textTransform: 'none', fontSize: '0.875rem' } }}
      >
        <Tab label="By Class" value="by-class" />
        <Tab label="By Students" value="by-students" />
      </Tabs>

      {mode === 'by-class' ? (
        <form onSubmit={byClassForm.handleSubmit(onSubmitByClass)}>
          <DialogContent className="flex flex-col gap-4" sx={{ pt: 2 }}>
            <FormControl fullWidth size="small" error={!!byClassForm.formState.errors.classId} sx={{ mt: 1 }}>
              <InputLabel>Class</InputLabel>
              <Controller
                control={byClassForm.control}
                name="classId"
                rules={{ required: 'Class is required' }}
                render={({ field }) => (
                  <Select label="Class" {...field}>
                    <MenuItem value="" disabled>Select class</MenuItem>
                    {classes.map((c) => (
                      <MenuItem key={c.id} value={c.id}>{c.name}</MenuItem>
                    ))}
                  </Select>
                )}
              />
              {byClassForm.formState.errors.classId && (
                <FormHelperText>{byClassForm.formState.errors.classId.message}</FormHelperText>
              )}
            </FormControl>
            <QuizSelect control={byClassForm.control} errors={byClassForm.formState.errors} />
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
      ) : (
        <form onSubmit={byStudentsForm.handleSubmit(onSubmitByStudents)}>
          <DialogContent className="flex flex-col gap-4" sx={{ pt: 2 }}>
            <Controller
              control={byStudentsForm.control}
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
                  onChange={(_, selected) => onChange(selected.map((s) => s.id))}
                  renderInput={(params) => (
                    <TextField
                      {...params}
                      name={name}
                      label="Students"
                      error={!!byStudentsForm.formState.errors.userIds}
                      helperText={byStudentsForm.formState.errors.userIds?.message as string | undefined}
                      inputRef={ref}
                      onBlur={onBlur}
                    />
                  )}
                  size="small"
                  sx={{ mt: 1 }}
                />
              )}
            />
            <QuizSelect control={byStudentsForm.control} errors={byStudentsForm.formState.errors} />
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
      )}
    </Dialog>
  );
};
