/** Contratos JSON de la API de Catalog (camelCase, enums como string). */

export type CabinClass = 'Economy' | 'PremiumEconomy' | 'Business' | 'First';

export interface Airport {
  readonly code: string;
  readonly name: string;
  readonly city: string;
  readonly country: string;
  readonly timeZoneId: string;
}

export interface FareClassOption {
  readonly cabin: CabinClass;
  readonly price: number;
  readonly currency: string;
  readonly availableSeats: number;
}

export interface FlightSearchResult {
  readonly flightId: string;
  readonly airlineCode: string;
  readonly flightNumber: string;
  readonly origin: string;
  readonly destination: string;
  /** ISO-8601 con offset. */
  readonly departureAt: string;
  /** ISO-8601 con offset. */
  readonly arrivalAt: string;
  readonly fareClasses: readonly FareClassOption[];
}

export interface FlightSearchParams {
  readonly origin: string;
  readonly destination: string;
  /** Formato `YYYY-MM-DD`. */
  readonly departureDate: string;
  readonly passengerCount?: number;
  readonly cabin?: CabinClass;
}
