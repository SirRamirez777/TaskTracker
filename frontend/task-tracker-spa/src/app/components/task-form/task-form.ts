import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { TaskService } from '../../services/task.service';
import { ActivatedRoute, Router } from '@angular/router';
import { Task, ProblemDetails, TaskStatus, TaskPriority } from '../../models/task.model';


@Component({
  selector: 'app-task-form',
  templateUrl: './task-form.html',
  styleUrls: ['./task-form.scss']
})
export class TaskFormComponent implements OnInit {
  taskForm!: FormGroup;
  loading = false;
  error?: string;
  taskId?: number;
  statusOptions: TaskStatus[] = ['New','InProgress','Done'];
  priorityOptions: TaskPriority[] = ['Low','Medium','High'];

  constructor(private fb: FormBuilder, private taskService: TaskService, private router: Router, private route: ActivatedRoute) {}

  ngOnInit(): void {
    this.taskForm = this.fb.group({
      title: ['', Validators.required],
      description: [''],
      status: ['New', Validators.required],
      priority: ['Medium', Validators.required],
      dueDate: ['']
    });

    this.route.params.subscribe(params => {
      if (params['id']) {
        this.taskId = +params['id'];
        this.loadTask(this.taskId);
      }
    });
  }

  loadTask(id: number) {
    this.loading = true;
    this.taskService.getTask(id).subscribe({
      next: task => {
        this.taskForm.patchValue({
          title: task.title,
          description: task.description,
          status: task.status,
          priority: task.priority,
          dueDate: task.dueDate ? task.dueDate.split('T')[0] : ''
        });
        this.loading = false;
      },
      error: (err: ProblemDetails) => { this.error = err.detail || err.title; this.loading = false; }
    });
  }

  submit() {
    if (this.taskForm.invalid) return;

    this.loading = true;
    this.error = undefined;

    const payload = this.taskForm.value;
    const obs = this.taskId ? this.taskService.updateTask(this.taskId, payload) : this.taskService.createTask(payload);

    obs.subscribe({
      next: () => { this.loading = false; this.router.navigate(['/tasks']); },
      error: (err: ProblemDetails) => { this.error = err.detail || err.title; this.loading = false; }
    });
  }
}
