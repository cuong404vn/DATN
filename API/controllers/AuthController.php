<?php

include_once './config/database.php';
include_once './models/User.php';
include_once './models/GameProgress.php';

class AuthController {
    private $db;
    private $user;
    
    public function __construct() {

        $database = new Database();
        $this->db = $database->getConnection();
        $this->user = new User($this->db);
    }
    

    public function register() {

        $data = json_decode(file_get_contents("php://input"));
        

        if(empty($data->username) || empty($data->password) || empty($data->email)) {
            http_response_code(400);
            echo json_encode(array("status" => "error", "message" => "Thiếu thông tin đăng ký"));
            return;
        }
        

        $this->user->Username = $data->username;
        if($this->user->usernameExists()) {
            http_response_code(400);
            echo json_encode(array("status" => "error", "message" => "Username đã tồn tại"));
            return;
        }
        

        $this->user->Password = $data->password;
        $this->user->Email = $data->email;
        

        if($this->user->create()) {

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
    

    public function login() {

        $data = json_decode(file_get_contents("php://input"));
        

        if(empty($data->username) || empty($data->password)) {
            http_response_code(400);
            echo json_encode(array("status" => "error", "message" => "Thiếu thông tin đăng nhập"));
            return;
        }
        

        $this->user->Username = $data->username;
        $this->user->Password = $data->password;
        

        if($this->user->login()) {

            $this->user->updateLastLogin();
            

            $token = bin2hex(random_bytes(16));
            

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