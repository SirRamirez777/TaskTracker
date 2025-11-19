# Task Tracker

Hi just a quick walkthrough of this project,
This repository contains a full-stack Task Tracker application, including i have divided int 2 as follows since the instruction was this should be in single repo:
- **Backend**: `TaskTrackerApi` (I used C# .NET Web API)
- **Frontend**: `task-tracker-spa` (I developed using Angular SPA)

The backend and frontend are separated into `backend/` and `frontend/` folders.

1. Navigate to the **backend folder**:

```bash
cd backend
Restore dependencies:
dotnet restore
Run the API:
dotnet run
The API should run at https://localhost:5500

2. Navigate to the **Frontend folder**:
cd frontend
Install dependencies:
npm install
Start the Angular development server:
ng serve
The SPA should run at http://localhost:4200 and communicate with the backend API.

3.Notes
*Make sure the backend is running before using the frontend.
*Update API URLs in the frontend if needed (e.g., environment.ts).

4. What i have implemented as per the task at hand.
**Backend (`backend/`)**:  
  C# .NET Web API handling CRUD operations for tasks, including:
  - REST endpoints: GET, POST, PUT, DELETE tasks
  - Data model: Task, TaskStatus, TaskPriority
  - ProblemDetails responses for consistent error handling

- **Frontend (`frontend/`)**:  
  Angular SPA providing user interface for task management:
  - Components: TaskListComponent, TaskFormComponent
  - Features: Create, edit, delete, search, sort tasks
  - Reactive forms and HTTP client for API communication

5.**Separate Backend and Frontend**  
  Keeping the backend and frontend in separate folders allows independent development and deployment.  

- **Angular SPA**  
  Provides a responsive and modern UI. Chose Angular over other frameworks for type safety (TypeScript) and easy integration with forms and routing.

- **Reactive Forms**  
  Used reactive forms for validation and maintainability.

- **REST API**  
  Standard RESTful API design ensures frontend can easily consume endpoints.

**Happy to assist if you having trouble navigating or running the application** :) 

