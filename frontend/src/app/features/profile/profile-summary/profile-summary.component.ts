import { CurrencyPipe } from '@angular/common';
import { Component, input } from '@angular/core';
import { UserResponseDto } from '../../../core/models/auth.models';

@Component({
  selector: 'app-profile-summary',
  imports: [CurrencyPipe],
  templateUrl: './profile-summary.component.html',
  styleUrl: './profile-summary.component.css'
})
export class ProfileSummaryComponent {
  readonly user = input.required<UserResponseDto>();
  readonly orderCount = input.required<number>();
  readonly lifetimeSpend = input.required<number>();
}
