import { ComponentFixture, TestBed } from '@angular/core/testing';
import { Airport, FlightSearchParams } from '../../../core/models/catalog.models';
import { FlightSearchForm } from './flight-search-form';

const AIRPORTS: Airport[] = [
  { code: 'MAD', name: 'Barajas', city: 'Madrid', country: 'ES', timeZoneId: 'Europe/Madrid' },
  { code: 'BCN', name: 'El Prat', city: 'Barcelona', country: 'ES', timeZoneId: 'Europe/Madrid' },
];

describe('FlightSearchForm', () => {
  let fixture: ComponentFixture<FlightSearchForm>;
  let emitted: FlightSearchParams[];
  let root: HTMLElement;

  const flush = async () => {
    fixture.detectChanges();
    await fixture.whenStable();
  };

  async function pick(testId: string, optionText: string) {
    const select = root.querySelector(`[data-testid="${testId}"]`) as HTMLElement;
    (select.querySelector('.mat-mdc-select-trigger') as HTMLElement).click();
    await flush();
    // Solo las opciones del panel de ESTE select (los paneles previos pueden seguir en el DOM).
    const panel = document.getElementById(select.getAttribute('aria-controls')!)!;
    const options = Array.from(panel.querySelectorAll<HTMLElement>('mat-option'));
    options.find((o) => o.textContent?.includes(optionText))!.click();
    await flush();
  }

  async function type(testId: string, value: string) {
    const input = root.querySelector(`[data-testid="${testId}"]`) as HTMLInputElement;
    input.value = value;
    input.dispatchEvent(new Event('input'));
    await flush();
  }

  const submit = async () => {
    (root.querySelector('button[type="submit"]') as HTMLButtonElement).click();
    await flush();
  };

  beforeEach(async () => {
    await TestBed.configureTestingModule({ imports: [FlightSearchForm] }).compileComponents();
    fixture = TestBed.createComponent(FlightSearchForm);
    fixture.componentRef.setInput('airports', AIRPORTS);
    emitted = [];
    fixture.componentInstance.search.subscribe((p) => emitted.push(p));
    root = fixture.nativeElement;
    await flush();
  });

  it('no emite si faltan campos obligatorios', async () => {
    await submit();
    expect(emitted).toEqual([]);
    expect(root.textContent).toContain('Selecciona el origen.');
  });

  it('no emite si origen = destino', async () => {
    await pick('origin', 'MAD');
    await pick('destination', 'MAD');
    await type('date', '12/25/2026');
    await submit();
    expect(emitted).toEqual([]);
    expect(root.textContent).toContain('distinto del origen');
  });

  it('emite los parámetros correctos con el formulario válido', async () => {
    await pick('origin', 'MAD');
    await pick('destination', 'BCN');
    await type('date', '12/25/2026');
    await type('passengers', '3');
    await pick('cabin', 'Business');
    await submit();
    expect(emitted).toEqual([
      {
        origin: 'MAD',
        destination: 'BCN',
        departureDate: '2026-12-25',
        passengerCount: 3,
        cabin: 'Business',
      },
    ]);
  });

  it('omite la cabina si no se elige y usa 1 pasajero por defecto', async () => {
    await pick('origin', 'MAD');
    await pick('destination', 'BCN');
    await type('date', '12/25/2026');
    await submit();
    expect(emitted).toEqual([
      { origin: 'MAD', destination: 'BCN', departureDate: '2026-12-25', passengerCount: 1 },
    ]);
  });

  it('rechaza pasajeros fuera de 1–9', async () => {
    await pick('origin', 'MAD');
    await pick('destination', 'BCN');
    await type('date', '12/25/2026');
    await type('passengers', '10');
    await submit();
    expect(emitted).toEqual([]);
  });
});
