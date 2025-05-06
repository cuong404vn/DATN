<?php

include_once './config/database.php';
include_once './models/GameProgress.php';

class GameController {
    private $db;
    private $game_progress;
    
    public function __construct() {

        $database = new Database();
        $this->db = $database->getConnection();
        $this->game_progress = new GameProgress($this->db);
    }
    

    public function getProgress($userID) {
        $this->game_progress->UserID = $userID;
        
        if($this->game_progress->getProgress()) {

            $mapUnlocked = json_decode($this->game_progress->MapUnlocked, true);
            
            http_response_code(200);
            echo json_encode(array(
                "status" => "success",
                "currentMap" => $this->game_progress->CurrentMap,
                "totalStars" => $this->game_progress->TotalStars,
                "unlockedMaps" => $mapUnlocked
            ));
        } else {

            http_response_code(200);
            echo json_encode(array(
                "status" => "success",
                "currentMap" => "ToaThanh",
                "totalStars" => 0,
                "unlockedMaps" => array(
                    array(
                        "mapID" => "ToaThanh",
                        "status" => "unlocked",
                        "stars" => 0,
                        "highScore" => 0
                    )
                )
            ));
        }
    }
    

    public function saveProgress() {

        $data = json_decode(file_get_contents("php://input"));
        

        if(empty($data->userID) || empty($data->mapID) || !isset($data->score) || !isset($data->stars)) {
            http_response_code(400);
            echo json_encode(array("status" => "error", "message" => "Thiếu thông tin tiến độ"));
            return;
        }
        

        $this->game_progress->UserID = $data->userID;
        $this->game_progress->CurrentMap = $data->mapID;
        

        $this->game_progress->getProgress();
        

        $mapUnlocked = json_decode($this->game_progress->MapUnlocked, true);
        if(!$mapUnlocked) $mapUnlocked = array();
        

        $currentStars = $this->game_progress->TotalStars;
        $oldStars = 0;
        

        $mapExists = false;
        foreach($mapUnlocked as &$map) {
            if($map['mapID'] == $data->mapID) {
                $mapExists = true;
                $oldStars = $map['stars'];
                

                if($data->stars > $map['stars']) {
                    $map['stars'] = $data->stars;
                }
                

                if($data->score > $map['highScore']) {
                    $map['highScore'] = $data->score;
                }
                
                $map['status'] = $data->status;
                break;
            }
        }
        

        if(!$mapExists) {
            $mapUnlocked[] = array(
                "mapID" => $data->mapID,
                "status" => $data->status,
                "stars" => $data->stars,
                "highScore" => $data->score
            );
        }
        

        if($data->status == "completed") {

            $nextMapID = $this->getNextMapID($data->mapID);
            if($nextMapID) {

                $nextMapExists = false;
                foreach($mapUnlocked as &$map) {
                    if($map['mapID'] == $nextMapID) {
                        $nextMapExists = true;

                        if($map['status'] == "locked") {
                            $map['status'] = "unlocked";
                        }
                        break;
                    }
                }
                

                if(!$nextMapExists) {
                    $mapUnlocked[] = array(
                        "mapID" => $nextMapID,
                        "status" => "unlocked",
                        "stars" => 0,
                        "highScore" => 0
                    );
                }
            }
        }
        

        error_log("MapUnlocked before: " . $this->game_progress->MapUnlocked);
        error_log("TotalStars before: " . $this->game_progress->TotalStars);


        $totalStars = 0;
        foreach($mapUnlocked as $map) {
            $stars = intval($map['stars']);
            error_log("Map: " . $map['mapID'] . ", Stars: " . $stars);
            $totalStars += $stars;
        }
        error_log("TotalStars calculated: " . $totalStars);
        $this->game_progress->TotalStars = $totalStars;
        $this->game_progress->MapUnlocked = json_encode($mapUnlocked);
        

        if($this->game_progress->saveProgress()) {
            http_response_code(200);
            echo json_encode(array(
                "status" => "success",
                "message" => "Lưu tiến độ thành công",
                "totalStars" => $this->game_progress->TotalStars,
                "unlockedMaps" => json_decode($this->game_progress->MapUnlocked)
            ));
        } else {
            http_response_code(500);
            echo json_encode(array("status" => "error", "message" => "Lưu tiến độ thất bại"));
        }
    }


    private function getNextMapID($currentMapID) {

        $mapOrder = array("ToaThanh", "KhuRung", "LongDat", "CamThanh");
        

        $currentIndex = array_search($currentMapID, $mapOrder);
        

        if($currentIndex === false || $currentIndex >= count($mapOrder) - 1) {
            return null;
        }
        

        return $mapOrder[$currentIndex + 1];
    }
}
?> 