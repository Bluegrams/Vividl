
namespace Vividl.Model
{
    /// <summary>
    /// Specifies the possible outcomes of a download process.
    /// </summary>
    public enum DownloadResult
    {
        Success, Cancelled, Failed
    }

    /// <summary>
    /// Specifies the possible states of a download item
    /// according to which its appearance in the list changes.
    /// </summary>
    public enum ItemState
    {
        None, Fetched, Downloading, Succeeded
    }

    /// <summary>
    /// Specifies available application themes.
    /// </summary>
    public enum Theme
    {
        Light, Dark
    }
}
