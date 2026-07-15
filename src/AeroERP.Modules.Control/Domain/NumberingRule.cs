using AeroERP.BuildingBlocks.Domain;

namespace AeroERP.Modules.Control.Domain;

/// <summary>
/// Numbering Rule 业务对象。
/// </summary>
public sealed class NumberingRule : Entity, IAggregateRoot
{
    /// <summary>
    /// 初始化Numbering Rule实例。
    /// </summary>
    private NumberingRule()
    {
    }

    /// <summary>
    /// 初始化Numbering Rule实例。
    /// </summary>
    /// <param name="documentType">业务单据类型。</param>
    /// <param name="prefix">编号前缀。</param>
    /// <param name="useDateSegment">use Date Segment 参数。</param>
    /// <param name="padding">流水号补零位数。</param>
    /// <param name="isEnabled">是否启用。</param>
    /// <param name="nextSequence">next Sequence 参数。</param>
    public NumberingRule(string documentType, string prefix, bool useDateSegment, int padding, bool isEnabled, int nextSequence = 1)
    {
        DocumentType = documentType;
        Prefix = prefix;
        UseDateSegment = useDateSegment;
        Padding = padding;
        IsEnabled = isEnabled;
        NextSequence = nextSequence;
    }

    /// <summary>
    /// 业务单据类型。
    /// </summary>
    public string DocumentType { get; private set; } = string.Empty;
    /// <summary>
    /// Prefix。
    /// </summary>
    public string Prefix { get; private set; } = string.Empty;
    /// <summary>
    /// Use Date Segment。
    /// </summary>
    public bool UseDateSegment { get; private set; } = true;
    /// <summary>
    /// Next Sequence。
    /// </summary>
    public int NextSequence { get; private set; } = 1;
    /// <summary>
    /// Padding。
    /// </summary>
    public int Padding { get; private set; } = 4;
    /// <summary>
    /// 是否启用。
    /// </summary>
    public bool IsEnabled { get; private set; } = true;

    /// <summary>
    /// 更新当前业务对象。
    /// </summary>
    /// <param name="prefix">编号前缀。</param>
    /// <param name="useDateSegment">use Date Segment 参数。</param>
    /// <param name="padding">流水号补零位数。</param>
    /// <param name="isEnabled">是否启用。</param>
    public void Update(string prefix, bool useDateSegment, int padding, bool isEnabled)
    {
        Prefix = prefix;
        UseDateSegment = useDateSegment;
        Padding = padding;
        IsEnabled = isEnabled;
        Touch();
    }

    /// <summary>
    /// Generate。
    /// </summary>
    public string Generate()
    {
        var sequence = NextSequence;
        NextSequence += 1;
        Touch();

        var dateSegment = UseDateSegment ? DateTime.UtcNow.ToString("yyyyMMdd") : string.Empty;
        var sequenceSegment = sequence.ToString().PadLeft(Padding, '0');
        return $"{Prefix}{dateSegment}{sequenceSegment}";
    }
}
