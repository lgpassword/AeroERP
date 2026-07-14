using AeroERP.BuildingBlocks.Domain;

namespace AeroERP.Modules.DocumentExchange.Domain;

/// <summary>
/// Import Batch 业务对象。
/// </summary>
public sealed class ImportBatch : Entity, IAggregateRoot
{
    /// <summary>
    /// 初始化Import Batch实例。
    /// </summary>
    private ImportBatch()
    {
    }

    /// <summary>
    /// 初始化Import Batch实例。
    /// </summary>
    /// <param name="batchNo">batch No 参数。</param>
    /// <param name="templateKey">template Key 参数。</param>
    /// <param name="fileName">file Name 参数。</param>
    /// <param name="createdBy">创建人。</param>
    public ImportBatch(string batchNo, string templateKey, string fileName, string createdBy)
    {
        BatchNo = batchNo;
        TemplateKey = templateKey;
        FileName = fileName;
        CreatedBy = createdBy;
    }

    /// <summary>
    /// Batch No。
    /// </summary>
    public string BatchNo { get; private set; } = string.Empty;
    /// <summary>
    /// Template Key。
    /// </summary>
    public string TemplateKey { get; private set; } = string.Empty;
    /// <summary>
    /// File Name。
    /// </summary>
    public string FileName { get; private set; } = string.Empty;
    /// <summary>
    /// 当前业务状态。
    /// </summary>
    public string Status { get; private set; } = DocumentExchangeStatus.Pending;
    /// <summary>
    /// Row Count。
    /// </summary>
    public int RowCount { get; private set; }
    /// <summary>
    /// Error Count。
    /// </summary>
    public int ErrorCount { get; private set; }
    /// <summary>
    /// Error Message。
    /// </summary>
    public string ErrorMessage { get; private set; } = string.Empty;
    /// <summary>
    /// 创建人。
    /// </summary>
    public string CreatedBy { get; private set; } = string.Empty;
    /// <summary>
    /// Completed By。
    /// </summary>
    public string CompletedBy { get; private set; } = string.Empty;
    /// <summary>
    /// Completed At Utc。
    /// </summary>
    public DateTimeOffset? CompletedAtUtc { get; private set; }

    /// <summary>
    /// Complete。
    /// </summary>
    /// <param name="rowCount">row Count 参数。</param>
    /// <param name="errorCount">error Count 参数。</param>
    /// <param name="actor">操作人。</param>
    public void Complete(int rowCount, int errorCount, string actor)
    {
        RowCount = rowCount;
        ErrorCount = errorCount;
        Status = DocumentExchangeStatus.Completed;
        CompletedBy = actor;
        CompletedAtUtc = DateTimeOffset.UtcNow;
        Touch();
    }

    /// <summary>
    /// Fail。
    /// </summary>
    /// <param name="errorMessage">error Message 参数。</param>
    public void Fail(string errorMessage)
    {
        Status = DocumentExchangeStatus.Failed;
        ErrorMessage = errorMessage;
        Touch();
    }
}
