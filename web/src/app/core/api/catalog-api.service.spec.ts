import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { APP_CONFIG } from '../config/app-config';
import { Airport, FlightSearchResult } from '../models/catalog.models';
import { CatalogApiService } from './catalog-api.service';

describe('CatalogApiService', () => {
  let service: CatalogApiService;
  let http: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        { provide: APP_CONFIG, useValue: { environment: 'dev', apiBaseUrl: 'http://api.test' } },
      ],
    });
    service = TestBed.inject(CatalogApiService);
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => http.verify());

  it('listAirports pide /api/airports y devuelve el body', () => {
    const body: Airport[] = [
      {
        code: 'BOG',
        name: 'El Dorado',
        city: 'Bogotá',
        country: 'CO',
        timeZoneId: 'America/Bogota',
      },
    ];
    let result: Airport[] | undefined;
    service.listAirports().subscribe((r) => (result = r));

    const req = http.expectOne('http://api.test/api/airports');
    expect(req.request.method).toBe('GET');
    req.flush(body);
    expect(result).toEqual(body);
  });

  it('searchFlights envía solo los parámetros obligatorios', () => {
    service
      .searchFlights({ origin: 'BOG', destination: 'MDE', departureDate: '2026-12-01' })
      .subscribe();

    const req = http.expectOne((r) => r.url === 'http://api.test/api/flights');
    expect(req.request.params.keys().sort()).toEqual(['departureDate', 'destination', 'origin']);
    expect(req.request.params.get('origin')).toBe('BOG');
    expect(req.request.params.get('destination')).toBe('MDE');
    expect(req.request.params.get('departureDate')).toBe('2026-12-01');
    req.flush([]);
  });

  it('searchFlights incluye passengerCount y cabin cuando se indican y devuelve el body', () => {
    const body: FlightSearchResult[] = [
      {
        flightId: 'f1',
        airlineCode: 'AA',
        flightNumber: '100',
        origin: 'BOG',
        destination: 'MDE',
        departureAt: '2026-12-01T08:00:00-05:00',
        arrivalAt: '2026-12-01T09:00:00-05:00',
        fareClasses: [{ cabin: 'Business', price: 100, currency: 'USD', availableSeats: 4 }],
      },
    ];
    let result: FlightSearchResult[] | undefined;
    service
      .searchFlights({
        origin: 'BOG',
        destination: 'MDE',
        departureDate: '2026-12-01',
        passengerCount: 2,
        cabin: 'Business',
      })
      .subscribe((r) => (result = r));

    const req = http.expectOne((r) => r.url === 'http://api.test/api/flights');
    expect(req.request.params.get('passengerCount')).toBe('2');
    expect(req.request.params.get('cabin')).toBe('Business');
    req.flush(body);
    expect(result).toEqual(body);
  });
});
