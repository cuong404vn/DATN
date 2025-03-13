<?php
// Import các file cần thiết
include_once './config/database.php';
include_once './models/GameProgress.php';

class GameController {
    private $db;
    private $game_progress;
    
    public function __construct() {
        // Kết nối database
        $database = new Database();
        $this->db = $database->getConnection();
        $this->game_progress = new GameProgress($this->db);
    }
    
    // Lấy tiến độ game
    public function getProgress($userID) {
        $this->game_progress->UserID = $userID;
        
        if($this->game_progress->getProgress()) {
            // Chuyển đổi MapUnlocked từ JSON string sang array
            $mapUnlocked = json_decode($this->game_progress->MapUnlocked, true);
            
            http_response_code(200);
            echo json_encode(array(
                "status" => "success",
                "currentMap" => $this->game_progress->CurrentMap,
                "totalStars" => $this->game_progress->TotalStars,
                "unlockedMaps" => $mapUnlocked
            ));
        } else {
            // Chưa có tiến độ, trả về mặc định
            http_response_code(200);
            echo json_encode(array(
                "status" => "success",
                "currentMap" => "map1",
                "totalStars" => 0,
                "unlockedMaps" => array(
                    array(
                        "mapID" => "map1",
                        "status" => "unlocked",
                        "stars" => 0,
                        "highScore" => 0
                    )
                )
            ));
        }
    }
    
    // Lưu tiến độ game
    public function saveProgress() {
        // Lấy dữ liệu từ request
        $data = json_decode(file_get_contents("php://input"));
        
        // Validate dữ liệu
        if(empty($data->userID) || empty($data->mapID) || !isset($data->score) || !isset($data->stars)) {
            http_response_code(400);
            echo json_encode(array("status" => "error", "message" => "Thiếu thông tin tiến độ"));
            return;
        }
        
        // Set các giá trị
        $this->game_progress->UserID = $data->userID;
        $this->game_progress->CurrentMap = $data->mapID;
        
        // Lấy tiến độ hiện tại
        $this->game_progress->getProgress();
        
        // Cập nhật tổng số sao
        $currentStars = $this->game_progress->TotalStars ?: 0;
        $mapUnlocked = json_decode($this->game_progress->MapUnlocked, true) ?: array();
        
        // Kiểm tra xem map đã tồn tại trong danh sách chưa
        $mapExists = false;
        $oldStars = 0;
        
        foreach($mapUnlocked as &$map) {
            if($map['mapID'] == $data->mapID) {
                $mapExists = true;
                $oldStars = $map['stars'];
                
                // Cập nhật thông tin map
                $map['status'] = $data->status;
                $map['stars'] = max($map['stars'], $data->stars); // Lấy số sao cao nhất
                $map['highScore'] = max($map['highScore'], $data->score); // Lấy điểm cao nhất
                break;
            }
        }
        
        // Nếu map chưa tồn tại, thêm mới
        if(!$mapExists) {
            $mapUnlocked[] = array(
                "mapID" => $data->mapID,
                "status" => $data->status,
                "stars" => $data->stars,
                "highScore" => $data->score
            );
        }
        
        // Cập nhật tổng số sao
        $this->game_progress->TotalStars = $currentStars + ($data->stars - $oldStars);
        $this->game_progress->MapUnlocked = json_encode($mapUnlocked);
        
        // Lưu tiến độ
        if($this->game_progress->saveProgress()) {
            http_response_code(200);
            echo json_encode(array(
                "status" => "success",
                "message" => "Lưu tiến độ thành công",
                "totalStars" => $this->game_progress->TotalStars
            ));
        } else {
            http_response_code(500);
            echo json_encode(array("status" => "error", "message" => "Lưu tiến độ thất bại"));
        }
    }
}
?> 