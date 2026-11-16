import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { APP_CONFIG } from '../config/app-config';
import { Airport, FlightSearchParams, FlightSearchResult } from '../models/catalog.models';

/** Cliente HTTP de la API de Catalog. */
@Injectable({ providedIn: 'root' })
export class CatalogApiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = inject(APP_CONFIG).apiBaseUrl;

  listAirports(): Observable<Airport[]> {
    return this.http.get<Airport[]>(`${this.baseUrl}/api/airports`);
  }

  searchFlights(params: FlightSearchParams): Observable<FlightSearchResult[]> {
    let httpParams = new HttpParams()
      .set('origin', params.origin)
      .set('destination', params.destination)
      .set('departureDate', params.departureDate);

    // Los opcionales se omiten cuando vienen vacíos.
    if (params.passengerCount !== undefined) {
      httpParams = httpParams.set('passengerCount', params.passengerCount);
    }
    if (params.cabin) {
      httpParams = httpParams.set('cabin', params.cabin);
    }

    return this.http.get<FlightSearchResult[]>(`${this.baseUrl}/api/flights`, {
      params: httpParams,
    });
  }
}
