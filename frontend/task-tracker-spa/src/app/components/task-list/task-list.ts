import { Component, OnInit } from '@angular/core';
import { Task, ProblemDetails } from '../../models/task.model';
import { TaskService } from '../../services/task.service';

@Component({
  selector: 'app-task-list',
  templateUrl: './task-list.html',
  styleUrls: ['./task-list.scss']
})
export class TaskListComponent implements OnInit {
  tasks: Task[] = [];
  loading = false;
  error?: string;
  search = '';
  sort: 'dueDate:asc' | 'dueDate:desc' = 'dueDate:asc';

  constructor(private taskService: TaskService) {}

  ngOnInit(): void {
    this.loadTasks();
  }

  loadTasks(): void {
    this.loading = true;
    this.error = undefined;
    this.taskService.getTasks(this.search, this.sort).subscribe({
      next: tasks => { this.tasks = tasks; this.loading = false; },
      error: (err: ProblemDetails) => { this.error = err.detail || err.title; this.loading = false; }
    });
  }

  onSearchChange(value: string) {
    this.search = value;
    this.loadTasks();
  }

  toggleSort() {
    this.sort = this.sort === 'dueDate:asc' ? 'dueDate:desc' : 'dueDate:asc';
    this.loadTasks();
  }
}
