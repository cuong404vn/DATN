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
    

    public function getInventory() {
        $query = "SELECT * FROM " . $this->table_name . " 
                  WHERE UserID = ?";
 
        $stmt = $this->conn->prepare($query);
        $stmt->bindParam(1, $this->UserID);
        $stmt->execute();
 
        return $stmt;
    }
    

    public function updateItem() {

        $query = "SELECT InventoryID, Quantity FROM " . $this->table_name . " 
                  WHERE UserID = ? AND ItemID = ?";
 
        $stmt = $this->conn->prepare($query);
        $stmt->bindParam(1, $this->UserID);
        $stmt->bindParam(2, $this->ItemID);
        $stmt->execute();
 
        if($stmt->rowCount() > 0) {

            $row = $stmt->fetch(PDO::FETCH_ASSOC);
            $current_quantity = $row['Quantity'];
            $new_quantity = $current_quantity + $this->Quantity;
            

            if($new_quantity <= 0) {
                $query = "DELETE FROM " . $this->table_name . " 
                          WHERE InventoryID = ?";
                          
                $stmt = $this->conn->prepare($query);
                $stmt->bindParam(1, $row['InventoryID']);
                
                return $stmt->execute();
            } else {

                $query = "UPDATE " . $this->table_name . " 
                          SET Quantity = ?, LastUpdated = NOW() 
                          WHERE InventoryID = ?";
                          
                $stmt = $this->conn->prepare($query);
                $stmt->bindParam(1, $new_quantity);
                $stmt->bindParam(2, $row['InventoryID']);
                
                return $stmt->execute();
            }
        } else {

            if($this->Quantity <= 0) {
                return false; 
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