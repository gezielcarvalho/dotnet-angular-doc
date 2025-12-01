
import { Component, OnInit } from '@angular/core';
import {
    FormArray,
    FormControl,
    FormGroup,
    ReactiveFormsModule,
    Validators,
} from '@angular/forms';

@Component({
    imports: [ReactiveFormsModule],
    selector: 'app-reactive-form',
    templateUrl: './reactive-form.component.html'
})
export class ReactiveFormComponent implements OnInit {
    onAddHobby() {
        (<FormArray>this.signupForm.get('hobbies'))?.push(
            new FormControl(null),
        );
    }
    onSubmit() {
        console.log(this.signupForm.value);
    }
    genders = ['male', 'female', 'other'];
    signupForm!: FormGroup;
    ngOnInit(): void {
        this.signupForm = new FormGroup({
            userData: new FormGroup({
                username: new FormControl(null, Validators.required),
                email: new FormControl(null, [
                    Validators.required,
                    Validators.email,
                ]),
            }),
            gender: new FormControl('female'),
            hobbies: new FormArray([]),
        });
    }
}
