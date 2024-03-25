using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace IPTVRelay.Library
{
    public partial class FfmpegWrapper
    {
        private enum ConsoleCtrlEvent
        {
            CtrlC = 0,
            CtrlBreak = 1,
            CtrlClose = 2,
            CtrlLogoff = 5,
            CtrlShutdown = 6
        }
        //ffmpeg -fflags +genpts -i "http://cf.goldvpn.me:80/f1fd4cadc5/3f8e52440a/325005"
        //-codec:v copy -codec:a copy -copyts -avoid_negative_ts disabled -max_muxing_queue_size 2048 -f hls -start_number 0
        //-hls_segment_filename "temp/bde14159333577fbc961d9a1a2c94baf%d.ts" -y "temp/bde14159333577fbc961d9a1a2c94baf.m3u8"

        static Process? RunningProcess = null;
        static readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1,1);
        public static async Task<Process> RehostStream(string input, string output, TimeSpan? timeout = null)
        {
            timeout ??= TimeSpan.FromMinutes(1);
            await _semaphore.WaitAsync();
            try
            {
                KillProcess(RunningProcess?.Id);
                Process ffmpeg = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "ffmpeg",
                        Arguments = $"-reconnect 1 -reconnect_at_eof 1 -reconnect_streamed 1 -reconnect_delay_max 2 -t {timeout.Value.TotalSeconds} -i \"{input}\" -codec:v libx264 -codec:a aac -hls_time 3 -f hls -hls_segment_filename \"{output}_%d.ts\" -y \"{output}.m3u8\"",
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };

                ffmpeg.OutputDataReceived += (s, e) => Console.WriteLine(e.Data);
                ffmpeg.ErrorDataReceived += (s, e) => Console.WriteLine(e.Data);

                ffmpeg.Start();
                ffmpeg.BeginOutputReadLine();
                ffmpeg.BeginErrorReadLine();
                ffmpeg.PriorityClass = ProcessPriorityClass.RealTime;
                RunningProcess = ffmpeg;
            }
            finally { _semaphore.Release(); }
            var killAction = new Action(() => KillProcess(RunningProcess?.Id, output));
            _ = Task.Run(async () =>
            {
                await Task.Delay(timeout ?? TimeSpan.FromMinutes(1));
                KillProcess(RunningProcess?.Id, output);
            });
            return RunningProcess;
        }

        private static void KillProcess(int? processId, string? filePrefix = null)
        {
            if (processId.HasValue)
            {
                Process? process = null;
                try { process = Process.GetProcessById(processId.Value); }
                catch { return; }
                if (!(process?.HasExited ?? true))
                {
                    process.Kill(true);
                    //GenerateConsoleCtrlEvent(ConsoleCtrlEvent.CtrlC, process.Id);
                    if (!process.WaitForExit(3))
                    {
                        process.Kill(true);
                    }
                }
            }
            if(filePrefix != null)
            {
                var files = System.IO.Directory.GetFiles(System.IO.Path.GetDirectoryName(filePrefix), $"{System.IO.Path.GetFileName(filePrefix)}*");
                foreach (var file in files)
                {
                    System.IO.File.Delete(file);
                }
            }
        }
    }
}
