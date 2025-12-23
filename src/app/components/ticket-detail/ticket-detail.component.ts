import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, ActivatedRoute, RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { TicketService } from '../../services/ticket.service';
import { Ticket, Comment } from '../../models/ticket.model';

@Component({
  selector: 'app-ticket-detail',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './ticket-detail.component.html',
  styleUrl: './ticket-detail.component.css'
})
export class TicketDetailComponent implements OnInit {
  ticket = signal<Ticket | null>(null);
  newComment = signal('');
  commentAuthor = signal('');

  constructor(
    private ticketService: TicketService,
    private router: Router,
    private route: ActivatedRoute
  ) {}

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    
    if (id) {
      this.loadTicket(id);
    } else {
      this.router.navigate(['/tickets']);
    }

    // Subscribe to ticket updates
    this.ticketService.tickets$.subscribe(() => {
      if (id) {
        this.loadTicket(id);
      }
    });
  }

  loadTicket(id: string): void {
    const ticket = this.ticketService.getTicketById(id);
    
    if (ticket) {
      this.ticket.set(ticket);
    } else {
      alert('Ticket not found');
      this.router.navigate(['/tickets']);
    }
  }

  async addComment(): Promise<void> {
    const ticket = this.ticket();
    if (!ticket) return;

    const author = this.commentAuthor().trim();
    const text = this.newComment().trim();

    if (!author) {
      alert('Please enter your name');
      return;
    }

    if (!text) {
      alert('Please enter a comment');
      return;
    }

    try {
      await this.ticketService.addComment(ticket.id, author, text);
      
      // Clear the form
      this.newComment.set('');
      this.commentAuthor.set('');
    } catch (error) {
      console.error('Error adding comment:', error);
      alert('Error adding comment. Please try again.');
    }
  }

  async deleteTicket(): Promise<void> {
    const ticket = this.ticket();
    if (!ticket) return;

    if (confirm('Are you sure you want to delete this ticket? This action cannot be undone.')) {
      try {
        await this.ticketService.deleteTicket(ticket.id);
        alert('Ticket deleted successfully');
        this.router.navigate(['/tickets']);
      } catch (error) {
        console.error('Error deleting ticket:', error);
        alert('Error deleting ticket. Please try again.');
      }
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

  updateCommentText(value: string): void {
    this.newComment.set(value);
  }

  updateCommentAuthor(value: string): void {
    this.commentAuthor.set(value);
  }
}
