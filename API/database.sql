


CREATE TABLE IF NOT EXISTS Users (
    UserID INT PRIMARY KEY AUTO_INCREMENT,  
    Username VARCHAR(50) UNIQUE NOT NULL,   
    Password VARCHAR(256) NOT NULL,         
    Email VARCHAR(100) UNIQUE NOT NULL,    
    NgayTaoTK DATETIME DEFAULT CURRENT_TIMESTAMP,
    LastLogin DATETIME                      
);


CREATE TABLE IF NOT EXISTS GameProgress (
    ProgressID INT PRIMARY KEY AUTO_INCREMENT, 
    UserID INT,                              
    CurrentMap VARCHAR(50),                  
    TotalStars INT DEFAULT 0,                
    LastSaveTime DATETIME,                 
    MapUnlocked TEXT,                        
    FOREIGN KEY (UserID) REFERENCES Users(UserID)
);


CREATE TABLE IF NOT EXISTS Inventory (
    InventoryID INT PRIMARY KEY AUTO_INCREMENT, 
    UserID INT,                               
    ItemID INT,                               
    Quantity INT DEFAULT 0,                  
    LastUpdated DATETIME,                     
    FOREIGN KEY (UserID) REFERENCES Users(UserID)
);


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


INSERT INTO Users (Username, Password, Email) VALUES
('test_user', '$2y$10$92IXUNpkjO0rOQ5byMi.Ye4oKoEa3Ro9llC/.og/at2.uheWG/igi', 'test@example.com');


INSERT INTO GameProgress (UserID, CurrentMap, TotalStars, LastSaveTime, MapUnlocked) VALUES
(1, 'map1', 3, NOW(), '[{"mapID":"map1","status":"completed","stars":3,"highScore":150},{"mapID":"map2","status":"unlocked","stars":0,"highScore":0}]');


INSERT INTO Inventory (UserID, ItemID, Quantity, LastUpdated) VALUES
(1, 1, 5, NOW()),
(1, 2, 2, NOW());


INSERT INTO QuestProgress (UserID, QuestStatus, CompletionTime, QuestData) VALUES
(1, 'COMPLETED', NOW(), '{"title":"Nhiệm vụ đầu tiên","description":"Hoàn thành map 1","rewards":{"exp":100,"items":[{"itemID":1,"quantity":2}]}}'),
(1, 'IN_PROGRESS', NULL, '{"title":"Nhiệm vụ thứ hai","description":"Tiêu diệt 10 quái","progress":{"current":5,"required":10},"rewards":{"exp":200,"items":[{"itemID":2,"quantity":1}]}}'); 