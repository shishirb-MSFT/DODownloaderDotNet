using System;

namespace DODownloader
{
    internal class DODownloadRanges
    {
        public DO_DOWNLOAD_RANGE[] Collection { get; private set; }
        public ulong TotalLength { get; private set; }
        public int Count => Collection?.Length ?? 0;

        // Zero ranges => full file download
        public DODownloadRanges()
        {
            Collection = null;
            TotalLength = 0;
        }

        // {Offset, Length} specified via 1D array
        public DODownloadRanges(ulong[] offsetLengthPairs)
        {
            if ((offsetLengthPairs.Length % 2) != 0)
            {
                throw new ArgumentException("Expected even number of elements in offsetLengthPairs argument");
            }

            Collection = new DO_DOWNLOAD_RANGE[offsetLengthPairs.Length / 2];
            TotalLength = 0;
            for (int i = 0, j = 0; i < offsetLengthPairs.Length; i += 2, j++)
            {
                Collection[j].Offset = offsetLengthPairs[i];
                Collection[j].Length = offsetLengthPairs[i + 1];
                TotalLength += offsetLengthPairs[i + 1];
            }
        }

        // {Offset, Length} specified via 2D array
        public DODownloadRanges(ulong[,] offsetLengthPairs)
        {
            int numRanges = offsetLengthPairs.GetLength(0);
            if (numRanges == 0)
            {
                throw new ArgumentException("Expected non-empty offsetLengthPairs argument");
            }
            if (offsetLengthPairs.GetLength(1) != 2)
            {
                throw new ArgumentException("Expected 2 columns in offsetLengthPairs argument");
            }

            Collection = new DO_DOWNLOAD_RANGE[numRanges];
            TotalLength = 0;
            for (int i = 0; i < numRanges; i++)
            {
                Collection[i].Offset = offsetLengthPairs[i, 0];
                Collection[i].Length = offsetLengthPairs[i, 1];
                TotalLength = offsetLengthPairs[i, 1];
            }
        }

        public static bool IsNullOrEmpty(DODownloadRanges ranges)
        {
            return (ranges == null) || (ranges.Count == 0);
        }
    }
}
