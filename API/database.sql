

-- Bảng Users: Lưu thông tin người dùng
CREATE TABLE IF NOT EXISTS Users (
    UserID INT PRIMARY KEY AUTO_INCREMENT,  -- ID định danh người dùng
    Username VARCHAR(50) UNIQUE NOT NULL,   -- Tên đăng nhập, không trùng lặp
    Password VARCHAR(256) NOT NULL,         -- Password đã mã hóa
    Email VARCHAR(100) UNIQUE NOT NULL,     -- Email người dùng
    NgayTaoTK DATETIME DEFAULT CURRENT_TIMESTAMP, -- Ngày tạo tài khoản
    LastLogin DATETIME                      -- Lần đăng nhập cuối
);

-- Bảng GameProgress: Lưu tiến độ game
CREATE TABLE IF NOT EXISTS GameProgress (
    ProgressID INT PRIMARY KEY AUTO_INCREMENT, -- ID tiến độ
    UserID INT,                               -- Liên kết với Users
    CurrentMap VARCHAR(50),                   -- Map hiện tại
    TotalStars INT DEFAULT 0,                 -- Tổng số sao
    LastSaveTime DATETIME,                    -- Thời điểm lưu cuối
    MapUnlocked TEXT,                         -- JSON chứa trạng thái các map
    FOREIGN KEY (UserID) REFERENCES Users(UserID)
);

-- Bảng Inventory: Lưu thông tin túi đồ
CREATE TABLE IF NOT EXISTS Inventory (
    InventoryID INT PRIMARY KEY AUTO_INCREMENT, -- ID túi đồ
    UserID INT,                                -- Liên kết với Users
    ItemID INT,                                -- ID của item
    Quantity INT DEFAULT 0,                    -- Số lượng item
    LastUpdated DATETIME,                      -- Cập nhật lần cuối
    FOREIGN KEY (UserID) REFERENCES Users(UserID)
);

-- Bảng QuestProgress: Lưu tiến trình nhiệm vụ
CREATE TABLE IF NOT EXISTS QuestProgress (
    QuestID INT PRIMARY KEY AUTO_INCREMENT,    -- ID nhiệm vụ
    UserID INT,                                -- Liên kết với Users
    QuestStatus ENUM('NOT_STARTED',            -- Trạng thái nhiệm vụ
                    'IN_PROGRESS',
                    'COMPLETED'),
    CompletionTime DATETIME,                   -- Thời gian hoàn thành
    QuestData TEXT,                            -- JSON chứa dữ liệu nhiệm vụ
    FOREIGN KEY (UserID) REFERENCES Users(UserID)
);

-- Thêm dữ liệu mẫu cho bảng Users
INSERT INTO Users (Username, Password, Email) VALUES
('test_user', '$2y$10$92IXUNpkjO0rOQ5byMi.Ye4oKoEa3Ro9llC/.og/at2.uheWG/igi', 'test@example.com');

-- Thêm dữ liệu mẫu cho bảng GameProgress
INSERT INTO GameProgress (UserID, CurrentMap, TotalStars, LastSaveTime, MapUnlocked) VALUES
(1, 'map1', 3, NOW(), '[{"mapID":"map1","status":"completed","stars":3,"highScore":150},{"mapID":"map2","status":"unlocked","stars":0,"highScore":0}]');

-- Thêm dữ liệu mẫu cho bảng Inventory
INSERT INTO Inventory (UserID, ItemID, Quantity, LastUpdated) VALUES
(1, 1, 5, NOW()),
(1, 2, 2, NOW());

-- Thêm dữ liệu mẫu cho bảng QuestProgress
INSERT INTO QuestProgress (UserID, QuestStatus, CompletionTime, QuestData) VALUES
(1, 'COMPLETED', NOW(), '{"title":"Nhiệm vụ đầu tiên","description":"Hoàn thành map 1","rewards":{"exp":100,"items":[{"itemID":1,"quantity":2}]}}'),
(1, 'IN_PROGRESS', NULL, '{"title":"Nhiệm vụ thứ hai","description":"Tiêu diệt 10 quái","progress":{"current":5,"required":10},"rewards":{"exp":200,"items":[{"itemID":2,"quantity":1}]}}'); 