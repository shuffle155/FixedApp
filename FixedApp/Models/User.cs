using System;
using System.Collections.Generic;

namespace FixedApp.Models;

public partial class User
{
    public int Id { get; set; }

    public string Username { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string Password { get; set; } = null!;

    public virtual ICollection<Document> Documents { get; set; } = new List<Document>();

    public virtual ICollection<Role> Roles { get; set; } = new List<Role>();
}
