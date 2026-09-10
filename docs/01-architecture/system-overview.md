# Kiến Trúc Tổng Quan Hệ Thống (System Overview)

## 1. Định Vị Công Nghệ
* **Backend:** C# ASP.NET Core Web API (.NET 10 LTS) áp dụng Clean Architecture.
* **Frontend:** React 19 + TypeScript (khởi tạo qua Vite), Tailwind CSS, shadcn/ui.
* **Database:** PostgreSQL (tận dụng kiểu dữ liệu `JSONB` cho cấu trúc câu hỏi động).
* **Công cụ quản trị dữ liệu:** DBeaver Community (thao tác trực quan tương tự SSMS).
* **ORM:** Entity Framework Core (EF Core) Code-First Migrations.
* **API Spec:** Tự động sinh tài liệu qua OpenAPI / Scalar tích hợp trong ASP.NET Core.

## 2. Sơ Đồ Khối Hệ Thống

```mermaid
graph TD
    Client[React 19 Frontend - Vite] -->|HTTPS / REST API| API[ASP.NET Core Web API]
    API -->|EF Core Npgsql| DB[(PostgreSQL Database)]
    API -->|HTTP REST| AI[AI Service API - Explanation]
    Dev[Lập trình viên] -->|DBeaver UI| DB
```

## 3. Luồng Dữ Liệu Thi & Chấm Điểm (Exam Flow)

```mermaid
sequenceDiagram
    autonumber
    actor User as Học sinh
    participant FE as React Frontend (Vite)
    participant API as ASP.NET Core API
    participant DB as PostgreSQL

    User->>FE: Bấm chọn đề thi và bắt đầu
    FE->>API: GET /api/v1/exams/{id}/take
    API->>DB: Truy vấn dữ liệu đề thi và câu hỏi
    DB-->>API: Trả về dữ liệu gốc
    API-->>FE: Trả về danh sách câu hỏi (ĐÃ ẨN đáp án đúng)
    FE->>FE: Bật Timer, lưu tạm từng câu chọn vào LocalStorage

    User->>FE: Bấm nút "Nộp bài"
    FE->>API: POST /api/v1/exams/{id}/submit (kèm mảng câu trả lời)
    API->>DB: Kiểm tra thời gian nộp, so khớp đáp án, ghi bảng Submissions
    API->>DB: Tự động ghi nhận câu sai vào MistakeBank
    DB-->>API: Xác nhận Transaction thành công
    API-->>FE: Trả về Điểm số, Đáp án đúng & Lời giải chi tiết
    FE-->>User: Hiển thị màn hình kết quả thi
```