namespace IdentityDefense.Domain.Entities;

public class User
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public string Role { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; private set; }

    private User()
    {
    }

    public User(string name, string email, string passwordHash, string role)
    {
        Id = Guid.NewGuid();
        Name = name;
        Email = email.ToLower().Trim();
        PasswordHash = passwordHash;
        Role = role;
        CreatedAt = DateTime.UtcNow;

        Validate();
    }

    private void Validate()
    {
        if (string.IsNullOrWhiteSpace(Name))
            throw new ArgumentException("Name is required.");

        if (string.IsNullOrWhiteSpace(Email))
            throw new ArgumentException("Email is required.");

        if (string.IsNullOrWhiteSpace(PasswordHash))
            throw new ArgumentException("Password hash is required.");

        if (Role is not "Admin" and not "Analyst" and not "Viewer")
            throw new ArgumentException("Invalid role.");
    }
}