# ADR 001: Lựa Chọn Nền Tảng Công Nghệ (Tech Stack)

## Trạng Thái
Accepted

## Người Quyết Định
Solo Developer

## Bối Cảnh
Dự án cần một nền tảng thi trắc nghiệm trực tuyến chuẩn thương mại, yêu cầu bảo mật đề thi, chấm điểm chính xác, khả năng mở rộng dữ liệu và tốc độ giao diện nhanh. Lập trình viên có nền tảng C#, hiểu cách sử dụng công cụ quản trị SQL (SSMS) và có kiến thức JavaScript cơ bản.

## Quyết Định
1. **Backend - C# ASP.NET Core Web API (.NET 10 LTS):**
   * Tận dụng khả năng kiểm soát kiểu dữ liệu mạnh mẽ (Strict Type-Safety), hiệu năng xử lý tác vụ cao.
   * Xây dựng theo mô hình Clean Architecture để tách biệt nghiệp vụ và hạ tầng.
   * Quản trị dữ liệu qua Entity Framework Core (EF Core).

2. **Database - PostgreSQL & DBeaver:**
   * Chọn PostgreSQL vì mã nguồn mở, tối ưu chi phí hạ tầng.
   * Hỗ trợ kiểu dữ liệu `JSONB` hoàn hảo để lưu danh sách phương án lựa chọn thay đổi linh hoạt của đề thi tiếng Anh.
   * Sử dụng công cụ **DBeaver Community** làm GUI quản trị cơ sở dữ liệu thay cho SSMS.

3. **Frontend - React 19 + TypeScript (Vite):**
   * Sử dụng React và TypeScript để đảm bảo trải nghiệm thi không bị tải lại trang, đồng bộ tức thời trạng thái câu hỏi và bộ đếm thời gian.
   * Dùng thư viện component `shadcn/ui` và `Tailwind CSS`.