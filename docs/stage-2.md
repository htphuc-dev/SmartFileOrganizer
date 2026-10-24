# Giai đoạn 2 — tác vụ nền, tiến trình và hủy

## Thay đổi

- MainForm chụp đường dẫn và danh sách file trên UI thread rồi dùng `await Task.Run(...)` để quét metadata/di chuyển trên worker. Chỉ event handler dùng `async void`; các luồng xử lý giao diện trả `Task`. Services vẫn là mã xử lý file đồng bộ, không phụ thuộc WinForms.
- Chỉ chạy một tác vụ tại một thời điểm. Trong lúc chạy, khóa ô đường dẫn, Browse, Scan Files và Organize; bật Cancel. Mỗi lần chạy có CancellationTokenSource riêng, được giải phóng trong finally sau khi worker kết thúc thật sự.
- Thêm ProgressBar và Cancel vào khu vực trạng thái, giữ phần nội dung xanh–trắng hiện tại. Tăng chiều cao Form 40 px để giữ nguyên kích thước danh sách. Các control và sự kiện được khai báo trong Designer; không sửa resource hoặc thêm package.
- Quét bằng `EnumerateFiles` trực tiếp trong thư mục, không quét con, không có lượt quét riêng để đếm. Dùng tiến trình marquee và số file đã đọc. Chỉ công bố danh sách đầy đủ khi quét thành công; lỗi hoặc hủy đều xóa kết quả và yêu cầu quét lại.
- Sắp xếp hiển thị số đã xử lý/tổng số, moved và failed; lỗi một file vẫn được tính là đã xử lý. Cancel dừng trước file tiếp theo, không ngắt hoặc hoàn tác File.Move đang chạy. File vừa di chuyển xong luôn được ghi nhận trước khi kiểm tra yêu cầu hủy tiếp theo.
- `FileOrganizationResult` giữ toàn bộ FileMoveResult theo thứ tự đầu vào cùng số lượng, trạng thái hủy và lỗi bất ngờ. Kết quả được lưu trước khi báo tiến trình, vì vậy việc nhận callback chậm hoặc callback thất bại không làm mất thông tin file đã di chuyển. Không truyền token cho chính Task.Run của batch để tránh bỏ qua bước trả kết quả.
- Services giới hạn báo tiến trình khoảng 100 ms/lần, ngoài thông báo đầu/cuối. `FileOrganizationProgress` chứa bản sao các kết quả mới kể từ lần báo trước. Progress được tạo trên UI thread; callback chỉ cập nhật các dòng liên quan. Không dựng lại toàn bộ DataGridView sau từng file.
- Khi worker kết thúc, MainForm ngừng nhận progress của lần chạy đó rồi đối chiếu lại toàn bộ kết quả. Callback đến muộn không ghi đè trạng thái hoàn thành, trạng thái Cancelling hoặc tác vụ mới. Dòng thành công bị loại khỏi danh sách chờ; dòng lỗi giữ `Failed: <nguyên nhân>` và tooltip; dòng chưa thử hiển thị `Not processed`. Nếu dòng chưa thử từng lỗi ở lần chạy trước, tooltip vẫn giữ nguyên nhân cũ.
- Sau Cancel, hiển thị `Cancelling...`, vô hiệu hóa Cancel và đợi worker trước khi mở lại các nút. Hủy quét không cho Organize; hủy sắp xếp vẫn cho Organize tiếp các file còn lại. Tiến trình cuối là số file thực sự đã xử lý, không ép 100% khi còn file chưa thử. Nếu yêu cầu hủy đến sau file cuối cùng, báo hoàn thành vì mọi file đã có kết quả.
- Đóng Form lúc đang chạy sẽ yêu cầu hủy một lần và `await` việc hoàn tất tác vụ cùng đối chiếu kết quả trước khi đóng; không chặn UI thread, không hiện thêm hộp thoại đóng lặp lại. Cancel riêng không đóng Form. Callback kiểm tra tác vụ hiện hành, yêu cầu đóng và trạng thái Dispose.
- Giữ các bảo vệ Giai đoạn 1: đối chiếu đường dẫn đã quét, xác nhận trước di chuyển, không ghi đè, xử lý trùng tên với file/thư mục, báo lỗi từng file và không xử lý lại file thành công. Chưa thêm checkbox, Undo, quy tắc tùy chỉnh hoặc các chức năng ngoài phạm vi.

## Build và kiểm tra

Chạy từ thư mục chứa `SmartFileOrganizer.slnx`, bằng .NET SDK 10 trên Windows:

```powershell
dotnet build SmartFileOrganizer/SmartFileOrganizer.csproj --configuration Release
dotnet run --project SmartFileOrganizer.Tests/SmartFileOrganizer.Tests.csproj --configuration Release
```

Bộ kiểm tra console có sẵn vẫn được sử dụng, không thêm framework/package test. `Program.cs` giữ các kiểm tra Giai đoạn 1 và gọi thêm các kiểm tra trong `Stage2Tests.cs`. Mỗi trường hợp tạo thư mục GUID riêng dưới `%TEMP%\SmartFileOrganizer.Stage1Tests` và chỉ dọn dẹp bên trong thư mục tạm đã xác minh.

Các kiểm tra Giai đoạn 2 dùng IProgress chạy trực tiếp để hủy/gây lỗi ngay tại một mốc kết quả xác định, không dùng sleep hoặc điều kiện thời gian tùy ý:

1. Token đã hủy trước khi quét: không đọc dữ liệu hoặc báo tiến trình.
2. Hủy sau metadata file đầu tiên: ném OperationCanceledException và không trả danh sách một phần; có thể quét lại đầy đủ.
3. Token đã hủy trước batch: không di chuyển hoặc tạo thư mục đích.
4. Hủy sau kết quả đầu tiên: giữ kết quả đã chuyển, giữ nguyên nguồn chưa xử lý, tiến trình nhỏ hơn tổng. Chạy tiếp chỉ xử lý file còn lại và giữ nguyên các đích cũ, kể cả xung đột với file/thư mục.
5. Lỗi thiếu nguồn/đích bị chặn không dừng các file khác; số processed bao gồm thất bại. Khắc phục lỗi và thử lại chỉ các nguồn còn lại.
6. Callback tiến trình ném lỗi bất ngờ sau lần chuyển đầu: kết quả thực tế vẫn được giữ và có thể tiếp tục mà không xử lý lại file đã chuyển.
7. Token được hủy ngay sau file cuối: giữ đúng kết quả hoàn thành và nội dung.

Kết quả thực tế ngày 24/10/2026:

- Build Release: thành công, 0 cảnh báo, 0 lỗi.
- Cả bộ Giai đoạn 1 và 2: **23 đạt, 0 thất bại, 1 bỏ qua** (24 kiểm tra) trên Windows.
- Kiểm tra bị bỏ qua vẫn là thử quét thư mục bị từ chối quyền ở Giai đoạn 1: môi trường không cho thiết lập ACL của thư mục tạm. Kiểm tra file bị khóa độc quyền, bảo toàn nội dung và thử lại đã đạt.
- Chưa chạy tương tác giao diện và chưa mở Visual Studio Designer. Khả năng kéo/vẽ lại cửa sổ, thao tác Cancel, đóng Form khi đang chạy và hình thức hiển thị cần được xác minh theo checklist bên dưới. Bộ kiểm tra tự động xác minh xử lý file và kết quả hủy, không thay thế kiểm tra UI.

## Tạo dữ liệu mẫu nhiều file

Chạy đoạn PowerShell sau trong một cửa sổ riêng. Nó chỉ tạo thư mục mới có GUID dưới thư mục tạm và in ra đường dẫn để dán vào ứng dụng. Có thể điều chỉnh `$fileCount` nếu tác vụ hoàn tất quá nhanh để thử Cancel.

```powershell
$samplePath = Join-Path ([System.IO.Path]::GetTempPath()) ('SmartFileOrganizer.Stage2Demo-' + [guid]::NewGuid().ToString('N'))
$fileCount = 20000
[System.IO.Directory]::CreateDirectory($samplePath) | Out-Null
for ($index = 0; $index -lt $fileCount; $index++) {
    $extension = switch ($index % 4) { 0 { '.TXT' } 1 { '.PNG' } 2 { '.ZIP' } 3 { '' } }
    $fileName = 'tệp-{0:D6}{1}' -f $index, $extension
    [System.IO.File]::WriteAllText((Join-Path $samplePath $fileName), "Dữ liệu mẫu $index " + ('x' * 1024))
}
$documentsPath = Join-Path $samplePath 'Documents'
[System.IO.Directory]::CreateDirectory($documentsPath) | Out-Null
[System.IO.File]::WriteAllText((Join-Path $documentsPath 'tệp-000000.TXT'), 'Nội dung đích cũ phải giữ nguyên')
[System.IO.Directory]::CreateDirectory((Join-Path $documentsPath 'tệp-000000 (1).TXT')) | Out-Null
$samplePath
```

Các đuôi PNG/ZIP ở đây chỉ là tên phục vụ phân loại, nội dung là văn bản mẫu. Không dùng tài liệu cá nhân. Tạo bộ mẫu mới cho các lần kiểm tra cần danh sách đầy đủ, vì những file đã chuyển sẽ nằm trong thư mục con và không được quét lại.

## Kiểm tra thủ công trên Windows / Visual Studio

Mở solution, đặt SmartFileOrganizer làm startup project và mở MainForm bằng **View Designer**. Kiểm tra thanh tiến trình/Cancel trong khu vực trạng thái và bố cục cũ vẫn hiển thị đúng, rồi chạy ứng dụng.

1. **Đang quét:** Dán đường dẫn mẫu, bấm Scan Files rồi kéo cửa sổ/che và mở lại cửa sổ. Cửa sổ phải vẽ lại được; ô đường dẫn/Browse/Scan/Organize bị khóa; Cancel được bật. Thanh chạy marquee, status tăng số file đã đọc. Bảng chỉ nhận danh sách đầy đủ sau khi thành công.
2. **Cancel quét:** Khi đang quét, bấm Cancel. Status chuyển Cancelling, Cancel bị khóa; các nút còn lại chỉ mở sau khi worker dừng. Cuối cùng báo Scan cancelled, bảng trống, Organize bị khóa. Chờ thêm và thử quét lại để kiểm tra callback cũ không đổi trạng thái mới.
3. **Đang sắp xếp:** Quét thành công rồi bấm Organize/Yes. Kéo cửa sổ, quan sát thanh và status processed/total, moved, failed. Các dòng thành công biến mất dần theo nhóm tiến trình, không dựng lại cả bảng. Tên đích cũ trong Documents phải giữ nguyên và nguồn trùng tên phải chuyển sang hậu tố (2).
4. **Cancel sắp xếp:** Bấm Cancel khi còn nhiều file. Chờ Cancelling kết thúc; đối chiếu file thực tế với moved/failed. Dòng thành công phải mất khỏi danh sách, file thất bại giữ Failed và lỗi, file chưa thử hiển thị Not processed. Khi chưa xử lý hết, giá trị tiến trình không đạt 100%. Nếu thao tác đã hoàn tất trước khi nhận Cancel, báo hoàn thành là đúng.
5. **Tiếp tục sau hủy:** Không bấm Scan. Bấm Organize/Yes lần nữa. Chỉ những file còn trong bảng được xử lý, file đã thành công không sinh thêm bản hoặc bị báo thiếu nguồn. Tổng của lần chạy mới là số file còn lại. Kiểm tra nội dung nguồn/đích cũ được giữ nguyên.
6. **Đóng khi đang chạy:** Trên bộ mẫu mới, bắt đầu quét hoặc sắp xếp rồi bấm X. Cửa sổ yêu cầu hủy, vẫn phản hồi và chỉ đóng sau khi worker dừng. Bấm X lặp lại không tạo thêm hộp thoại. Mở lại ứng dụng/kiểm tra thư mục để xác nhận các file đã chuyển vẫn còn nguyên. Thử riêng nút Cancel để xác nhận nó không tự đóng Form.
7. **Lỗi từng file:** Dùng một thư mục tạm nhỏ khác với `retry.txt` và `photo.PNG`. Quét xong, tạo một **file** tên `Documents` trong thư mục nguồn (tạo sau khi quét để nó không thuộc danh sách xử lý). Organize/Yes: TXT phải Failed với tooltip đầy đủ; PNG vẫn thành công; failed được tính vào processed. Xóa file chặn Documents, không Scan, rồi Organize lại: TXT phải chuyển được. Có thể xóa một nguồn sau khi quét hoặc giữ nguồn bằng FileShare.None để thử lỗi thiếu file/khóa file.
8. **Bảo vệ Giai đoạn 1:** Quét A, đổi đường dẫn sang B (hoặc chọn lại qua Browse): xóa kết quả/lỗi và khóa Organize. Hủy hộp thoại Confirm Organization bằng No: không tạo đích/di chuyển nguồn và giữ danh sách. Thử sắp xếp cột trong bảng trước khi Organize để kiểm tra kết quả vẫn gắn đúng file.

Hủy chỉ được quan sát giữa các bước/file; một thao tác hệ thống file đang bị chặn phải kết thúc trước khi tác vụ xác nhận đã hủy. Tác vụ không cố ngắt File.Move hoặc hoàn tác file đã chuyển.

Thay đổi được giữ trong working tree, chưa commit hoặc push. Tài liệu Giai đoạn 1 hiện có được giữ nguyên.
