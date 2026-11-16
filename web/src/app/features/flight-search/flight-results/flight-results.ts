import { CurrencyPipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, input } from '@angular/core';
import { MatCardModule } from '@angular/material/card';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { Airport, FlightSearchResult } from '../../../core/models/catalog.models';

@Component({
  selector: 'app-flight-results',
  imports: [CurrencyPipe, MatCardModule, MatProgressSpinnerModule],
  templateUrl: './flight-results.html',
  styleUrl: './flight-results.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class FlightResults {
  readonly flights = input<readonly FlightSearchResult[]>([]);
  readonly airports = input<readonly Airport[]>([]);
  readonly loading = input(false);
  readonly error = input<string | null>(null);

  /** Formatea una fecha ISO en la zona horaria del aeropuerto (o la local si se desconoce). */
  protected formatTime(iso: string, airportCode: string): string {
    const timeZone = this.airports().find((a) => a.code === airportCode)?.timeZoneId;
    const options: Intl.DateTimeFormatOptions = {
      dateStyle: 'medium',
      timeStyle: 'short',
      ...(timeZone ? { timeZone } : {}),
    };
    try {
      return new Intl.DateTimeFormat('es-ES', options).format(new Date(iso));
    } catch {
      return new Intl.DateTimeFormat('es-ES', { dateStyle: 'medium', timeStyle: 'short' }).format(
        new Date(iso),
      );
    }
  }
}
