# FileOrganizer

Ứng dụng desktop được xây dựng bằng **C# Windows Forms**, hỗ trợ quét, phân loại và tự động sắp xếp các tệp tin vào các thư mục tương ứng.

## 📌 Tổng quan dự án

**FileOrganizer** là một ứng dụng desktop được xây dựng nhằm hỗ trợ người dùng quản lý và sắp xếp các tệp tin trong một thư mục.

### Ứng dụng cho phép người dùng:

- Chọn thư mục cần xử lý.
- Quét các tệp tin trong thư mục.
- Phân loại tệp dựa trên phần mở rộng.
- Tự động tạo các thư mục theo từng loại tệp.
- Di chuyển tệp vào thư mục tương ứng.

## 🔄 Luồng hoạt động

```text
Chọn thư mục
     ↓
Quét tệp
     ↓
Phân tích phần mở rộng
     ↓
Xác định loại tệp
     ↓
Phân loại
     ↓
Di chuyển tệp
```

📂 Ví dụ
Thư mục ban đầu
```text
Downloads/
├── photo.jpg
├── document.pdf
├── song.mp3
└── video.mp4
```
Sau khi tổ chức:
```text 
Downloads/
├── Images/
│   └── photo.jpg
│
├── Documents/
│   └── document.pdf
│
├── Audio/
│   └── song.mp3
│
└── Videos/
    └── video.mp4
```
### Hiện tại ứng ứng dụng đang ở version 1.0 vẫn đang update và hoàn thiện các tính năng
