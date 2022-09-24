using Database;
using Database.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace CatalogManager.Segment
{
    public class ShakaPackagerStrategy : ISegmenterStrategy
    {
        public void Segment(List<VideoSegmentQueue> videosToSegment)
        {
            var firstVideo = videosToSegment.First();
            var inputFolder = Path.GetDirectoryName(firstVideo.InputDirectory);
            var exe = @"c:\shaka-packager\packager.exe";

            var audioInit = Path.Combine(inputFolder, @"audio\init.mp4");
            var audioItems = Path.Combine(inputFolder, @"audio\$Number$.m4s");
            var args = $@"'in=""{firstVideo.InputDirectory}"",stream=audio,init_segment=""{audioInit}"",segment_template=""{audioItems}""' ";

            foreach (var video in videosToSegment)
            {
                var videoInit = Path.Combine(inputFolder, @$"{video.Height}p\init.mp4");
                var videoItems = Path.Combine(inputFolder, @$"{video.Height}p\$Number$.m4s");
                string fileToSegment = @$"'in=""{video.InputDirectory}"",stream=video,init_segment=""{videoInit}"",segment_template=""{videoItems}""' ";
                args += fileToSegment;
            }
            var dashFile = Path.Combine(inputFolder, "dash.mpd");
            var hlsFile = Path.Combine(inputFolder, "hls.m3u8");
            args += @$"--generate_static_live_mpd --mpd_output ""{dashFile}"" --hls_master_playlist_output ""{hlsFile}"" ";

            Process proc = new Process();
            proc.StartInfo.FileName = exe;
            proc.StartInfo.Arguments = args;
            proc.StartInfo.CreateNoWindow = false; // Set to true if we want to hide output.
            proc.StartInfo.UseShellExecute = false;
            if (!proc.Start())
            {
                throw new Exception("Error starting the HandbrakeCLI process.");
            }
            proc.WaitForExit();
            proc.Close();

            if (!File.Exists(dashFile) || !File.Exists(hlsFile))
                throw new FileNotFoundException($"The manifest files could not be created in {inputFolder}");
        }
    }
}
