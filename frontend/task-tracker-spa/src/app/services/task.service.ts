import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { environment } from '../../environments/environment';
import { Task, ProblemDetails } from '../models/task.model';
import { catchError } from 'rxjs/operators';
import { throwError, Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class TaskService {
  private baseUrl = environment.apiBaseUrl + '/tasks';

  constructor(private http: HttpClient) {}

  getTasks(q?: string, sort?: 'dueDate:asc' | 'dueDate:desc'): Observable<Task[]> {
    let params = new HttpParams();
    if (q) params = params.set('q', q);
    if (sort) params = params.set('sort', sort);
    return this.http.get<Task[]>(this.baseUrl, { params })
      .pipe(catchError(this.handleError));
  }

  getTask(id: number): Observable<Task> {
    return this.http.get<Task>(`${this.baseUrl}/${id}`).pipe(catchError(this.handleError));
  }

  createTask(task: Partial<Task>): Observable<Task> {
    return this.http.post<Task>(this.baseUrl, task).pipe(catchError(this.handleError));
  }

  updateTask(id: number, task: Partial<Task>): Observable<Task> {
    return this.http.put<Task>(`${this.baseUrl}/${id}`, task).pipe(catchError(this.handleError));
  }

  deleteTask(id: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`).pipe(catchError(this.handleError));
  }

  private handleError(error: any) {
    let problem: ProblemDetails = {
      title: 'Unknown error'
    };

    if (error.error && typeof error.error === 'object') {
      problem = error.error;
    } else if (error.message) {
      problem.title = error.message;
    }

    return throwError(() => problem);
  }
}
