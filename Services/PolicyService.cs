using ClaimsPortal.Models;

namespace ClaimsPortal.Services;

public interface IPolicyService
{
    Task<List<Policy>> SearchPoliciesAsync(string searchTerm, string searchType); // searchType: "PolicyNumber", "InsuredName", "VIN"
    Task<Policy?> GetPolicyAsync(string policyNumber);
    Task<List<PolicyVehicle>> GetVehiclesByPolicyAsync(string policyNumber);
}

public class MockPolicyService : IPolicyService
{
    private readonly List<Policy> _policies = [];

    public MockPolicyService()
    {
        InitializeData();
    }

    private void InitializeData()
    {
        _policies.Add(new Policy
        {
            PolicyNumber = "CAF001711",
            RenewalNumber = "CAF001711-01",
            InsuredName = "John Smith",
            DoingBusinessAs = "JS Auto Services",
            PhoneNumber = "(555) 123-4567",
            Email = "john.smith@email.com",
            Address = "123 Main Street",
            Address2 = "Suite 100",
            City = "Springfield",
            State = "IL",
            ZipCode = "62701",
            ContactPerson = "John Smith",
            ContactPhone = "(555) 123-4567",
            ContactEmail = "john.smith@email.com",
            EffectiveDate = new DateTime(2024, 1, 15),
            ExpiryDate = new DateTime(2025, 1, 15),
            CancelDate = null,
            Status = "Active",
            Vehicles = new List<PolicyVehicle>
            {
                new PolicyVehicle { Vin = "1G1FB1S98D1234567", Make = "Chevrolet", Model = "Malibu", Year = 2013, IsActive = true },
                new PolicyVehicle { Vin = "5TDJKRFH0LS987654", Make = "Toyota", Model = "Sienna", Year = 2012, IsActive = true }
            }
        });

        _policies.Add(new Policy
        {
            PolicyNumber = "CAF001712",
            RenewalNumber = "CAF001712-01",
            InsuredName = "Jane Doe",
            DoingBusinessAs = "JD Enterprises",
            PhoneNumber = "(555) 234-5678",
            Email = "jane.doe@email.com",
            Address = "456 Oak Avenue",
            Address2 = "Apt 205",
            City = "Chicago",
            State = "IL",
            ZipCode = "60601",
            ContactPerson = "Jane Doe",
            ContactPhone = "(555) 234-5678",
            ContactEmail = "jane.doe@email.com",
            EffectiveDate = new DateTime(2024, 3, 20),
            ExpiryDate = new DateTime(2025, 3, 20),
            CancelDate = null,
            Status = "Active",
            Vehicles = new List<PolicyVehicle>
            {
                new PolicyVehicle { Vin = "2G1FB1E39D1123456", Make = "Chevrolet", Model = "Impala", Year = 2013, IsActive = true },
                new PolicyVehicle { Vin = "4T1BF1FK5CU987654", Make = "Toyota", Model = "Camry", Year = 2012, IsActive = true }
            }
        });

        _policies.Add(new Policy
        {
            PolicyNumber = "CAF001713",
            RenewalNumber = "CAF001713-01",
            InsuredName = "Robert Johnson",
            DoingBusinessAs = "RJ Transport",
            PhoneNumber = "(555) 345-6789",
            Email = "robert.johnson@email.com",
            Address = "789 Elm Street",
            Address2 = "",
            City = "Naperville",
            State = "IL",
            ZipCode = "60540",
            ContactPerson = "Robert Johnson",
            ContactPhone = "(555) 345-6789",
            ContactEmail = "robert.johnson@email.com",
            EffectiveDate = new DateTime(2023, 6, 10),
            ExpiryDate = new DateTime(2024, 6, 10),
            CancelDate = new DateTime(2024, 11, 15),
            Status = "Cancelled",
            Vehicles = new List<PolicyVehicle>
            {
                new PolicyVehicle { Vin = "1HGCM82633A004352", Make = "Honda", Model = "Accord", Year = 2003, IsActive = true }
            }
        });

        _policies.Add(new Policy
        {
            PolicyNumber = "CAF001714",
            RenewalNumber = "CAF001714-01",
            InsuredName = "Sarah Williams",
            DoingBusinessAs = "",
            PhoneNumber = "(555) 456-7890",
            Email = "sarah.williams@email.com",
            Address = "321 Pine Road",
            Address2 = "",
            City = "Aurora",
            State = "IL",
            ZipCode = "60504",
            ContactPerson = "Sarah Williams",
            ContactPhone = "(555) 456-7890",
            ContactEmail = "sarah.williams@email.com",
            EffectiveDate = new DateTime(2024, 2, 1),
            ExpiryDate = new DateTime(2025, 2, 1),
            CancelDate = null,
            Status = "Active",
            Vehicles = new List<PolicyVehicle>
            {
                new PolicyVehicle { Vin = "WBADT43452G770599", Make = "BMW", Model = "3 Series", Year = 2002, IsActive = true }
            }
        });
    }

    public Task<List<Policy>> SearchPoliciesAsync(string searchTerm, string searchType)
    {
        var results = searchType switch
        {
            "PolicyNumber" => _policies.Where(p => p.PolicyNumber.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)).ToList(),
            "InsuredName" => _policies.Where(p => p.InsuredName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)).ToList(),
            "VIN" => _policies.Where(p => p.Vehicles.Any(v => v.Vin.EndsWith(searchTerm) || v.Vin.Contains(searchTerm))).ToList(),
            _ => []
        };

        return Task.FromResult(results);
    }

    public Task<Policy?> GetPolicyAsync(string policyNumber)
    {
        var policy = _policies.FirstOrDefault(p => p.PolicyNumber == policyNumber);
        return Task.FromResult(policy);
    }

    public Task<List<PolicyVehicle>> GetVehiclesByPolicyAsync(string policyNumber)
    {
        var policy = _policies.FirstOrDefault(p => p.PolicyNumber == policyNumber);
        return Task.FromResult(policy?.Vehicles ?? []);
    }
}
