<?php
// Entry point của API
// Include các file cần thiết
include_once './config/cors.php';
include_once './config/database.php';

// Lấy URL request
$request_uri = $_SERVER['REQUEST_URI'];
$url_parts = explode('/', trim($request_uri, '/'));

// Bỏ qua phần đầu URL (thường là 'api')
array_shift($url_parts);

// Xác định controller và action
$controller = isset($url_parts[0]) ? $url_parts[0] : '';
$action = isset($url_parts[1]) ? $url_parts[1] : '';

// Routing
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
        
        // Kiểm tra token
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
        
        // Kiểm tra token
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
        
        // Kiểm tra token
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
?> 