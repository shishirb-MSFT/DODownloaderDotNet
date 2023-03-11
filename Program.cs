using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace DODownloader
{
    internal class Program
    {
        private static void PrintUsage()
        {
            Console.WriteLine("DODownloader.exe <url> <outputFilePath> [offset,length pairs]");
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

        static int Main(string[] args)
        {
            if ((args.Length != 2) && (args.Length != 3))
            {
                PrintUsage();
                return 1;
            }

            string url = args[0];
            string outputFilePath = args[1];
            if (File.Exists(outputFilePath))
            {
                File.Delete(outputFilePath);
            }

            // Partial or full file download?
            DODownloadRanges downloadRanges = (args.Length == 3) ?
                ParseDownloadRanges(args[2]) : new DODownloadRanges();

            var factory = new DODownloadFactory(callerName: "DODownloader App");
            var file = new DOFile(url);
            var download = factory.CreateDownloadWithFileOutput(file, outputFilePath);
            download.SetForeground();
            download.StartAndWaitUntilCompletion(downloadRanges, 60);

            var fileSize = new FileInfo(outputFilePath).Length;
            Console.WriteLine($"Download completed, file of size {fileSize} bytes written to {outputFilePath}");

            return 0;
        }
    }
}
