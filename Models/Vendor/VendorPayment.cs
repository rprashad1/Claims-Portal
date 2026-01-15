namespace ClaimsPortal.Models;

/// <summary>
/// Vendor Payment Information
/// Stores payment preferences and frequency for a vendor
/// Supports multiple payment dates/days for high-volume vendors
/// </summary>
public class VendorPayment
{
    /// <summary>
    /// Whether vendor receives bulk payments
    /// </summary>
    public bool ReceivesBulkPayments { get; set; } = false;

    /// <summary>
    /// Bulk payment frequency (Monthly or Weekly)
    /// Only applicable if ReceivesBulkPayments = true
    /// </summary>
    public PaymentFrequency? Frequency { get; set; }

    /// <summary>
    /// Payment dates for monthly payments (1-31 or 32 for Last day of month)
    /// Supports multiple dates for high-volume vendors (e.g., twice per month)
    /// Only applicable if Frequency = Monthly
    /// </summary>
    public List<int> SelectedDates { get; set; } = new();

    /// <summary>
    /// Payment days for weekly payments (Monday-Friday)
    /// Supports multiple days for high-volume vendors (e.g., twice per week)
    /// Only applicable if Frequency = Weekly
    /// </summary>
    public List<DayOfWeek> SelectedDays { get; set; } = new();

    /// <summary>
    /// Alias for Frequency (backward compatibility)
    /// </summary>
    public BulkPaymentFrequency? PaymentFrequency
    {
        get => Frequency.HasValue ? (BulkPaymentFrequency?)((int)Frequency.Value) : null;
        set => Frequency = value.HasValue ? (PaymentFrequency?)((int)value.Value) : null;
    }

    /// <summary>
    /// Alias for SelectedDates (backward compatibility)
    /// </summary>
    public List<int> PaymentDatesOfMonth
    {
        get => SelectedDates;
        set => SelectedDates = value;
    }

    /// <summary>
    /// Alias for SelectedDays (backward compatibility)
    /// </summary>
    public List<PaymentDay> PaymentDaysOfWeek
    {
        get => SelectedDays.Select(d => (PaymentDay)((int)d - 1)).ToList();
        set => SelectedDays = value.Select(d => (DayOfWeek)((int)d + 1)).ToList();
    }

    /// <summary>
    /// Check if payment configuration is valid
    /// </summary>
    public bool IsPaymentConfigValid()
    {
        if (!ReceivesBulkPayments)
            return true;

        if (!Frequency.HasValue)
            return false;

        if (Frequency == Models.PaymentFrequency.Monthly)
        {
            // Must have at least one date selected
            if (!SelectedDates.Any())
                return false;

            // All dates must be valid (1-32, where 32 = last day)
            return SelectedDates.All(d => d > 0 && d <= 32);
        }

        if (Frequency == Models.PaymentFrequency.Weekly)
        {
            // Must have at least one day selected
            return SelectedDays.Any();
        }

        return false;
    }

    /// <summary>
    /// Get human-readable payment schedule
    /// </summary>
    public string GetPaymentScheduleDisplay()
    {
        if (!ReceivesBulkPayments)
            return "No bulk payments";

        if (Frequency == Models.PaymentFrequency.Monthly)
        {
            if (!SelectedDates.Any())
                return "Monthly - Not configured";

            var sortedDates = SelectedDates.OrderBy(d => d).ToList();
            var dateStrings = sortedDates.Select(d => d == 32 ? "Last day" : $"Day {d}").ToList();

            if (dateStrings.Count == 1)
                return $"Monthly - {dateStrings[0]}";

            return $"Monthly - {string.Join(", ", dateStrings)}";
        }

        if (Frequency == Models.PaymentFrequency.Weekly)
        {
            if (!SelectedDays.Any())
                return "Weekly - Not configured";

            var sortedDays = SelectedDays.OrderBy(d => (int)d).ToList();

            if (sortedDays.Count == 1)
                return $"Weekly - Every {sortedDays[0]}";

            return $"Weekly - {string.Join(", ", sortedDays)}";
        }

        return "Payment schedule not configured";
    }

    /// <summary>
    /// Add a payment date (for monthly payments)
    /// Prevents duplicates
    /// </summary>
    public void AddPaymentDate(int date)
    {
        if (date > 0 && date <= 32 && !SelectedDates.Contains(date))
        {
            SelectedDates.Add(date);
        }
    }

    /// <summary>
    /// Remove a payment date (for monthly payments)
    /// </summary>
    public void RemovePaymentDate(int date)
    {
        SelectedDates.Remove(date);
    }

    /// <summary>
    /// Add a payment day (for weekly payments)
    /// Prevents duplicates
    /// </summary>
    public void AddPaymentDay(DayOfWeek day)
    {
        if (!SelectedDays.Contains(day))
        {
            SelectedDays.Add(day);
        }
    }

    /// <summary>
    /// Remove a payment day (for weekly payments)
    /// </summary>
    public void RemovePaymentDay(DayOfWeek day)
    {
        SelectedDays.Remove(day);
    }

    /// <summary>
    /// Check if a specific date is selected (for monthly)
    /// </summary>
    public bool IsDateSelected(int date)
    {
        return SelectedDates.Contains(date);
    }

    /// <summary>
    /// Check if a specific day is selected (for weekly)
    /// </summary>
    public bool IsDaySelected(DayOfWeek day)
    {
        return SelectedDays.Contains(day);
    }

    /// <summary>
    /// Clear all payment dates/days
    /// </summary>
    public void ClearPaymentSchedule()
    {
        SelectedDates.Clear();
        SelectedDays.Clear();
    }
}
