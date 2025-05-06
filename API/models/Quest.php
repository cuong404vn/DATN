<?php
class Quest {
    private $conn;
    private $table_name = "QuestProgress";
 
    public $QuestID;
    public $UserID;
    public $QuestStatus;
    public $CompletionTime;
    public $QuestData;
 
    public function __construct($db) {
        $this->conn = $db;
    }
    

    public function getQuests() {
        $query = "SELECT * FROM " . $this->table_name . " 
                  WHERE UserID = ?";
 
        $stmt = $this->conn->prepare($query);
        $stmt->bindParam(1, $this->UserID);
        $stmt->execute();
 
        return $stmt;
    }
    

    public function updateQuest() {

        $query = "SELECT QuestID FROM " . $this->table_name . " 
                  WHERE UserID = ? AND QuestID = ?";
 
        $stmt = $this->conn->prepare($query);
        $stmt->bindParam(1, $this->UserID);
        $stmt->bindParam(2, $this->QuestID);
        $stmt->execute();
 
        if($stmt->rowCount() > 0) {

            $query = "UPDATE " . $this->table_name . " 
                      SET QuestStatus = :status";
            

            if($this->QuestStatus == 'COMPLETED') {
                $query .= ", CompletionTime = NOW()";
            }
            
            $query .= ", QuestData = :questData 
                      WHERE QuestID = :questID AND UserID = :userID";
        } else {

            $query = "INSERT INTO " . $this->table_name . " 
                      SET UserID = :userID, 
                          QuestID = :questID,
                          QuestStatus = :status, 
                          QuestData = :questData";
        }
 
        $stmt = $this->conn->prepare($query);
 

        $this->QuestStatus = htmlspecialchars(strip_tags($this->QuestStatus));
        $this->QuestData = htmlspecialchars(strip_tags($this->QuestData));
 

        $stmt->bindParam(":userID", $this->UserID);
        $stmt->bindParam(":questID", $this->QuestID);
        $stmt->bindParam(":status", $this->QuestStatus);
        $stmt->bindParam(":questData", $this->QuestData);
 
        return $stmt->execute();
    }
}
?> 