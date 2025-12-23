import { Routes } from '@angular/router';
import { DashboardComponent } from './components/dashboard/dashboard.component';
import { TicketListComponent } from './components/ticket-list/ticket-list.component';
import { TicketFormComponent } from './components/ticket-form/ticket-form.component';
import { TicketDetailComponent } from './components/ticket-detail/ticket-detail.component';

export const routes: Routes = [
  {
    path: '',
    component: DashboardComponent,
    title: 'Dashboard - Ticket Management'
  },
  {
    path: 'tickets',
    component: TicketListComponent,
    title: 'All Tickets - Ticket Management'
  },
  {
    path: 'tickets/new',
    component: TicketFormComponent,
    title: 'Create Ticket - Ticket Management'
  },
  {
    path: 'tickets/:id',
    component: TicketDetailComponent,
    title: 'Ticket Details - Ticket Management'
  },
  {
    path: 'tickets/:id/edit',
    component: TicketFormComponent,
    title: 'Edit Ticket - Ticket Management'
  },
  {
    path: '**',
    redirectTo: '',
    pathMatch: 'full'
  }
];
