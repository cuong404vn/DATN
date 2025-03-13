<?php
class Inventory {
    private $conn;
    private $table_name = "Inventory";
 
    public $InventoryID;
    public $UserID;
    public $ItemID;
    public $Quantity;
    public $LastUpdated;
 
    public function __construct($db) {
        $this->conn = $db;
    }
    
    // Lấy inventory của user
    public function getInventory() {
        $query = "SELECT * FROM " . $this->table_name . " 
                  WHERE UserID = ?";
 
        $stmt = $this->conn->prepare($query);
        $stmt->bindParam(1, $this->UserID);
        $stmt->execute();
 
        return $stmt;
    }
    
    // Thêm/cập nhật item trong inventory
    public function updateItem() {
        // Kiểm tra xem item đã có trong inventory chưa
        $query = "SELECT InventoryID, Quantity FROM " . $this->table_name . " 
                  WHERE UserID = ? AND ItemID = ?";
 
        $stmt = $this->conn->prepare($query);
        $stmt->bindParam(1, $this->UserID);
        $stmt->bindParam(2, $this->ItemID);
        $stmt->execute();
 
        if($stmt->rowCount() > 0) {
            // Item đã tồn tại, cập nhật số lượng
            $row = $stmt->fetch(PDO::FETCH_ASSOC);
            $current_quantity = $row['Quantity'];
            $new_quantity = $current_quantity + $this->Quantity;
            
            // Nếu số lượng <= 0, xóa item
            if($new_quantity <= 0) {
                $query = "DELETE FROM " . $this->table_name . " 
                          WHERE InventoryID = ?";
                          
                $stmt = $this->conn->prepare($query);
                $stmt->bindParam(1, $row['InventoryID']);
                
                return $stmt->execute();
            } else {
                // Cập nhật số lượng
                $query = "UPDATE " . $this->table_name . " 
                          SET Quantity = ?, LastUpdated = NOW() 
                          WHERE InventoryID = ?";
                          
                $stmt = $this->conn->prepare($query);
                $stmt->bindParam(1, $new_quantity);
                $stmt->bindParam(2, $row['InventoryID']);
                
                return $stmt->execute();
            }
        } else {
            // Item chưa tồn tại, thêm mới
            if($this->Quantity <= 0) {
                return false; // Không thêm item với số lượng <= 0
            }
            
            $query = "INSERT INTO " . $this->table_name . " 
                      SET UserID = ?, ItemID = ?, Quantity = ?, LastUpdated = NOW()";
                      
            $stmt = $this->conn->prepare($query);
            $stmt->bindParam(1, $this->UserID);
            $stmt->bindParam(2, $this->ItemID);
            $stmt->bindParam(3, $this->Quantity);
            
            return $stmt->execute();
        }
    }
}
?> 