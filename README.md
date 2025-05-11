# Ứng dụng Demo Đăng nhập ASP.NET Core

Ứng dụng demo ASP.NET Core này cho phép người dùng đăng nhập bằng Google OAuth và username/password thông thường.

## Yêu cầu

- .NET 8.0 SDK trở lên
- SQL Server (có thể sử dụng LocalDB)
- Tài khoản Google Cloud để cấu hình OAuth

## Cài đặt

1. Clone hoặc tải xuống dự án
2. Mở terminal và di chuyển đến thư mục dự án
3. Sao chép file cấu hình mẫu và điền thông tin của bạn:
   ```
   cp Login/appsettings.Example.json Login/appsettings.json
   ```
4. Thiết lập User Secrets để lưu thông tin nhạy cảm (xem phần User Secrets bên dưới)
5. Chạy migrations để tạo cơ sở dữ liệu:
   ```
   dotnet ef database update
   ```
6. Cấu hình Google OAuth (xem phần bên dưới)
7. Chạy ứng dụng:
   ```
   dotnet run
   ```

## Bảo mật thông tin nhạy cảm

### User Secrets

ASP.NET Core cung cấp công cụ User Secrets để lưu thông tin nhạy cảm mà không cần lưu trong mã nguồn. Để sử dụng:

1. Khởi tạo user secrets trong dự án:
   ```
   cd Login
   dotnet user-secrets init
   ```

2. Thêm thông tin nhạy cảm:
   ```
   dotnet user-secrets set "Authentication:Google:ClientId" "your-client-id"
   dotnet user-secrets set "Authentication:Google:ClientSecret" "your-client-secret"
   dotnet user-secrets set "Authentication:Jwt:Key" "your-secure-jwt-key-at-least-16-characters-long"
   ```

3. Hoặc bạn có thể tạo file `secrets.json` trong thư mục User Secrets. Vị trí của thư mục này:
   - Windows: `%APPDATA%\Microsoft\UserSecrets\<user_secrets_id>\secrets.json`
   - macOS/Linux: `~/.microsoft/usersecrets/<user_secrets_id>/secrets.json`

   ID của User Secrets có thể tìm thấy trong file `Login.csproj`.

   Sao chép nội dung từ `secrets.json.Example` và điền thông tin thực tế vào file này.

### Gitignore

Dự án này sử dụng `.gitignore` để không theo dõi file cấu hình chứa thông tin nhạy cảm. Các file sau không nên được commit lên Git:

- `Login/appsettings.json` - Chứa cấu hình chính
- `Login/appsettings.Development.json` - Cấu hình môi trường phát triển
- `Login/appsettings.Production.json` - Cấu hình môi trường sản xuất
- `Login/appsettings.Staging.json` - Cấu hình môi trường dàn dựng
- Thư mục User Secrets - Đã được tự động loại trừ

## Cấu hình Google OAuth

Để sử dụng tính năng đăng nhập bằng Google, bạn cần tạo một OAuth 2.0 Client ID trong Google Cloud Console:

1. Truy cập [Google Cloud Console](https://console.cloud.google.com/)
2. Tạo một project mới hoặc chọn project hiện có
3. Truy cập vào **APIs & Services > Credentials**
4. Nhấp vào **Create Credentials** và chọn **OAuth client ID**
5. Chọn **Web application** làm loại ứng dụng
6. Đặt tên cho ứng dụng của bạn
7. Thêm các địa chỉ vào **Authorized JavaScript origins**:
   - `http://localhost:5059` (cho HTTP)
   - `https://localhost:7009` (cho HTTPS)
   - `http://localhost`
8. Thêm các địa chỉ vào **Authorized redirect URIs**:
   - `http://localhost:5059/signin-google`
   - `https://localhost:7009/signin-google`
   - `http://localhost:5059/callback`
   - `https://localhost:7009/callback`
   - `http://localhost:5059`
   - `https://localhost:7009`
   
   Redirect URIs là các đường dẫn mà Google OAuth sẽ chuyển hướng người dùng sau khi họ đăng nhập qua Google. Đây là điểm cuối nơi mà ứng dụng của bạn sẽ nhận mã xác thực từ Google.
9. Nhấp vào **Create**
10. Sao chép **Client ID** và **Client Secret**
11. Lưu thông tin này vào User Secrets như hướng dẫn ở phần trên

## Tính năng

- Đăng nhập với Google OAuth
- Đăng ký và đăng nhập bằng username/password
- Bảo vệ API bằng JWT token
- Dashboard hiển thị thông tin người dùng đăng nhập
- Cơ sở dữ liệu SQL Server lưu trữ thông tin người dùng
- RESTful API cho xác thực

## Cấu trúc dự án

- **Controllers/**: Chứa các controller cho API và giao diện
- **Models/**: Các mô hình dữ liệu
- **Services/**: Các dịch vụ xác thực
- **Data/**: Cấu hình Entity Framework Core
- **Views/**: Giao diện người dùng

## API Endpoints

### Xác thực
- **POST /api/auth/google**: Đăng nhập bằng Google OAuth
- **POST /api/auth/register**: Đăng ký người dùng mới
- **POST /api/auth/login**: Đăng nhập bằng username/password
- **GET /api/auth/me**: Lấy thông tin người dùng hiện tại (yêu cầu xác thực)

## Ghi chú bảo mật

- Thay đổi JWT Key trong môi trường production
- Sử dụng HTTPS trong môi trường production
- Không nên sử dụng người dùng sa cho SQL Server trong môi trường production
- Đảm bảo cấp phép đúng quyền cho cơ sở dữ liệu
- Không commit các thông tin nhạy cảm như Client ID, Client Secret và JWT Key lên Git 
