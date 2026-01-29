using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ClaimsPortal.Data;
using ClaimsPortal.Services;

namespace ClaimsPortal.Services
{
    public class LetterGenerationWorker : BackgroundService
    {
        private readonly ILogger<LetterGenerationWorker> _logger;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly TimeSpan _pollInterval = TimeSpan.FromSeconds(5);

        public LetterGenerationWorker(ILogger<LetterGenerationWorker> logger, IServiceScopeFactory scopeFactory)
        {
            _logger = logger;
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("LetterGenerationWorker started");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var db = scope.ServiceProvider.GetRequiredService<ClaimsPortalDbContext>();

                    // Atomically claim a single pending job using UPDATE...OUTPUT to avoid races across worker instances
                    LetterGenQueue? job = null;
                    var conn = db.Database.GetDbConnection();
                    await conn.OpenAsync(stoppingToken);
                    try
                    {
                        using var cmd = conn.CreateCommand();
                        cmd.CommandTimeout = 60;
                        cmd.CommandText = @" 
;WITH cte AS (
    SELECT TOP (1) *
    FROM dbo.LetterGen_Queue WITH (ROWLOCK, READPAST)
    WHERE [Status] = 'Pending'
    ORDER BY CreatedAt
)
UPDATE cte
SET [Status] = 'InProgress', LastAttemptAt = SYSUTCDATETIME(), ProcessingHostname = @host, Tries = Tries + 1
OUTPUT inserted.QueueId, inserted.ClaimNumber, inserted.SelectedRuleIds, inserted.Status, inserted.Tries, inserted.CreatedAt, inserted.LastAttemptAt, inserted.LastError, inserted.ProcessingHostname;";
                        var p = cmd.CreateParameter();
                        p.ParameterName = "@host";
                        p.Value = Environment.MachineName;
                        cmd.Parameters.Add(p);

                        using var reader = await cmd.ExecuteReaderAsync(stoppingToken);
                        if (await reader.ReadAsync(stoppingToken))
                        {
                            job = new LetterGenQueue
                            {
                                QueueId = reader.GetInt64(0),
                                ClaimNumber = reader.IsDBNull(1) ? string.Empty : reader.GetString(1),
                                SelectedRuleIds = reader.IsDBNull(2) ? null : reader.GetString(2),
                                Status = reader.IsDBNull(3) ? "Pending" : reader.GetString(3),
                                Tries = reader.IsDBNull(4) ? 0 : reader.GetInt32(4),
                                CreatedAt = reader.IsDBNull(5) ? DateTimeOffset.UtcNow : reader.GetFieldValue<DateTimeOffset>(5),
                                LastAttemptAt = reader.IsDBNull(6) ? null : reader.GetFieldValue<DateTimeOffset>(6),
                                LastError = reader.IsDBNull(7) ? null : reader.GetString(7),
                                ProcessingHostname = reader.IsDBNull(8) ? null : reader.GetString(8)
                            };
                        }
                    }
                    finally
                    {
                        try { conn.Close(); } catch { }
                    }

                    if (job == null)
                    {
                        await Task.Delay(_pollInterval, stoppingToken);
                        continue;
                    }

                    _logger.LogInformation("Processing LetterGen job {QueueId} for Claim {ClaimNumber}", job.QueueId, job.ClaimNumber);

                    try
                    {
                        var claimService = scope.ServiceProvider.GetRequiredService<IDatabaseClaimService>();
                        var selected = job.SelectedRuleIds?.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()).ToList();

                        await claimService.GenerateLettersForClaimAsync(job.ClaimNumber, selected);

                        // mark completed
                        using (var innerScope = _scopeFactory.CreateScope())
                        {
                            var innerDb = innerScope.ServiceProvider.GetRequiredService<ClaimsPortalDbContext>();
                            var q = await innerDb.LetterGenQueue.FirstOrDefaultAsync(x => x.QueueId == job.QueueId, stoppingToken);
                            if (q != null)
                            {
                                q.Status = "Completed";
                                q.LastAttemptAt = DateTimeOffset.UtcNow;
                                q.LastError = null;
                                await innerDb.SaveChangesAsync(stoppingToken);
                            }
                        }

                        _logger.LogInformation("LetterGen job {QueueId} completed for Claim {ClaimNumber}", job.QueueId, job.ClaimNumber);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Processing LetterGen job {QueueId} failed for Claim {ClaimNumber}", job.QueueId, job.ClaimNumber);

                        using (var innerScope = _scopeFactory.CreateScope())
                        {
                            var innerDb = innerScope.ServiceProvider.GetRequiredService<ClaimsPortalDbContext>();
                            var q = await innerDb.LetterGenQueue.FirstOrDefaultAsync(x => x.QueueId == job.QueueId, stoppingToken);
                            if (q != null)
                            {
                                q.LastError = ex.ToString();
                                q.LastAttemptAt = DateTimeOffset.UtcNow;
                                q.Tries = q.Tries + 1;
                                if (q.Tries >= 5)
                                {
                                    q.Status = "Failed";
                                }
                                else
                                {
                                    q.Status = "Pending";
                                }
                                await innerDb.SaveChangesAsync(stoppingToken);
                            }
                        }
                    }
                }
                catch (TaskCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    // Shutting down
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unexpected error in LetterGenerationWorker loop");
                    await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
                }
            }

            _logger.LogInformation("LetterGenerationWorker stopping");
        }
    }
}
