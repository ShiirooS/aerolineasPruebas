import { ComponentFixture, TestBed } from '@angular/core/testing';
import { FlightSearchResult } from '../../../core/models/catalog.models';
import { FlightResults } from './flight-results';

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

describe('FlightResults', () => {
  let fixture: ComponentFixture<FlightResults>;
  const text = () => (fixture.nativeElement as HTMLElement).textContent ?? '';

  beforeEach(async () => {
    await TestBed.configureTestingModule({ imports: [FlightResults] }).compileComponents();
    fixture = TestBed.createComponent(FlightResults);
  });

  it('renderiza los vuelos con tarifas', async () => {
    fixture.componentRef.setInput('flights', [FLIGHT]);
    await fixture.whenStable();
    expect(text()).toContain('IB 3001');
    expect(text()).toContain('MAD → BCN');
    expect(text()).toContain('Sale:');
    expect(text()).toContain('Llega:');
    expect(text()).toContain('Economy');
    expect(text()).toMatch(/89[.,]50/);
    expect(text()).toContain('12 asientos');
  });

  it('muestra el estado vacío', async () => {
    fixture.componentRef.setInput('flights', []);
    await fixture.whenStable();
    expect(text()).toContain('No hay vuelos para esa búsqueda');
  });

  it('muestra el estado de carga', async () => {
    fixture.componentRef.setInput('loading', true);
    await fixture.whenStable();
    expect(text()).toContain('Buscando vuelos');
  });

  it('muestra el error legible', async () => {
    fixture.componentRef.setInput('error', 'Algo falló');
    await fixture.whenStable();
    expect(text()).toContain('Algo falló');
    expect(text()).not.toContain('No hay vuelos');
  });
});
