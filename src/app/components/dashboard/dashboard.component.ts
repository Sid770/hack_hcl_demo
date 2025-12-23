import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { TicketService } from '../../services/ticket.service';
import { TicketStats, TicketStatus, TicketPriority } from '../../models/ticket.model';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.css'
})
export class DashboardComponent implements OnInit {
  stats = signal<TicketStats>({
    total: 0,
    open: 0,
    inProgress: 0,
    resolved: 0,
    closed: 0,
    highPriority: 0
  });

  recentTickets = signal<any[]>([]);

  constructor(private ticketService: TicketService) {}

  ngOnInit(): void {
    this.loadDashboardData();
    
    // Subscribe to ticket changes
    this.ticketService.tickets$.subscribe(() => {
      this.loadDashboardData();
    });
  }

  private async loadDashboardData(): Promise<void> {
    this.stats.set(await this.ticketService.getTicketStats());
    
    // Get 5 most recent tickets
    const allTickets = this.ticketService.getAllTickets();
    this.recentTickets.set(
      allTickets
        .sort((a, b) => b.createdAt.getTime() - a.createdAt.getTime())
        .slice(0, 5)
    );
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
}
