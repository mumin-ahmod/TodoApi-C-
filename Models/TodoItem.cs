using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace TodoApi.Models
{
    public class TodoItem
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public bool IsComplete { get; set; }

        public int UserId { get; set; } // Foreign key to User

        [JsonIgnore]
        [ValidateNever]
        public User User { get; set; } // Navigation property
    }
}