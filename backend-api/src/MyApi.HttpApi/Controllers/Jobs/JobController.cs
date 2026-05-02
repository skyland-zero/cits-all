using System;
using System.Collections.Generic;
using System.Linq;
using Cits;
using Hangfire;
using Hangfire.Storage;
using Hangfire.Storage.Monitoring;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MyApi.HttpApi.Controllers.Jobs;

/// <summary>
/// 定时任务/后台任务管理接口
/// </summary>
[Route("api/system/jobs")]
[Authorize]
public class JobController : BaseApiController
{
    // --- Monitoring ---

    [HttpGet("stats")]
    public ActionResult<StatisticsDto> GetStats()
    {
        var monitoringApi = JobStorage.Current.GetMonitoringApi();
        var stats = monitoringApi.GetStatistics();
        return Ok(stats);
    }

    [HttpGet("processing")]
    public ActionResult<List<JobInfoDto>> GetProcessingJobs([FromQuery] int from = 0, [FromQuery] int count = 20)
    {
        var monitoringApi = JobStorage.Current.GetMonitoringApi();
        var jobs = monitoringApi.ProcessingJobs(from, count)
            .Select(j => new JobInfoDto { Id = j.Key, JobName = j.Value.Job?.ToString() ?? "Unknown", ServerId = j.Value.ServerId, StartedAt = j.Value.StartedAt })
            .ToList();
        return Ok(jobs);
    }

    [HttpGet("failed")]
    public ActionResult<List<JobInfoDto>> GetFailedJobs([FromQuery] int from = 0, [FromQuery] int count = 20)
    {
        var monitoringApi = JobStorage.Current.GetMonitoringApi();
        var jobs = monitoringApi.FailedJobs(from, count)
            .Select(j => new JobInfoDto { Id = j.Key, JobName = j.Value.Job?.ToString() ?? "Unknown", ExceptionDetails = j.Value.ExceptionDetails, FailedAt = j.Value.FailedAt })
            .ToList();
        return Ok(jobs);
    }

    [HttpGet("recurring")]
    public ActionResult<List<RecurringJobDto>> GetRecurringJobs()
    {
        using var connection = JobStorage.Current.GetConnection();
        var jobs = connection.GetRecurringJobs()
            .Select(j => new RecurringJobDto 
            { 
                Id = j.Id, 
                JobName = j.Job?.ToString() ?? "Unknown", 
                Cron = j.Cron, 
                Queue = j.Queue, 
                NextExecution = j.NextExecution,
                LastExecution = j.LastExecution,
                LastJobId = j.LastJobId,
                LastJobState = j.LastJobState
            })
            .ToList();
        return Ok(jobs);
    }

    // --- Actions ---

    [HttpPost("recurring/{id}/trigger")]
    public ActionResult TriggerRecurringJob(string id)
    {
        RecurringJob.TriggerJob(id);
        return Ok();
    }

    [HttpDelete("recurring/{id}")]
    public ActionResult RemoveRecurringJob(string id)
    {
        RecurringJob.RemoveIfExists(id);
        return Ok();
    }

    [HttpDelete("{id}")]
    public ActionResult DeleteJob(string id)
    {
        var deleted = BackgroundJob.Delete(id);
        return deleted ? Ok() : NotFound();
    }
}

public class JobInfoDto
{
    public string Id { get; set; } = string.Empty;
    public string JobName { get; set; } = string.Empty;
    public string? ServerId { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? FailedAt { get; set; }
    public string? ExceptionDetails { get; set; }
}

public class RecurringJobDto
{
    public string Id { get; set; } = string.Empty;
    public string JobName { get; set; } = string.Empty;
    public string Cron { get; set; } = string.Empty;
    public string Queue { get; set; } = string.Empty;
    public DateTime? NextExecution { get; set; }
    public DateTime? LastExecution { get; set; }
    public string? LastJobId { get; set; }
    public string? LastJobState { get; set; }
}
