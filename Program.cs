using System;
using System.IO;

// Sample code to showcase usage of Delivery Optimization client's COM API via C# Interop.
// COM API is documented here: https://learn.microsoft.com/en-us/windows/win32/delivery_optimization/do-reference

namespace DODownloader
{
    internal class Program
    {
        static int Main(string[] args)
        {
            // Example invocations:
            //  > $url = "http://dl.delivery.mp.microsoft.com/filestreamingservice/files/52fa8751-747d-479d-8f22-e32730cc0eb1"
            // Full file streaming download: > .\DODownloader.exe --url $url
            // Partial file streaming download: > .\DODownloader.exe --url $url --ranges 10,65536,131072,65536
            // Full file download: > .\DODownloader.exe --url $url --output-file-path $env:TEMP\testfile.dat
            if (!Options.TryParseArgs(args, out Options options))
            {
                Console.WriteLine("Usage: DODownloader.exe --url <url> [--output-file-path <path>]"
                    + " [--ranges <offset0,length0,offset1,length1,...>]");
                return 1;
            }

            options.SetRangesIfEmpty();

            var factory = new DODownloadFactory(callerName: "DODownloader App");
            var file = new DOFile(options.Url);

            DODownload download = null;
            SequentialStreamReceiver downloadDataSink = null;
            try
            {
                if (options.IsStreamDownload)
                {
                    downloadDataSink = new SequentialStreamReceiver();
                    download = factory.CreateDownloadWithStreamOutput(file, downloadDataSink);
                }
                else
                {
                    if (File.Exists(options.OutputFilePath))
                    {
                        File.Delete(options.OutputFilePath);
                    }
                    download = factory.CreateDownloadWithFileOutput(file, options.OutputFilePath);
                }

                download.SetForeground();
                download.StartAndWaitUntilTransferred(options.DownloadRanges, completionTimeSecs: 60);
                // Here, we can do something more like query stats from download.
                // Then let DO client know that we are done with this download object.
                download.Finalize2();

                if (options.IsStreamDownload)
                {
                    Console.WriteLine($"Download completed, received {downloadDataSink.TotalBytesReceived} bytes via"
                        + $" {downloadDataSink.TotalCallsReceived} stream write calls");
                }
                else
                {
                    var fileSize = new FileInfo(options.OutputFilePath).Length;
                    Console.WriteLine($"Download completed, output file size {fileSize} bytes at {options.OutputFilePath}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Download failed. Exception: hr: {ex.HResult:X}, {ex.Message}\n{ex.StackTrace}");
                download?.Abort();
            }

            return 0;
        }

        struct Options
        {
            public string Url;
            public string OutputFilePath;
            public DODownloadRanges DownloadRanges;
            public bool IsStreamDownload => string.IsNullOrEmpty(OutputFilePath);

            public void SetRangesIfEmpty()
            {
                if (DownloadRanges != null)
                {
                    return;
                }

                if (IsStreamDownload)
                {
                    // Streaming download requires a range to specified (upto and including Win11 22H2).
                    // Range of offset = 0, length = max-uint64 indicates a full file range request.
                    DownloadRanges = new DODownloadRanges(new[] { 0ul, ulong.MaxValue });
                }
                else
                {
                    // Full file download is indicated with empty/zero ranges object
                    DownloadRanges = new DODownloadRanges();
                }
            }

            public static bool TryParseArgs(string[] args, out Options options)
            {
                options = new Options();
                for (int i = 0; i < args.Length; i++)
                {
                    if (args[i].Equals("--url"))
                    {
                        if ((i + 1) >= args.Length) return false;
                        options.Url = args[++i];
                    }
                    else if (args[i].Equals("--output-file-path"))
                    {
                        if ((i + 1) >= args.Length) return false;
                        options.OutputFilePath = args[++i];
                    }
                    else if (args[i].Equals("--ranges"))
                    {
                        if ((i + 1) >= args.Length) return false;
                        try
                        {
                            options.DownloadRanges = ParseDownloadRanges(args[++i]);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error parsing ranges argument: {ex.Message}");
                            return false;
                        }
                    }
                    else
                    {
                        Console.WriteLine($"Unknown cmdline option '{args[i]}'");
                        return false;
                    }
                }
                return !string.IsNullOrEmpty(options.Url);
            }

            private static DODownloadRanges ParseDownloadRanges(string arg)
            {
                string[] offsetsAndLengths = arg.Split(',');
                var offsetLengths = new ulong[offsetsAndLengths.Length];
                int i = 0;
                foreach (var val in offsetsAndLengths)
                {
                    offsetLengths[i++] = Convert.ToUInt64(val);
                }
                return new DODownloadRanges(offsetLengths);
            }
        }
    }
}
