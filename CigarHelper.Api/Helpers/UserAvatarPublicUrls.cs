namespace CigarHelper.Api.Helpers;

/// <summary>
/// В БД в <c>User.AvatarUrl</c> хранится либо внешний URL, либо ключ объекта в хранилище после загрузки через API.
/// </summary>
public static class UserAvatarPublicUrls
{
    public static string? ToPublicUrl(int userId, string? avatarUrl)
    {
        if (string.IsNullOrWhiteSpace(avatarUrl))
            return null;

        var v = avatarUrl.Trim();
        if (v.Length >= 7
            && (v.StartsWith("http://", StringComparison.OrdinalIgnoreCase)
                || v.StartsWith("https://", StringComparison.OrdinalIgnoreCase)))
        {
            return v;
        }

        return $"/api/users/{userId}/avatar";
    }

    public static bool IsExternalHttpUrl(string? avatarUrl)
    {
        if (string.IsNullOrWhiteSpace(avatarUrl))
            return false;

        var v = avatarUrl.Trim();
        return v.Length >= 7
               && (v.StartsWith("http://", StringComparison.OrdinalIgnoreCase)
                   || v.StartsWith("https://", StringComparison.OrdinalIgnoreCase));
    }
}
