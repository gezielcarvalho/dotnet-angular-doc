import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import {
    FormBuilder,
    FormGroup,
    ReactiveFormsModule,
    Validators,
} from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { AuthService } from '../../../shared/services/auth.service';
import { LoginRequest } from '../../../shared/models/auth.models';
import { LoginSchema } from './login.schema';

@Component({
    selector: 'app-login',
    standalone: true,
    imports: [CommonModule, ReactiveFormsModule, RouterModule],
    templateUrl: './login.component.html',
    styleUrls: ['./login.component.css'],
})
export class LoginComponent implements OnInit {
    loginForm!: FormGroup;
    isLoading = false;
    errorMessage = '';
    showRegister = false;

    constructor(
        private fb: FormBuilder,
        private authService: AuthService,
        private router: Router,
    ) {}

    ngOnInit(): void {
        this.loginForm = this.fb.group({
            username: ['', [Validators.required, Validators.minLength(3)]],
            password: ['', [Validators.required, Validators.minLength(6)]],
            // Registration fields
            email: [''],
            firstName: [''],
            lastName: [''],
            confirmPassword: [''],
        });

        // Update validators based on mode
        this.updateFormValidators();
    }

    toggleMode(): void {
        this.showRegister = !this.showRegister;
        this.errorMessage = '';
        this.loginForm.reset();
        this.updateFormValidators();
    }

    updateFormValidators(): void {
        if (this.showRegister) {
            this.loginForm
                .get('email')
                ?.setValidators([Validators.required, Validators.email]);
            this.loginForm
                .get('firstName')
                ?.setValidators([Validators.required]);
            this.loginForm
                .get('lastName')
                ?.setValidators([Validators.required]);
            this.loginForm
                .get('confirmPassword')
                ?.setValidators([Validators.required]);
        } else {
            this.loginForm.get('email')?.clearValidators();
            this.loginForm.get('firstName')?.clearValidators();
            this.loginForm.get('lastName')?.clearValidators();
            this.loginForm.get('confirmPassword')?.clearValidators();
        }
        this.loginForm.get('email')?.updateValueAndValidity();
        this.loginForm.get('firstName')?.updateValueAndValidity();
        this.loginForm.get('lastName')?.updateValueAndValidity();
        this.loginForm.get('confirmPassword')?.updateValueAndValidity();
    }

    onSubmit(): void {
        if (this.loginForm.invalid) {
            this.loginForm.markAllAsTouched();
            return;
        }

        this.isLoading = true;
        this.errorMessage = '';

        if (this.showRegister) {
            this.register();
        } else {
            this.login();
        }
    }

    login(): void {
        const credentials: LoginRequest = {
            username: this.loginForm.value.username,
            password: this.loginForm.value.password,
        };

        if (LoginSchema.safeParse(credentials).success === false) {
            this.errorMessage = 'Invalid login data';
            this.isLoading = false;
            return;
        }

        this.authService.login(credentials).subscribe({
            next: response => {
                this.isLoading = false;
                if (response.success) {
                    console.log('Login successful');
                    this.router.navigate(['/']);
                } else {
                    this.errorMessage = response.message || 'Login failed';
                }
            },
            error: error => {
                this.isLoading = false;
                this.errorMessage = 'An error occurred during login';
                console.error('Login error:', error);
            },
        });
    }

    register(): void {
        if (
            this.loginForm.value.password !==
            this.loginForm.value.confirmPassword
        ) {
            this.errorMessage = 'Passwords do not match';
            this.isLoading = false;
            return;
        }

        const registerData = {
            username: this.loginForm.value.username,
            email: this.loginForm.value.email,
            password: this.loginForm.value.password,
            confirmPassword: this.loginForm.value.confirmPassword,
            firstName: this.loginForm.value.firstName,
            lastName: this.loginForm.value.lastName,
        };

        this.authService.register(registerData).subscribe({
            next: response => {
                this.isLoading = false;
                if (response.success) {
                    console.log('Registration successful');
                    this.router.navigate(['/']);
                } else {
                    this.errorMessage =
                        response.message || 'Registration failed';
                }
            },
            error: error => {
                this.isLoading = false;
                this.errorMessage = 'An error occurred during registration';
                console.error('Registration error:', error);
            },
        });
    }

    isFieldInvalid(fieldName: string): boolean {
        const field = this.loginForm.get(fieldName);
        return !!(field && field.invalid && (field.dirty || field.touched));
    }

    getFieldError(fieldName: string): string {
        const field = this.loginForm.get(fieldName);
        if (field?.hasError('required')) {
            return `${this.getFieldLabel(fieldName)} is required`;
        }
        if (field?.hasError('minlength')) {
            const minLength = field.errors?.['minlength'].requiredLength;
            return `${this.getFieldLabel(
                fieldName,
            )} must be at least ${minLength} characters`;
        }
        if (field?.hasError('email')) {
            return 'Please enter a valid email address';
        }
        return '';
    }

    getFieldLabel(fieldName: string): string {
        const labels: { [key: string]: string } = {
            username: 'Username',
            password: 'Password',
            email: 'Email',
            firstName: 'First Name',
            lastName: 'Last Name',
            confirmPassword: 'Confirm Password',
        };
        return labels[fieldName] || fieldName;
    }
}
