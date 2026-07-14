using AeroERP.BuildingBlocks.Domain;

namespace AeroERP.Modules.MobileWork.Domain;

/// <summary>
/// Mobile Scan Event 业务对象。
/// </summary>
public sealed class MobileScanEvent : Entity, IAggregateRoot
{
    /// <summary>
    /// 初始化Mobile Scan Event实例。
    /// </summary>
    private MobileScanEvent()
    {
    }

    /// <summary>
    /// 初始化Mobile Scan Event实例。
    /// </summary>
    /// <param name="scanNo">scan No 参数。</param>
    /// <param name="deviceCode">device Code 参数。</param>
    /// <param name="barcode">条码内容。</param>
    /// <param name="targetModule">target Module 参数。</param>
    /// <param name="action">业务动作。</param>
    /// <param name="documentNo">业务单据编号。</param>
    /// <param name="result">执行结果。</param>
    /// <param name="message">执行消息。</param>
    /// <param name="actor">操作人。</param>
    public MobileScanEvent(string scanNo, string deviceCode, string barcode, string targetModule, string action, string documentNo, string result, string message, string actor)
    {
        ScanNo = scanNo;
        DeviceCode = deviceCode;
        Barcode = barcode;
        TargetModule = targetModule;
        Action = action;
        DocumentNo = documentNo;
        Result = result;
        Message = message;
        Actor = actor;
    }

    /// <summary>
    /// Scan No。
    /// </summary>
    public string ScanNo { get; private set; } = string.Empty;
    /// <summary>
    /// Device Code。
    /// </summary>
    public string DeviceCode { get; private set; } = string.Empty;
    /// <summary>
    /// Barcode。
    /// </summary>
    public string Barcode { get; private set; } = string.Empty;
    /// <summary>
    /// Target Module。
    /// </summary>
    public string TargetModule { get; private set; } = string.Empty;
    /// <summary>
    /// Action。
    /// </summary>
    public string Action { get; private set; } = string.Empty;
    /// <summary>
    /// 业务单据编号。
    /// </summary>
    public string DocumentNo { get; private set; } = string.Empty;
    /// <summary>
    /// 执行结果。
    /// </summary>
    public string Result { get; private set; } = string.Empty;
    /// <summary>
    /// 执行消息。
    /// </summary>
    public string Message { get; private set; } = string.Empty;
    /// <summary>
    /// 操作人。
    /// </summary>
    public string Actor { get; private set; } = string.Empty;
}
