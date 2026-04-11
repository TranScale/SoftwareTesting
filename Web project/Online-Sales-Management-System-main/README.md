# Online-Sales-Management-System

## TiDB + Code First (ASP.NET Core)

Project đã được cấu hình để chạy với TiDB qua EF Core (Pomelo MySQL provider).

### 1) Cấu hình kết nối

File `Online Sales Management System/appsettings.json`:

- `ConnectionStrings:DefaultConnection`: trỏ tới TiDB endpoint.
- `Database:InitializationMode`: mặc định là `EnsureCreatedOnce` để tạo schema theo model (Code First) đúng 1 lần, các lần chạy sau sẽ bỏ qua.
- `Database:AutoResetOnMissingTables`: tự reset schema 1 lần nếu phát hiện DB bị tạo dở (thiếu bảng sau lần chạy lỗi).
- TLS hiện đang để `SslMode=Required` (đã mã hóa kết nối). Nếu bạn muốn verify CA chặt hơn, có thể đổi connection string theo chứng chỉ CA của TiDB Cloud.

Mật khẩu đang đặt trong `Online Sales Management System/appsettings.Development.json` tại key:

- `TiDB:Password`

### 2) Chạy dự án

Trong VS Code:

- Nhấn `Ctrl + Shift + B` để chạy task `Run Web (dotnet watch)`.

### 3) Nếu muốn dùng EF Migrations thay cho EnsureCreated

1. Xóa migration SQL Server cũ trong thư mục `Online Sales Management System/Migrations` (giữ lại thư mục, xóa các file `.cs` bên trong).
2. Đổi `Database:InitializationMode` thành `Migrate`.
3. Tạo migration mới theo provider MySQL/TiDB:

```bash
dotnet ef migrations add InitTiDb --project "Online Sales Management System/Online Sales Management System.csproj"
```

4. Chạy lại project để apply migration.

### 4) Chạy web thuần (không đụng DB init)

Đặt:

- `Database:InitializationMode = None`
