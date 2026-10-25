# SmartFileOrganizer

Ứng dụng desktop giúp xem trước và sắp xếp file trong một thư mục theo phần mở rộng. Đây là dự án thực hành C# WinForms của **Hoàng Phúc**, tập trung vào thao tác file an toàn, tác vụ nền và cấu hình phân loại.

Repository: [htphuc-dev/SmartFileOrganizer](https://github.com/htphuc-dev/SmartFileOrganizer).

## Tính năng

- Quét file, hiển thị nhóm, dung lượng và đường dẫn đích dự kiến.
- Chọn từng file bằng checkbox, Select All/Clear Selection và xem tổng dung lượng đã chọn; lựa chọn đi theo file khi sắp xếp bảng.
- Organize các file được chọn; tự tạo thư mục nhóm và thêm hậu tố `(1)`, `(2)` khi trùng tên với file hoặc thư mục. Không ghi đè.
- Báo lỗi theo file; giữ lại file lỗi/chưa xử lý để chọn và thử lại.
- Quét, Organize và Undo chạy nền, có tiến trình và Cancel; khóa thao tác xung đột trong lúc chạy.
- Undo giao dịch Organize gần nhất có file chuyển thành công trong phiên ứng dụng; hỗ trợ khôi phục một phần rồi thử lại.
- Settings cho phép thêm, sửa, xóa nhóm, đổi thư mục đích và quy tắc phần mở rộng; lưu cấu hình JSON.
- Giao diện xanh–trắng có icon, hỗ trợ thay đổi kích thước MainForm/Settings và hộp thoại About.

## Công nghệ và môi trường

- C#, Windows Forms, .NET 10 (`net10.0-windows`).
- FontAwesome.Sharp **6.6.0**; JSON qua `System.Text.Json`.
- Windows, .NET 10 SDK; kết nối NuGet khi restore lần đầu.
- Visual Studio 2026 hỗ trợ SDK đang dùng, với workload **.NET desktop development**. Xem [bảng tương thích SDK/Visual Studio của Microsoft](https://learn.microsoft.com/en-us/dotnet/core/porting/versioning-sdk-msbuild-vs) nếu IDE không nhận SDK.

### Mở bằng Visual Studio

1. Clone repository hoặc mở thư mục mã nguồn.
2. Chọn **Open a project or solution** và mở `SmartFileOrganizer.slnx`.
3. Restore NuGet, chọn `SmartFileOrganizer` làm startup project.
4. Chọn cấu hình **Release**, build solution và nhấn F5 để chạy.

### Dùng dòng lệnh

Nếu chưa có mã nguồn:

```powershell
git clone https://github.com/htphuc-dev/SmartFileOrganizer.git
cd SmartFileOrganizer
```

Chạy từ thư mục chứa `SmartFileOrganizer.slnx`:

```powershell
dotnet restore SmartFileOrganizer.slnx -m:1
dotnet build SmartFileOrganizer.slnx --configuration Release --no-restore -m:1
dotnet run --project SmartFileOrganizer/SmartFileOrganizer.csproj --configuration Release --no-build
```

`-m:1` giới hạn MSBuild ở một worker khi restore/build solution.

Project kiểm tra dùng **console runner riêng**, không dùng framework test. Sau khi build solution, chạy trên Windows:

```powershell
dotnet run --project SmartFileOrganizer.Tests/SmartFileOrganizer.Tests.csproj --configuration Release --no-build
```

Runner in `PASS`/`FAIL`/`SKIP`, tổng kết số kiểm tra và trả exit code khác 0 khi có lỗi. Các kiểm tra dùng thư mục tạm riêng, cấu hình tạm và đối chiếu nội dung file. Một số kiểm tra tạo control WinForms trong tiến trình để kiểm tra trạng thái/bố cục; chúng không thay thế kiểm tra trực quan. Kiểm tra ACL có thể báo `SKIP` nếu môi trường không cho đổi quyền của thư mục tạm.

## Cách sử dụng

1. **Chọn thư mục** bằng Browse hoặc nhập đường dẫn.
2. **Scan Files** để đọc file ở ngay trong thư mục đó và tạo bảng xem trước.
3. **Chọn file** bằng checkbox, Select All hoặc Clear Selection. Kiểm tra số lượng và tổng dung lượng đã chọn.
4. **Xem trước** nhóm, đường dẫn đích và tooltip nếu nội dung dài.
5. **Organize**, kiểm tra hộp thoại rồi xác nhận. Chỉ các file đã chọn được di chuyển.

Khi một file lỗi, các file khác vẫn được xử lý. Sau khi sửa nguyên nhân, chọn các file còn lại và Organize lại. Đổi đường dẫn hoặc lưu quy tắc khác sẽ yêu cầu quét lại trước khi Organize.

### Cancel và đóng cửa sổ

Cancel yêu cầu dừng trước file tiếp theo; thao tác file hiện tại hoàn tất và được ghi nhận trước khi dừng. Cancel không tự hoàn tác những file đã chuyển. Quét bị hủy sẽ không công bố danh sách chưa đầy đủ. Nếu đóng MainForm trong lúc chạy, ứng dụng yêu cầu hủy và chờ tác vụ kết thúc trước khi đóng.

### Undo

Sau khi Organize chuyển thành công ít nhất một file, Undo được bật khi tác vụ kết thúc, kể cả khi bảng đã trống. Xác nhận Undo để đưa file về **đúng đường dẫn nguồn đã ghi**, sử dụng đường dẫn đích thực tế sau khi xử lý trùng tên.

- Đổi thư mục, quét lại hoặc đổi Settings không xóa lịch sử Undo.
- Organize mới có file chuyển thành công sẽ thay giao dịch cũ; nếu không có file thành công thì giữ lịch sử cũ. Điều này cũng áp dụng cho lần chạy bị hủy/lỗi một phần.
- Undo lỗi hoặc bị hủy giữ lại các file chưa khôi phục để thử lại; khôi phục hết thì nút tắt.
- Undo không ghi đè vị trí nguồn đã bị chiếm và không tự đổi tên file khôi phục.

**Kiểm tra trước Undo:** file tại đích đã ghi phải còn tồn tại, là file thường, không phải thư mục/reparse point, và có kích thước cùng thời gian sửa UTC khớp thông tin lúc chuyển. Đây là kiểm tra metadata, **không phải hash nội dung hoặc định danh file**: thay nội dung nhưng giữ nguyên kích thước/thời gian sửa có thể không được phát hiện. File thiếu, có dấu hiệu thay đổi hoặc bị khóa sẽ báo lỗi và giữ bản ghi để thử lại. Thư mục nguồn đã mất cũng có thể khiến khôi phục thất bại. Các thư mục nhóm rỗng không bị tự xóa.

Lịch sử chỉ nằm trong bộ nhớ của phiên đang chạy, không được lưu để khôi phục sau khi thoát ứng dụng.

### Settings và cấu hình

Mở Settings khi không có tác vụ đang chạy. Mỗi nhóm có tên hiển thị, một tên thư mục con và các phần mở rộng. Ví dụ: nhóm `SourceCode`, thư mục `Sources`, phần mở rộng `cs, .cpp; .PY` được chuẩn hóa thành `.cs`, `.cpp`, `.py`.

- Không cho một phần mở rộng thuộc nhiều nhóm; tên nhóm và thư mục đích phải duy nhất.
- Thư mục đích phải là một tên thư mục con hợp lệ của Windows; không nhận đường dẫn tuyệt đối, `..` hoặc tên thiết bị dành riêng.
- Nhóm Others là fallback bắt buộc, không thể xóa hoặc gán phần mở rộng; có thể đổi tên hiển thị và tên thư mục.
- Save xác thực và ghi cấu hình thành công rồi mới cập nhật quy tắc đang dùng. Cancel/đóng cửa sổ bỏ bản nháp. Restore Defaults chỉ đổi bản nháp, cần Save để áp dụng.

Cấu hình được lưu tại:

```text
%LOCALAPPDATA%\SmartFileOrganizer\settings.json
```

Chưa có file cấu hình thì ứng dụng dùng mặc định, không tự tạo file khi mở. Nếu JSON hỏng hoặc quy tắc không hợp lệ, ứng dụng cảnh báo và dùng mặc định tạm thời; giữ file cũ và yêu cầu xác nhận trước khi Save thay thế. Lưu thất bại giữ nguyên cấu hình đang hoạt động.

## Ví dụ với nhóm mặc định

Nhóm mặc định: `Images`, `Documents`, `Videos`, `Music`, `Archives`, `Applications`, `Others`.

Trước khi sắp xếp:

```text
ThuMuc/
├── ảnh.jpg
├── báo cáo.PDF
├── video.mp4
├── baihat.mp3
├── tailieu.zip
├── setup.exe
└── README
```

Sau khi chọn tất cả và Organize thành công:

```text
ThuMuc/
├── Images/ảnh.jpg
├── Documents/báo cáo.PDF
├── Videos/video.mp4
├── Music/baihat.mp3
├── Archives/tailieu.zip
├── Applications/setup.exe
└── Others/README
```

## Phạm vi và giới hạn

- Chỉ quét file ở cấp thư mục hiện tại, không đệ quy vào thư mục con.
- Phân loại theo phần mở rộng cuối cùng, không phân tích nội dung; không phân biệt hoa/thường. File không có phần mở rộng hoặc không khớp quy tắc vào Others. Ví dụ `archive.tar.gz` được xét theo `.gz`.
- Xem trước là đường dẫn dự kiến, không thay đổi hệ thống file. Xung đột được kiểm tra lại lúc di chuyển; tên thực tế có thể khác nếu đích thay đổi. Một xung đột phát sinh đúng lúc di chuyển sẽ được báo lỗi để thử lại, không ghi đè.
- Quyền truy cập, file đang bị khóa hoặc đường dẫn không còn tồn tại có thể làm một số thao tác thất bại; cần giải quyết nguyên nhân rồi thử lại.
- Mã nguồn hiện chạy qua Visual Studio/.NET SDK. Chưa cung cấp bộ cài hoặc bản thực thi độc lập để tải về.

## Cấu trúc project

```text
SmartFileOrganizer.slnx
SmartFileOrganizer/
├── Forms/       # MainForm, Settings, About
├── Services/    # Quét, di chuyển, Undo, lưu/kiểm tra cấu hình
├── Models/      # File, nhóm, cấu hình và kết quả tác vụ
├── Helpers/     # Chọn file, định dạng dung lượng, hỗ trợ cửa sổ
├── Assets/      # Icon ứng dụng
└── Program.cs
SmartFileOrganizer.Tests/  # Console runner và kiểm tra hồi quy
```

Tác giả: **Hoàng Phúc**.
