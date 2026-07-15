using AeroERP.BuildingBlocks.Results;
using AeroERP.Modules.Reporting.Contracts;

namespace AeroERP.Modules.Reporting.Services;

/// <summary>
/// Reporting Service 服务契约，定义模块对外提供的业务能力。
/// </summary>
public interface IReportingService
{
    /// <summary>
    /// 获取Overview。
    /// </summary>
    /// <param name="cancellationToken">请求取消令牌。</param>
    Task<ReportingOverviewDto> GetOverviewAsync(CancellationToken cancellationToken);
    /// <summary>
    /// 执行Upsert Definition。
    /// </summary>
    /// <param name="request">请求参数。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    Task<OperationResult<ReportDefinitionDto>> UpsertDefinitionAsync(UpsertReportDefinitionRequest request, CancellationToken cancellationToken);
    /// <summary>
    /// 执行Run Report。
    /// </summary>
    /// <param name="request">请求参数。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    Task<OperationResult<ReportRunRecordDto>> RunReportAsync(RunReportRequest request, CancellationToken cancellationToken);
    /// <summary>
    /// 创建Export Task。
    /// </summary>
    /// <param name="request">请求参数。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    Task<OperationResult<ReportExportTaskDto>> CreateExportTaskAsync(CreateReportExportTaskRequest request, CancellationToken cancellationToken);
}
