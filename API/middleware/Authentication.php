<?php
class Authentication {
    public function validateToken() {
        // Lấy header Authorization
        $headers = getallheaders();
        $auth_header = isset($headers['Authorization']) ? $headers['Authorization'] : '';
        
        // Kiểm tra token
        if (empty($auth_header) || !preg_match('/Bearer\s(\S+)/', $auth_header, $matches)) {
            http_response_code(401);
            echo json_encode(array("status" => "error", "message" => "Token không hợp lệ hoặc không tồn tại"));
            return false;
        }
        
        $token = $matches[1];
        
        // Trong môi trường thực tế, bạn sẽ kiểm tra token trong database
        // Nhưng hiện tại chúng ta sẽ chấp nhận mọi token để test
        return true;
    }
}
?> 