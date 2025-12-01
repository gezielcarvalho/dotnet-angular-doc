import { LoginSchema } from 'src/app/public/screens/login/login.schema';
import z from 'zod';

export type LoginRequest = z.infer<typeof LoginSchema>;

export interface RegisterRequest {
    username: string;
    email: string;
    password: string;
    confirmPassword: string;
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
