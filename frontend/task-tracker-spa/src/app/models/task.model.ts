export type TaskStatus = 'New' | 'InProgress' | 'Done';
export type TaskPriority = 'Low' | 'Medium' | 'High';

export interface Task {
  id: number;
  title: string;
  description?: string;
  status: TaskStatus;
  priority: TaskPriority;
  dueDate?: string; // ISO string
  createdAt: string; // ISO string
}

export interface ProblemDetails {
  type?: string;
  title: string;
  status?: number;
  detail?: string;
}
