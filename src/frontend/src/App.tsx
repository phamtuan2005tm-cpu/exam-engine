import { useState } from 'react';
import { LoginForm } from './components/auth/LoginForm';
import { RegisterForm } from './components/auth/RegisterForm';
import { LogOut } from 'lucide-react';

export default function App() {
  const [view, setView] = useState<'login' | 'register'>('login');
  const [currentUser, setCurrentUser] = useState<string | null>(null);

  const handleLogout = () => {
    localStorage.removeItem('accessToken');
    localStorage.removeItem('refreshToken');
    setCurrentUser(null);
  };

  return (
    <main className="flex min-h-screen items-center justify-center bg-slate-100 p-4">
      {currentUser ? (
        <div className="w-full max-w-md p-8 text-center bg-white rounded-2xl shadow-xl space-y-4">
          <h2 className="text-2xl font-bold text-slate-800">Xin chào, {currentUser}!</h2>
          <p className="text-slate-600">Bạn đã đăng nhập thành công vào ExamEngine.</p>
          <button
            onClick={handleLogout}
            className="flex items-center justify-center w-full py-2.5 px-4 font-medium text-white transition bg-red-600 rounded-lg hover:bg-red-700"
          >
            <LogOut className="w-4 h-4 mr-2" /> Đăng xuất
          </button>
        </div>
      ) : view === 'login' ? (
        <LoginForm
          onSwitchToRegister={() => setView('register')}
          onLoginSuccess={(name) => setCurrentUser(name)}
        />
      ) : (
        <RegisterForm onSwitchToLogin={() => setView('login')} />
      )}
    </main>
  );
}