using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;

namespace DODownloader
{
    internal class DODownloadCallback : IDODownloadStatusCallback
    {
        public int CountDownloadCompletes { get; private set; }
        public int CountDownloadErrors { get; private set; }
        public int CountDownloadExtendedErrors { get; private set; }

        private DO_DOWNLOAD_STATUS LastDownloadStatus;

        private AutoResetEvent statusChangeEvent;

        private object stateLock;

        private string downloadId;

        public DODownloadCallback(string downloadId)
        {
            this.downloadId = downloadId;
            stateLock = new object();
            statusChangeEvent = new AutoResetEvent(false);
            Reset();
        }

        public DO_DOWNLOAD_STATUS GetStatus(DODownload download)
        {
            var status = download.GetStatus();
            lock (stateLock)
            {
                LastDownloadStatus = status;
            }
            return status;
        }

        public DO_DOWNLOAD_STATUS GetLastDownloadStatus()
        {
            lock (stateLock)
            {
                return LastDownloadStatus;
            }
        }

        public bool GetDownloadCompleteStatus()
        {
            lock (stateLock)
            {
                return (CountDownloadCompletes > 0);
            }
        }

        public virtual void OnStatusChange([MarshalAs(UnmanagedType.Interface)] IDODownload download, ref DO_DOWNLOAD_STATUS status)
        {
            LogStatus(ref status);

            lock (stateLock)
            {
                LastDownloadStatus = status;

                switch (status.State)
                {
                    case DODownloadState.Paused:
                        Console.WriteLine($"{downloadId}: Paused state. Error: 0x{status.Error:X}, ExtError: 0x{status.ExtendedError:X}");
                        if (status.Error != 0)
                        {
                            ++CountDownloadErrors;
                        }

                        if (status.ExtendedError != 0)
                        {
                            ++CountDownloadExtendedErrors;
                        }
                        break;

                    case DODownloadState.Transferred:
                        ++CountDownloadCompletes;
                        break;
                }
            }

            statusChangeEvent.Set();
            Marshal.FinalReleaseComObject(download);
        }

        public void WaitForState(DODownloadState waitForState, int waitTimeSecs, DODownload download, DODownloadState[] bailoutStates = null)
        {
            const int pollingLoopIntervalMsecs = 3 * 1000;

            bailoutStates = bailoutStates ?? new DODownloadState[] { };
            Console.WriteLine($"{downloadId}: Waiting {waitTimeSecs}s for download to change to {waitForState} state");

            var totalWaitTime = waitTimeSecs * 1000;
            var curWaitTime = Math.Min(pollingLoopIntervalMsecs, totalWaitTime);
            var stopwatch = Stopwatch.StartNew();

            // The loop wakes and executes after every OnStatusChange callback.
            // The loop also wakes if there hasn't been a callback for 3 seconds
            // or when it is time to execute the optional action.
            while (true)
            {
                DO_DOWNLOAD_STATUS status;

                if (statusChangeEvent.WaitOne(curWaitTime))
                {
                    // Got an OnStatusChange callback
                    lock (stateLock)
                    {
                        status = LastDownloadStatus;
                    }
                }
                else
                {
                    // No callback, poll now
                    status = GetStatus(download);
                    LogStatus(ref status);
                }

                if (status.State == waitForState)
                {
                    break;
                }

                if (bailoutStates.Contains(status.State))
                {
                    // Download reached one of the unexpected states
                    throw new Exception($"{downloadId}: Hit unexpected {status.State} state while waiting for {waitForState} state. Error: 0x{status.Error:X} ExtendedError: 0x{status.ExtendedError:X}.");
                }

                var elapsedMs = (int)stopwatch.ElapsedMilliseconds;
                if (elapsedMs >= totalWaitTime)
                {
                    // timeout
                    throw new TimeoutException($"{downloadId}: Download did not reach {waitForState} state in {waitTimeSecs} s. Error: 0x{status.Error:X} ExtendedError: 0x{status.ExtendedError:X}.");
                }

                curWaitTime = Math.Min(pollingLoopIntervalMsecs, totalWaitTime - elapsedMs);
            }
        }

        public void Reset()
        {
            lock (stateLock)
            {
                CountDownloadCompletes = CountDownloadErrors = CountDownloadExtendedErrors = 0;
                LastDownloadStatus = new DO_DOWNLOAD_STATUS();
            }
            statusChangeEvent?.Reset();
        }

        private void LogStatus(ref DO_DOWNLOAD_STATUS status)
        {
            Console.WriteLine($"{downloadId}: {status.State}, {status.BytesTransferred} / {status.BytesTotal}, " +
                $"{(int)((status.BytesTotal > 0) ? 100.0 * status.BytesTransferred / status.BytesTotal : 0)}%");
        }
    }
}
