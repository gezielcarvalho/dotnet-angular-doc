import { TestBed } from '@angular/core/testing';
import { TokenService } from './token.service';

describe('TokenService', () => {
    let service: TokenService;
    let localStorageMock: { [key: string]: string };

    beforeEach(() => {
        // Mock localStorage
        localStorageMock = {};

        spyOn(localStorage, 'getItem').and.callFake((key: string) => {
            return localStorageMock[key] || null;
        });

        spyOn(localStorage, 'setItem').and.callFake(
            (key: string, value: string) => {
                localStorageMock[key] = value;
            },
        );

        spyOn(localStorage, 'removeItem').and.callFake((key: string) => {
            delete localStorageMock[key];
        });

        TestBed.configureTestingModule({});
        service = TestBed.inject(TokenService);
    });

    it('should be created', () => {
        expect(service).toBeTruthy();
    });

    describe('Token Management', () => {
        it('should save token to localStorage', () => {
            const token = 'test-token-123';
            service.saveToken(token);
            expect(localStorage.setItem).toHaveBeenCalledWith(
                'document_auth_token',
                token,
            );
        });

        it('should get token from localStorage', () => {
            localStorageMock['document_auth_token'] = 'test-token-123';
            const token = service.getToken();
            expect(token).toBe('test-token-123');
        });

        it('should return null when token does not exist', () => {
            const token = service.getToken();
            expect(token).toBeNull();
        });

        it('should remove token from localStorage', () => {
            localStorageMock['document_auth_token'] = 'test-token-123';
            service.removeToken();
            expect(localStorage.removeItem).toHaveBeenCalledWith(
                'document_auth_token',
            );
        });
    });

    describe('User Management', () => {
        it('should save user to localStorage', () => {
            const user = {
                id: '1',
                username: 'testuser',
                email: 'test@example.com',
            };
            service.saveUser(user);
            expect(localStorage.setItem).toHaveBeenCalledWith(
                'document_user_info',
                JSON.stringify(user),
            );
        });

        it('should get user from localStorage', () => {
            const user = { id: '1', username: 'testuser' };
            localStorageMock['document_user_info'] = JSON.stringify(user);
            const retrievedUser = service.getUser();
            expect(retrievedUser).toEqual(user);
        });

        it('should return null when user does not exist', () => {
            const user = service.getUser();
            expect(user).toBeNull();
        });

        it('should remove user from localStorage', () => {
            service.removeUser();
            expect(localStorage.removeItem).toHaveBeenCalledWith(
                'document_user_info',
            );
        });
    });

    describe('Clear', () => {
        it('should clear both token and user', () => {
            service.clear();
            expect(localStorage.removeItem).toHaveBeenCalledWith(
                'document_auth_token',
            );
            expect(localStorage.removeItem).toHaveBeenCalledWith(
                'document_user_info',
            );
        });
    });

    describe('Token Expiration', () => {
        it('should return true when token does not exist', () => {
            expect(service.isTokenExpired()).toBe(true);
        });

        it('should return true for expired token', () => {
            // Create a token that expired 1 hour ago
            const expiredTime = Math.floor(Date.now() / 1000) - 3600;
            const payload = btoa(JSON.stringify({ exp: expiredTime }));
            const token = `header.${payload}.signature`;
            localStorageMock['document_auth_token'] = token;

            expect(service.isTokenExpired()).toBe(true);
        });

        it('should return false for valid token', () => {
            // Create a token that expires in 1 hour
            const futureTime = Math.floor(Date.now() / 1000) + 3600;
            const payload = btoa(JSON.stringify({ exp: futureTime }));
            const token = `header.${payload}.signature`;
            localStorageMock['document_auth_token'] = token;

            expect(service.isTokenExpired()).toBe(false);
        });

        it('should return false for token without expiration', () => {
            const payload = btoa(JSON.stringify({ userId: '123' }));
            const token = `header.${payload}.signature`;
            localStorageMock['document_auth_token'] = token;

            expect(service.isTokenExpired()).toBe(false);
        });

        it('should return true for invalid token format', () => {
            localStorageMock['document_auth_token'] = 'invalid-token';
            expect(service.isTokenExpired()).toBe(true);
        });
    });
});
