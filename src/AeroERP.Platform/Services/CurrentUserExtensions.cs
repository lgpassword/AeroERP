namespace AeroERP.Platform.Services;

/// <summary>
/// Current User Extensions 业务对象。
/// </summary>
public static class CurrentUserExtensions
{
    /// <summary>
    /// 获取Actor。
    /// </summary>
    /// <param name="currentUser">current User 参数。</param>
    public static string GetActor(this ICurrentUserAccessor currentUser)
    {
        return string.IsNullOrWhiteSpace(currentUser.DisplayName)
            ? currentUser.UserName
            : currentUser.DisplayName;
    }
}
