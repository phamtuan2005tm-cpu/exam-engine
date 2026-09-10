# Đặc Tả Luồng Nghiệp Vụ: Quy Trình Thi Thử (Exam Taking Flow)

## 1. Sơ Đồ Quy Trình Hoạt Động (Activity Diagram)

```mermaid
flowchart TD
    Start([1. Học sinh chọn đề thi]) --> ClickStart[2. Bấm Bắt đầu làm bài]
    ClickStart --> CheckDraft{Có bài thi đang làm dở?}
    
    CheckDraft -- Có --> RestoreDraft[Khôi phục các câu đã chọn trước đó]
    CheckDraft -- Không --> InitExam[Tạo phiên làm bài mới]
    
    RestoreDraft --> ExamRoom[3. Vào màn hình Phòng thi ảo]
    InitExam --> ExamRoom
    
    ExamRoom --> StudentAction{Hành động của học sinh}
    
    StudentAction -- Chọn phương án --> AutoSaveState[Ghi nhận lựa chọn ngay lập tức]
    AutoSaveState --> UpdateGrid[Đổi trạng thái hiển thị câu đã làm]
    UpdateGrid --> StudentAction
    
    StudentAction -- Tải lại trang / Rớt mạng --> KeepProgress[Giữ nguyên bài thi & thời gian]
    KeepProgress --> ExamRoom
    
    StudentAction -- Hết thời gian quy định --> ForceSubmit[Khóa giao diện & Tự động nộp bài]
    StudentAction -- Bấm Nộp bài chủ động --> CheckCompletion{Đã làm hết tất cả các câu?}
    
    CheckCompletion -- Còn câu trống --> ConfirmDialog[Hiển thị cảnh báo: Còn câu chưa làm]
    ConfirmDialog -- Quay lại làm tiếp --> StudentAction
    ConfirmDialog -- Xác nhận nộp --> SendSubmission[Gửi bài nộp về hệ thống]
    
    ForceSubmit --> SendSubmission
    
    SendSubmission --> ProcessGrading[4. Hệ thống chấm điểm & Ghi nhận]
    subgraph Quá Trình Xử Lý Hệ Thống
        ProcessGrading --> ValidateTime[Kiểm tra thời gian nộp hợp lệ]
        ValidateTime --> CalcScore[So khớp kết quả & Tính điểm thang 10]
        CalcScore --> RecordHistory[Lưu lịch sử bài nộp]
        RecordHistory --> UpdateMistakes[Tự động chuyển câu sai vào Sổ tay câu sai]
    end
    
    UpdateMistakes --> ClearDraft[Xóa dữ liệu làm dở]
    ClearDraft --> ShowResult[5. Hiển thị bảng điểm & Lời giải chi tiết]
    ShowResult --> End([Kết thúc lượt thi])