import React, { useState } from 'react';
import { authApi } from '../../api/authApi';
import { LogIn, Mail, Lock, AlertCircle, Loader2 } from 'lucide-react';

interface LoginFormProps {
  onSwitchToRegister: () => void;
  onLoginSuccess: (fullName: string) => void;
}

export const LoginForm: React.FC<LoginFormProps> = ({ onSwitchToRegister, onLoginSuccess }) => {
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [loading, setLoading] = useState(false);
  const [errorMessage, setErrorMessage] = useState('');

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setErrorMessage('');
    setLoading(true);

    try {
      const response = await authApi.login({ email, password });
      if (response.success && response.data) {
        // Lưu token vào bộ nhớ để apiClient tự gắn vào các request sau
        localStorage.setItem('accessToken', response.data.accessToken);
        localStorage.setItem('refreshToken', response.data.refreshToken);
        onLoginSuccess(response.data.fullName);
      } else {
        setErrorMessage(response.message || 'Đăng nhập không thành công.');
      }
    } catch (error: any) {
      setErrorMessage(error.response?.data?.message || 'Có lỗi kết nối đến máy chủ.');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="w-full max-w-md p-8 space-y-6 bg-white rounded-2xl shadow-xl border border-slate-100">
      <div className="text-center space-y-2">
        <h2 className="text-3xl font-extrabold text-slate-800">Đăng Nhập</h2>
        <p className="text-sm text-slate-500">Hệ thống thi trực tuyến ExamEngine</p>
      </div>

      {errorMessage && (
        <div className="flex items-center gap-2 p-3 text-sm text-red-600 bg-red-50 border border-red-200 rounded-lg">
          <AlertCircle className="w-4 h-4 shrink-0" />
          <span>{errorMessage}</span>
        </div>
      )}

      <form onSubmit={handleSubmit} className="space-y-4">
        <div>
          <label className="block mb-1 text-sm font-medium text-slate-700">Email</label>
          <div className="relative">
            <Mail className="absolute w-5 h-5 text-slate-400 -translate-y-1/2 left-3 top-1/2" />
            <input
              type="email"
              required
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              placeholder="name@example.com"
              className="w-full py-2.5 pl-10 pr-4 text-sm border rounded-lg border-slate-300 focus:outline-none focus:ring-2 focus:ring-indigo-500"
            />
          </div>
        </div>

        <div>
          <label className="block mb-1 text-sm font-medium text-slate-700">Mật khẩu</label>
          <div className="relative">
            <Lock className="absolute w-5 h-5 text-slate-400 -translate-y-1/2 left-3 top-1/2" />
            <input
              type="password"
              required
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              placeholder="••••••••"
              className="w-full py-2.5 pl-10 pr-4 text-sm border rounded-lg border-slate-300 focus:outline-none focus:ring-2 focus:ring-indigo-500"
            />
          </div>
        </div>

        <button
          type="submit"
          disabled={loading}
          className="flex items-center justify-center w-full py-2.5 font-medium text-white transition bg-indigo-600 rounded-lg hover:bg-indigo-700 disabled:opacity-50"
        >
          {loading ? <Loader2 className="w-5 h-5 animate-spin" /> : <><LogIn className="w-4 h-4 mr-2" /> Đăng nhập</>}
        </button>
      </form>

      <div className="text-sm text-center text-slate-600">
        Chưa có tài khoản?{' '}
        <button
          onClick={onSwitchToRegister}
          className="font-medium text-indigo-600 hover:underline"
        >
          Đăng ký ngay
        </button>
      </div>
    </div>
  );
};