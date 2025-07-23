namespace TodoApi.Models
{
    public class TodoItem
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public bool IsComplete { get; set; }

        public int UserId { get; set; } // Foreign key to User
        public User User { get; set; } // Navigation property
    }
}