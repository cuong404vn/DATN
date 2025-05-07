# HƯỚNG DẪN TRIỂN KHAI API GAME DATN

## 1. TỔNG QUAN HỆ THỐNG

API Game DATN là một RESTful API được xây dựng bằng PHP thuần, sử dụng mô hình MVC (Model-View-Controller) để quản lý các chức năng của một trò chơi. Hệ thống này bao gồm các chức năng chính:

- **Xác thực người dùng**: Đăng ký, đăng nhập
- **Quản lý tiến trình game**: Lưu và lấy thông tin tiến trình của người chơi
- **Quản lý kho đồ (Inventory)**: Thêm, xóa, xem vật phẩm trong game
- **Quản lý nhiệm vụ (Quest)**: Theo dõi và cập nhật trạng thái các nhiệm vụ

### 1.1. Kiến trúc hệ thống

```
├── config/                 # Cấu hình hệ thống
│   ├── cors.php            # Cấu hình CORS
│   └── database.php        # Cấu hình kết nối CSDL
├── controllers/            # Xử lý logic nghiệp vụ
│   ├── AuthController.php  # Xử lý xác thực
│   ├── GameController.php  # Xử lý tiến trình game
│   ├── InventoryController.php # Xử lý kho đồ
│   └── QuestController.php # Xử lý nhiệm vụ
├── middleware/             # Middleware
│   └── Authentication.php  # Xác thực token
├── models/                 # Mô hình dữ liệu
│   ├── GameProgress.php    # Mô hình tiến trình game
│   ├── Inventory.php       # Mô hình kho đồ
│   ├── Quest.php           # Mô hình nhiệm vụ
│   └── User.php            # Mô hình người dùng
├── api.php                 # Entry point chính cho API
├── index.php               # Entry point thay thế
├── database.sql            # Cấu trúc CSDL
└── HUONG_DAN_TRIEN_KHAI.md # File hướng dẫn này
```

## 2. YÊU CẦU HỆ THỐNG

### 2.1. Yêu cầu phần cứng
- Không có yêu cầu đặc biệt về phần cứng, máy chủ thông thường đủ để chạy

### 2.2. Yêu cầu phần mềm
- PHP 7.4 trở lên
- MySQL 5.7 trở lên
- Web server (Apache/Nginx)
- Hỗ trợ PDO PHP extension
- mod_rewrite (cho Apache) hoặc tương đương (cho Nginx)

## 3. CẤU TRÚC CƠ SỞ DỮ LIỆU

### 3.1. Bảng Users
```sql
CREATE TABLE IF NOT EXISTS Users (
    UserID INT PRIMARY KEY AUTO_INCREMENT,  
    Username VARCHAR(50) UNIQUE NOT NULL,   
    Password VARCHAR(256) NOT NULL,         
    Email VARCHAR(100) UNIQUE NOT NULL,    
    NgayTaoTK DATETIME DEFAULT CURRENT_TIMESTAMP,
    LastLogin DATETIME                      
);
```

### 3.2. Bảng GameProgress
```sql
CREATE TABLE IF NOT EXISTS GameProgress (
    ProgressID INT PRIMARY KEY AUTO_INCREMENT, 
    UserID INT,                              
    CurrentMap VARCHAR(50),                  
    TotalStars INT DEFAULT 0,                
    LastSaveTime DATETIME,                 
    MapUnlocked TEXT,                        
    FOREIGN KEY (UserID) REFERENCES Users(UserID)
);
```

### 3.3. Bảng Inventory
```sql
CREATE TABLE IF NOT EXISTS Inventory (
    InventoryID INT PRIMARY KEY AUTO_INCREMENT, 
    UserID INT,                               
    ItemID INT,                               
    Quantity INT DEFAULT 0,                  
    LastUpdated DATETIME,                     
    FOREIGN KEY (UserID) REFERENCES Users(UserID)
);
```

### 3.4. Bảng QuestProgress
```sql
CREATE TABLE IF NOT EXISTS QuestProgress (
    QuestID INT PRIMARY KEY AUTO_INCREMENT,
    UserID INT,                               
    QuestStatus ENUM('NOT_STARTED',           
                    'IN_PROGRESS',
                    'COMPLETED'),
    CompletionTime DATETIME,                 
    QuestData TEXT,                            
    FOREIGN KEY (UserID) REFERENCES Users(UserID)
);
```

## 4. HƯỚNG DẪN CÀI ĐẶT

### 4.1. Chuẩn bị môi trường

#### 4.1.1. Cài đặt XAMPP (Windows)
1. Tải XAMPP từ [trang chủ](https://www.apachefriends.org/download.html)
2. Cài đặt XAMPP với các thành phần Apache, MySQL, PHP
3. Khởi động dịch vụ Apache và MySQL từ XAMPP Control Panel

#### 4.1.2. Cài đặt LAMP (Linux)
```bash
# Ubuntu/Debian
sudo apt update
sudo apt install apache2 mysql-server php libapache2-mod-php php-mysql

# CentOS/RHEL
sudo yum install httpd mysql-server php php-mysqlnd
sudo systemctl start httpd
sudo systemctl start mysqld
```

### 4.2. Triển khai mã nguồn

1. Clone hoặc tải mã nguồn vào thư mục web server
   ```bash
   # Ví dụ: với XAMPP trên Windows
   git clone <repository_url> C:/xampp/htdocs/game-api
   
   # Hoặc copy thư mục API vào htdocs
   ```

2. Đảm bảo web server có quyền đọc/ghi vào thư mục

### 4.3. Thiết lập cơ sở dữ liệu

1. Tạo database mới
   ```sql
   CREATE DATABASE game_database;
   USE game_database;
   ```

2. Import cấu trúc database từ file database.sql
   ```bash
   # Sử dụng phpMyAdmin:
   # Mở phpMyAdmin -> Chọn database -> Tab Import -> Chọn file database.sql -> Import
   
   # Hoặc sử dụng command line:
   mysql -u username -p game_database < /path/to/database.sql
   ```

3. Cập nhật thông tin kết nối trong file `config/database.php`
   ```php
   private $host = "localhost";
   private $db_name = "game_database"; // Tên database của bạn
   private $username = "root"; // Tên người dùng MySQL
   private $password = ""; // Mật khẩu MySQL
   ```

### 4.4. Cấu hình Web Server

#### 4.4.1. Apache (với .htaccess)
Tạo file `.htaccess` trong thư mục gốc của project:
```
RewriteEngine On
RewriteCond %{REQUEST_FILENAME} !-f
RewriteCond %{REQUEST_FILENAME} !-d
RewriteRule ^(.*)$ api.php [QSA,L]
```

#### 4.4.2. Nginx
```nginx
server {
    listen 80;
    server_name example.com;
    root /path/to/game-api;

    location / {
        try_files $uri $uri/ /api.php?$query_string;
    }

    location ~ \.php$ {
        include fastcgi_params;
        fastcgi_param SCRIPT_FILENAME $document_root$fastcgi_script_name;
        fastcgi_pass unix:/var/run/php/php7.4-fpm.sock;
        fastcgi_param PATH_INFO $fastcgi_path_info;
    }
}
```

## 5. CHI TIẾT CÁC ENDPOINT API

### 5.1. Xác thực (Authentication)

#### 5.1.1. Đăng ký
- **URL**: `/auth/register`
- **Method**: POST
- **Body**:
  ```json
  {
    "username": "new_user",
    "password": "secure_password",
    "email": "user@example.com"
  }
  ```
- **Thành công**:
  ```json
  {
    "status": "success",
    "message": "Đăng ký thành công",
    "token": "generated_token",
    "userData": {
      "username": "new_user"
    }
  }
  ```

#### 5.1.2. Đăng nhập
- **URL**: `/auth/login`
- **Method**: POST
- **Body**:
  ```json
  {
    "username": "existing_user",
    "password": "correct_password"
  }
  ```
- **Thành công**:
  ```json
  {
    "status": "success",
    "message": "Đăng nhập thành công",
    "token": "generated_token",
    "userData": {
      "userID": 1,
      "username": "existing_user",
      "currentMap": "ToaThanh",
      "totalStars": 5
    }
  }
  ```

### 5.2. Tiến trình Game (Game Progress)

#### 5.2.1. Lấy tiến trình
- **URL**: `/progress/get/{userID}`
- **Method**: GET
- **Headers**: `Authorization: Bearer {token}`
- **Thành công**:
  ```json
  {
    "status": "success",
    "currentMap": "ToaThanh",
    "totalStars": 5,
    "unlockedMaps": [
      {
        "mapID": "ToaThanh",
        "status": "completed",
        "stars": 3,
        "highScore": 150
      },
      {
        "mapID": "KhuRung",
        "status": "unlocked",
        "stars": 2,
        "highScore": 120
      }
    ]
  }
  ```

#### 5.2.2. Lưu tiến trình
- **URL**: `/progress/save`
- **Method**: POST
- **Headers**: `Authorization: Bearer {token}`
- **Body**:
  ```json
  {
    "userID": 1,
    "mapID": "KhuRung",
    "score": 180,
    "stars": 3,
    "status": "completed"
  }
  ```
- **Thành công**:
  ```json
  {
    "status": "success",
    "message": "Lưu tiến độ thành công",
    "totalStars": 8,
    "unlockedMaps": [
      {
        "mapID": "ToaThanh",
        "status": "completed",
        "stars": 3,
        "highScore": 150
      },
      {
        "mapID": "KhuRung",
        "status": "completed",
        "stars": 3,
        "highScore": 180
      },
      {
        "mapID": "LongDat",
        "status": "unlocked",
        "stars": 0,
        "highScore": 0
      }
    ]
  }
  ```

### 5.3. Kho đồ (Inventory)

#### 5.3.1. Lấy thông tin kho đồ
- **URL**: `/inventory/get/{userID}`
- **Method**: GET
- **Headers**: `Authorization: Bearer {token}`
- **Thành công**:
  ```json
  {
    "status": "success",
    "items": [
      {
        "itemID": 1,
        "quantity": 5
      },
      {
        "itemID": 2,
        "quantity": 2
      }
    ],
    "capacity": 5,
    "usedSpace": 2
  }
  ```

#### 5.3.2. Cập nhật kho đồ
- **URL**: `/inventory/update`
- **Method**: POST
- **Headers**: `Authorization: Bearer {token}`
- **Body**:
  ```json
  {
    "userID": 1,
    "itemID": 3,
    "quantity": 1,
    "action": "add"
  }
  ```
- **Thành công**:
  ```json
  {
    "status": "success",
    "message": "Cập nhật inventory thành công"
  }
  ```

### 5.4. Nhiệm vụ (Quest)

#### 5.4.1. Lấy thông tin nhiệm vụ
- **URL**: `/quests/get/{userID}`
- **Method**: GET
- **Headers**: `Authorization: Bearer {token}`
- **Thành công**:
  ```json
  {
    "status": "success",
    "quests": [
      {
        "questID": 1,
        "status": "COMPLETED",
        "completionTime": "2025-05-07 10:30:00",
        "title": "Nhiệm vụ đầu tiên",
        "description": "Hoàn thành map 1",
        "rewards": {
          "exp": 100,
          "items": [
            {
              "itemID": 1,
              "quantity": 2
            }
          ]
        }
      },
      {
        "questID": 2,
        "status": "IN_PROGRESS",
        "completionTime": null,
        "title": "Nhiệm vụ thứ hai",
        "description": "Tiêu diệt 10 quái",
        "progress": {
          "current": 5,
          "required": 10
        },
        "rewards": {
          "exp": 200,
          "items": [
            {
              "itemID": 2,
              "quantity": 1
            }
          ]
        }
      }
    ]
  }
  ```

#### 5.4.2. Cập nhật nhiệm vụ
- **URL**: `/quests/update`
- **Method**: POST
- **Headers**: `Authorization: Bearer {token}`
- **Body**:
  ```json
  {
    "userID": 1,
    "questID": 2,
    "status": "IN_PROGRESS",
    "questData": {
      "title": "Nhiệm vụ thứ hai",
      "description": "Tiêu diệt 10 quái",
      "progress": {
        "current": 7,
        "required": 10
      },
      "rewards": {
        "exp": 200,
        "items": [
          {
            "itemID": 2,
            "quantity": 1
          }
        ]
      }
    }
  }
  ```
- **Thành công**:
  ```json
  {
    "status": "success",
    "message": "Cập nhật quest thành công"
  }
  ```


## 7. HƯỚNG DẪN KIỂM THỬ API

### 7.1. Sử dụng Postman

1. Tải và cài đặt [Postman](https://www.postman.com/downloads/)
2. Tạo collection mới cho API game
3. Thiết lập các request với các thông số như đã mô tả ở phần 5

### 7.2. Sử dụng cURL

Dưới đây là các ví dụ sử dụng cURL để kiểm thử API:

#### Đăng ký
```bash
curl -X POST \
  http://localhost/game-api/auth/register \
  -H 'Content-Type: application/json' \
  -d '{
    "username": "test_user",
    "password": "password123",
    "email": "test@example.com"
}'
```

#### Đăng nhập
```bash
curl -X POST \
  http://localhost/game-api/auth/login \
  -H 'Content-Type: application/json' \
  -d '{
    "username": "test_user",
    "password": "password123"
}'
```

#### Lấy tiến trình game
```bash
curl -X GET \
  http://localhost/game-api/progress/get/1 \
  -H 'Authorization: Bearer your_token_here'
```

### 7.3. Kiểm thử tự động

Có thể triển khai kiểm thử tự động bằng PHPUnit để đảm bảo tính ổn định của API:

1. Cài đặt PHPUnit:
```bash
composer require --dev phpunit/phpunit
```

2. Tạo thư mục tests và viết các test case:
```php
// tests/AuthTest.php
use PHPUnit\Framework\TestCase;

class AuthTest extends TestCase
{
    public function testRegister()
    {

    }
    
    public function testLogin()
    {
       
      
    }
}
