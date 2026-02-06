namespace ClaimsPortal.Models.Dto
{
    public class UpdateUserDto
    {
        public string? Password { get; set; }
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public string? Telephone { get; set; }
        public string? Extension { get; set; }
        public string? Status { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public long? AssignmentGroupId { get; set; }
        public long? RoleId { get; set; }
        public long? SupervisorUserId { get; set; }
        public decimal? ExpenseReserve { get; set; }
        public decimal? IndemnityReserve { get; set; }
        public decimal? ExpensePayment { get; set; }
        public decimal? IndemnityPayment { get; set; }
    }
}
