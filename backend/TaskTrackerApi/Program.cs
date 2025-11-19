using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.EntityFrameworkCore;
using TaskTrackerApi.Data;
using TaskTrackerApi.Dtos;
using TaskTrackerApi.Models;
using Microsoft.OpenApi.Models;
using ModelTaskStatus = TaskTrackerApi.Models.TaskStatus;

var builder = WebApplication.CreateBuilder(args);

// ---------------------- Services ----------------------

// Use System.Text.Json to send enums as strings
builder.Services.Configure<Microsoft.AspNetCore.Mvc.JsonOptions>(options =>
{
    options.JsonSerializerOptions.WriteIndented = true;
});

// EF Core InMemory
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseInMemoryDatabase("TaskTrackerDb"));

// Http request logging
builder.Services.AddHttpLogging(logging =>
{
    logging.LoggingFields = HttpLoggingFields.RequestPropertiesAndHeaders |
                            HttpLoggingFields.ResponsePropertiesAndHeaders;
});

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// CORS
var allowedOrigin = "http://localhost:4200";
builder.Services.AddCors(options =>
{
    options.AddPolicy("SpaDevPolicy", policy =>
    {
        policy.WithOrigins(allowedOrigin)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

var app = builder.Build();

// ---------------------- Middleware ----------------------
app.UseHttpLogging();

app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        context.Response.ContentType = "application/problem+json";

        var exceptionHandlerPathFeature =
            context.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerPathFeature>();
        var ex = exceptionHandlerPathFeature?.Error;

        var problem = new ProblemDetails
        {
            Type = "https://example.com/probs/server-error",
            Title = "An unexpected error occurred.",
            Status = StatusCodes.Status500InternalServerError,
            Detail = ex?.Message ?? "An unexpected error occurred."
        };

        var traceId = System.Diagnostics.Activity.Current?.Id ?? context.TraceIdentifier;
        problem.Extensions["traceId"] = traceId;

        context.Response.StatusCode = problem.Status.Value;
        await context.Response.WriteAsJsonAsync(problem);
    });
});

app.UseCors("SpaDevPolicy");

// Swagger only in Development
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Seed data
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    SeedData(db);
}

// ---------------------- Endpoints ----------------------
var tasks = app.MapGroup("/api/tasks");

// GET /api/tasks?q=..&sort=dueDate:asc|desc
tasks.MapGet("", async (AppDbContext db, string? q, string? sort) =>
{
    IQueryable<TaskItem> query = db.Tasks;

    if (!string.IsNullOrWhiteSpace(q))
    {
        var qLower = q.Trim().ToLowerInvariant();
        query = query.Where(t =>
            (t.Title ?? "").ToLower().Contains(qLower) ||
            (t.Description ?? "").ToLower().Contains(qLower));
    }

    var sortDirection = "asc";
    if (!string.IsNullOrWhiteSpace(sort))
    {
        var parts = sort.Split(':', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 2 && parts[0] == "dueDate" && (parts[1] == "asc" || parts[1] == "desc"))
            sortDirection = parts[1];
    }

    query = sortDirection == "desc"
        ? query.OrderByDescending(t => t.DueDate ?? DateTime.MaxValue)
        : query.OrderBy(t => t.DueDate ?? DateTime.MaxValue);

    return Results.Ok(await query.ToListAsync());
});

// GET /api/tasks/{id}
tasks.MapGet("/{id:int}", async (AppDbContext db, int id) =>
{
    var item = await db.Tasks.FindAsync(id);
    return item is not null
        ? Results.Ok(item)
        : Results.Problem(
            detail: $"Task with id {id} not found.",
            title: "Not Found",
            statusCode: StatusCodes.Status404NotFound,
            type: "https://example.com/probs/not-found");
});

// POST /api/tasks
tasks.MapPost("", async (AppDbContext db, CreateTaskDto dto) =>
{
    if (string.IsNullOrWhiteSpace(dto.Title))
        return ValidationProblem("Title is required and cannot be empty.");

    if (dto.Status is not null && !Enum.IsDefined(typeof(ModelTaskStatus), dto.Status.Value))
        return ValidationProblem("Status must be one of: New, InProgress, Done.");

    if (dto.Priority is not null && !Enum.IsDefined(typeof(TaskPriority), dto.Priority.Value))
        return ValidationProblem("Priority must be one of: Low, Medium, High.");

    var newTask = new TaskItem
    {
        Title = dto.Title!.Trim(),
        Description = string.IsNullOrWhiteSpace(dto.Description) ? null : dto.Description!.Trim(),
        Status = dto.Status ?? ModelTaskStatus.New,
        Priority = dto.Priority ?? TaskPriority.Medium,
        DueDate = dto.DueDate?.ToUniversalTime(),
        CreatedAt = DateTime.UtcNow
    };

    db.Tasks.Add(newTask);
    await db.SaveChangesAsync();

    return Results.Created($"/api/tasks/{newTask.Id}", newTask);
});

// PUT /api/tasks/{id}
tasks.MapPut("/{id:int}", async (AppDbContext db, int id, UpdateTaskDto dto) =>
{
    if (string.IsNullOrWhiteSpace(dto.Title))
        return ValidationProblem("Title is required and cannot be empty.");

    if (dto.Status is not null && !Enum.IsDefined(typeof(ModelTaskStatus), dto.Status.Value))
        return ValidationProblem("Status must be one of: New, InProgress, Done.");

    if (dto.Priority is not null && !Enum.IsDefined(typeof(TaskPriority), dto.Priority.Value))
        return ValidationProblem("Priority must be one of: Low, Medium, High.");

    var item = await db.Tasks.FindAsync(id);
    if (item is null)
        return Results.Problem(
            detail: $"Task with id {id} not found.",
            title: "Not Found",
            statusCode: StatusCodes.Status404NotFound,
            type: "https://example.com/probs/not-found");

    item.Title = dto.Title!.Trim();
    item.Description = string.IsNullOrWhiteSpace(dto.Description) ? null : dto.Description!.Trim();
    item.Status = dto.Status ?? item.Status;
    item.Priority = dto.Priority ?? item.Priority;
    item.DueDate = dto.DueDate?.ToUniversalTime();

    await db.SaveChangesAsync();
    return Results.Ok(item);
});

// DELETE /api/tasks/{id}
tasks.MapDelete("/{id:int}", async (AppDbContext db, int id) =>
{
    var item = await db.Tasks.FindAsync(id);
    if (item is null)
        return Results.Problem(
            detail: $"Task with id {id} not found.",
            title: "Not Found",
            statusCode: StatusCodes.Status404NotFound,
            type: "https://example.com/probs/not-found");

    db.Tasks.Remove(item);
    await db.SaveChangesAsync();
    return Results.NoContent();
});

// Validation helper
IResult ValidationProblem(string detail)
{
    var problem = new ValidationProblemDetails
    {
        Type = "https://example.com/probs/validation",
        Title = "One or more validation errors occurred.",
        Status = StatusCodes.Status400BadRequest,
        Detail = detail
    };
    problem.Extensions["errors"] = new[] { detail };
    return Results.Problem(detail: detail, title: problem.Title, statusCode: problem.Status, type: problem.Type);
}

app.Run();

// ---------------------- Seed method ----------------------
static void SeedData(AppDbContext db)
{
    if (db.Tasks.Any()) return;

    var now = DateTime.UtcNow;

    db.Tasks.AddRange(
        new TaskItem
        {
            Title = "Create project skeleton",
            Description = "Initialize repository, create .NET project, add minimal files.",
            Status = ModelTaskStatus.Done,
            Priority = TaskPriority.High,
            DueDate = now.AddDays(-3),
            CreatedAt = now.AddDays(-10)
        },
        new TaskItem
        {
            Title = "Implement backend API",
            Description = "Implement endpoints and validation for Task Tracker.",
            Status = ModelTaskStatus.InProgress,
            Priority = TaskPriority.High,
            DueDate = now.AddDays(2),
            CreatedAt = now.AddDays(-2)
        },
        new TaskItem
        {
            Title = "Design Angular frontend",
            Description = "Create components and services that talk to the API.",
            Status = ModelTaskStatus.New,
            Priority = TaskPriority.Medium,
            DueDate = now.AddDays(7),
            CreatedAt = now
        },
        new TaskItem
        {
            Title = "Write tests",
            Description = "Add unit & integration tests for endpoints.",
            Status = ModelTaskStatus.New,
            Priority = TaskPriority.Low,
            CreatedAt = now
        }
    );

    db.SaveChanges();
}
