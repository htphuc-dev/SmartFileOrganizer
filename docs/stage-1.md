# Giai đoạn 1 — quét và di chuyển file an toàn

## Thay đổi chính

- Mỗi kết quả quét gắn với một đường dẫn tuyệt đối đã chuẩn hóa (bỏ khoảng trắng ngoài và dấu phân cách cuối, giữ nguyên đường dẫn gốc ổ đĩa). Chỉ ghi nhận đường dẫn sau khi quét thành công. Trước khi di chuyển, đường dẫn hiện tại phải khớp với đường dẫn đã quét, so sánh không phân biệt hoa thường trên Windows.
- Thay đổi nội dung ô đường dẫn hoặc chọn thư mục qua Browse, kể cả chọn lại cùng thư mục, sẽ xóa danh sách, lỗi cũ và vô hiệu hóa Organize. Sự kiện TextChanged chỉ được đăng ký một lần trong Designer.
- FileScanner chỉ đọc file trực tiếp trong thư mục, không đệ quy. Lỗi liệt kê hoặc metadata được truyền lên giao diện; không trả về kết quả quét một phần. Thư mục rỗng là một lần quét thành công với 0 file; quét lỗi có trạng thái và hộp thoại riêng, không giữ lại kết quả cũ.
- FileOrganizer.MoveFile trả về FileMoveResult với SourcePath, DestinationPath, Success và ErrorMessage. Thành công trả đường dẫn đích thực tế, bao gồm hậu tố trùng tên. Khi thất bại, DestinationPath là đường dẫn đã thử nếu xác định được, hoặc chuỗi rỗng khi đầu vào không thể chuẩn hóa.
- Kiểm tra thuộc tính nguồn bằng File.GetAttributes để giữ nguyên nguyên nhân lỗi truy cập và từ chối nguồn là thư mục. Các lỗi đường dẫn, thiếu nguồn, thiếu quyền và I/O được trả về theo từng file để các file tiếp theo vẫn được xử lý.
- Hậu tố ` (1)`, ` (2)`… bỏ qua cả tên file và tên thư mục đã tồn tại, kể cả file không có phần mở rộng. File.Move dùng `overwrite: false`; xung đột phát sinh sau khi chọn tên sẽ trở thành lỗi, không ghi đè. Không có bước tự xóa nguồn khi di chuyển lỗi.
- DataGridView thêm cột Result: `Ready` hoặc `Failed: <nguyên nhân>`, với tooltip chứa toàn bộ lỗi. Dòng liên kết với FileItem qua Tag nên việc sắp xếp cột không làm gán sai kết quả.
- Sau mỗi thành công, loại file khỏi danh sách chờ ngay trước khi cập nhật giao diện. Giữ các file thất bại cùng lỗi để bấm Organize thử lại mà không quét lại. Hiển thị số thành công/thất bại của từng lần chạy; khi có lỗi bất ngờ, hiển thị thêm số file chưa thử.
- Giữ hộp thoại xác nhận. Khóa ô đường dẫn, Browse, Scan và Organize trong lúc xử lý; khôi phục trạng thái bằng finally. Đã bỏ Application.DoEvents. Không thêm async, tác vụ nền, thư viện UI hay thay đổi framework/quy tắc phân loại.
- Giữ bố cục xanh–trắng hiện có; chỉ thêm cột, điều chỉnh tỷ lệ chiều rộng và nối sự kiện trong MainForm.Designer.cs. Không đổi resource.

## Build và chạy kiểm tra

Tại thư mục chứa `SmartFileOrganizer.slnx`, dùng .NET SDK 10 trên Windows:

```powershell
dotnet build SmartFileOrganizer/SmartFileOrganizer.csproj --configuration Release
dotnet run --project SmartFileOrganizer.Tests/SmartFileOrganizer.Tests.csproj --configuration Release
```

SmartFileOrganizer.Tests là chương trình console tham chiếu trực tiếp dự án, không thêm package test. Chương trình trả exit code khác 0 khi có kiểm tra thất bại; các kiểm tra không thể thiết lập do giới hạn môi trường được in rõ `SKIP`, không tính là đạt. Chạy bằng `dotnet run`, không dùng `dotnet test` cho bộ kiểm tra này.

Mỗi kiểm tra chỉ tạo dữ liệu trong thư mục con có GUID riêng dưới `%TEMP%\SmartFileOrganizer.Stage1Tests`. Việc dọn dẹp xác minh đường dẫn nằm dưới thư mục tạm này trước khi xóa. Không dùng thư mục tài liệu thật.

Các trường hợp được kiểm tra:

- Quét thư mục rỗng, thư mục không tồn tại và đường dẫn là file.
- Metadata, tên tiếng Việt, phần mở rộng viết hoa, tất cả quy tắc phân loại hiện có và không quét thư mục con.
- Di chuyển thành công, đường dẫn trả về và bảo toàn nội dung nhị phân.
- Trùng tên với file/thư mục, hậu tố liên tiếp, bảo toàn dữ liệu đích cũ, file không có phần mở rộng.
- Nguồn bị xóa sau khi quét; lỗi một file không ngăn file tiếp theo được di chuyển.
- Thư mục đích bị một file chặn; bảo toàn nguồn/đích và thử lại thành công với cùng FileItem sau khi bỏ chặn.
- Từ chối nguồn là thư mục và đầu vào có đường dẫn không hợp lệ.
- Trên Windows: quét thư mục bị từ chối quyền (cần được sửa ACL của thư mục tạm); file bị khóa độc quyền, giữ nguyên nguồn rồi thử lại sau khi mở khóa.

Kết quả chạy thực tế ngày 24/10/2026:

- Build Release: thành công, 0 cảnh báo, 0 lỗi.
- Bộ kiểm tra: **16 đạt, 0 thất bại, 1 bỏ qua** trên Windows.
- Trường hợp bỏ qua: kiểm tra quét bị từ chối quyền, vì môi trường trả UnauthorizedAccessException khi thiết lập ACL của thư mục tạm. Chưa xác minh hành vi thiếu quyền bằng kiểm tra này; có thể chạy lại ở môi trường cho phép sửa ACL hoặc kiểm tra thủ công bên dưới.
- Chưa chạy kiểm tra tương tác giao diện và chưa mở Visual Studio Designer. Các bước bên dưới cần được kiểm tra trong Visual Studio. Xung đột tên do tiến trình khác tạo đúng giữa lúc kiểm tra và di chuyển chưa được mô phỏng tự động; mã sử dụng File.Move không cho ghi đè.

## Kiểm tra thủ công trong Visual Studio

Mở solution, đặt SmartFileOrganizer làm startup project. Mở MainForm bằng **View Designer** để kiểm tra Designer vẫn tải được, bố cục/cột Result hiển thị đúng. Chạy ứng dụng và chỉ dùng dữ liệu thử trong `%TEMP%`.

1. **Đổi đường dẫn A sang B:** Tạo hai thư mục tạm A/B với nội dung khác nhau. Quét A và xác nhận các dòng có Result = Ready. Sửa ô đường dẫn sang B: danh sách/lỗi cũ phải mất ngay, Organize bị khóa. Đổi lại A vẫn phải quét lại. Thử chọn B qua Browse và thử chọn lại đúng A qua Browse: cả hai đều vô hiệu hóa kết quả cũ. Quét lại rồi đối chiếu Destination nằm dưới đúng thư mục đã quét.
2. **Rỗng và quét lỗi:** Quét thư mục rỗng: status báo Scan complete với 0 file, Organize bị khóa. Quét một thư mục có file, sau đó xóa/đổi tên thư mục đó bằng cửa sổ ngoài và bấm Scan lần nữa: phải báo Scan failed với nguyên nhân, xóa kết quả cũ và khóa Organize. Thử thêm đường dẫn không hợp lệ hoặc thư mục tạm bị từ chối quyền nếu môi trường hỗ trợ.
3. **Hủy xác nhận:** Quét thư mục có file, bấm Organize rồi chọn No. File nguồn, danh sách, lỗi và trạng thái sẵn sàng phải được giữ nguyên; không tạo thư mục đích và không di chuyển gì.
4. **Trùng tên:** Trong A, tạo `report.txt` và thư mục `Documents` chứa file `report.txt` cùng thư mục `report (1).txt`. Quét A rồi Organize/Yes: tạo `Documents\report (2).txt`, giữ nguyên file/thư mục đã có, nội dung file mới đúng với nguồn. Thử tương tự với file không có phần mở rộng trong Others.
5. **Xem lỗi từng file và tiếp tục xử lý:** Tạo trong A các file `retry.txt`, `photo.PNG`, và một file không có phần mở rộng tên `Documents` để chặn thư mục Documents. Quét, thử sắp xếp các dòng theo tên trước khi Organize/Yes. File ảnh vẫn được chuyển vào Images; `retry.txt` phải còn trong danh sách với `Failed: ...` và tooltip nguyên nhân đầy đủ. Đối chiếu số moved/failed với dữ liệu thực tế. File nguồn bị lỗi phải giữ nguyên nội dung. Có thể thử thêm việc xóa một file sau khi quét để xem lỗi thiếu nguồn.
6. **Khắc phục và thử lại không quét:** Sau bước 5, bỏ file đang chặn Documents nếu nó chưa được chuyển vào Others. Không bấm Scan; bấm Organize/Yes. Chỉ các file còn lại được thử lại; file thành công trước đó không được xử lý lại hoặc sinh thêm bản có hậu tố. Khi hoàn tất, danh sách trống và Organize bị khóa. Thử tương tự bằng cách giữ file tạm với FileShare.None rồi giải phóng khóa.
7. **Trạng thái điều khiển:** Khi xử lý, ô đường dẫn và cả ba nút bị khóa; sau thành công hoặc lỗi, ô đường dẫn/Browse/Scan được mở lại, Organize chỉ mở khi còn file chờ của lần quét hợp lệ. Xử lý vẫn đồng bộ nên giao diện có thể tạm ngừng phản hồi với thư mục lớn; async/progress/cancel nằm ngoài Giai đoạn 1.

Thay đổi được giữ trong working tree để xem xét; chưa commit hoặc push.
