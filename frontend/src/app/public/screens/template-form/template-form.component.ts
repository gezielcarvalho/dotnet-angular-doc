import { NgFor, NgIf } from '@angular/common';
import { Component, ViewChild } from '@angular/core';
import { FormsModule, NgForm } from '@angular/forms';

@Component({
    imports: [FormsModule, NgIf, NgFor],
    selector: 'app-template-form',
    templateUrl: './template-form.component.html',
    styleUrls: ['./template-form.component.css']
})
export class TemplateFormComponent {
    @ViewChild('formExample') formExample!: NgForm;
    defaultQuestion = 'pet';
    defaultGender = 'female';
    genders = ['male', 'female', 'other'];
    suggestUserName() {
        const suggestedName = 'Superuser';
        this.formExample.form.patchValue({
            userData: {
                username: suggestedName,
            },
        });
    }

    onSubmit() {
        console.log(this.formExample);
        this.formExample.reset();
    }
}
