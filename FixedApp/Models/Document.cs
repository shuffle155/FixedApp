using System;
using System.Collections.Generic;

namespace FixedApp.Models;

public partial class Document
{
    public int Id { get; set; }

    public string Title { get; set; } = null!;

    public string Content { get; set; } = null!;

    public string? Path { get; set; }

    public int? OwnerId { get; set; }

    public int AccessLevel { get; set; }

    public DateTime CreatedDate { get; set; }

    public virtual User? Owner { get; set; }
}
