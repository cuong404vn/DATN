<?php


include_once './config/cors.php';


$request_uri = $_SERVER['REQUEST_URI'];
$base_path = '/api/'; 


if (strpos($request_uri, $base_path) === 0) {

    $path = substr($request_uri, strlen($base_path));
    $url_parts = explode('/', trim($path, '/'));
    

    $controller = isset($url_parts[0]) ? $url_parts[0] : '';
    $action = isset($url_parts[1]) ? $url_parts[1] : '';
    

    switch ($controller) {
        case 'auth':
            include_once './controllers/AuthController.php';
            $auth_controller = new AuthController();
            
            if ($action == 'register') {
                $auth_controller->register();
            } elseif ($action == 'login') {
                $auth_controller->login();
            } else {
                http_response_code(404);
                echo json_encode(array("status" => "error", "message" => "Endpoint không tồn tại"));
            }
            break;
            
        case 'progress':
            include_once './middleware/Authentication.php';
            include_once './controllers/GameController.php';
            

            $auth = new Authentication();
            if (!$auth->validateToken()) {
                break;
            }
            
            $game_controller = new GameController();
            
            if ($action == 'get' && isset($url_parts[2])) {
                $game_controller->getProgress($url_parts[2]);
            } elseif ($action == 'save') {
                $game_controller->saveProgress();
            } else {
                http_response_code(404);
                echo json_encode(array("status" => "error", "message" => "Endpoint không tồn tại"));
            }
            break;
            
        case 'inventory':
            include_once './middleware/Authentication.php';
            include_once './controllers/InventoryController.php';
            

            $auth = new Authentication();
            if (!$auth->validateToken()) {
                break;
            }
            
            $inventory_controller = new InventoryController();
            
            if ($action == 'get' && isset($url_parts[2])) {
                $inventory_controller->getInventory($url_parts[2]);
            } elseif ($action == 'update') {
                $inventory_controller->updateInventory();
            } else {
                http_response_code(404);
                echo json_encode(array("status" => "error", "message" => "Endpoint không tồn tại"));
            }
            break;
            
        case 'quests':
            include_once './middleware/Authentication.php';
            include_once './controllers/QuestController.php';
            

            $auth = new Authentication();
            if (!$auth->validateToken()) {
                break;
            }
            
            $quest_controller = new QuestController();
            
            if ($action == 'get' && isset($url_parts[2])) {
                $quest_controller->getQuests($url_parts[2]);
            } elseif ($action == 'update') {
                $quest_controller->updateQuest();
            } else {
                http_response_code(404);
                echo json_encode(array("status" => "error", "message" => "Endpoint không tồn tại"));
            }
            break;
            
        default:
            http_response_code(404);
            echo json_encode(array("status" => "error", "message" => "Endpoint không tồn tại"));
            break;
    }
} else {

    http_response_code(404);
    echo json_encode(array("status" => "error", "message" => "API endpoint không tồn tại"));
}
?> 