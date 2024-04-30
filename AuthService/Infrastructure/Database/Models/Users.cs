using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace AuthService.Infrastructure.Database.Models;

public class Users
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [BsonElement("email")]
    public string? Email { get; set; }

    [BsonElement("password")]
    public string? Password { get; set; }

    [BsonElement("isActive")]
    public bool? IsActive { get; set; }

    [BsonElement("hasValidVerificationToken")]
    public bool? HasValidVerificationToken { get; set; }

    [BsonElement("verificationToken")]
    public string? VerificationToken {  get; set; }

    [BsonElement("firstName")]
    public string? FirstName { get; set; }

    [BsonElement("lastName")]
    public string? LastName { get; set; }

    [BsonElement("createdAt")]
    public DateTime? CreatedAt { get; set; }

    [BsonElement("updatedAt")]
    public DateTime? UpdatedAt { get; set; }

    // Defining a public constructor to
    // set default values.
    public Users()
    {
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
        IsActive = false;
        VerificationToken = "";
        HasValidVerificationToken = false;
    }
    
}
