export interface Ticket {
  id: string;
  title: string;
  description: string;
  category: TicketCategory;
  priority: TicketPriority;
  status: TicketStatus;
  assignedTo: string;
  reportedBy: string;
  createdAt: Date;
  updatedAt: Date;
  resolvedAt?: Date;
  comments: Comment[];
}

export interface Comment {
  id: string;
  ticketId: string;
  author: string;
  text: string;
  createdAt: Date;
}

export enum TicketCategory {
  TECHNICAL = 'Technical',
  BILLING = 'Billing',
  GENERAL = 'General',
  FEATURE_REQUEST = 'Feature Request',
  BUG = 'Bug'
}

export enum TicketPriority {
  LOW = 'Low',
  MEDIUM = 'Medium',
  HIGH = 'High',
  CRITICAL = 'Critical'
}

export enum TicketStatus {
  OPEN = 'Open',
  IN_PROGRESS = 'In Progress',
  RESOLVED = 'Resolved',
  CLOSED = 'Closed',
  ON_HOLD = 'On Hold'
}

export interface TicketStats {
  total: number;
  open: number;
  inProgress: number;
  resolved: number;
  closed: number;
  highPriority: number;
}
