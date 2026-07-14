using AeroERP.BuildingBlocks.Domain;

namespace AeroERP.Modules.Planning.Domain;

/// <summary>
/// Barcode Execution 业务对象。
/// </summary>
public sealed class BarcodeExecution : Entity
{
    /// <summary>
    /// 初始化Barcode Execution实例。
    /// </summary>
    private BarcodeExecution()
    {
    }

    /// <summary>
    /// 初始化Barcode Execution实例。
    /// </summary>
    /// <param name="executionNo">execution No 参数。</param>
    /// <param name="barcode">条码内容。</param>
    /// <param name="action">业务动作。</param>
    /// <param name="result">执行结果。</param>
    /// <param name="message">执行消息。</param>
    /// <param name="documentType">业务单据类型。</param>
    /// <param name="documentId">业务单据标识。</param>
    /// <param name="documentNo">业务单据编号。</param>
    /// <param name="actor">操作人。</param>
    public BarcodeExecution(
        string executionNo,
        string barcode,
        string action,
        string result,
        string message,
        string documentType,
        Guid? documentId,
        string documentNo,
        string actor)
    {
        ExecutionNo = executionNo;
        Barcode = barcode;
        Action = action;
        Result = result;
        Message = message;
        DocumentType = documentType;
        DocumentId = documentId;
        DocumentNo = documentNo;
        Actor = actor;
    }

    /// <summary>
    /// Execution No。
    /// </summary>
    public string ExecutionNo { get; private set; } = string.Empty;
    /// <summary>
    /// Barcode。
    /// </summary>
    public string Barcode { get; private set; } = string.Empty;
    /// <summary>
    /// Action。
    /// </summary>
    public string Action { get; private set; } = string.Empty;
    /// <summary>
    /// 执行结果。
    /// </summary>
    public string Result { get; private set; } = string.Empty;
    /// <summary>
    /// 执行消息。
    /// </summary>
    public string Message { get; private set; } = string.Empty;
    /// <summary>
    /// 业务单据类型。
    /// </summary>
    public string DocumentType { get; private set; } = string.Empty;
    /// <summary>
    /// Document Id。
    /// </summary>
    public Guid? DocumentId { get; private set; }
    /// <summary>
    /// 业务单据编号。
    /// </summary>
    public string DocumentNo { get; private set; } = string.Empty;
    /// <summary>
    /// 操作人。
    /// </summary>
    public string Actor { get; private set; } = string.Empty;
}
