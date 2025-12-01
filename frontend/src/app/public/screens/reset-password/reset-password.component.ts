import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { HttpClient } from '@angular/common/http';
import { ActivatedRoute, Router } from '@angular/router';

@Component({
    selector: 'app-reset-password',
    standalone: true,
    imports: [CommonModule, ReactiveFormsModule],
    templateUrl: './reset-password.component.html',
})
export class ResetPasswordComponent {
    form = this.fb.group({
        token: [''],
        newPassword: ['', [Validators.required, Validators.minLength(8)]],
        confirmPassword: ['', [Validators.required]],
    });

    isLoading = false;
    message = '';

    constructor(
        private fb: FormBuilder,
        private http: HttpClient,
        private route: ActivatedRoute,
        private router: Router,
    ) {
        // DEBUG: log the current browser href, query params and token
        const token = this.route.snapshot.queryParamMap.get('token') || '';
        try {
            console.log(
                '[ResetPasswordComponent] window.location.href =',
                window?.location?.href,
            );
        } catch (err) {
            console.log(
                '[ResetPasswordComponent] window.location.href (error reading):',
                err,
            );
        }
        console.log('[ResetPasswordComponent] queryParamMap token =', token);
        this.form.patchValue({ token });
    }

    submit() {
        if (this.form.invalid) return;
        this.isLoading = true;
        const payload = {
            token: this.form.value.token,
            newPassword: this.form.value.newPassword,
            confirmPassword: this.form.value.confirmPassword,
        };
        this.http.post('/api/auth/reset-password', payload).subscribe({
            next: () => {
                this.message = 'Password has been reset. You can now sign in.';
                this.isLoading = false;
                setTimeout(() => this.router.navigate(['/login']), 1500);
            },
            error: () => {
                this.message = 'Invalid or expired token.';
                this.isLoading = false;
            },
        });
    }
}
