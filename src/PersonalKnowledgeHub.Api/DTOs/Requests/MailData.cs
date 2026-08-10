using System.ComponentModel.DataAnnotations;

namespace PersonalKnowledgeHub.DTOs.Requests;

public class MailData
{
    [Required]
    public required string EmailToId { get; set; }
    [Required]
    public required string EmailToName { get; set; }
    [Required]
    public required string EmailSubject { get; set; }
    [Required]
    public required string EmailBody { get; set; }
}