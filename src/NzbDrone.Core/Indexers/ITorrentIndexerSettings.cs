namespace NzbDrone.Core.Indexers
{
    public interface ITorrentIndexerSettings
    {
        int MinimumSeeders { get; set; }
    }
}
