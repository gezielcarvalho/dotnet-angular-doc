import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { HttpClient, HttpClientModule } from '@angular/common/http';

@Component({
    standalone: true,
    selector: 'app-request-password-reset',
    imports: [CommonModule, ReactiveFormsModule, HttpClientModule],
    templateUrl: './request-password-reset.component.html',
})
export class RequestPasswordResetComponent {
    form = this.fb.group({
        email: ['', [Validators.required, Validators.email]],
    });

    isLoading = false;
    message = '';

    constructor(private fb: FormBuilder, private http: HttpClient) {}

    submit() {
        if (this.form.invalid) return;
        this.isLoading = true;
        const payload = {
            email: this.form.value.email,
            origin: window.location.origin,
        };
        this.http.post('/api/auth/request-password-reset', payload).subscribe({
            next: () => {
                this.message = 'If that email exists, a reset link was sent.';
                this.isLoading = false;
            },
            error: () => {
                this.message = 'An error occurred. Please try again later.';
                this.isLoading = false;
            },
        });
    }
}
