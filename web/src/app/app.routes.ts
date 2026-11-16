import { Routes } from '@angular/router';

export const routes: Routes = [
  {
    path: '',
    loadComponent: () =>
      import('./features/flight-search/flight-search-page/flight-search-page').then(
        (m) => m.FlightSearchPage,
      ),
  },
];
