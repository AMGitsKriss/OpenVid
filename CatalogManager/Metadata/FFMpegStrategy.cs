using CatalogManager.Models;
using System;
using System.Diagnostics;

namespace CatalogManager.Metadata
{
    public class FFMpegStrategy : IMetadataStrategy
    {
        public MediaProperties GetMetadata(string location)
        {
            // TODO - Try catch. The path can be wrong and we don't want to it fall over vaguely.
            string cmd = $"-v error -select_streams v:0 -show_entries stream=width,height,duration -show_entries format=duration -of csv=s=x:p=0 \"{location}\"";
            Process proc = new Process();
            proc.StartInfo.FileName = @"c:\ffmpeg\ffprobe.exe";
            proc.StartInfo.Arguments = cmd;
            proc.StartInfo.CreateNoWindow = true;
            proc.StartInfo.RedirectStandardOutput = true;
            proc.StartInfo.RedirectStandardError = true;
            proc.StartInfo.UseShellExecute = false;
            if (!proc.Start())
            {
                Console.WriteLine("Error starting");
            }
            string outputString = proc.StandardOutput.ReadToEnd();
            string[] metaData = outputString.Trim().Split(new char[] { 'x', '\n' });
            // Remove the milliseconds
            MediaProperties properties = new MediaProperties()
            {
                Width = int.Parse(metaData[0]),
                Height = int.Parse(metaData[1]),
                Duration = TimeSpan.FromSeconds(double.Parse(metaData.Length > 3 ? metaData[3].Trim() : metaData[2].Trim()))
            };
            proc.WaitForExit();
            proc.Close();
            return properties;
        }

        public void CreateThumbnail(string videoPath, string thumbPath)
        {
            var cmd = $" -y -itsoffset -1 -i \"{videoPath}\" -vcodec mjpeg -vframes 60 -filter:v \"scale=300:168:force_original_aspect_ratio=decrease,pad=300:168:-1:-1:color=black\" \"{thumbPath}\"";

            var startInfo = new ProcessStartInfo
            {
                WindowStyle = ProcessWindowStyle.Normal,
                FileName = @"c:\ffmpeg\ffmpeg.exe", // TODO - Make configurable
                Arguments = cmd
            };

            var process = new Process
            {
                StartInfo = startInfo
            };

            process.Start();
            process.WaitForExit();
            process.Close();
        }
    }
}
