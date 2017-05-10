using System;
using System.IO;
using System.Linq;
using Nancy;
using Nancy.Responses;
using NLog;
using NzbDrone.Common.Disk;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Tv;

namespace NzbDrone.Api.Frontend.Mappers
{
    public class MediaFileMapper : StaticResourceMapperBase
    {
        private readonly IMediaFileService _mediaFileService;
        private readonly ISeriesService _seriesService;
        private readonly IDiskProvider _diskProvider;
        private readonly StringComparison _caseSensitive;

        public MediaFileMapper(IMediaFileService mediaFileService, ISeriesService seriesService, IDiskProvider diskProvider, Logger logger)
            : base(diskProvider, logger)
        {
            _mediaFileService = mediaFileService;
            _seriesService = seriesService;
            _diskProvider = diskProvider;

            if (!RuntimeInfo.IsProduction)
            {
                _caseSensitive = StringComparison.OrdinalIgnoreCase;
            }

        }

        public override string Map(string resourceUrl)
        {
            var path = resourceUrl.Replace("/download/", "").Replace('/', Path.DirectorySeparatorChar);

            var episodeIdStr = resourceUrl.Split('/').Last();

            int id;

            if (int.TryParse(episodeIdStr, out id))
            {
                var episodeFile = _mediaFileService.Get(id);
                var series = _seriesService.GetSeries(episodeFile.SeriesId);
                return Path.Combine(series.Path, episodeFile.RelativePath);
            }
            return string.Empty;
        }

        public override bool CanHandle(string resourceUrl)
        {
            return resourceUrl.StartsWith("/download/");
        }

        public override Response GetResponse(string resourceUrl)
        {
            var filePath = Map(resourceUrl);

            if (_diskProvider.FileExists(filePath, _caseSensitive))
            {
                return new StreamResponse(() => GetContentStream(filePath), MimeTypes.GetMimeType(filePath)).AsAttachment(Path.GetFileName(filePath));
            }

            return new NotFoundResponse();
        }

    }
}