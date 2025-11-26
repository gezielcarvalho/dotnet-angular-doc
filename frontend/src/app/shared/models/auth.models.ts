export interface LoginRequest {
    username: string;
    password: string;
}

export interface RegisterRequest {
    username: string;
    email: string;
    password: string;
    firstName: string;
    lastName: string;
}

export interface ChangePasswordRequest {
    currentPassword: string;
    newPassword: string;
    confirmPassword: string;
}

export interface LoginResponse {
    token: string;
    username: string;
    email: string;
    role: string;
}

export interface UserInfo {
    id: string;
    username: string;
    email: string;
    firstName: string;
    lastName: string;
    role: string;
}

export interface ApiResponse<T> {
    success: boolean;
    message: string;
    data: T | null;
}
