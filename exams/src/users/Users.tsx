import { useState } from 'react';
import { useForm } from 'react-hook-form';
import { useMutation } from 'react-query';
import { useCacheUpdate } from '@/hooks/useCacheUpdate';
import { toast } from 'react-toastify';
import { ioc } from '@/utils/ioc';
import * as chikexamsService from '@/services/chikexams.service';
import { enums, types } from '@/services/chikexams.service';
import { useUsers } from '@/services/chikexams.hooks';
import { CacheKeys } from '@/utils/cache-keys.utils';
import { Search, Plus, Edit, ChevronDown } from 'lucide-react';
import {
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  Button,
  TextField,
  Tabs,
  Tab,
  Menu,
  MenuItem,
  CircularProgress,
} from '@mui/material';

type UserTab = 'all' | 'teachers' | 'students';
type UserModalMode = 'create-teacher' | 'create-student' | 'edit';

type UserFormData = {
  username: string;
  password: string;
};

const getRoleLabel = (roles: types['UserRole'][] | null | undefined) => {
  if (!roles || roles.length === 0) return 'None';
  return roles
    .map((r) => {
      if (r === enums.UserRole.Admin) return 'Admin';
      if (r === enums.UserRole.Teacher) return 'Teacher';
      if (r === enums.UserRole.Student) return 'Student';
      return 'Unknown';
    })
    .join(', ');
};

export const Users = ({
  searchUsers = ioc((keys) => keys.searchUsers) || chikexamsService.searchUsers,
  createUser = ioc((keys) => keys.createUser) || chikexamsService.createUser,
  updateUser = ioc((keys) => keys.updateUser) || chikexamsService.updateUser,
}: {
  searchUsers?: typeof chikexamsService.searchUsers;
  createUser?: typeof chikexamsService.createUser;
  updateUser?: typeof chikexamsService.updateUser;
}) => {
  const usersCache = useCacheUpdate(CacheKeys.searchUsers);
  const [tab, setTab] = useState<UserTab>('all');
  const [search, setSearch] = useState('');
  const [modalMode, setModalMode] = useState<UserModalMode | null>(null);
  const [editingUser, setEditingUser] = useState<types['User'] | null>(null);
  const [anchorEl, setAnchorEl] = useState<null | HTMLElement>(null);

  const roleFilter =
    tab === 'teachers'
      ? enums.UserRole.Teacher
      : tab === 'students'
        ? enums.UserRole.Student
        : undefined;

  const { data: users, isLoading } = useUsers({
    params: {
      Username: search || undefined,
    },
    searchUsers,
  });

  const filteredUsers = (users ?? []).filter((u) => {
    if (tab === 'teachers') return u.roles?.includes(enums.UserRole.Teacher);
    if (tab === 'students') return u.roles?.includes(enums.UserRole.Student);
    return true;
  });

  const { register, handleSubmit, reset, formState: { errors } } = useForm<UserFormData>();

  const createMutation = useMutation({
    mutationFn: async (data: UserFormData) => {
      const role =
        modalMode === 'create-teacher' ? enums.UserRole.Teacher : enums.UserRole.Student;
      return await createUser({ username: data.username, password: data.password, roles: [role] });
    },
    onSuccess: () => {
      toast.success('User created successfully');
      usersCache.invalidateAndRefetch();
      closeModal();
    },
    onError: () => {
      toast.error('Failed to create user');
    },
  });

  const updateMutation = useMutation({
    mutationFn: async (data: UserFormData) => {
      if (!editingUser) return;
      return await updateUser(editingUser.id, { username: data.username });
    },
    onSuccess: () => {
      toast.success('User updated successfully');
      usersCache.invalidateAndRefetch();
      closeModal();
    },
    onError: () => {
      toast.error('Failed to update user');
    },
  });

  const closeModal = () => {
    setModalMode(null);
    setEditingUser(null);
    reset();
  };

  const openEdit = (user: types['User']) => {
    setEditingUser(user);
    setModalMode('edit');
    reset({ username: user.username ?? '' });
  };

  const onSubmit = (data: UserFormData) => {
    if (modalMode === 'edit') {
      updateMutation.mutate(data);
    } else {
      createMutation.mutate(data);
    }
  };

  const isSubmitting = createMutation.isLoading || updateMutation.isLoading;

  const modalTitle =
    modalMode === 'create-teacher'
      ? 'New Teacher'
      : modalMode === 'create-student'
        ? 'New Student'
        : 'Edit User';

  const roleForModal =
    modalMode === 'create-teacher'
      ? 'Teacher'
      : modalMode === 'create-student'
        ? 'Student'
        : getRoleLabel(editingUser?.roles);

  return (
    <div>
      <div className="flex items-center justify-between mb-6">
        <h1 className="text-2xl font-bold" style={{ color: '#211A1E' }}>
          Users
        </h1>
        <div>
          <Button
            variant="contained"
            endIcon={<ChevronDown size={16} />}
            startIcon={<Plus size={16} />}
            onClick={(e) => setAnchorEl(e.currentTarget)}
            sx={{ backgroundColor: '#314CB6', '&:hover': { backgroundColor: '#2a3f9e' } }}
          >
            New User
          </Button>
          <Menu
            anchorEl={anchorEl}
            open={Boolean(anchorEl)}
            onClose={() => setAnchorEl(null)}
          >
            <MenuItem
              onClick={() => {
                setAnchorEl(null);
                setModalMode('create-teacher');
                reset({ username: '', password: '' });
              }}
            >
              New Teacher
            </MenuItem>
            <MenuItem
              onClick={() => {
                setAnchorEl(null);
                setModalMode('create-student');
                reset({ username: '', password: '' });
              }}
            >
              New Student
            </MenuItem>
          </Menu>
        </div>
      </div>

      <div className="bg-white rounded-xl shadow-sm" style={{ border: '1px solid #E5E7EB' }}>
        <div className="flex items-center justify-between px-4 pt-4 pb-2">
          <Tabs
            value={tab}
            onChange={(_, val) => setTab(val)}
            sx={{ '& .MuiTab-root': { textTransform: 'none', fontSize: '0.875rem' } }}
          >
            <Tab label="All" value="all" />
            <Tab label="Teachers" value="teachers" />
            <Tab label="Students" value="students" />
          </Tabs>
          <div className="flex items-center gap-2 px-3 py-1.5 rounded-lg" style={{ border: '1px solid #E5E7EB', backgroundColor: '#F9FAFB' }}>
            <Search size={16} style={{ color: '#6B7280' }} />
            <input
              type="text"
              placeholder="Search..."
              value={search}
              onChange={(e) => setSearch(e.target.value)}
              className="text-sm outline-none bg-transparent"
              style={{ color: '#211A1E', width: 160 }}
            />
          </div>
        </div>

        <div className="overflow-x-auto">
          <table className="w-full">
            <thead>
              <tr style={{ borderTop: '1px solid #E5E7EB', borderBottom: '1px solid #E5E7EB' }}>
                <th className="px-4 py-3 text-left text-xs font-semibold uppercase" style={{ color: '#6B7280' }}>Username</th>
                <th className="px-4 py-3 text-left text-xs font-semibold uppercase" style={{ color: '#6B7280' }}>Roles</th>
                <th className="px-4 py-3 text-left text-xs font-semibold uppercase" style={{ color: '#6B7280' }}>Created</th>
                <th className="px-4 py-3 text-left text-xs font-semibold uppercase" style={{ color: '#6B7280' }}>Actions</th>
              </tr>
            </thead>
            <tbody>
              {isLoading ? (
                <tr>
                  <td colSpan={4} className="px-4 py-8 text-center">
                    <CircularProgress size={24} />
                  </td>
                </tr>
              ) : filteredUsers.length === 0 ? (
                <tr>
                  <td colSpan={4} className="px-4 py-8 text-center text-sm" style={{ color: '#6B7280' }}>
                    No users found
                  </td>
                </tr>
              ) : (
                filteredUsers.map((user) => (
                  <tr
                    key={user.id}
                    style={{ borderBottom: '1px solid #F3F4F6' }}
                    className="hover:bg-slate-50 transition-colors"
                  >
                    <td className="px-4 py-3 text-sm font-medium" style={{ color: '#211A1E' }}>
                      {user.username}
                    </td>
                    <td className="px-4 py-3 text-sm" style={{ color: '#6B7280' }}>
                      {getRoleLabel(user.roles)}
                    </td>
                    <td className="px-4 py-3 text-sm" style={{ color: '#6B7280' }}>
                      {new Date(user.createdAt).toLocaleDateString()}
                    </td>
                    <td className="px-4 py-3">
                      <button
                        className="flex items-center gap-1 text-xs px-2 py-1 rounded"
                        style={{ color: '#314CB6', border: '1px solid #314CB6' }}
                        onClick={() => openEdit(user)}
                      >
                        <Edit size={12} />
                        Edit
                      </button>
                    </td>
                  </tr>
                ))
              )}
            </tbody>
          </table>
        </div>
      </div>

      <Dialog open={modalMode !== null} onClose={closeModal} maxWidth="xs" fullWidth>
        <form onSubmit={handleSubmit(onSubmit)}>
          <DialogTitle>{modalTitle}</DialogTitle>
          <DialogContent className="flex flex-col gap-4 pt-2">
            <TextField
              label="Username"
              fullWidth
              size="small"
              {...register('username', { required: 'Username is required' })}
              error={!!errors.username}
              helperText={errors.username?.message}
              sx={{ mt: 1 }}
            />
            {modalMode !== 'edit' && (
              <TextField
                label="Password"
                type="password"
                fullWidth
                size="small"
                {...register('password', { required: 'Password is required' })}
                error={!!errors.password}
                helperText={errors.password?.message}
              />
            )}
            <TextField
              label="Role"
              fullWidth
              size="small"
              value={roleForModal}
              InputProps={{ readOnly: true }}
            />
          </DialogContent>
          <DialogActions>
            <Button onClick={closeModal} color="inherit">Cancel</Button>
            <Button
              type="submit"
              variant="contained"
              disabled={isSubmitting}
              sx={{ backgroundColor: '#314CB6', '&:hover': { backgroundColor: '#2a3f9e' } }}
            >
              {isSubmitting ? 'Saving...' : 'Save'}
            </Button>
          </DialogActions>
        </form>
      </Dialog>
    </div>
  );
};
