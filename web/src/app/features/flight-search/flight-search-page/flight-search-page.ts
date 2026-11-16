import { HttpErrorResponse } from '@angular/common/http';
import { ChangeDetectionStrategy, Component, DestroyRef, inject, signal } from '@angular/core';
import { Subscription } from 'rxjs';
import { CatalogApiService } from '../../../core/api/catalog-api.service';
import {
  Airport,
  FlightSearchParams,
  FlightSearchResult,
} from '../../../core/models/catalog.models';
import { FlightResults } from '../flight-results/flight-results';
import { FlightSearchForm } from '../flight-search-form/flight-search-form';

@Component({
  selector: 'app-flight-search-page',
  imports: [FlightSearchForm, FlightResults],
  templateUrl: './flight-search-page.html',
  styleUrl: './flight-search-page.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class FlightSearchPage {
  private readonly api = inject(CatalogApiService);
  private searchSub?: Subscription;

  protected readonly airports = signal<readonly Airport[]>([]);
  protected readonly loading = signal(false);
  protected readonly error = signal<string | null>(null);
  protected readonly results = signal<readonly FlightSearchResult[] | null>(null);

  constructor() {
    const airportsSub = this.api.listAirports().subscribe({
      next: (airports) => this.airports.set(airports),
      error: (err) => this.error.set(this.toMessage(err, 'No se pudieron cargar los aeropuertos.')),
    });
    inject(DestroyRef).onDestroy(() => {
      airportsSub.unsubscribe();
      this.searchSub?.unsubscribe();
    });
  }

  protected onSearch(params: FlightSearchParams): void {
    this.searchSub?.unsubscribe(); // descarta una búsqueda anterior en curso
    this.loading.set(true);
    this.error.set(null);
    this.searchSub = this.api.searchFlights(params).subscribe({
      next: (flights) => {
        this.results.set(flights);
        this.loading.set(false);
      },
      error: (err) => {
        this.results.set([]);
        this.error.set(
          this.toMessage(err, 'No se pudo completar la búsqueda. Inténtalo de nuevo.'),
        );
        this.loading.set(false);
      },
    });
  }

  private toMessage(err: unknown, fallback: string): string {
    if (err instanceof HttpErrorResponse) {
      if (err.status === 0) return 'No se pudo conectar con el servidor.';
      if (err.status === 400) return 'Los datos de la búsqueda no son válidos.';
    }
    return fallback;
  }
}
