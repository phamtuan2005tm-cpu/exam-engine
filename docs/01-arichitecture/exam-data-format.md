# Quy Chuẩn Định Dạng Dữ Liệu Đề Thi (Exam JSON Schema)

Tài liệu này quy định cấu trúc JSON chuẩn dùng để:
1. Nhập đề thi hàng loạt (Seed data / Batch Ingestion) vào hệ thống.
2. Trả payload câu hỏi an toàn từ Backend C# về React Frontend (đã ẩn `correctAnswer` và `explanation`).

---

## 1. Cấu Trúc JSON Đầy Đủ (Được lưu trữ tại Backend Database)

```json
{
  "examCode": "ENG9-HN-2026-01",
  "title": "Đề Thi Tuyển Sinh Vào Lớp 10 Môn Tiếng Anh - Hà Nội",
  "durationMinutes": 60,
  "totalQuestions": 3,
  "sections": [
    {
      "sectionType": "Phonetics",
      "instruction": "Choose the word whose underlined part is pronounced differently from that of the others.",
      "questions": [
        {
          "orderIndex": 1,
          "stem": "A. invited | B. wanted | C. ended | D. liked",
          "options": [
            { "key": "A", "text": "invited" },
            { "key": "B", "text": "wanted" },
            { "key": "C", "text": "ended" },
            { "key": "D", "text": "liked" }
          ],
          "correctAnswer": "D",
          "explanation": {
            "translation": "Chọn từ có phần gạch chân phát âm khác với các từ còn lại.",
            "grammar_focus": "Quy tắc phát âm đuôi -ed: Phát âm là /t/ khi tận cùng là các phụ âm vô thanh (/k/, /p/, /f/, /s/, /ʃ/, /tʃ/). Phát âm là /ɪd/ khi tận cùng là /t/, /d/.",
            "details": "'invited', 'wanted', 'ended' đều có tận cùng là /t/ hoặc /d/ nên phát âm là /ɪd/. Riêng 'liked' tận cùng là /k/ nên phát âm là /t/."
          }
        }
      ]
    },
    {
      "sectionType": "Grammar",
      "instruction": "Choose the best option to complete each of the following sentences.",
      "questions": [
        {
          "orderIndex": 2,
          "stem": "If the weather _______ nice tomorrow, we will go camping in the park.",
          "options": [
            { "key": "A", "text": "is" },
            { "key": "B", "text": "was" },
            { "key": "C", "text": "will be" },
            { "key": "D", "text": "would be" }
          ],
          "correctAnswer": "A",
          "explanation": {
            "translation": "Nếu ngày mai thời tiết đẹp, chúng tôi sẽ đi cắm trại ở công viên.",
            "grammar_focus": "Cấu trúc Câu điều kiện Loại 1: If + S + V(hiện tại đơn), S + will + V-bare.",
            "details": "Mệnh đề phụ 'If' của câu điều kiện loại 1 chia ở thì hiện tại đơn. Chủ ngữ 'the weather' là danh từ không đếm được nên dùng to be 'is'."
          }
        }
      ]
    },
    {
      "sectionType": "Reading",
      "instruction": "Read the passage and choose the correct answer to each question.",
      "passageContent": "Vietnamese conical hat, or 'Non La', is a typical symbol of the Vietnamese people. The hat is made from simple and available materials such as palm leaves, bamboo, and bark of Moc tree. The making of Non La is a traditional craft passed down through generations in many rural villages...",
      "questions": [
        {
          "orderIndex": 3,
          "stem": "What is the main material used to make Non La according to the passage?",
          "options": [
            { "key": "A", "text": "Palm leaves and bamboo" },
            { "key": "B", "text": "Silk and plastic" },
            { "key": "C", "text": "Cotton and paper" },
            { "key": "D", "text": "Bark of Moc tree only" }
          ],
          "correctAnswer": "A",
          "explanation": {
            "translation": "Vật liệu chính được dùng để làm nón lá theo đoạn văn là gì?",
            "grammar_focus": "Kỹ năng đọc quét thông tin (Scanning details).",
            "details": "Thông tin nằm ở câu thứ 2: 'The hat is made from simple and available materials such as palm leaves, bamboo...'. Phương án A là đáp án chính xác nhất."
          }
        }
      ]
    }
  ]
}
```

---

## 2. Cấu Trúc JSON Trả Về Phía Client Khi Làm Bài (GET /take)
* **Bảo mật:** Loại bỏ hoàn toàn `correctAnswer` và `explanation`.

```json
{
  "examId": "8f3b6c2d-9478-43e1-a20c-99c0e5a1b32d",
  "title": "Đề Thi Tuyển Sinh Vào Lớp 10 Môn Tiếng Anh - Hà Nội",
  "durationMinutes": 60,
  "totalQuestions": 3,
  "questions": [
    {
      "questionId": "c4d11b5e-2b7c-48c9-9407-7e6d015c7a01",
      "orderIndex": 1,
      "sectionType": "Phonetics",
      "passageContent": null,
      "stem": "A. invited | B. wanted | C. ended | D. liked",
      "options": [
        { "key": "A", "text": "invited" },
        { "key": "B", "text": "wanted" },
        { "key": "C", "text": "ended" },
        { "key": "D", "text": "liked" }
      ]
    }
  ]
}
```