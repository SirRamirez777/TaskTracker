using System;
using System.ComponentModel.DataAnnotations;

namespace TaskTrackerApi.Models
{
    public enum TaskStatus { New, InProgress, Done }
    public enum TaskPriority { Low, Medium, High }

    public class TaskItem
    {
        public int Id { get; set; }

        [Required]
        public string Title { get; set; } = default!;

        public string? Description { get; set; }

        public TaskStatus Status { get; set; } = TaskStatus.New;

        public TaskPriority Priority { get; set; } = TaskPriority.Medium;

        public DateTime? DueDate { get; set; } // should be stored as UTC DateTime

        public DateTime CreatedAt { get; set; } // set on create (UTC)
    }
}
