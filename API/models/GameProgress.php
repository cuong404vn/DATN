<?php
class GameProgress {
    private $conn;
    private $table_name = "GameProgress";
 
    public $ProgressID;
    public $UserID;
    public $CurrentMap;
    public $TotalStars;
    public $LastSaveTime;
    public $MapUnlocked;
 
    public function __construct($db) {
        $this->conn = $db;
    }
    

    public function getProgress() {
        $query = "SELECT * FROM " . $this->table_name . " 
                  WHERE UserID = ?";
 
        $stmt = $this->conn->prepare($query);
        $stmt->bindParam(1, $this->UserID);
        $stmt->execute();
 
        if($stmt->rowCount() > 0) {
            $row = $stmt->fetch(PDO::FETCH_ASSOC);
            
            $this->ProgressID = $row['ProgressID'];
            $this->CurrentMap = $row['CurrentMap'];
            $this->TotalStars = $row['TotalStars'];
            $this->LastSaveTime = $row['LastSaveTime'];
            $this->MapUnlocked = $row['MapUnlocked'];
            
            return true;
        }
        
        return false;
    }
    

    public function saveProgress() {

        $query = "SELECT ProgressID FROM " . $this->table_name . " 
                  WHERE UserID = ?";
              
        $stmt = $this->conn->prepare($query);
        $stmt->bindParam(1, $this->UserID);
        $stmt->execute();
        
        if($stmt->rowCount() > 0) {

            $row = $stmt->fetch(PDO::FETCH_ASSOC);
            $this->ProgressID = $row['ProgressID'];
            
            $query = "UPDATE " . $this->table_name . " 
                      SET CurrentMap = :currentMap, 
                          TotalStars = :totalStars, 
                          LastSaveTime = NOW(), 
                          MapUnlocked = :mapUnlocked 
                      WHERE ProgressID = :progressID";
                      
            $stmt = $this->conn->prepare($query);
            $stmt->bindParam(":progressID", $this->ProgressID);
        } else {

            $query = "INSERT INTO " . $this->table_name . " 
                      SET UserID = :userID, 
                          CurrentMap = :currentMap, 
                          TotalStars = :totalStars, 
                          LastSaveTime = NOW(), 
                          MapUnlocked = :mapUnlocked";
                      
            $stmt = $this->conn->prepare($query);
            $stmt->bindParam(":userID", $this->UserID);
        }


        $stmt->bindParam(":currentMap", $this->CurrentMap);
        $stmt->bindParam(":totalStars", $this->TotalStars);
        $stmt->bindParam(":mapUnlocked", $this->MapUnlocked);

        return $stmt->execute();
    }
}
?> 