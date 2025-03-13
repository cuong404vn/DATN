<?php
// Import các file cần thiết
include_once './config/database.php';
include_once './models/User.php';
include_once './models/GameProgress.php';

class AuthController {
    private $db;
    private $user;
    
    public function __construct() {
        // Kết nối database
        $database = new Database();
        $this->db = $database->getConnection();
        $this->user = new User($this->db);
    }
    
    // Xử lý đăng ký
    public function register() {
        // Lấy dữ liệu từ request
        $data = json_decode(file_get_contents("php://input"));
        
        // Validate dữ liệu
        if(empty($data->username) || empty($data->password) || empty($data->email)) {
            http_response_code(400);
            echo json_encode(array("status" => "error", "message" => "Thiếu thông tin đăng ký"));
            return;
        }
        
        // Kiểm tra username đã tồn tại chưa
        $this->user->Username = $data->username;
        if($this->user->usernameExists()) {
            http_response_code(400);
            echo json_encode(array("status" => "error", "message" => "Username đã tồn tại"));
            return;
        }
        
        // Set các giá trị
        $this->user->Password = $data->password;
        $this->user->Email = $data->email;
        
        // Tạo user
        if($this->user->create()) {
            // Tạo token
            $token = bin2hex(random_bytes(16));
            
            http_response_code(200);
            echo json_encode(array(
                "status" => "success",
                "message" => "Đăng ký thành công",
                "token" => $token,
                "userData" => array(
                    "username" => $this->user->Username
                )
            ));
        } else {
            http_response_code(500);
            echo json_encode(array("status" => "error", "message" => "Đăng ký thất bại"));
        }
    }
    
    // Xử lý đăng nhập
    public function login() {
        // Lấy dữ liệu từ request
        $data = json_decode(file_get_contents("php://input"));
        
        // Validate dữ liệu
        if(empty($data->username) || empty($data->password)) {
            http_response_code(400);
            echo json_encode(array("status" => "error", "message" => "Thiếu thông tin đăng nhập"));
            return;
        }
        
        // Set các giá trị
        $this->user->Username = $data->username;
        $this->user->Password = $data->password;
        
        // Kiểm tra đăng nhập
        if($this->user->login()) {
            // Cập nhật thời gian đăng nhập
            $this->user->updateLastLogin();
            
            // Tạo token
            $token = bin2hex(random_bytes(16));
            
            // Lấy thông tin game progress
            $game_progress = new GameProgress($this->db);
            $game_progress->UserID = $this->user->UserID;
            $game_progress->getProgress();
            
            http_response_code(200);
            echo json_encode(array(
                "status" => "success",
                "message" => "Đăng nhập thành công",
                "token" => $token,
                "userData" => array(
                    "userID" => $this->user->UserID,
                    "username" => $this->user->Username,
                    "currentMap" => $game_progress->CurrentMap,
                    "totalStars" => $game_progress->TotalStars
                )
            ));
        } else {
            http_response_code(401);
            echo json_encode(array("status" => "error", "message" => "Thông tin đăng nhập không đúng"));
        }
    }
}
?> 