using Database;

namespace CatalogManager.Segment
{
    public class ShakaPackagerStrategy : ISegmenterStrategy
    {
        private readonly IVideoRepository _repository;

        public ShakaPackagerStrategy(IVideoRepository repository)
        {
            _repository = repository;
        }
        public object Segment()
        {
            var command = @"
./packager `
  'in=""Z:\\inetpub\\wwwcdn\\520.mp4"",stream=audio,init_segment=audio/init.mp4,segment_template=audio/$Number$.m4s' `
  'in=""Z:\\inetpub\\wwwcdn\\360.mp4"",stream=video,init_segment=h264_360p/init.mp4,segment_template=h264_360p/$Number$.m4s' `
  'in=""Z:\\inetpub\\wwwcdn\\520.mp4"",stream=video,init_segment=h264_520p/init.mp4,segment_template=h264_520p/$Number$.m4s' `
  --generate_static_live_mpd--mpd_output h264.mpd `
  --hls_master_playlist_output h264_master.m3u8
";
            var inputFileName = "c:\\video\\example.mp4";
            var fileResolution = "720p";
            string fileToSegment = @$"'in=""{inputFileName}"",stream=video,init_segment=h264_{fileResolution}/init.mp4,segment_template=h264_{fileResolution}/$Number$.m4s' ";

            var audioToSegment = $@"'in=""{inputFileName}"",stream=audio,init_segment=audio/init.mp4,segment_template=audio/$Number$.m4s' ";
            return null;
        }
    }
}
