export interface User {
  id: string;
  name: string;
  email: string;
  role: UserRole;
}

export enum UserRole {
  ADMIN = 'Admin',
  AGENT = 'Agent',
  USER = 'User'
}
