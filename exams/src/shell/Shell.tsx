import { Outlet, NavLink, useNavigate } from 'react-router';
import { DocumentTitle } from './DocumentTitle';
import { useAuth } from '@/auth';
import { enums } from '@/services/chikexams.service';
import { BookOpen, Users, FileText, ClipboardList, ScrollText, LogOut, Key } from 'lucide-react';
import { useState } from 'react';
import { AppContexts } from '@/main.context';
import { AuthProvider } from '@/auth';

const navLinkClass = ({ isActive }: { isActive: boolean }) =>
  `flex items-center gap-2 px-3 py-2 rounded-lg text-sm font-medium transition-colors ${
    isActive
      ? 'text-white'
      : 'text-slate-300 hover:text-white hover:bg-white/10'
  }`;

const activeStyle = { backgroundColor: '#314CB6' };

export const ShellLayout = () => {
  const { profile, logout, isLoggingOut } = useAuth();
  const navigate = useNavigate();
  const [userMenuOpen, setUserMenuOpen] = useState(false);

  const isAdmin = profile?.roles?.includes(enums.UserRole.Admin) ?? false;
  const isTeacher = profile?.roles?.includes(enums.UserRole.Teacher) ?? false;
  const isStudent = profile?.roles?.includes(enums.UserRole.Student) ?? false;

  const handleLogout = () => {
    logout();
    navigate('/login');
  };

  return (
    <div className="flex flex-col h-screen" style={{ backgroundColor: '#EBF2FA' }}>
      <DocumentTitle />
      {/* Top bar */}
      <header
        className="flex items-center justify-between px-6 py-3 shadow-sm flex-shrink-0"
        style={{ backgroundColor: '#211A1E' }}
      >
        <div className="flex items-center gap-2">
          <div
            className="w-8 h-8 rounded-full flex items-center justify-center"
            style={{ backgroundColor: '#314CB6' }}
          >
            <BookOpen className="text-white" size={16} />
          </div>
          <span className="text-white font-bold text-lg">Chik.Exams</span>
        </div>

        <div className="flex items-center gap-3">
          <div className="relative">
            <button
              className="flex items-center gap-2 text-white text-sm px-3 py-1 rounded-lg hover:bg-white/10 transition-colors"
              onClick={() => setUserMenuOpen(!userMenuOpen)}
            >
              <span>{profile?.username ?? 'User'}</span>
              <span className="text-xs">▾</span>
            </button>
            {userMenuOpen && (
              <div
                className="absolute right-0 mt-1 w-48 bg-white rounded-lg shadow-lg py-1 z-50"
                style={{ border: '1px solid #E5E7EB' }}
              >
                <button
                  className="flex items-center gap-2 w-full px-4 py-2 text-sm hover:bg-slate-50 text-left"
                  style={{ color: '#211A1E' }}
                  onClick={() => {
                    setUserMenuOpen(false);
                    navigate('/settings/password');
                  }}
                >
                  <Key size={14} />
                  Change Password
                </button>
              </div>
            )}
          </div>
          <button
            className="flex items-center gap-2 text-white text-sm px-3 py-1 rounded-lg hover:bg-white/10 transition-colors"
            onClick={handleLogout}
            disabled={isLoggingOut}
          >
            <LogOut size={16} />
            Logout
          </button>
        </div>
      </header>

      <div className="flex flex-1 overflow-hidden">
        {/* Sidebar */}
        <aside
          className="w-56 flex-shrink-0 flex flex-col py-4 px-3 gap-1"
          style={{ backgroundColor: '#27272a' }}
        >
          {isAdmin && (
            <>
              <NavLink
                to="/users"
                className={navLinkClass}
                style={({ isActive }) => (isActive ? activeStyle : {})}
              >
                <Users size={16} />
                Users
              </NavLink>
              <NavLink
                to="/quizzes"
                className={navLinkClass}
                style={({ isActive }) => (isActive ? activeStyle : {})}
              >
                <FileText size={16} />
                Quizzes
              </NavLink>
              <NavLink
                to="/exams"
                className={navLinkClass}
                style={({ isActive }) => (isActive ? activeStyle : {})}
              >
                <ClipboardList size={16} />
                Exams
              </NavLink>
              <NavLink
                to="/audit-logs"
                className={navLinkClass}
                style={({ isActive }) => (isActive ? activeStyle : {})}
              >
                <ScrollText size={16} />
                Audit Logs
              </NavLink>
            </>
          )}
          {isTeacher && !isAdmin && (
            <>
              <NavLink
                to="/quizzes"
                className={navLinkClass}
                style={({ isActive }) => (isActive ? activeStyle : {})}
              >
                <FileText size={16} />
                Quizzes
              </NavLink>
              <NavLink
                to="/exams"
                className={navLinkClass}
                style={({ isActive }) => (isActive ? activeStyle : {})}
              >
                <ClipboardList size={16} />
                Exams
              </NavLink>
              <NavLink
                to="/users?role=students"
                className={navLinkClass}
                style={({ isActive }) => (isActive ? activeStyle : {})}
              >
                <Users size={16} />
                Students
              </NavLink>
            </>
          )}
          {isStudent && !isAdmin && !isTeacher && (
            <NavLink
              to="/my-exams"
              className={navLinkClass}
              style={({ isActive }) => (isActive ? activeStyle : {})}
            >
              <ClipboardList size={16} />
              My Exams
            </NavLink>
          )}
        </aside>

        {/* Main content */}
        <main className="flex-1 overflow-auto p-6">
          <Outlet />
        </main>
      </div>
    </div>
  );
};

export const Shell = () => {
  return (
    <AppContexts>
      <AuthProvider>
        <ShellLayout />
      </AuthProvider>
    </AppContexts>
  );
};
