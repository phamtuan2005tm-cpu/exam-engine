import axios from 'axios';

// Đọc địa chỉ Backend từ .env (nếu không tìm thấy sẽ lấy mặc định port 7000)
const BASE_URL = import.meta.env.VITE_API_BASE_URL || 'https://localhost:7000/api';

export const apiClient = axios.create({
  baseURL: BASE_URL,
  headers: {
    'Content-Type': 'application/json',
  },
});

// Trạm chặn Request: Tự động đính kèm Access Token vào mọi yêu cầu
apiClient.interceptors.request.use(
  (config) => {
    const token = localStorage.getItem('accessToken');
    if (token && config.headers) {
      config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
  },
  (error) => {
    return Promise.reject(error);
  }
);