import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, ActivatedRoute, RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { TicketService } from '../../services/ticket.service';
import { Ticket, TicketCategory, TicketPriority, TicketStatus } from '../../models/ticket.model';

@Component({
  selector: 'app-ticket-form',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './ticket-form.component.html',
  styleUrl: './ticket-form.component.css'
})
export class TicketFormComponent implements OnInit {
  isEditMode = signal(false);
  ticketId = signal<string | null>(null);
  
  formData = signal({
    title: '',
    description: '',
    category: TicketCategory.GENERAL,
    priority: TicketPriority.MEDIUM,
    status: TicketStatus.OPEN,
    assignedTo: '',
    reportedBy: ''
  });

  categoryOptions = Object.values(TicketCategory);
  priorityOptions = Object.values(TicketPriority);
  statusOptions = Object.values(TicketStatus);

  errors = signal<Record<string, string>>({});

  constructor(
    private ticketService: TicketService,
    private router: Router,
    private route: ActivatedRoute
  ) {}

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    
    if (id) {
      this.isEditMode.set(true);
      this.ticketId.set(id);
      this.loadTicket(id);
    }
  }

  loadTicket(id: string): void {
    const ticket = this.ticketService.getTicketById(id);
    
    if (ticket) {
      this.formData.set({
        title: ticket.title,
        description: ticket.description,
        category: ticket.category,
        priority: ticket.priority,
        status: ticket.status,
        assignedTo: ticket.assignedTo,
        reportedBy: ticket.reportedBy
      });
    } else {
      alert('Ticket not found');
      this.router.navigate(['/tickets']);
    }
  }

  updateField(field: string, value: any): void {
    this.formData.set({
      ...this.formData(),
      [field]: value
    });
  }

  validateForm(): boolean {
    const newErrors: Record<string, string> = {};
    const data = this.formData();

    if (!data.title.trim()) {
      newErrors['title'] = 'Title is required';
    } else if (data.title.length < 5) {
      newErrors['title'] = 'Title must be at least 5 characters';
    }

    if (!data.description.trim()) {
      newErrors['description'] = 'Description is required';
    } else if (data.description.length < 10) {
      newErrors['description'] = 'Description must be at least 10 characters';
    }

    if (!data.assignedTo.trim()) {
      newErrors['assignedTo'] = 'Assigned to is required';
    }

    if (!data.reportedBy.trim()) {
      newErrors['reportedBy'] = 'Reported by is required';
    }

    this.errors.set(newErrors);
    return Object.keys(newErrors).length === 0;
  }

  async onSubmit(): Promise<void> {
    if (!this.validateForm()) {
      alert('Please fix the errors in the form');
      return;
    }

    const data = this.formData();

    try {
      if (this.isEditMode() && this.ticketId()) {
        // Update existing ticket
        const updated = await this.ticketService.updateTicket(this.ticketId()!, {
          title: data.title,
          description: data.description,
          category: data.category,
          priority: data.priority,
          status: data.status,
          assignedTo: data.assignedTo,
          reportedBy: data.reportedBy
        });

        if (updated) {
          alert('Ticket updated successfully!');
          this.router.navigate(['/tickets', this.ticketId()]);
        }
      } else {
        // Create new ticket
        const newTicket = await this.ticketService.createTicket({
          title: data.title,
          description: data.description,
          category: data.category,
          priority: data.priority,
          status: data.status,
          assignedTo: data.assignedTo,
          reportedBy: data.reportedBy
        });

        alert('Ticket created successfully!');
        this.router.navigate(['/tickets', newTicket.id]);
      }
    } catch (error) {
      console.error('Error saving ticket:', error);
      alert('Error saving ticket. Please try again.');
    }
  }

  onCancel(): void {
    if (this.isEditMode() && this.ticketId()) {
      this.router.navigate(['/tickets', this.ticketId()]);
    } else {
      this.router.navigate(['/tickets']);
    }
  }
}
