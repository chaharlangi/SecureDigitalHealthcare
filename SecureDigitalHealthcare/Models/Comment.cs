using System;
using System.Collections.Generic;

namespace SecureDigitalHealthcare.Models;

public partial class Comment
{
    public int Id { get; set; }

    public int? SenderId { get; set; }

    public int? ReceiverId { get; set; }

    public DateTime? Date { get; set; }

    public int? ReplyTo { get; set; }

    public string? Text { get; set; }

    public virtual ICollection<Comment> InverseReplyToNavigation { get; set; } = new List<Comment>();

    public virtual User? Receiver { get; set; }

    public virtual Comment? ReplyToNavigation { get; set; }

    public virtual User? Sender { get; set; }
}
