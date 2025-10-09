using System;
using NMKR.Shared.Functions;
using NMKR.Shared.Model;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace NMKR.BackgroundService.HostedServices
{
    public class LifesignLogger : BackgroundService
    {
        protected override async Task<Task> ExecuteAsync(CancellationToken cancellationToken)
        {
           

            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    if (GlobalFunctions.ServerId != 0)
                    {
                        await using EasynftprojectsContext db = new(GlobalFunctions.optionsBuilder.Options);
                        await db.Database.ExecuteSqlRawAsync(
                            $"update backgroundserver set lastlifesign=NOW() where id={GlobalFunctions.ServerId}",
                            cancellationToken: cancellationToken);
                    }

                    await Task.Delay(TimeSpan.FromSeconds(10), cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
            }

            return Task.CompletedTask;
        }
    }
}
