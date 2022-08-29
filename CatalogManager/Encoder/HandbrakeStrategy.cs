using Database.Models;
using System;
using System.Diagnostics;
using System.IO;

namespace CatalogManager.Encoder
{
    public class HandbrakeStrategy : IEncoderStrategy
    {
        public void Run(VideoEncodeQueue queueItem)
        {
            Console.WriteLine("Converting file {0}", Path.GetFileNameWithoutExtension(queueItem.InputDirectory));
            // TODO - FileName should be configurable.
            // TODO - CreateNoWindow should be configurable.
            string exe = @"C:\handbrakecli\HandBrakeCLI.exe";
            string dimensionArgs = queueItem.IsVertical ? $" --maxWidth {queueItem.MaxHeight}" : $" --maxHeight {queueItem.MaxHeight}";
            string args = $@" -i ""{queueItem.InputDirectory}"" -o ""{queueItem.OutputDirectory}"" -e {queueItem.Encoder} --encoder-preset {queueItem.RenderSpeed} -f {queueItem.Format} --optimize -q {queueItem.Quality} {dimensionArgs}";
            
            Process proc = new Process();
            proc.StartInfo.FileName = exe;
            proc.StartInfo.Arguments = args;
            proc.StartInfo.CreateNoWindow = false;
            //proc.StartInfo.RedirectStandardOutput = false;
            //proc.StartInfo.RedirectStandardError = false;
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