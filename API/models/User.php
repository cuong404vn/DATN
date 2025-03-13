<?php
class User {
    private $conn;
    private $table_name = "Users";
 
    public $UserID;
    public $Username;
    public $Password;
    public $Email;
    public $NgayTaoTK;
    public $LastLogin;
 
    public function __construct($db) {
        $this->conn = $db;
    }
    
    // Đăng ký user mới
    public function create() {
        $query = "INSERT INTO " . $this->table_name . " 
                  SET Username = :username, 
                      Password = :password, 
                      Email = :email";
 
        $stmt = $this->conn->prepare($query);
 
        // Sanitize
        $this->Username = htmlspecialchars(strip_tags($this->Username));
        $this->Email = htmlspecialchars(strip_tags($this->Email));
        
        // Hash password
        $this->Password = password_hash($this->Password, PASSWORD_DEFAULT);
 
        // Bind values
        $stmt->bindParam(":username", $this->Username);
        $stmt->bindParam(":password", $this->Password);
        $stmt->bindParam(":email", $this->Email);
 
        if($stmt->execute()) {
            return true;
        }
        return false;
    }
    
    // Kiểm tra đăng nhập
    public function login() {
        $query = "SELECT UserID, Username, Password FROM " . $this->table_name . " 
                  WHERE Username = ?";
 
        $stmt = $this->conn->prepare($query);
        $stmt->bindParam(1, $this->Username);
        $stmt->execute();
 
        if($stmt->rowCount() > 0) {
            $row = $stmt->fetch(PDO::FETCH_ASSOC);
            if(password_verify($this->Password, $row['Password'])) {
                $this->UserID = $row['UserID'];
                return true;
            }
        }
        return false;
    }
    
    // Kiểm tra username đã tồn tại chưa
    public function usernameExists() {
        $query = "SELECT UserID FROM " . $this->table_name . " 
                  WHERE Username = ?";
 
        $stmt = $this->conn->prepare($query);
        $stmt->bindParam(1, $this->Username);
        $stmt->execute();
 
        return $stmt->rowCount() > 0;
    }
    
    // Cập nhật thời gian đăng nhập
    public function updateLastLogin() {
        $query = "UPDATE " . $this->table_name . " 
                  SET LastLogin = NOW() 
                  WHERE UserID = ?";
 
        $stmt = $this->conn->prepare($query);
        $stmt->bindParam(1, $this->UserID);
        
        return $stmt->execute();
    }
}
?> 