import React, { useState } from 'react';
import { authApi } from '../../api/authApi';
import { UserPlus, Mail, Lock, User, KeyRound, AlertCircle, CheckCircle2, Loader2 } from 'lucide-react';

interface RegisterFormProps {
  onSwitchToLogin: () => void;
}

export const RegisterForm: React.FC<RegisterFormProps> = ({ onSwitchToLogin }) => {
  // Trạng thái dữ liệu form
  const [formData, setFormData] = useState({
    fullName: '',
    email: '',
    password: '',
    confirmPassword: '',
  });

  // Bước hiện tại: 1 = Nhập thông tin, 2 = Nhập mã OTP
  const [step, setStep] = useState<1 | 2>(1);
  const [otpCode, setOtpCode] = useState('');

  const [loading, setLoading] = useState(false);
  const [errorMessage, setErrorMessage] = useState('');
  const [successMessage, setSuccessMessage] = useState('');

  const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    setFormData({ ...formData, [e.target.name]: e.target.value });
  };

  // 1. Xử lý gửi đăng ký thông tin
  const handleRegisterSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setErrorMessage('');

    if (formData.password !== formData.confirmPassword) {
      setErrorMessage('Mật khẩu xác nhận không khớp!');
      return;
    }

    setLoading(true);
    try {
      const response = await authApi.register(formData);
      if (response.success) {
        setSuccessMessage('Đăng ký thành công! Vui lòng kiểm tra email để lấy mã OTP.');
        setStep(2); // Chuyển sang giao diện nhập OTP
      } else {
        setErrorMessage(response.message || 'Đăng ký thất bại.');
      }
    } catch (error: any) {
      setErrorMessage(error.response?.data?.message || 'Có lỗi xảy ra khi kết nối máy chủ.');
    } finally {
      setLoading(false);
    }
  };

  // 2. Xử lý gửi mã xác thực OTP
  const handleVerifyOtpSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setErrorMessage('');
    setLoading(true);

    try {
      const response = await authApi.verifyEmail({
        email: formData.email,
        token: otpCode.trim(),
      });

      if (response.success) {
        alert('Xác thực tài khoản thành công! Bạn có thể đăng nhập ngay bây giờ.');
        onSwitchToLogin();
      } else {
        setErrorMessage(response.message || 'Mã xác thực không hợp lệ.');
      }
    } catch (error: any) {
      setErrorMessage(error.response?.data?.message || 'Lỗi xác thực mã OTP.');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="w-full max-w-md p-8 space-y-6 bg-white rounded-2xl shadow-xl border border-slate-100">
      <div className="text-center space-y-2">
        <h2 className="text-3xl font-extrabold text-slate-800">
          {step === 1 ? 'Đăng Ký' : 'Xác Thực Email'}
        </h2>
        <p className="text-sm text-slate-500">
          {step === 1 ? 'Tạo tài khoản thí sinh ExamEngine' : `Nhập mã 6 chữ số đã gửi tới ${formData.email}`}
        </p>
      </div>

      {errorMessage && (
        <div className="flex items-center gap-2 p-3 text-sm text-red-600 bg-red-50 border border-red-200 rounded-lg">
          <AlertCircle className="w-4 h-4 shrink-0" />
          <span>{errorMessage}</span>
        </div>
      )}

      {successMessage && step === 2 && (
        <div className="flex items-center gap-2 p-3 text-sm text-green-700 bg-green-50 border border-green-200 rounded-lg">
          <CheckCircle2 className="w-4 h-4 shrink-0" />
          <span>{successMessage}</span>
        </div>
      )}

      {step === 1 ? (
        // FORM ĐĂNG KÝ
        <form onSubmit={handleRegisterSubmit} className="space-y-4">
          <div>
            <label className="block mb-1 text-sm font-medium text-slate-700">Họ và tên</label>
            <div className="relative">
              <User className="absolute w-5 h-5 text-slate-400 -translate-y-1/2 left-3 top-1/2" />
              <input
                type="text"
                name="fullName"
                required
                value={formData.fullName}
                onChange={handleChange}
                placeholder="Nguyễn Văn A"
                className="w-full py-2.5 pl-10 pr-4 text-sm border rounded-lg border-slate-300 focus:outline-none focus:ring-2 focus:ring-indigo-500"
              />
            </div>
          </div>

          <div>
            <label className="block mb-1 text-sm font-medium text-slate-700">Email</label>
            <div className="relative">
              <Mail className="absolute w-5 h-5 text-slate-400 -translate-y-1/2 left-3 top-1/2" />
              <input
                type="email"
                name="email"
                required
                value={formData.email}
                onChange={handleChange}
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
                name="password"
                required
                value={formData.password}
                onChange={handleChange}
                placeholder="••••••••"
                className="w-full py-2.5 pl-10 pr-4 text-sm border rounded-lg border-slate-300 focus:outline-none focus:ring-2 focus:ring-indigo-500"
              />
            </div>
          </div>

          <div>
            <label className="block mb-1 text-sm font-medium text-slate-700">Xác nhận mật khẩu</label>
            <div className="relative">
              <Lock className="absolute w-5 h-5 text-slate-400 -translate-y-1/2 left-3 top-1/2" />
              <input
                type="password"
                name="confirmPassword"
                required
                value={formData.confirmPassword}
                onChange={handleChange}
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
            {loading ? <Loader2 className="w-5 h-5 animate-spin" /> : <><UserPlus className="w-4 h-4 mr-2" /> Đăng ký tài khoản</>}
          </button>
        </form>
      ) : (
        // FORM NHẬP OTP
        <form onSubmit={handleVerifyOtpSubmit} className="space-y-4">
          <div>
            <label className="block mb-1 text-sm font-medium text-slate-700">Mã xác thực (6 số)</label>
            <div className="relative">
              <KeyRound className="absolute w-5 h-5 text-slate-400 -translate-y-1/2 left-3 top-1/2" />
              <input
                type="text"
                required
                maxLength={6}
                value={otpCode}
                onChange={(e) => setOtpCode(e.target.value)}
                placeholder="123456"
                className="w-full py-2.5 pl-10 pr-4 text-center tracking-widest text-lg font-bold border rounded-lg border-slate-300 focus:outline-none focus:ring-2 focus:ring-indigo-500"
              />
            </div>
          </div>

          <button
            type="submit"
            disabled={loading || otpCode.trim().length < 6}
            className="flex items-center justify-center w-full py-2.5 font-medium text-white transition bg-green-600 rounded-lg hover:bg-green-700 disabled:opacity-50"
          >
            {loading ? <Loader2 className="w-5 h-5 animate-spin" /> : 'Kích hoạt tài khoản'}
          </button>

          <button
            type="button"
            onClick={() => setStep(1)}
            className="w-full text-xs text-center text-slate-500 hover:text-slate-700"
          >
            ← Quay lại đổi thông tin
          </button>
        </form>
      )}

      <div className="text-sm text-center text-slate-600">
        Đã có tài khoản?{' '}
        <button
          onClick={onSwitchToLogin}
          className="font-medium text-indigo-600 hover:underline"
        >
          Đăng nhập
        </button>
      </div>
    </div>
  );
};