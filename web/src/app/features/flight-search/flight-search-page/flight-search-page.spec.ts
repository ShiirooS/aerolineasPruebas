import { HttpErrorResponse } from '@angular/common/http';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { Observable, of, throwError } from 'rxjs';
import { CatalogApiService } from '../../../core/api/catalog-api.service';
import {
  Airport,
  FlightSearchParams,
  FlightSearchResult,
} from '../../../core/models/catalog.models';
import { FlightSearchForm } from '../flight-search-form/flight-search-form';
import { FlightSearchPage } from './flight-search-page';

const AIRPORTS: Airport[] = [
  { code: 'MAD', name: 'Barajas', city: 'Madrid', country: 'ES', timeZoneId: 'Europe/Madrid' },
  { code: 'BCN', name: 'El Prat', city: 'Barcelona', country: 'ES', timeZoneId: 'Europe/Madrid' },
];
const PARAMS: FlightSearchParams = {
  origin: 'MAD',
  destination: 'BCN',
  departureDate: '2026-12-25',
};
const FLIGHT: FlightSearchResult = {
  flightId: 'f1',
  airlineCode: 'IB',
  flightNumber: '3001',
  origin: 'MAD',
  destination: 'BCN',
  departureAt: '2026-12-25T08:00:00+01:00',
  arrivalAt: '2026-12-25T09:15:00+01:00',
  fareClasses: [{ cabin: 'Economy', price: 89.5, currency: 'EUR', availableSeats: 12 }],
};

describe('FlightSearchPage', () => {
  let fixture: ComponentFixture<FlightSearchPage>;
  let searchFlights: ReturnType<typeof vi.fn>;
  const text = () => (fixture.nativeElement as HTMLElement).textContent ?? '';

  async function setup(search: () => Observable<FlightSearchResult[]>) {
    searchFlights = vi.fn(search);
    const fake: Pick<CatalogApiService, 'listAirports' | 'searchFlights'> = {
      listAirports: () => of(AIRPORTS),
      searchFlights: searchFlights as unknown as CatalogApiService['searchFlights'],
    };
    await TestBed.configureTestingModule({
      imports: [FlightSearchPage],
      providers: [{ provide: CatalogApiService, useValue: fake }],
    }).compileComponents();
    fixture = TestBed.createComponent(FlightSearchPage);
    await fixture.whenStable();
  }

  const triggerSearch = async () => {
    fixture.debugElement
      .query((d) => d.componentInstance instanceof FlightSearchForm)
      .componentInstance.search.emit(PARAMS);
    await fixture.whenStable();
  };

  it('llama a searchFlights y muestra los resultados', async () => {
    await setup(() => of([FLIGHT]));
    await triggerSearch();
    expect(searchFlights).toHaveBeenCalledWith(PARAMS);
    expect(text()).toContain('IB 3001');
  });

  it('muestra el estado vacío', async () => {
    await setup(() => of([]));
    await triggerSearch();
    expect(text()).toContain('No hay vuelos para esa búsqueda');
  });

  it('muestra un error legible cuando falla la API', async () => {
    await setup(() => throwError(() => new HttpErrorResponse({ status: 500 })));
    await triggerSearch();
    expect(text()).toContain('No se pudo completar la búsqueda');
  });
});
