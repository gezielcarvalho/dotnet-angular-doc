import { Injectable } from '@angular/core';

@Injectable({
    providedIn: 'root',
})
export class TokenService {
    private readonly TOKEN_KEY = 'document_auth_token';
    private readonly USER_KEY = 'document_user_info';

    saveToken(token: string): void {
        localStorage.setItem(this.TOKEN_KEY, token);
    }

    getToken(): string | null {
        return localStorage.getItem(this.TOKEN_KEY);
    }

    removeToken(): void {
        localStorage.removeItem(this.TOKEN_KEY);
    }

    saveUser(user: any): void {
        localStorage.setItem(this.USER_KEY, JSON.stringify(user));
    }

    getUser(): any {
        const user = localStorage.getItem(this.USER_KEY);
        return user ? JSON.parse(user) : null;
    }

    removeUser(): void {
        localStorage.removeItem(this.USER_KEY);
    }

    clear(): void {
        this.removeToken();
        this.removeUser();
    }

    isTokenExpired(): boolean {
        const token = this.getToken();
        if (!token) return true;

        try {
            // Decode JWT token (simple base64 decode)
            const payload = JSON.parse(atob(token.split('.')[1]));
            const exp = payload.exp;

            if (!exp) return false;

            // Check if token is expired (with 30 second buffer)
            return Date.now() >= exp * 1000 - 30000;
        } catch (error) {
            return true;
        }
    }
}
