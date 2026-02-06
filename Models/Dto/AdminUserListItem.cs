using System;

namespace ClaimsPortal.Models.Dto
{
    public class AdminUserListItem
    {
        public long UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public string? Telephone { get; set; }
        public string? Extension { get; set; }
        public string? Status { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public ClaimsPortal.Models.AssignmentGroup? AssignmentGroup { get; set; }
        public ClaimsPortal.Models.Role? Role { get; set; }
        public long? SupervisorUserId { get; set; }
        public decimal? ExpenseReserve { get; set; }
        public decimal? IndemnityReserve { get; set; }
        public decimal? ExpensePayment { get; set; }
        public decimal? IndemnityPayment { get; set; }
    }
}
