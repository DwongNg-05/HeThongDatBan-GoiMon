# Hệ thống đặt bàn và gọi món

Dự án ASP.NET Core MVC (.NET 10), dùng SQL Server. Task **S1-01** bổ sung đăng nhập quản lý bằng tên tài khoản hoặc số điện thoại và truy vết người sửa tên món/giá.

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
5. Đăng xuất. Thử tên tài khoản hoặc số điện thoại không tồn tại: hiển thị **Tài khoản không tồn tại !**. Nếu tài khoản tồn tại nhưng sai mật khẩu hoặc ngừng hoạt động, hiển thị **Tên đăng nhập/số điện thoại hoặc mật khẩu không hợp lệ.** Theo yêu cầu cập nhật, thông báo phân biệt tài khoản không tồn tại, thay thế AC ban đầu về lỗi chung không tiết lộ sự tồn tại của tài khoản.

Tên đăng nhập không phân biệt chữ hoa/thường; khoảng trắng đầu/cuối tên đăng nhập hoặc số điện thoại được bỏ qua. Số điện thoại được nhập đúng dạng đã lưu, ví dụ `0900000001`. Mật khẩu được kiểm tra nguyên trạng, phân biệt chữ hoa/thường.

Phiên đăng nhập dùng cookie bảo vệ bởi ASP.NET Core Data Protection, chứa ID, tên tài khoản và vai trò quản lý; không chứa mật khẩu/hash. Đây là cookie phiên trình duyệt, với giới hạn tuyệt đối của vé xác thực là 14 ngày. Không có bộ đếm khóa sau 5 lần sai và không có hết phiên sau 30 phút không thao tác. Tài khoản ngừng hoạt động không thể đăng nhập; phiên đang có cũng bị từ chối ở yêu cầu tiếp theo.

Phạm vi thực đơn của task này chỉ gồm danh sách mẫu và sửa tên/giá để chứng minh truy vết; chưa triển khai CRUD thực đơn đầy đủ. Lịch sử hiển thị 100 thay đổi gần nhất, database giữ toàn bộ lịch sử. Các luồng sửa thực đơn tiếp theo cần đi qua `MenuService` hoặc bổ sung cơ chế truy vết tương đương. Người thực hiện được lấy từ phiên đăng nhập, không nhận từ dữ liệu gửi lên. Cập nhật món và lịch sử được lưu trong cùng giao dịch; `RowVersion` ngăn ghi đè khi hai người cùng sửa một phiên bản món.

## Kết nối SQL Server khác

Cấu hình mặc định nằm ở `HeThongDatBan-GoiMon/appsettings.json`. Có thể dùng biến môi trường PowerShell để thay thế mà không sửa file chung:

```powershell
$env:ConnectionStrings__DefaultConnection = 'Server=TEN_MAY\TEN_INSTANCE;Database=HeThongDatBanGoiMon;Trusted_Connection=True;TrustServerCertificate=True'
dotnet run --project HeThongDatBan-GoiMon --launch-profile http
```

Trong chuỗi PowerShell thực tế, dùng **một** dấu `\` giữa tên máy và instance (ví dụ `Server=.\SQLEXPRESS`). Nếu dùng SQL Authentication, lưu cấu hình chứa mật khẩu bằng biến môi trường hoặc User Secrets; không commit vào Git. `TrustServerCertificate=True` chỉ phục vụ kết nối phát triển cục bộ; cấu hình chứng chỉ phù hợp khi triển khai thật.

Nếu gặp lỗi không kết nối được LocalDB, kiểm tra đã cài SQL Server Express LocalDB và chạy `SqlLocalDB start MSSQLLocalDB`, hoặc cấu hình instance SQL Server của máy như trên.

## Migration và kiểm thử

Công cụ EF Core được cố định phiên bản trong `.config/dotnet-tools.json`:

```bash
dotnet tool restore
dotnet ef database update --project HeThongDatBan-GoiMon
dotnet test HeThongDatBan-GoiMon.slnx
```

Không bắt buộc chạy lệnh migration thủ công khi demo ở Development. Môi trường khác không tự migrate/seed; cần chuẩn bị database và tài khoản riêng trước khi triển khai.

Bộ kiểm thử tích hợp dùng SQL Server thật (mặc định LocalDB), tạo database riêng tên `HeThongDatBanGoiMon_Tests_<GUID>` và xóa database đó sau khi hoàn tất. Không dùng database demo. Có thể đặt `TEST_SQLSERVER_CONNECTION` để chạy trên SQL Server khác; tài khoản kiểm thử cần quyền tạo/xóa database. Tên database luôn được thay bằng tên ngẫu nhiên riêng của bộ test.

Các tình huống được kiểm tra: đăng nhập bằng tên/số điện thoại; thông báo tài khoản không tồn tại và thông báo sai mật khẩu; hash có salt; đúng định danh trong cookie; hai tài khoản ghi lịch sử riêng; ngăn giả mạo người sửa; từ chối tài khoản ngừng hoạt động; chặn truy cập khi chưa đăng nhập; đăng xuất; chống CSRF; không chuyển hướng tới website bên ngoài; không khóa sau 6 lần sai; kiểm tra dữ liệu đầu vào và xung đột cập nhật.

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
