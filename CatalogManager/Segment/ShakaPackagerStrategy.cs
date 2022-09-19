using Database;
using Database.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace CatalogManager.Segment
{
    public class ShakaPackagerStrategy : ISegmenterStrategy
    {
        public void Segment(List<VideoSegmentQueue> videosToSegment)
        {
            var firstVideo = videosToSegment.First();
            var exe = @"c:\shaka-packager\packager.exe";
            var args = $@"'in=""{firstVideo.InputDirectory}"",stream=audio,init_segment=audio/init.mp4,segment_template=audio/$Number$.m4s' ";

            foreach (var video in videosToSegment)
            {
                string fileToSegment = @$"'in=""{video.InputDirectory}"",stream=video,init_segment={video.Height}p/init.mp4,segment_template={video.Height}p/$Number$.m4s' ";
                args += fileToSegment;
            }
            args += "--generate_static_live_mpd--mpd_output dash.mpd --hls_master_playlist_output hls.m3u8 ";

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
        }
    }
}
