# Hệ thống đặt bàn và gọi món

Dự án ASP.NET Core MVC (.NET 10), dùng SQL Server. **S1-01 Task 1** gồm đăng nhập quản lý bằng tên tài khoản hoặc số điện thoại và truy vết người sửa tên món/giá. **Task 2** bổ sung khóa đăng nhập 15 phút sau 5 lần sai trong 15 phút.

## Yêu cầu

- Git.
- .NET SDK 10.0.
- SQL Server LocalDB trên Windows (có thể cài qua Visual Studio Installer), hoặc một SQL Server riêng mà bạn có quyền tạo database.
- Trình soạn thảo tùy chọn; nếu dùng Visual Studio, cần phiên bản hỗ trợ .NET 10 và workload ASP.NET.

## Clone và chạy

Repository của nhóm: https://github.com/DwongNg-05/HeThongDatBan-GoiMon. Với repository Private, thành viên cần chấp nhận lời mời trước khi clone.

```bash
git clone https://github.com/DwongNg-05/HeThongDatBan-GoiMon.git
cd HeThongDatBan-GoiMon
dotnet restore
dotnet build --no-restore
dotnet run --project HeThongDatBan-GoiMon --launch-profile http
```

Mở http://localhost:5184. Ứng dụng chuyển đến trang đăng nhập. Dừng ứng dụng bằng Ctrl+C.

Trong môi trường Development, lần chạy đầu tự áp dụng migration, tạo database `HeThongDatBanGoiMon` trên `(localdb)\MSSQLLocalDB` và tạo dữ liệu demo. Mỗi máy có database riêng; dữ liệu SQL Server không được đưa lên GitHub. Dữ liệu đã sửa được giữ lại khi khởi động lại.

Có thể mở `HeThongDatBan-GoiMon.slnx` trong Visual Studio để phát triển.

## Demo Task S1-01

| Tên đăng nhập | Số điện thoại | Mật khẩu demo | Trạng thái |
| --- | --- | --- | --- |
| `quanly1` | `0900000001` | `Demo@12345` | Hoạt động |
| `quanly2` | `0900000002` | `Demo@12345` | Hoạt động |

Đây là thông tin **công khai dành riêng cho demo**. Database chỉ lưu `PasswordHash`, được tạo bằng ASP.NET Core `PasswordHasher` với salt riêng cho từng tài khoản. Không dùng các tài khoản demo cho môi trường thật. Seeder chỉ chạy khi môi trường là Development và `DemoData:Enabled=true`; không tự tạo lại mật khẩu hoặc kích hoạt lại tài khoản đã sửa.

1. Đăng nhập bằng `quanly1` và mật khẩu demo. Kiểm tra thông báo thành công và tên tài khoản trên thanh điều hướng.
2. Vào **Thực đơn → Chỉnh sửa**, sửa tên món rồi lưu.
3. Mở **Lịch sử thay đổi**: kiểm tra tên/ID người thực hiện, thời gian Việt Nam và giá trị trước/sau.
4. Đăng xuất; đăng nhập bằng `0900000002` và mật khẩu demo. Sửa giá một món và kiểm tra lịch sử ghi `quanly2`.
5. Đăng xuất. Thử tên tài khoản/số điện thoại không tồn tại hoặc sai mật khẩu khi tài khoản chưa bị khóa: cùng hiển thị **Tên đăng nhập/số điện thoại hoặc mật khẩu không hợp lệ.** Task 2 sử dụng lại thông báo chung, không còn thông báo riêng xác nhận tài khoản không tồn tại.

Tên đăng nhập không phân biệt chữ hoa/thường; khoảng trắng đầu/cuối tên đăng nhập hoặc số điện thoại được bỏ qua. Số điện thoại được nhập đúng dạng đã lưu, ví dụ `0900000001`. Mật khẩu được kiểm tra nguyên trạng, phân biệt chữ hoa/thường.

Phiên đăng nhập dùng cookie bảo vệ bởi ASP.NET Core Data Protection, chứa ID, tên tài khoản và vai trò quản lý; không chứa mật khẩu/hash. Đây là cookie phiên trình duyệt, với giới hạn tuyệt đối của vé xác thực là 14 ngày. Chưa có hết phiên sau 30 phút không thao tác. Tài khoản ngừng hoạt động không thể đăng nhập; phiên đang có cũng bị từ chối ở yêu cầu tiếp theo.

## Demo Task 2 — Khóa đăng nhập tạm thời

1. Dùng `quanly1` hoặc `0900000001`, nhập sai mật khẩu 4 lần: nhận thông báo lỗi chung và vẫn có thể thử lại.
2. Nhập sai lần thứ 5 trong 15 phút: tài khoản bị khóa đăng nhập 15 phút, giao diện hiển thị đồng hồ **15:00** đếm ngược.
3. Nhập đúng `Demo@12345` khi đang khóa: vẫn bị từ chối, giao diện hiển thị thời gian còn lại. Thử lại không kéo dài thời gian khóa.
4. Chờ hết 15 phút rồi nhập đúng: đăng nhập thành công và xóa các lần sai liên quan. Có thể dùng `quanly2` để tiếp tục demo các chức năng khác trong lúc `quanly1` bị khóa.

Quy tắc được áp dụng ở máy chủ và lưu trong SQL Server:

- Bảng `FailedLoginAttempts` lưu `AccountId` và `OccurredAtUtc` cho từng lần sai mật khẩu của tài khoản đang hoạt động. Tên đăng nhập và số điện thoại dùng chung lịch sử của tài khoản đó.
- Chỉ tính các lần có thời điểm **lớn hơn hiện tại trừ 15 phút và không vượt quá hiện tại**; lần sai đúng ở ranh giới 15 phút không còn được tính. Các bản ghi cũ được dọn ở lần thử tiếp theo.
- `ManagerAccounts.LockoutEndUtc` lưu thời điểm hết khóa (UTC). Khóa tạm thời không đổi trạng thái quản trị `IsActive` và không đăng xuất phiên đã có.
- Trong thời gian khóa, yêu cầu bị từ chối không ghi thêm lần sai và không gia hạn khóa. Sau khi hết khóa, lần thử mới bắt đầu chu kỳ mới; đăng nhập thành công xóa toàn bộ lần sai và thời điểm khóa.
- Thời gian còn lại được làm tròn lên tới giây và hiển thị dạng phút:giây, kể cả khi không bật JavaScript. JavaScript cập nhật đếm ngược; SQL Server và đồng hồ máy chủ quyết định khi nào được đăng nhập. Khi đổi sang tài khoản khác, đồng hồ cũ được ẩn để tránh nhầm lẫn.
- Không tạo bản ghi lỗi cho tài khoản không tồn tại, không tiết lộ sự tồn tại qua thông báo. Tài khoản ngừng hoạt động cũng nhận lỗi chung.
- Mỗi yêu cầu đăng nhập khóa cập nhật bản ghi tài khoản trong giao dịch SQL Server để các lần thử đồng thời không làm thất thoát số lần sai.

Migration `AddLoginLockout` chỉ thêm bảng/cột, giữ nguyên tài khoản, món và lịch sử thay đổi hiện có. Khi chạy ở Development, ứng dụng tự cập nhật database. Khi xem SQL Server bằng SSMS, dùng `(localdb)\MSSQLLocalDB`, chọn database `HeThongDatBanGoiMon` rồi Refresh bảng.

Phạm vi thực đơn của task này chỉ gồm danh sách mẫu và sửa tên/giá để chứng minh truy vết; chưa triển khai CRUD thực đơn đầy đủ. Lịch sử hiển thị 100 thay đổi gần nhất, database giữ toàn bộ lịch sử. Các luồng sửa thực đơn tiếp theo cần đi qua `MenuService` hoặc bổ sung cơ chế truy vết tương đương. Người thực hiện được lấy từ phiên đăng nhập, không nhận từ dữ liệu gửi lên. Cập nhật món và lịch sử được lưu trong cùng giao dịch; `RowVersion` ngăn ghi đè khi hai người cùng sửa một phiên bản món.

## Kết nối SQL Server khác

Có thể tạo file `HeThongDatBan-GoiMon/appsettings.Local.json` để cấu hình riêng cho mỗi máy trong môi trường Development. File này đã được loại khỏi Git. Ví dụ máy có instance `MSSQLSERVER07`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost\\MSSQLSERVER07;Database=HeThongDatBanGoiMon;Trusted_Connection=True;TrustServerCertificate=True"
  }
}
```

Khi dùng cấu hình này, ứng dụng kết nối SQL Server trực tiếp và không khởi động LocalDB. Trong SSMS, nhập `localhost\MSSQLSERVER07`, chọn Windows Authentication và database `HeThongDatBanGoiMon`. Biến môi trường và tham số dòng lệnh vẫn có ưu tiên cao hơn file cấu hình riêng. Chuyển server chỉ đổi nơi kết nối, không tự chuyển dữ liệu; cần sao chép dữ liệu trước nếu muốn giữ tài khoản, thực đơn và lịch sử cũ.

Cấu hình mặc định nằm ở `HeThongDatBan-GoiMon/appsettings.json`. Có thể dùng biến môi trường PowerShell để thay thế mà không sửa file chung:

```powershell
$env:ConnectionStrings__DefaultConnection = 'Server=TEN_MAY\TEN_INSTANCE;Database=HeThongDatBanGoiMon;Trusted_Connection=True;TrustServerCertificate=True'
dotnet run --project HeThongDatBan-GoiMon --launch-profile http
```

Trong chuỗi PowerShell thực tế, dùng **một** dấu `\` giữa tên máy và instance (ví dụ `Server=.\SQLEXPRESS`). Nếu dùng SQL Authentication, lưu cấu hình chứa mật khẩu bằng biến môi trường hoặc User Secrets; không commit vào Git. `TrustServerCertificate=True` chỉ phục vụ kết nối phát triển cục bộ; cấu hình chứng chỉ phù hợp khi triển khai thật.

Nếu gặp lỗi không kết nối được LocalDB, kiểm tra đã cài SQL Server Express LocalDB và chạy `SqlLocalDB start MSSQLLocalDB`, hoặc cấu hình instance SQL Server của máy như trên.

Ở Development trên Windows, ứng dụng chủ động chạy `SqlLocalDB start`, đọc địa chỉ named pipe hiện tại bằng `SqlLocalDB info` và dùng địa chỉ đó để kết nối. Nếu đầu ra không chứa địa chỉ hợp lệ, ứng dụng đọc địa chỉ instance tương ứng từ registry của người dùng Windows hiện tại. Mỗi địa chỉ được kiểm tra kết nối tới `master` trước khi dùng cho migration; không lấy nhầm địa chỉ của instance khác. Địa chỉ được đọc lại mỗi lần khởi động, không ghi cố định vào cấu hình và không thay đổi database. Các kết nối SQL Server thông thường không đi qua bước này. Nếu công cụ LocalDB hoặc kết nối vẫn lỗi, ứng dụng báo lỗi kèm đầu ra công cụ để chẩn đoán, không bỏ qua bước cập nhật database. Tham khảo [SqlLocalDB utility của Microsoft](https://learn.microsoft.com/en-us/sql/tools/sqllocaldb-utility).

## Migration và kiểm thử

Công cụ EF Core được cố định phiên bản trong `.config/dotnet-tools.json`:

```bash
dotnet tool restore
dotnet ef database update --project HeThongDatBan-GoiMon
dotnet test HeThongDatBan-GoiMon.slnx
```

Không bắt buộc chạy lệnh migration thủ công khi demo ở Development. Môi trường khác không tự migrate/seed; cần chuẩn bị database và tài khoản riêng trước khi triển khai.

Bộ kiểm thử tích hợp dùng SQL Server thật (mặc định LocalDB), tạo database riêng tên `HeThongDatBanGoiMon_Tests_<GUID>` và xóa database đó sau khi hoàn tất. Không dùng database demo. Có thể đặt `TEST_SQLSERVER_CONNECTION` để chạy trên SQL Server khác; tài khoản kiểm thử cần quyền tạo/xóa database. Tên database luôn được thay bằng tên ngẫu nhiên riêng của bộ test.

Các tình huống được kiểm tra: đăng nhập bằng tên/số điện thoại; lỗi chung khi sai thông tin; hash có salt; đúng định danh trong cookie; hai tài khoản ghi lịch sử riêng; ngăn giả mạo người sửa; từ chối tài khoản ngừng hoạt động; chặn truy cập khi chưa đăng nhập; đăng xuất; chống CSRF; không chuyển hướng tới website bên ngoài; kiểm tra dữ liệu đầu vào và xung đột cập nhật.

Kiểm thử Task 2 dùng `TimeProvider` giả lập để kiểm tra 1–4 lần sai, khóa ở lần thứ 5, thời gian còn lại, từ chối mật khẩu đúng trong lúc khóa, hết khóa đúng mốc 15 phút, cửa sổ thời gian trượt, làm sạch sau thành công và nhiều yêu cầu đồng thời. Đồng hồ giả lập chỉ có trong dự án kiểm thử; ứng dụng thật dùng thời gian hệ thống và khóa đủ 15 phút.

Nếu có Node.js, chạy thêm kiểm thử đồng hồ trên giao diện bằng `node --test HeThongDatBan-GoiMon.Tests/login-lockout.test.cjs` (không cần cài thư viện npm). Node.js chỉ phục vụ kiểm thử JavaScript, không bắt buộc để chạy ứng dụng.

Tham khảo triển khai: [cookie authentication](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/cookie?view=aspnetcore-10.0), [PasswordHasher](https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.identity.passwordhasher-1?view=aspnetcore-10.0), [EF Core SQL Server](https://learn.microsoft.com/en-us/ef/core/providers/sql-server/).

## Cấu trúc

- `HeThongDatBan-GoiMon/Controllers`: xử lý yêu cầu.
- `HeThongDatBan-GoiMon/Models`: mô hình dữ liệu.
- `HeThongDatBan-GoiMon/Data` và `Migrations`: database, dữ liệu demo và cấu trúc SQL Server.
- `HeThongDatBan-GoiMon/Services`: xác thực và lưu thay đổi có truy vết.
- `HeThongDatBan-GoiMon/Views`: giao diện Razor.
- `HeThongDatBan-GoiMon/wwwroot`: CSS, JavaScript và tài nguyên tĩnh.
- `HeThongDatBan-GoiMon/Program.cs`: cấu hình và khởi chạy ứng dụng.
- `HeThongDatBan-GoiMon.Tests`: kiểm thử tích hợp đăng nhập và truy vết trên SQL Server.

## Cách làm việc nhóm

Mỗi công việc dùng một nhánh riêng, ví dụ:

```bash
git switch main
git pull --ff-only origin main
git switch -c feature/dat-ban
```

Sau khi hoàn thành, kiểm tra build, commit các file liên quan và đẩy nhánh:

```bash
dotnet build
git add <cac-file-da-thay-doi>
git commit -m "Them chuc nang dat ban"
git push -u origin feature/dat-ban
```

Tạo Pull Request vào `main` trên GitHub để nhóm xem xét trước khi gộp. Thay các giá trị trong dấu `<...>` bằng thông tin thực tế.

Không commit mật khẩu, token hay chuỗi kết nối có thông tin đăng nhập. Dùng biến môi trường hoặc .NET User Secrets cho cấu hình bí mật. Các thư mục build và thiết lập IDE cá nhân đã được loại khỏi Git.
