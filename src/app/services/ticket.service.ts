import { Injectable, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Ticket, TicketCategory, TicketPriority, TicketStatus, Comment, TicketStats } from '../models/ticket.model';
import { BehaviorSubject, firstValueFrom } from 'rxjs';
import { tap } from 'rxjs/operators';

@Injectable({
  providedIn: 'root'
})
export class TicketService {
  private apiUrl = 'http://localhost:5000/api';
  private tickets = signal<Ticket[]>([]);
  private ticketsSubject = new BehaviorSubject<Ticket[]>([]);
  public tickets$ = this.ticketsSubject.asObservable();

  constructor(private http: HttpClient) {
    this.loadTicketsFromAPI();
  }

  private async loadTicketsFromAPI(): Promise<void> {
    try {
      const tickets = await firstValueFrom(
        this.http.get<Ticket[]>(`${this.apiUrl}/tickets`).pipe(
          tap(data => this.normalizeTicketDates(data))
        )
      );
      this.tickets.set(tickets);
      this.ticketsSubject.next(tickets);
    } catch (error) {
      console.error('Error loading tickets from API:', error);
    }
  }

  private normalizeTicketDates(tickets: Ticket[]): void {
    tickets.forEach(ticket => {
      ticket.createdAt = new Date(ticket.createdAt);
      ticket.updatedAt = new Date(ticket.updatedAt);
      if (ticket.resolvedAt) {
        ticket.resolvedAt = new Date(ticket.resolvedAt);
      }
      if (ticket.comments) {
        ticket.comments.forEach(comment => {
          comment.createdAt = new Date(comment.createdAt);
        });
      }
    });
  }

  getAllTickets(): Ticket[] {
    return this.tickets();
  }

  getTicketById(id: string): Ticket | undefined {
    return this.tickets().find(ticket => ticket.id === id);
  }

  async createTicket(ticket: Omit<Ticket, 'id' | 'createdAt' | 'updatedAt' | 'comments'>): Promise<Ticket> {
    try {
      const newTicket = await firstValueFrom(
        this.http.post<Ticket>(`${this.apiUrl}/tickets`, ticket).pipe(
          tap(data => {
            data.createdAt = new Date(data.createdAt);
            data.updatedAt = new Date(data.updatedAt);
          })
        )
      );
      await this.loadTicketsFromAPI();
      return newTicket;
    } catch (error) {
      console.error('Error creating ticket:', error);
      throw error;
    }
  }

  async updateTicket(id: string, updates: Partial<Ticket>): Promise<Ticket | null> {
    try {
      const ticket = this.getTicketById(id);
      if (!ticket) return null;
      const updatedTicket = { ...ticket, ...updates };
      await firstValueFrom(
        this.http.put(`${this.apiUrl}/tickets/${id}`, updatedTicket)
      );
      await this.loadTicketsFromAPI();
      return this.getTicketById(id) || null;
    } catch (error) {
      console.error('Error updating ticket:', error);
      throw error;
    }
  }

  async deleteTicket(id: string): Promise<boolean> {
    try {
      await firstValueFrom(
        this.http.delete(`${this.apiUrl}/tickets/${id}`)
      );
      await this.loadTicketsFromAPI();
      return true;
    } catch (error) {
      console.error('Error deleting ticket:', error);
      return false;
    }
  }

  async addComment(ticketId: string, author: string, text: string): Promise<Comment | null> {
    try {
      const comment = { ticketId, author, text };
      const newComment = await firstValueFrom(
        this.http.post<Comment>(`${this.apiUrl}/comments`, comment).pipe(
          tap(data => {
            data.createdAt = new Date(data.createdAt);
          })
        )
      );
      await this.loadTicketsFromAPI();
      return newComment;
    } catch (error) {
      console.error('Error adding comment:', error);
      return null;
    }
  }

  async getTicketStats(): Promise<TicketStats> {
    try {
      return await firstValueFrom(
        this.http.get<TicketStats>(`${this.apiUrl}/tickets/stats`)
      );
    } catch (error) {
      console.error('Error fetching stats:', error);
      const tickets = this.tickets();
      return {
        total: tickets.length,
        open: tickets.filter((t: Ticket) => t.status === TicketStatus.OPEN).length,
        inProgress: tickets.filter((t: Ticket) => t.status === TicketStatus.IN_PROGRESS).length,
        resolved: tickets.filter((t: Ticket) => t.status === TicketStatus.RESOLVED).length,
        closed: tickets.filter((t: Ticket) => t.status === TicketStatus.CLOSED).length,
        highPriority: tickets.filter((t: Ticket) => 
          t.priority === TicketPriority.HIGH || t.priority === TicketPriority.CRITICAL
        ).length
      };
    }
  }

  getTicketsByStatus(status: TicketStatus): Ticket[] {
    return this.tickets().filter(ticket => ticket.status === status);
  }

  getTicketsByPriority(priority: TicketPriority): Ticket[] {
    return this.tickets().filter(ticket => ticket.priority === priority);
  }

  async searchTickets(searchTerm: string): Promise<Ticket[]> {
    try {
      const tickets = await firstValueFrom(
        this.http.get<Ticket[]>(`${this.apiUrl}/tickets/search?term=${encodeURIComponent(searchTerm)}`).pipe(
          tap(data => this.normalizeTicketDates(data))
        )
      );
      return tickets;
    } catch (error) {
      console.error('Error searching tickets:', error);
      const term = searchTerm.toLowerCase();
      return this.tickets().filter(ticket =>
        ticket.title.toLowerCase().includes(term) ||
        ticket.description.toLowerCase().includes(term) ||
        ticket.assignedTo.toLowerCase().includes(term) ||
        ticket.reportedBy.toLowerCase().includes(term)
      );
    }
  }
}
