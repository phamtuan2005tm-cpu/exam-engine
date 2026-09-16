// 1. Dữ liệu Backend trả về khi đăng nhập thành công
export interface AuthResponseDto {
  accessToken: string;    // Token dùng để gọi các API bảo mật
  refreshToken: string;   // Token dùng để xin cấp mới Access Token
  userId: string;
  email: string;
  fullName: string;
  roles: string[];        // Danh sách quyền (ví dụ: ["Student"] hoặc ["Admin"])
}

// 2. Vỏ bọc phản hồi chuẩn của Backend (Generic Response)
export interface ApiResponse<T> {
  success: boolean;       // Gọi thành công hay thất bại (true/false)
  message?: string;       // Lời nhắn thông báo (nếu có)
  data?: T;               // Dữ liệu chính trả về (có thể là chuỗi, object hoặc list)
  errors?: string[];      // Danh sách chi tiết lỗi nếu thất bại
}

// 3. Dữ liệu gửi lên khi bấm Đăng ký tài khoản
export interface RegisterRequest {
  email: string;
  fullName: string;
  password: string;
  confirmPassword: string;
}

// 4. Dữ liệu gửi lên khi bấm Đăng nhập
export interface LoginRequest {
  email: string;
  password: string;
}

// 5. Dữ liệu gửi lên khi nhập mã OTP xác thực Email
export interface VerifyEmailRequest {
  email: string;
  token: string;           // Mã OTP gồm 6 số
}
// 6. Gửi email xin cấp mã quên mật khẩu
export interface ForgotPasswordRequest {
  email: string;
}

// 7. Nhập mã xác nhận cùng mật khẩu mới
export interface ResetPasswordRequest {
  email: string;
  code: string;
  newPassword: string;
  confirmNewPassword: string;
}

// 8. Gửi refresh token lên để lấy access token mới
export interface RefreshTokenRequest {
  refreshToken: string;
}