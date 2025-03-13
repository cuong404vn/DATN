<?php
// Import các file cần thiết
include_once './config/database.php';
include_once './models/Inventory.php';

class InventoryController {
    private $db;
    private $inventory;
    
    public function __construct() {
        // Kết nối database
        $database = new Database();
        $this->db = $database->getConnection();
        $this->inventory = new Inventory($this->db);
    }
    
    // Lấy inventory
    public function getInventory($userID) {
        $this->inventory->UserID = $userID;
        $stmt = $this->inventory->getInventory();
        
        $items = array();
        
        while($row = $stmt->fetch(PDO::FETCH_ASSOC)) {
            $items[] = array(
                "itemID" => $row['ItemID'],
                "quantity" => $row['Quantity']
            );
        }
        
        // Thông tin về capacity
        $capacity = 5; // Giới hạn 5 slot
        $usedSpace = count($items);
        
        http_response_code(200);
        echo json_encode(array(
            "status" => "success",
            "items" => $items,
            "capacity" => $capacity,
            "usedSpace" => $usedSpace
        ));
    }
    
    // Cập nhật inventory
    public function updateInventory() {
        // Lấy dữ liệu từ request
        $data = json_decode(file_get_contents("php://input"));
        
        // Validate dữ liệu
        if(empty($data->userID) || empty($data->itemID) || !isset($data->quantity) || empty($data->action)) {
            http_response_code(400);
            echo json_encode(array("status" => "error", "message" => "Thiếu thông tin inventory"));
            return;
        }
        
        // Set các giá trị
        $this->inventory->UserID = $data->userID;
        $this->inventory->ItemID = $data->itemID;
        
        // Xác định số lượng dựa vào action
        if($data->action == "add") {
            $this->inventory->Quantity = abs($data->quantity);
        } else if($data->action == "remove") {
            $this->inventory->Quantity = -abs($data->quantity);
        } else {
            http_response_code(400);
            echo json_encode(array("status" => "error", "message" => "Action không hợp lệ"));
            return;
        }
        
        // Cập nhật inventory
        if($this->inventory->updateItem()) {
            http_response_code(200);
            echo json_encode(array(
                "status" => "success",
                "message" => "Cập nhật inventory thành công"
            ));
        } else {
            http_response_code(500);
            echo json_encode(array("status" => "error", "message" => "Cập nhật inventory thất bại"));
        }
    }
}
?> 