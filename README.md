# Hệ thống đặt bàn và gọi món

Dự án base ASP.NET Core MVC (.NET 10) để các thành viên clone và phát triển cùng nhau. Hiện chỉ có bộ khung mặc định, chưa có chức năng nghiệp vụ hoặc cơ sở dữ liệu.

## Yêu cầu

- Git.
- .NET SDK 10.0.
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

Mở http://localhost:5184. Dừng ứng dụng bằng Ctrl+C.

Có thể mở `HeThongDatBan-GoiMon.slnx` trong Visual Studio để phát triển.

## Cấu trúc

- `HeThongDatBan-GoiMon/Controllers`: xử lý yêu cầu.
- `HeThongDatBan-GoiMon/Models`: mô hình dữ liệu.
- `HeThongDatBan-GoiMon/Views`: giao diện Razor.
- `HeThongDatBan-GoiMon/wwwroot`: CSS, JavaScript và tài nguyên tĩnh.
- `HeThongDatBan-GoiMon/Program.cs`: cấu hình và khởi chạy ứng dụng.

## Database dùng chung

Schema database được quản lý bằng Entity Framework Core migration trong thư mục `HeThongDatBan-GoiMon/Migrations`. Không tạo hoặc sửa bảng thủ công bằng SSMS.

Mỗi thành viên dùng SQL Server database local tên `RestaurantDev`. Sau khi kéo code mới, chạy:

```bash
cd HeThongDatBan-GoiMon
dotnet tool restore
dotnet tool run dotnet-ef database update
```

Lệnh trên tạo hoặc cập nhật database theo toàn bộ migration đã được gộp vào `main`. Database local của từng người có thể có dữ liệu thử khác nhau, nhưng cấu trúc bảng sẽ giống nhau.

Khi cần thay đổi schema, tạo migration trên nhánh chức năng và đưa cả Model, DbContext và migration vào Pull Request. Không gửi file `.mdf` hoặc backup database qua nhóm.

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
