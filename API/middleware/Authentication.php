<?php
class Authentication {
    public function validateToken() {

        $headers = getallheaders();
        $auth_header = isset($headers['Authorization']) ? $headers['Authorization'] : '';
        

        if (empty($auth_header) || !preg_match('/Bearer\s(\S+)/', $auth_header, $matches)) {
            http_response_code(401);
            echo json_encode(array("status" => "error", "message" => "Token không hợp lệ hoặc không tồn tại"));
            return false;
        }
        
        $token = $matches[1];
        


        return true;
    }
}
?> 