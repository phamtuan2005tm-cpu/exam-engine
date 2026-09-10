# Đặc Tả Yêu Cầu Hệ Thống (System Requirements Specification - SRS)

## Thông Tin Tài Liệu
* **Dự án:** Exam Engine - Nền tảng Luyện thi Tiếng Anh vào Lớp 10
* **Phiên bản:** 1.0.0
* **Tác giả:** Solo Developer
* **Trạng thái:** Approved

---

## 1. Yêu Cầu Chức Năng (Functional Requirements - FR)

### 1.1. Quản Lý & Hiển Thị Đề Thi (Exam Catalog)
* **FR-01 (Danh mục đề thi):** Hệ thống hiển thị danh sách đề thi kèm thông tin: Tên tỉnh/thành phố, Năm phát hành, Số câu hỏi, Thời gian làm bài và Trạng thái hoàn thành kèm điểm số cao nhất (nếu đã làm).
* **FR-02 (Tải đề thi an toàn - Server-side Masking):** Khi bắt đầu bài thi, API chỉ trả về danh sách câu hỏi, nội dung bài đọc và các lựa chọn ($A, B, C, D$). **Hệ thống tuyệt đối không gửi `CorrectAnswer` hoặc `Explanation` về Client trước khi nộp bài.**
* **FR-03 (Điều hướng câu hỏi):** Cho phép học sinh di chuyển qua lại giữa các câu bằng danh sách số câu (Matrix Grid) hoặc nút "Câu trước / Câu tiếp theo".

### 1.2. Phòng Thi Ảo & Chống Mất Dữ Liệu (Exam Engine & Auto-Save)
* **FR-04 (Kiểm soát thời gian độc lập):** Bộ đếm thời gian hiển thị đếm ngược từng giây tại Client. Mốc thời gian bắt đầu (`StartedAt`) được lưu trữ và kiểm tra tại Server.
* **FR-05 (Tự động lưu tạm - Auto-save):** Hệ thống tự động lưu lựa chọn của học sinh vào `localStorage` ngay khi click chọn đáp án. Khôi phục nguyên vẹn trạng thái khi học sinh tải lại trang (F5) hoặc mở lại trình duyệt.
* **FR-06 (Cảnh báo & Thu bài):**
  * Hiển thị popup cảnh báo số lượng câu chưa hoàn thành khi học sinh bấm "Nộp bài".
  * Tự động khóa thao tác chọn và kích hoạt nộp bài (Auto-submit) khi đồng hồ đếm ngược về `00:00`.

### 1.3. Chấm Điểm & Báo Cáo Lời Giải (Grading & Explanation)
* **FR-07 (Chấm điểm Server-side):** Máy chủ so khớp mảng câu trả lời với đáp án gốc trong Database, tính điểm theo thang điểm 10 theo công thức:
  $$\text{Score} = \left(\frac{\text{Số câu đúng}}{\text{Tổng số câu}}\right) \times 10$$
* **FR-08 (Chi tiết kết quả & Lời giải):** Sau khi nộp bài, hiển thị:
  * Điểm số tổng kết và thời gian làm bài thực tế.
  * Trạng thái từng câu: Đúng (Xanh lá), Sai (Đỏ), Bỏ trống (Xám).
  * Đáp án đúng kèm lời giải chi tiết: Dịch nghĩa tiếng Việt, Cấu trúc ngữ pháp trọng tâm và Lý do lựa chọn/loại trừ phương án.

### 1.4. Sổ Tay Câu Sai (Mistake Bank - USP)
* **FR-09 (Tự động ghi nhận câu sai):** Tự động lọc toàn bộ câu trả lời sai (`UserAnswer != CorrectAnswer`) sau mỗi bài thi và lưu vào bảng `MistakeRecords` kèm biến đếm số lần sai (`WrongCount`).
* **FR-10 (Luyện tập lại câu sai):** Cho phép học sinh lọc danh sách câu sai theo chuyên đề (Ngữ pháp, Từ vựng, Đọc hiểu, Phát âm) và tạo phiên thi ôn tập lại riêng các câu hỏi này.

---

## 2. Yêu Cầu Phi Chức Năng (Non-Functional Requirements - NFR)

### 2.1. Bảo Mật & Tính Toàn Vẹn (Security & Integrity)
* **NFR-SEC-01 (Chống lộ đáp án):** Trường `CorrectAnswer` và `Explanation` chỉ được serialize và trả về sau khi bài nộp đã được chấm thành công trên máy chủ.
* **NFR-SEC-02 (Chống gian lận thời gian):** Máy chủ từ chối tiếp nhận bài thi nếu mốc thời gian nộp vi phạm điều kiện:
  $$\text{SubmittedAt} > \text{StartedAt} + \text{DurationMinutes} + 60\text{s (ân hạn mạng)}$$

### 2.2. Hiệu Năng (Performance)
* **NFR-PERF-01 (Tốc độ chấm điểm):** Thời gian máy chủ tính điểm và trả kết quả sau khi nộp bài phải đạt $\le 1.0$ giây với đề thi 50 câu.
* **NFR-PERF-02 (Phản hồi giao diện):** Thời gian chuyển câu và đổi màu trạng thái trên ma trận số câu phải $\le 50\text{ms}$.

### 2.3. Độ Tin Cậy & Phục Hồi (Reliability & Resilience)
* **NFR-REL-01 (Chống gián đoạn kết nối):** Khi mất kết nối mạng tạm thời, học sinh vẫn tiếp tục tích chọn đáp án bình thường (dữ liệu lưu tại LocalStorage) và nộp bài khi kết nối được thiết lập lại.
* **NFR-REL-02 (Giao dịch nguyên khối):** Toàn bộ thao tác ghi nhận `ExamSubmission`, `SubmissionDetail` và `MistakeBank` phải nằm trong một Database Transaction duy nhất để tránh phân mảnh dữ liệu.

### 2.4. Khả Năng Sử Dụng (Usability)
* **NFR-USA-01 (Bố cục phòng thi):** Giao diện chia 2 cột rõ ràng, cỡ chữ câu hỏi tối thiểu 16px, độ tương phản màu sắc đạt chuẩn WCAG AA.
* **NFR-USA-02 (Tương thích nền tảng):** Hỗ trợ hoạt động mượt mà trên Chrome, Edge, Safari và Firefox (phiên bản mới nhất).