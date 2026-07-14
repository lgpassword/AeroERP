using AeroERP.BuildingBlocks.Results;
using AeroERP.Modules.DocumentExchange.Contracts;

namespace AeroERP.Modules.DocumentExchange.Services;

/// <summary>
/// Document Exchange Service 服务契约，定义模块对外提供的业务能力。
/// </summary>
public interface IDocumentExchangeService
{
    /// <summary>
    /// 获取Overview。
    /// </summary>
    /// <param name="cancellationToken">请求取消令牌。</param>
    Task<DocumentExchangeOverviewDto> GetOverviewAsync(CancellationToken cancellationToken);
    /// <summary>
    /// 执行Upsert Import Template。
    /// </summary>
    /// <param name="request">请求参数。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    Task<OperationResult<ImportTemplateDto>> UpsertImportTemplateAsync(UpsertImportTemplateRequest request, CancellationToken cancellationToken);
    /// <summary>
    /// 执行Upsert Field Mapping。
    /// </summary>
    /// <param name="request">请求参数。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    Task<OperationResult<ImportFieldMappingDto>> UpsertFieldMappingAsync(UpsertImportFieldMappingRequest request, CancellationToken cancellationToken);
    /// <summary>
    /// 创建Import Batch。
    /// </summary>
    /// <param name="request">请求参数。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    Task<OperationResult<ImportBatchDto>> CreateImportBatchAsync(CreateImportBatchRequest request, CancellationToken cancellationToken);
    /// <summary>
    /// 执行Complete Import Batch。
    /// </summary>
    /// <param name="id">业务对象标识。</param>
    /// <param name="request">请求参数。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    Task<OperationResult<ImportBatchDto>> CompleteImportBatchAsync(Guid id, CompleteImportBatchRequest request, CancellationToken cancellationToken);
    /// <summary>
    /// 执行Fail Import Batch。
    /// </summary>
    /// <param name="id">业务对象标识。</param>
    /// <param name="request">请求参数。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    Task<OperationResult<ImportBatchDto>> FailImportBatchAsync(Guid id, FailFileTaskRequest request, CancellationToken cancellationToken);
    /// <summary>
    /// 创建Export Task。
    /// </summary>
    /// <param name="request">请求参数。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    Task<OperationResult<ExportFileTaskDto>> CreateExportTaskAsync(CreateExportFileTaskRequest request, CancellationToken cancellationToken);
    /// <summary>
    /// 执行Complete Export Task。
    /// </summary>
    /// <param name="id">业务对象标识。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    Task<OperationResult<ExportFileTaskDto>> CompleteExportTaskAsync(Guid id, CancellationToken cancellationToken);
    /// <summary>
    /// 执行Fail Export Task。
    /// </summary>
    /// <param name="id">业务对象标识。</param>
    /// <param name="request">请求参数。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    Task<OperationResult<ExportFileTaskDto>> FailExportTaskAsync(Guid id, FailFileTaskRequest request, CancellationToken cancellationToken);
    /// <summary>
    /// 执行Upsert Print Template。
    /// </summary>
    /// <param name="request">请求参数。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    Task<OperationResult<PrintTemplateDto>> UpsertPrintTemplateAsync(UpsertPrintTemplateRequest request, CancellationToken cancellationToken);
    /// <summary>
    /// 创建Print Job。
    /// </summary>
    /// <param name="request">请求参数。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    Task<OperationResult<PrintJobDto>> CreatePrintJobAsync(CreatePrintJobRequest request, CancellationToken cancellationToken);
    /// <summary>
    /// 执行Complete Print Job。
    /// </summary>
    /// <param name="id">业务对象标识。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    Task<OperationResult<PrintJobDto>> CompletePrintJobAsync(Guid id, CancellationToken cancellationToken);
    /// <summary>
    /// 执行Fail Print Job。
    /// </summary>
    /// <param name="id">业务对象标识。</param>
    /// <param name="request">请求参数。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    Task<OperationResult<PrintJobDto>> FailPrintJobAsync(Guid id, FailFileTaskRequest request, CancellationToken cancellationToken);
}
