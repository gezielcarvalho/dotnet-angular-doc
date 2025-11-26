namespace Backend.Models.EDM
{
    public class User : SoftDeletableEntity
    {
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string? Department { get; set; }
        public string Role { get; set; } = "Viewer";
        public bool IsActive { get; set; } = true;
        public DateTime? LastLoginAt { get; set; }
        
        // Navigation properties
        public ICollection<Folder> OwnedFolders { get; set; } = new List<Folder>();
        public ICollection<Document> OwnedDocuments { get; set; } = new List<Document>();
        public ICollection<Permission> Permissions { get; set; } = new List<Permission>();
        public ICollection<Comment> Comments { get; set; } = new List<Comment>();
        public ICollection<Workflow> InitiatedWorkflows { get; set; } = new List<Workflow>();
        public ICollection<WorkflowStep> AssignedWorkflowSteps { get; set; } = new List<WorkflowStep>();
    }
}
