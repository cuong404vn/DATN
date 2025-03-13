<?php
// Import các file cần thiết
include_once './config/database.php';
include_once './models/Quest.php';

class QuestController {
    private $db;
    private $quest;
    
    public function __construct() {
        // Kết nối database
        $database = new Database();
        $this->db = $database->getConnection();
        $this->quest = new Quest($this->db);
    }
    
    // Lấy danh sách quest
    public function getQuests($userID) {
        $this->quest->UserID = $userID;
        $stmt = $this->quest->getQuests();
        
        $quests = array();
        
        while($row = $stmt->fetch(PDO::FETCH_ASSOC)) {
            $quest_data = json_decode($row['QuestData'], true);
            
            $quests[] = array(
                "questID" => $row['QuestID'],
                "status" => $row['QuestStatus'],
                "completionTime" => $row['CompletionTime'],
                "title" => $quest_data['title'],
                "description" => $quest_data['description'],
                "progress" => isset($quest_data['progress']) ? $quest_data['progress'] : null,
                "rewards" => $quest_data['rewards']
            );
        }
        
        http_response_code(200);
        echo json_encode(array(
            "status" => "success",
            "quests" => $quests
        ));
    }
    
    // Cập nhật quest
    public function updateQuest() {
        // Lấy dữ liệu từ request
        $data = json_decode(file_get_contents("php://input"));
        
        // Validate dữ liệu
        if(empty($data->userID) || empty($data->questID) || empty($data->status) || empty($data->questData)) {
            http_response_code(400);
            echo json_encode(array("status" => "error", "message" => "Thiếu thông tin quest"));
            return;
        }
        
        // Set các giá trị
        $this->quest->UserID = $data->userID;
        $this->quest->QuestID = $data->questID;
        $this->quest->QuestStatus = $data->status;
        $this->quest->QuestData = json_encode($data->questData);
        
        // Cập nhật quest
        if($this->quest->updateQuest()) {
            http_response_code(200);
            echo json_encode(array(
                "status" => "success",
                "message" => "Cập nhật quest thành công"
            ));
        } else {
            http_response_code(500);
            echo json_encode(array("status" => "error", "message" => "Cập nhật quest thất bại"));
        }
    }
}
?> 