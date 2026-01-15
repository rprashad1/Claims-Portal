using System;
using System.Data;
using System.Threading.Tasks;
using System.Text.Json;
using Microsoft.Data.SqlClient;

internal class ClaimInspectorApp
{
    private static async Task<int> Main(string[] args)
    {
        var baseDir = System.IO.Directory.GetCurrentDirectory();
        var candidate = System.IO.Path.Combine(baseDir, "appsettings.json");
        if (!System.IO.File.Exists(candidate))
        {
            candidate = System.IO.Path.Combine(AppContext.BaseDirectory, "..", "..", "appsettings.json");
        }
        if (!System.IO.File.Exists(candidate))
        {
            Console.WriteLine($"appsettings.json not found at {candidate}");
            return 1;
        }

        string json = await System.IO.File.ReadAllTextAsync(candidate);
        using var doc = JsonDocument.Parse(json);
        string? conn = null;
        if (doc.RootElement.TryGetProperty("ConnectionStrings", out var cs) && cs.TryGetProperty("ClaimsConnection", out var cc))
        {
            conn = cc.GetString();
        }
        if (string.IsNullOrEmpty(conn))
        {
            Console.WriteLine("Connection string 'ClaimsConnection' not found in appsettings.json");
            return 1;
        }

        Console.WriteLine("Querying FnolPropertyDamages for ClaimNumber='C26000013'...");
        using var cn = new SqlConnection(conn);
        await cn.OpenAsync();

        var cmd = cn.CreateCommand();
        cmd.CommandText = @"SELECT FnolPropertyDamageId, ClaimNumber, PropertyType, PropertyDescription, OwnerName, OwnerPhone, OwnerEmail, OwnerAddress, PropertyAddress, EstimatedDamage, DamageDescription, FnolId
                            FROM FnolPropertyDamages
                            WHERE ClaimNumber = @claim OR FnolId IN (SELECT FnolId FROM FNOL WHERE ClaimNumber = @claim)";
        cmd.Parameters.Add(new SqlParameter("@claim", SqlDbType.NVarChar, 4000) { Value = "C26000013" });

        using var reader = await cmd.ExecuteReaderAsync();
        var list = new System.Collections.Generic.List<string>();
        while (await reader.ReadAsync())
        {
            var id = reader["FnolPropertyDamageId"]?.ToString() ?? "";
            var type = reader["PropertyType"]?.ToString() ?? "";
            var desc = reader["PropertyDescription"]?.ToString() ?? "";
            var owner = reader["OwnerName"]?.ToString() ?? "";
            var est = reader["EstimatedDamage"]?.ToString() ?? "";
            list.Add($"Id={id}; Type={type}; Desc={desc}; Owner={owner}; Est={est}");
        }

        Console.WriteLine($"Found {list.Count} property damage rows.");
        for (int i = 0; i < list.Count; i++) Console.WriteLine($"[{i}] {list[i]}");

        return 0;
    }
}