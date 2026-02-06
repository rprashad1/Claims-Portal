using System;
using System.Collections.Generic;

namespace ClaimsPortal.Models
{
    public class Role
    {
        public long RoleId { get; set; }
        public string RoleName { get; set; } = string.Empty;
        public string? Description { get; set; }
    }

    public class AssignmentGroup
    {
        public long GroupId { get; set; }
        public string GroupCode { get; set; } = string.Empty;
        public string GroupName { get; set; } = string.Empty;
        public string? Description { get; set; }
    }

    public class User
    {
        public long UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public string? Telephone { get; set; }
        public string? Extension { get; set; }
        public string Status { get; set; } = "Active"; // Active / Disabled
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        // FKs
        public long? AssignmentGroupId { get; set; }
        public AssignmentGroup? AssignmentGroup { get; set; }
        public long? RoleId { get; set; }
        public Role? Role { get; set; }
        public long? SupervisorUserId { get; set; }

        // Reserve / payment limits
        public decimal? ExpenseReserve { get; set; }
        public decimal? IndemnityReserve { get; set; }
        public decimal? ExpensePayment { get; set; }
        public decimal? IndemnityPayment { get; set; }

        public string? CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
    }
}
