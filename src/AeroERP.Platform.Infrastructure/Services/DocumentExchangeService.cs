using AeroERP.BuildingBlocks.Results;
using AeroERP.Modules.DocumentExchange.Contracts;
using AeroERP.Modules.DocumentExchange.Domain;
using AeroERP.Modules.DocumentExchange.Services;
using AeroERP.Platform.Infrastructure.Persistence;
using AeroERP.Platform.Services;
using Microsoft.EntityFrameworkCore;

namespace AeroERP.Platform.Infrastructure.Services;

/// <summary>
/// Document Exchange Service 业务服务实现，承载模块核心应用逻辑。
/// </summary>
public sealed class DocumentExchangeService(
    AeroErpDbContext dbContext,
    IAuditWriter auditWriter,
    ICurrentUserAccessor currentUser) : IDocumentExchangeService
{
    /// <summary>
    /// 获取Overview。
    /// </summary>
    /// <param name="cancellationToken">请求取消令牌。</param>
    public async Task<DocumentExchangeOverviewDto> GetOverviewAsync(CancellationToken cancellationToken)
    {
        var importTemplates = await dbContext.ImportTemplates.AsNoTracking().OrderBy(x => x.TemplateKey).ToListAsync(cancellationToken);
        var mappings = await dbContext.ImportFieldMappings.AsNoTracking().OrderBy(x => x.TemplateKey).ThenBy(x => x.TargetField).ToListAsync(cancellationToken);
        var importBatches = (await dbContext.ImportBatches.AsNoTracking().ToListAsync(cancellationToken))
            .OrderBy(x => x.Status)
            .ThenByDescending(x => x.UpdatedAtUtc)
            .Take(80)
            .ToList();
        var exportTasks = (await dbContext.ExportFileTasks.AsNoTracking().ToListAsync(cancellationToken))
            .OrderBy(x => x.Status)
            .ThenByDescending(x => x.UpdatedAtUtc)
            .Take(80)
            .ToList();
        var printTemplates = await dbContext.PrintTemplates.AsNoTracking().OrderBy(x => x.TemplateKey).ToListAsync(cancellationToken);
        var printJobs = (await dbContext.PrintJobs.AsNoTracking().ToListAsync(cancellationToken))
            .OrderBy(x => x.Status)
            .ThenByDescending(x => x.UpdatedAtUtc)
            .Take(80)
            .ToList();
        var auditRecords = (await dbContext.FileAuditRecords.AsNoTracking().ToListAsync(cancellationToken))
            .OrderByDescending(x => x.CreatedAtUtc)
            .Take(80)
            .ToList();

        var metrics = new List<DocumentExchangeMetricDto>
        {
            new("import-templates", "导入模板", importTemplates.Count(x => x.IsEnabled), "个"),
            new("print-templates", "打印模板", printTemplates.Count(x => x.IsEnabled), "个"),
            new("open-imports", "未完成导入", importBatches.Count(x => x.Status != DocumentExchangeStatus.Completed), "批"),
            new("open-files", "未完成文件任务", exportTasks.Count(x => x.Status != DocumentExchangeStatus.Completed) + printJobs.Count(x => x.Status != DocumentExchangeStatus.Completed), "个")
        };

        return new DocumentExchangeOverviewDto(
            importTemplates.Select(MapImportTemplate).ToList(),
            mappings.Select(MapFieldMapping).ToList(),
            importBatches.Select(MapImportBatch).ToList(),
            exportTasks.Select(MapExportTask).ToList(),
            printTemplates.Select(MapPrintTemplate).ToList(),
            printJobs.Select(MapPrintJob).ToList(),
            auditRecords.Select(MapAudit).ToList(),
            metrics);
    }

    /// <summary>
    /// Upsert Import Template Async。
    /// </summary>
    /// <param name="request">请求参数。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    public async Task<OperationResult<ImportTemplateDto>> UpsertImportTemplateAsync(UpsertImportTemplateRequest request, CancellationToken cancellationToken)
    {
        var key = NormalizeKey(request.TemplateKey);
        var targetModule = NormalizeKey(request.TargetModule);
        var displayName = NormalizeText(request.DisplayName);
        var fileType = NormalizeText(request.FileType).ToUpperInvariant();
        if (string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(displayName) || string.IsNullOrWhiteSpace(targetModule) || string.IsNullOrWhiteSpace(fileType))
        {
            return OperationResult<ImportTemplateDto>.Failure("导入模板编码、名称、目标模块和文件类型不能为空。");
        }

        if (!currentUser.CanAccessModule(targetModule))
        {
            return OperationResult<ImportTemplateDto>.Failure("当前账号不能访问该目标模块。");
        }

        var actor = currentUser.GetActor();
        var template = await dbContext.ImportTemplates.FirstOrDefaultAsync(x => x.TemplateKey == key, cancellationToken);
        if (template is null)
        {
            template = new ImportTemplate(key, displayName, targetModule, fileType, request.IsEnabled, actor);
            dbContext.ImportTemplates.Add(template);
        }
        else
        {
            template.Update(displayName, targetModule, fileType, request.IsEnabled, actor);
        }

        await SaveFileAuditAsync("ImportTemplate", "ImportTemplateUpserted", key, "Success", "导入模板已保存。", actor, cancellationToken);
        return OperationResult<ImportTemplateDto>.Success(MapImportTemplate(template));
    }

    /// <summary>
    /// Upsert Field Mapping Async。
    /// </summary>
    /// <param name="request">请求参数。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    public async Task<OperationResult<ImportFieldMappingDto>> UpsertFieldMappingAsync(UpsertImportFieldMappingRequest request, CancellationToken cancellationToken)
    {
        var templateKey = NormalizeKey(request.TemplateKey);
        var sourceField = NormalizeText(request.SourceField);
        var targetField = NormalizeText(request.TargetField);
        if (string.IsNullOrWhiteSpace(templateKey) || string.IsNullOrWhiteSpace(sourceField) || string.IsNullOrWhiteSpace(targetField))
        {
            return OperationResult<ImportFieldMappingDto>.Failure("模板编码、来源字段和目标字段不能为空。");
        }

        var template = await dbContext.ImportTemplates.AsNoTracking().FirstOrDefaultAsync(x => x.TemplateKey == templateKey && x.IsEnabled, cancellationToken);
        if (template is null)
        {
            return OperationResult<ImportFieldMappingDto>.Failure("导入模板不存在或已停用。");
        }

        var actor = currentUser.GetActor();
        var mapping = await dbContext.ImportFieldMappings.FirstOrDefaultAsync(x => x.TemplateKey == templateKey && x.TargetField == targetField, cancellationToken);
        if (mapping is null)
        {
            mapping = new ImportFieldMapping(templateKey, sourceField, targetField, request.IsRequired, NormalizeText(request.TransformRule), actor);
            dbContext.ImportFieldMappings.Add(mapping);
        }
        else
        {
            mapping.Update(sourceField, targetField, request.IsRequired, NormalizeText(request.TransformRule), actor);
        }

        await SaveFileAuditAsync("FieldMapping", "FieldMappingUpserted", $"{templateKey}:{targetField}", "Success", "字段映射已保存。", actor, cancellationToken);
        return OperationResult<ImportFieldMappingDto>.Success(MapFieldMapping(mapping));
    }

    /// <summary>
    /// 创建Import Batch。
    /// </summary>
    /// <param name="request">请求参数。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    public async Task<OperationResult<ImportBatchDto>> CreateImportBatchAsync(CreateImportBatchRequest request, CancellationToken cancellationToken)
    {
        var templateKey = NormalizeKey(request.TemplateKey);
        var fileName = NormalizeText(request.FileName);
        if (string.IsNullOrWhiteSpace(templateKey) || string.IsNullOrWhiteSpace(fileName))
        {
            return OperationResult<ImportBatchDto>.Failure("导入模板和文件名不能为空。");
        }

        var template = await dbContext.ImportTemplates.AsNoTracking().FirstOrDefaultAsync(x => x.TemplateKey == templateKey && x.IsEnabled, cancellationToken);
        if (template is null)
        {
            return OperationResult<ImportBatchDto>.Failure("导入模板不存在或已停用。");
        }

        var actor = currentUser.GetActor();
        var batch = new ImportBatch(NextNo("IB"), templateKey, fileName, actor);
        dbContext.ImportBatches.Add(batch);
        await SaveFileAuditAsync("ImportBatch", "ImportBatchCreated", batch.BatchNo, "Success", "导入批次已创建。", actor, cancellationToken);
        return OperationResult<ImportBatchDto>.Success(MapImportBatch(batch));
    }

    /// <summary>
    /// Complete Import Batch Async。
    /// </summary>
    /// <param name="id">业务对象标识。</param>
    /// <param name="request">请求参数。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    public async Task<OperationResult<ImportBatchDto>> CompleteImportBatchAsync(Guid id, CompleteImportBatchRequest request, CancellationToken cancellationToken)
    {
        if (request.RowCount < 0 || request.ErrorCount < 0)
        {
            return OperationResult<ImportBatchDto>.Failure("行数和错误数不能小于 0。");
        }

        var batch = await dbContext.ImportBatches.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (batch is null)
        {
            return OperationResult<ImportBatchDto>.Failure("导入批次不存在。");
        }

        if (batch.Status == DocumentExchangeStatus.Completed)
        {
            return OperationResult<ImportBatchDto>.Failure("导入批次已完成。");
        }

        var actor = currentUser.GetActor();
        batch.Complete(request.RowCount, request.ErrorCount, actor);
        await SaveFileAuditAsync("ImportBatch", "ImportBatchCompleted", batch.BatchNo, "Success", "导入批次已完成。", actor, cancellationToken);
        return OperationResult<ImportBatchDto>.Success(MapImportBatch(batch));
    }

    /// <summary>
    /// Fail Import Batch Async。
    /// </summary>
    /// <param name="id">业务对象标识。</param>
    /// <param name="request">请求参数。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    public async Task<OperationResult<ImportBatchDto>> FailImportBatchAsync(Guid id, FailFileTaskRequest request, CancellationToken cancellationToken)
    {
        var batch = await dbContext.ImportBatches.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (batch is null)
        {
            return OperationResult<ImportBatchDto>.Failure("导入批次不存在。");
        }

        var error = NormalizeText(request.Error);
        if (string.IsNullOrWhiteSpace(error))
        {
            return OperationResult<ImportBatchDto>.Failure("失败原因不能为空。");
        }

        var actor = currentUser.GetActor();
        batch.Fail(error);
        await SaveFileAuditAsync("ImportBatch", "ImportBatchFailed", batch.BatchNo, "Failed", error, actor, cancellationToken);
        return OperationResult<ImportBatchDto>.Success(MapImportBatch(batch));
    }

    /// <summary>
    /// 创建Export Task。
    /// </summary>
    /// <param name="request">请求参数。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    public async Task<OperationResult<ExportFileTaskDto>> CreateExportTaskAsync(CreateExportFileTaskRequest request, CancellationToken cancellationToken)
    {
        var sourceModule = NormalizeKey(request.SourceModule);
        var fileName = NormalizeText(request.FileName);
        var format = NormalizeText(request.Format).ToUpperInvariant();
        if (string.IsNullOrWhiteSpace(sourceModule) || string.IsNullOrWhiteSpace(fileName) || string.IsNullOrWhiteSpace(format))
        {
            return OperationResult<ExportFileTaskDto>.Failure("来源模块、文件名和格式不能为空。");
        }

        if (!currentUser.CanAccessModule(sourceModule))
        {
            return OperationResult<ExportFileTaskDto>.Failure("当前账号不能访问该来源模块。");
        }

        var actor = currentUser.GetActor();
        var task = new ExportFileTask(NextNo("EF"), sourceModule, fileName, format, actor);
        dbContext.ExportFileTasks.Add(task);
        await SaveFileAuditAsync("ExportFile", "ExportTaskCreated", task.ExportNo, "Success", "导出文件任务已创建。", actor, cancellationToken);
        return OperationResult<ExportFileTaskDto>.Success(MapExportTask(task));
    }

    /// <summary>
    /// Complete Export Task Async。
    /// </summary>
    /// <param name="id">业务对象标识。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    public async Task<OperationResult<ExportFileTaskDto>> CompleteExportTaskAsync(Guid id, CancellationToken cancellationToken)
    {
        var task = await dbContext.ExportFileTasks.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (task is null)
        {
            return OperationResult<ExportFileTaskDto>.Failure("导出文件任务不存在。");
        }

        if (task.Status == DocumentExchangeStatus.Completed)
        {
            return OperationResult<ExportFileTaskDto>.Failure("导出文件任务已完成。");
        }

        var actor = currentUser.GetActor();
        task.Complete(actor);
        await SaveFileAuditAsync("ExportFile", "ExportTaskCompleted", task.ExportNo, "Success", "导出文件任务已完成。", actor, cancellationToken);
        return OperationResult<ExportFileTaskDto>.Success(MapExportTask(task));
    }

    /// <summary>
    /// Fail Export Task Async。
    /// </summary>
    /// <param name="id">业务对象标识。</param>
    /// <param name="request">请求参数。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    public async Task<OperationResult<ExportFileTaskDto>> FailExportTaskAsync(Guid id, FailFileTaskRequest request, CancellationToken cancellationToken)
    {
        var task = await dbContext.ExportFileTasks.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (task is null)
        {
            return OperationResult<ExportFileTaskDto>.Failure("导出文件任务不存在。");
        }

        var error = NormalizeText(request.Error);
        if (string.IsNullOrWhiteSpace(error))
        {
            return OperationResult<ExportFileTaskDto>.Failure("失败原因不能为空。");
        }

        var actor = currentUser.GetActor();
        task.Fail();
        await SaveFileAuditAsync("ExportFile", "ExportTaskFailed", task.ExportNo, "Failed", error, actor, cancellationToken);
        return OperationResult<ExportFileTaskDto>.Success(MapExportTask(task));
    }

    /// <summary>
    /// Upsert Print Template Async。
    /// </summary>
    /// <param name="request">请求参数。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    public async Task<OperationResult<PrintTemplateDto>> UpsertPrintTemplateAsync(UpsertPrintTemplateRequest request, CancellationToken cancellationToken)
    {
        var key = NormalizeKey(request.TemplateKey);
        var displayName = NormalizeText(request.DisplayName);
        var targetModule = NormalizeKey(request.TargetModule);
        var contentType = NormalizeText(request.ContentType);
        if (string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(displayName) || string.IsNullOrWhiteSpace(targetModule) || string.IsNullOrWhiteSpace(contentType))
        {
            return OperationResult<PrintTemplateDto>.Failure("打印模板编码、名称、目标模块和内容类型不能为空。");
        }

        if (!currentUser.CanAccessModule(targetModule))
        {
            return OperationResult<PrintTemplateDto>.Failure("当前账号不能访问该目标模块。");
        }

        var actor = currentUser.GetActor();
        var template = await dbContext.PrintTemplates.FirstOrDefaultAsync(x => x.TemplateKey == key, cancellationToken);
        if (template is null)
        {
            template = new PrintTemplate(key, displayName, targetModule, contentType, NormalizeText(request.TemplateBody), request.IsEnabled, actor);
            dbContext.PrintTemplates.Add(template);
        }
        else
        {
            template.Update(displayName, targetModule, contentType, NormalizeText(request.TemplateBody), request.IsEnabled, actor);
        }

        await SaveFileAuditAsync("PrintTemplate", "PrintTemplateUpserted", key, "Success", "打印模板已保存。", actor, cancellationToken);
        return OperationResult<PrintTemplateDto>.Success(MapPrintTemplate(template));
    }

    /// <summary>
    /// 创建Print Job。
    /// </summary>
    /// <param name="request">请求参数。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    public async Task<OperationResult<PrintJobDto>> CreatePrintJobAsync(CreatePrintJobRequest request, CancellationToken cancellationToken)
    {
        var templateKey = NormalizeKey(request.TemplateKey);
        var documentNo = NormalizeText(request.DocumentNo);
        if (string.IsNullOrWhiteSpace(templateKey) || string.IsNullOrWhiteSpace(documentNo))
        {
            return OperationResult<PrintJobDto>.Failure("打印模板和单据号不能为空。");
        }

        var template = await dbContext.PrintTemplates.AsNoTracking().FirstOrDefaultAsync(x => x.TemplateKey == templateKey && x.IsEnabled, cancellationToken);
        if (template is null)
        {
            return OperationResult<PrintJobDto>.Failure("打印模板不存在或已停用。");
        }

        var actor = currentUser.GetActor();
        var job = new PrintJob(NextNo("PJ"), templateKey, documentNo, actor);
        dbContext.PrintJobs.Add(job);
        await SaveFileAuditAsync("PrintJob", "PrintJobCreated", job.JobNo, "Success", "打印任务已创建。", actor, cancellationToken);
        return OperationResult<PrintJobDto>.Success(MapPrintJob(job));
    }

    /// <summary>
    /// Complete Print Job Async。
    /// </summary>
    /// <param name="id">业务对象标识。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    public async Task<OperationResult<PrintJobDto>> CompletePrintJobAsync(Guid id, CancellationToken cancellationToken)
    {
        var job = await dbContext.PrintJobs.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (job is null)
        {
            return OperationResult<PrintJobDto>.Failure("打印任务不存在。");
        }

        if (job.Status == DocumentExchangeStatus.Completed)
        {
            return OperationResult<PrintJobDto>.Failure("打印任务已完成。");
        }

        var actor = currentUser.GetActor();
        job.Complete(actor);
        await SaveFileAuditAsync("PrintJob", "PrintJobCompleted", job.JobNo, "Success", "打印任务已完成。", actor, cancellationToken);
        return OperationResult<PrintJobDto>.Success(MapPrintJob(job));
    }

    /// <summary>
    /// Fail Print Job Async。
    /// </summary>
    /// <param name="id">业务对象标识。</param>
    /// <param name="request">请求参数。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    public async Task<OperationResult<PrintJobDto>> FailPrintJobAsync(Guid id, FailFileTaskRequest request, CancellationToken cancellationToken)
    {
        var job = await dbContext.PrintJobs.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (job is null)
        {
            return OperationResult<PrintJobDto>.Failure("打印任务不存在。");
        }

        var error = NormalizeText(request.Error);
        if (string.IsNullOrWhiteSpace(error))
        {
            return OperationResult<PrintJobDto>.Failure("失败原因不能为空。");
        }

        var actor = currentUser.GetActor();
        job.Fail();
        await SaveFileAuditAsync("PrintJob", "PrintJobFailed", job.JobNo, "Failed", error, actor, cancellationToken);
        return OperationResult<PrintJobDto>.Success(MapPrintJob(job));
    }

    /// <summary>
    /// Save File Audit Async。
    /// </summary>
    /// <param name="category">业务分类。</param>
    /// <param name="action">业务动作。</param>
    /// <param name="targetNo">target No 参数。</param>
    /// <param name="result">执行结果。</param>
    /// <param name="message">执行消息。</param>
    /// <param name="actor">操作人。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    private async Task SaveFileAuditAsync(string category, string action, string targetNo, string result, string message, string actor, CancellationToken cancellationToken)
    {
        dbContext.FileAuditRecords.Add(new FileAuditRecord(NextNo("FA"), category, action, targetNo, result, message, actor));
        await dbContext.SaveChangesAsync(cancellationToken);
        await auditWriter.WriteAsync("DocumentExchange", action, actor, targetNo, cancellationToken);
    }

    /// <summary>
    /// Next No。
    /// </summary>
    /// <param name="prefix">编号前缀。</param>
    private static string NextNo(string prefix) => $"{prefix}-{DateTime.UtcNow:yyyyMMddHHmmssfff}";

    /// <summary>
    /// Normalize Text。
    /// </summary>
    /// <param name="value">数值或配置值。</param>
    private static string NormalizeText(string value) => value?.Trim() ?? string.Empty;

    /// <summary>
    /// Normalize Key。
    /// </summary>
    /// <param name="value">数值或配置值。</param>
    private static string NormalizeKey(string value) => NormalizeText(value).ToLowerInvariant();

    /// <summary>
    /// 注册Import Template 路由。
    /// </summary>
    /// <param name="template">模板对象。</param>
    private static ImportTemplateDto MapImportTemplate(ImportTemplate template) =>
        new(template.Id, template.TemplateKey, template.DisplayName, template.TargetModule, template.FileType, template.IsEnabled, template.UpdatedBy, template.UpdatedAtUtc);

    /// <summary>
    /// 注册Field Mapping 路由。
    /// </summary>
    /// <param name="mapping">字段映射。</param>
    private static ImportFieldMappingDto MapFieldMapping(ImportFieldMapping mapping) =>
        new(mapping.Id, mapping.TemplateKey, mapping.SourceField, mapping.TargetField, mapping.IsRequired, mapping.TransformRule, mapping.UpdatedBy, mapping.UpdatedAtUtc);

    /// <summary>
    /// 注册Import Batch 路由。
    /// </summary>
    /// <param name="batch">导入批次。</param>
    private static ImportBatchDto MapImportBatch(ImportBatch batch) =>
        new(batch.Id, batch.BatchNo, batch.TemplateKey, batch.FileName, batch.Status, batch.RowCount, batch.ErrorCount, batch.ErrorMessage, batch.CreatedBy, batch.CompletedBy, batch.CompletedAtUtc, batch.UpdatedAtUtc);

    /// <summary>
    /// 注册Export Task 路由。
    /// </summary>
    /// <param name="task">任务对象。</param>
    private static ExportFileTaskDto MapExportTask(ExportFileTask task) =>
        new(task.Id, task.ExportNo, task.SourceModule, task.FileName, task.Format, task.Status, task.RequestedBy, task.CompletedBy, task.CompletedAtUtc, task.UpdatedAtUtc);

    /// <summary>
    /// 注册Print Template 路由。
    /// </summary>
    /// <param name="template">模板对象。</param>
    private static PrintTemplateDto MapPrintTemplate(PrintTemplate template) =>
        new(template.Id, template.TemplateKey, template.DisplayName, template.TargetModule, template.ContentType, template.TemplateBody, template.IsEnabled, template.UpdatedBy, template.UpdatedAtUtc);

    /// <summary>
    /// 注册Print Job 路由。
    /// </summary>
    /// <param name="job">任务对象。</param>
    private static PrintJobDto MapPrintJob(PrintJob job) =>
        new(job.Id, job.JobNo, job.TemplateKey, job.DocumentNo, job.Status, job.RequestedBy, job.CompletedBy, job.CompletedAtUtc, job.UpdatedAtUtc);

    /// <summary>
    /// 注册Audit 路由。
    /// </summary>
    /// <param name="audit">审计记录。</param>
    private static FileAuditRecordDto MapAudit(FileAuditRecord audit) =>
        new(audit.Id, audit.AuditNo, audit.Category, audit.Action, audit.TargetNo, audit.Result, audit.Message, audit.Actor, audit.CreatedAtUtc);
}
