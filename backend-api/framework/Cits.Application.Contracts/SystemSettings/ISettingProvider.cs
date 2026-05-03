namespace Cits.SystemSettings;

public interface ISettingProvider
{
    Task<string?> GetStringAsync(string key, string? defaultValue = null);

    Task<bool> GetBoolAsync(string key, bool defaultValue = false);

    Task<int> GetIntAsync(string key, int defaultValue = 0);

    Task<decimal> GetDecimalAsync(string key, decimal defaultValue = 0);
}
