namespace BasketService.Domain.Entities;

/// <summary>
/// User aggregate root — minimal representation for local dev.
/// Will be extended or replaced when integrating with Azure AD B2C.
/// </summary>
public class User
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Factory method — creates a new User instance.
    /// </summary>
    public static User Create(string email, string name)
    {
        return new User
        {
            Id = Guid.NewGuid(),
            Email = email,
            Name = name,
            CreatedAt = DateTime.UtcNow
        };
    }
}
