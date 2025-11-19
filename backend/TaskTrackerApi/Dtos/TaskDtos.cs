using System;
using System.ComponentModel.DataAnnotations;
using ModelTaskStatus = TaskTrackerApi.Models.TaskStatus;

using TaskTrackerApi.Models;

namespace TaskTrackerApi.Dtos
{
    public class CreateTaskDto
    {
        [Required]
        public string? Title { get; set; }

        public string? Description { get; set; }

        // Make enum nullable
        public ModelTaskStatus? Status { get; set; }


        public TaskPriority? Priority { get; set; }

        public DateTime? DueDate { get; set; }
    }

    public class UpdateTaskDto
    {
        [Required]
        public string? Title { get; set; }

        public string? Description { get; set; }

        // Make enum nullable
        public ModelTaskStatus? Status { get; set; }


        public TaskPriority? Priority { get; set; }

        public DateTime? DueDate { get; set; }
    }
}
