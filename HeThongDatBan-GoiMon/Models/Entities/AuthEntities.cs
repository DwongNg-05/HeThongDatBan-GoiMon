namespace HeThongDatBan_GoiMon.Models.Entities;

public sealed class Role
{
    public int Id { get; set; }
    public string Code { get; set; } = null!;
    public string Name { get; set; } = null!;
    public ICollection<EmployeeAccount> EmployeeAccounts { get; set; } = new List<EmployeeAccount>();
    public ICollection<RoleModulePermission> Permissions { get; set; } = new List<RoleModulePermission>();
}

public sealed class AppModule
{
    public int Id { get; set; }
    public string Code { get; set; } = null!;
    public string Name { get; set; } = null!;
    public ICollection<RoleModulePermission> Permissions { get; set; } = new List<RoleModulePermission>();
}

public sealed class RoleModulePermission
{
    public int RoleId { get; set; }
    public int AppModuleId { get; set; }
    public string AccessLevel { get; set; } = null!;
    public string Scope { get; set; } = null!;
    public Role Role { get; set; } = null!;
    public AppModule AppModule { get; set; } = null!;
}

public sealed class EmployeeAccount
{
    public int Id { get; set; }
    public int RoleId { get; set; }
    public string UserName { get; set; } = null!;
    public string FullName { get; set; } = null!;
    public string PhoneNumber { get; set; } = null!;
    public string PasswordHash { get; set; } = null!;
    public bool IsActive { get; set; } = true;
    public bool MustChangePassword { get; set; } = true;
    public DateTimeOffset? PasswordChangedAtUtc { get; set; }
    public DateTimeOffset? LockoutUntilUtc { get; set; }
    public Role Role { get; set; } = null!;
    public ICollection<FailedLoginAttempt> FailedLoginAttempts { get; set; } = new List<FailedLoginAttempt>();
    public ICollection<LoginAuditLog> LoginAuditLogs { get; set; } = new List<LoginAuditLog>();
}

public sealed class FailedLoginAttempt
{
    public long Id { get; set; }
    public int EmployeeAccountId { get; set; }
    public DateTimeOffset OccurredAtUtc { get; set; }
    public EmployeeAccount EmployeeAccount { get; set; } = null!;
}

public sealed class LoginAuditLog
{
    public long Id { get; set; }
    public int EmployeeAccountId { get; set; }
    public bool WasSuccessful { get; set; }
    public string? IpAddress { get; set; }
    public DateTimeOffset OccurredAtUtc { get; set; }
    public EmployeeAccount EmployeeAccount { get; set; } = null!;
}
