import { apiClient } from '../services/apiClient';
import type {
  AuthResponseDto,
  ApiResponse,
  RegisterRequest,
  LoginRequest,
  VerifyEmailRequest,
} from '../types/auth.types';

export const authApi = {
  // 1. Gửi dữ liệu đăng ký tài khoản
  register: async (payload: RegisterRequest) => {
    const response = await apiClient.post<ApiResponse<string>>('/Auth/register', payload);
    return response.data;
  },

  // 2. Gửi mã OTP xác nhận email
  verifyEmail: async (payload: VerifyEmailRequest) => {
    const response = await apiClient.post<ApiResponse<string>>('/Auth/verify-email', payload);
    return response.data;
  },

  // 3. Đăng nhập hệ thống
  login: async (payload: LoginRequest) => {
    const response = await apiClient.post<ApiResponse<AuthResponseDto>>('/Auth/login', payload);
    return response.data;
  },
};