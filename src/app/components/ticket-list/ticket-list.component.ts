import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { TicketService } from '../../services/ticket.service';
import { Ticket, TicketStatus, TicketPriority, TicketCategory } from '../../models/ticket.model';

@Component({
  selector: 'app-ticket-list',
  standalone: true,
  imports: [CommonModule, RouterLink, FormsModule],
  templateUrl: './ticket-list.component.html',
  styleUrl: './ticket-list.component.css'
})
export class TicketListComponent implements OnInit {
  tickets = signal<Ticket[]>([]);
  filteredTickets = signal<Ticket[]>([]);
  searchTerm = signal('');
  filterStatus = signal<string>('all');
  filterPriority = signal<string>('all');
  filterCategory = signal<string>('all');
  sortBy = signal<'date' | 'priority' | 'status'>('date');
  sortOrder = signal<'asc' | 'desc'>('desc');

  statusOptions = Object.values(TicketStatus);
  priorityOptions = Object.values(TicketPriority);
  categoryOptions = Object.values(TicketCategory);

  constructor(private ticketService: TicketService) {}

  ngOnInit(): void {
    this.loadTickets();
    
    // Subscribe to ticket changes
    this.ticketService.tickets$.subscribe(() => {
      this.loadTickets();
    });
  }

  loadTickets(): void {
    this.tickets.set(this.ticketService.getAllTickets());
    this.applyFilters();
  }

  onSearch(event: Event): void {
    const input = event.target as HTMLInputElement;
    this.searchTerm.set(input.value);
    this.applyFiltersAsync();
  }

  onFilterStatus(event: Event): void {
    const select = event.target as HTMLSelectElement;
    this.filterStatus.set(select.value);
    this.applyFilters();
  }

  onFilterPriority(event: Event): void {
    const select = event.target as HTMLSelectElement;
    this.filterPriority.set(select.value);
    this.applyFilters();
  }

  onFilterCategory(event: Event): void {
    const select = event.target as HTMLSelectElement;
    this.filterCategory.set(select.value);
    this.applyFilters();
  }

  onSort(field: 'date' | 'priority' | 'status'): void {
    if (this.sortBy() === field) {
      this.sortOrder.set(this.sortOrder() === 'asc' ? 'desc' : 'asc');
    } else {
      this.sortBy.set(field);
      this.sortOrder.set('desc');
    }
    this.applyFilters();
  }

  async applyFiltersAsync(): Promise<void> {
    // Apply search using API
    if (this.searchTerm()) {
      const filtered = await this.ticketService.searchTickets(this.searchTerm());
      this.filteredTickets.set(filtered);
      return;
    }
    this.applyFilters();
  }

  applyFilters(): void {
    let filtered = [...this.tickets()];

    // Apply search locally if no API search term
    if (this.searchTerm() && filtered.length > 0) {
      const term = this.searchTerm().toLowerCase();
      filtered = filtered.filter(ticket =>
        ticket.title.toLowerCase().includes(term) ||
        ticket.description.toLowerCase().includes(term) ||
        ticket.assignedTo.toLowerCase().includes(term) ||
        ticket.reportedBy.toLowerCase().includes(term)
      );
    }

    // Apply status filter
    if (this.filterStatus() !== 'all') {
      filtered = filtered.filter(t => t.status === this.filterStatus());
    }

    // Apply priority filter
    if (this.filterPriority() !== 'all') {
      filtered = filtered.filter(t => t.priority === this.filterPriority());
    }

    // Apply category filter
    if (this.filterCategory() !== 'all') {
      filtered = filtered.filter(t => t.category === this.filterCategory());
    }

    // Apply sorting
    filtered.sort((a, b) => {
      let comparison = 0;

      if (this.sortBy() === 'date') {
        comparison = a.createdAt.getTime() - b.createdAt.getTime();
      } else if (this.sortBy() === 'priority') {
        const priorityOrder = { 'Critical': 4, 'High': 3, 'Medium': 2, 'Low': 1 };
        comparison = priorityOrder[a.priority] - priorityOrder[b.priority];
      } else if (this.sortBy() === 'status') {
        comparison = a.status.localeCompare(b.status);
      }

      return this.sortOrder() === 'asc' ? comparison : -comparison;
    });

    this.filteredTickets.set(filtered);
  }

  async deleteTicket(id: string, event: Event): Promise<void> {
    event.preventDefault();
    event.stopPropagation();
    
    if (confirm('Are you sure you want to delete this ticket?')) {
      await this.ticketService.deleteTicket(id);
    }
  }

  getStatusClass(status: string): string {
    const statusMap: Record<string, string> = {
      'Open': 'status-open',
      'In Progress': 'status-progress',
      'Resolved': 'status-resolved',
      'Closed': 'status-closed',
      'On Hold': 'status-hold'
    };
    return statusMap[status] || '';
  }

  getPriorityClass(priority: string): string {
    const priorityMap: Record<string, string> = {
      'Low': 'priority-low',
      'Medium': 'priority-medium',
      'High': 'priority-high',
      'Critical': 'priority-critical'
    };
    return priorityMap[priority] || '';
  }

  getSortIcon(field: string): string {
    if (this.sortBy() !== field) return '↕️';
    return this.sortOrder() === 'asc' ? '↑' : '↓';
  }
}
