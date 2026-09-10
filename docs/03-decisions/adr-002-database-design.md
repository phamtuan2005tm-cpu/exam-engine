# ADR 002: Thiết Kế Bảng Questions Kết Hợp Cột JSONB Trong PostgreSQL

## Trạng Thái
Accepted

## Người Quyết Định
Solo Developer

## Bối Cảnh
Hệ thống đề thi tiếng Anh có nhiều dạng câu hỏi: Trắc nghiệm 4 lựa chọn, bài đọc hiểu kèm danh sách câu hỏi con, hoặc các câu hỏi có số lượng phương án thay đổi. 

Thông thường trong thiết kế quan hệ truyền thống (RDBMS), các phương án lựa chọn ($A, B, C, D$) sẽ được tách thành một bảng riêng biệt `question_options` có khóa ngoại trỏ về `questions`.

## Vấn Đề
1. Một đề thi có 50 câu, tương ứng ít nhất 200 bản ghi lựa chọn. Mỗi lần học sinh lấy đề hoặc nộp bài, hệ thống phải thực hiện phép `JOIN` giữa bảng `questions` và `question_options`.
2. Tạo ra nhiều bản ghi phân mảnh trong database, làm phức tạp hóa câu lệnh truy vấn Entity Framework Core.
3. Các phương án lựa chọn gắn liền hoàn toàn với vòng đời của câu hỏi, không bao giờ được tái sử dụng độc lập ở nơi khác.

## Quyết Định
1. **Lưu options dưới dạng cột `JSONB` ngay trong bảng `questions`:**
   * Mảng options lưu cấu trúc: `[{"key": "A", "text": "..."}, {"key": "B", "text": "..."}]`.
2. **Lưu explanation dưới dạng cột `JSONB`:**
   * Gói gọn bản dịch nghĩa, công thức ngữ pháp trọng tâm và giải thích chi tiết trong một cột duy nhất thay vì tạo thêm 3 cột rời rạc.

## Hệ Quả (Consequences)
* **Tích cực:**
  * Truy vấn lấy trọn vẹn một đề thi chỉ tốn duy nhất 1 câu lệnh `SELECT * FROM questions WHERE exam_id = @id ORDER BY order_index ASC`, không cần `JOIN` bảng phụ.
  * Tốc độ đọc đề thi tăng đáng kể, giảm tải I/O cho PostgreSQL.
  * Tận dụng khả năng hỗ trợ native `JSONB` của PostgreSQL và thư viện `Npgsql.EntityFrameworkCore.PostgreSQL`.
* **Tiêu cực / Ràng buộc:**
  * C# Backend cần cấu hình `HasColumnType("jsonb")` trong Entity Framework Core DbContext.