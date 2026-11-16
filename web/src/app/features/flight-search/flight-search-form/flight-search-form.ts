import { ChangeDetectionStrategy, Component, computed, input, output, signal } from '@angular/core';
import { FormField, form, max, min, required, validate } from '@angular/forms/signals';
import { provideNativeDateAdapter } from '@angular/material/core';
import { MatButtonModule } from '@angular/material/button';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { Airport, CabinClass, FlightSearchParams } from '../../../core/models/catalog.models';

export const CABIN_OPTIONS: readonly { value: CabinClass; label: string }[] = [
  { value: 'Economy', label: 'Economy' },
  { value: 'PremiumEconomy', label: 'Premium Economy' },
  { value: 'Business', label: 'Business' },
  { value: 'First', label: 'Primera' },
];

interface FlightSearchModel {
  origin: string;
  destination: string;
  departureDate: Date | null;
  passengerCount: number | null;
  cabin: CabinClass | '';
}

/** `Date` local -> `YYYY-MM-DD` (sin pasar por UTC). */
export function toIsoDate(date: Date): string {
  const pad = (n: number) => String(n).padStart(2, '0');
  return `${date.getFullYear()}-${pad(date.getMonth() + 1)}-${pad(date.getDate())}`;
}

@Component({
  selector: 'app-flight-search-form',
  imports: [
    FormField,
    MatButtonModule,
    MatDatepickerModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
  ],
  providers: [provideNativeDateAdapter()],
  templateUrl: './flight-search-form.html',
  styleUrl: './flight-search-form.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class FlightSearchForm {
  readonly airports = input.required<readonly Airport[]>();
  readonly disabled = input(false);
  readonly search = output<FlightSearchParams>();

  protected readonly cabins = CABIN_OPTIONS;
  protected readonly airportOptions = computed(() =>
    this.airports().map((a) => ({ code: a.code, label: `${a.code} — ${a.city}` })),
  );

  private readonly model = signal<FlightSearchModel>({
    origin: '',
    destination: '',
    departureDate: null,
    passengerCount: 1,
    cabin: '',
  });

  protected readonly searchForm = form(this.model, (p) => {
    required(p.origin);
    required(p.destination);
    required(p.departureDate);
    required(p.passengerCount);
    min(p.passengerCount, 1);
    max(p.passengerCount, 9);
    validate(p.destination, ({ value, valueOf }) =>
      value() && value() === valueOf(p.origin)
        ? { kind: 'sameAirport', message: 'El destino debe ser distinto del origen.' }
        : undefined,
    );
  });

  protected submit(event: Event): void {
    event.preventDefault();
    const f = this.searchForm();
    if (f.invalid()) {
      f.markAsTouched();
      this.markAllTouched();
      return;
    }
    const v = this.model();
    this.search.emit({
      origin: v.origin,
      destination: v.destination,
      departureDate: toIsoDate(v.departureDate as Date),
      passengerCount: v.passengerCount as number,
      ...(v.cabin ? { cabin: v.cabin } : {}),
    });
  }

  private markAllTouched(): void {
    const f = this.searchForm;
    for (const field of [f.origin, f.destination, f.departureDate, f.passengerCount, f.cabin]) {
      field().markAsTouched();
    }
  }
}
