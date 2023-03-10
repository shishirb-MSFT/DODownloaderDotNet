using System;
using System.Runtime.InteropServices;

namespace DODownloader
{
    /// <summary>
    /// Utility class provides easy way to manage a DO download. A download manages only one file.
    /// </summary>
    internal class DODownload
    {
        public DODownloadCallback Handler { get; private set; }

        public IDODownload ClientDownload { get; private set; }

        public Guid Id { get; private set; }

        public DOFile File { get; private set; }
        public string OutputFilePath { get; private set; }

        public DODownload(DOFile file, string callerName, string outputFilePath)
        {
            OutputFilePath = outputFilePath;
            CommonInitNewDownload(file, callerName);
            if (!string.IsNullOrEmpty(outputFilePath))
            {
                ClientDownload.SetProperty(DODownloadProperty.LocalPath, outputFilePath);
            }
        }

        /// <summary>
        /// Creates a DO Download for a url and sets the stream interface
        /// </summary>
        //public DODownload(DOFile file, DataStreamReader dataStream, bool setNonVolatile = true)
        //{
        //    CommonInitNewDownload(file, setNonVolatile: setNonVolatile);
        //    fileData = new DownloadFileData(file, dataStream.DestFilePath);
        //    SetDataStream(dataStream);
        //}

        /// <summary>
        /// Use EnumDownloads to find the IDODownload by its ID and create a DODownload from
        /// that IDODownload and the DOFile and local path specified.
        /// </summary>
        /// <param name="downloadId">Download ID to find</param>
        /// <param name="file">DOFile that was used to create the original DODownload</param>
        /// <param name="tempDestPath">Destination path that was used to create the original DODownload</param>
        //public DODownload(Guid downloadId, DOFile file, string destPath)
        //{
        //    var enumDownloads = new EnumeratedDownloads(DODownloadProperty.Id, downloadId.ToString());
        //    enumDownloads.VerifyCount(1);
        //    enumDownloads.VerifyExists(DODownloadProperty.Id, downloadId.ToString());
        //    enumDownloads.VerifyExists(DODownloadProperty.ContentId, file.FileID);
        //    enumDownloads.VerifyExists(DODownloadProperty.LocalPath, destPath);

        //    SetDownload(enumDownloads.At(0));
        //    SetHandler(new DODownloadCallback(Id.ToString()));
        //    fileData = new DownloadFileData(file, destPath);
        //}

        /// <summary>
        /// Downloads the specified file to specified the path and returns a DODownload object
        /// </summary>
        /// <param name="file">DOFile object that specifies which file to download</param>
        /// <param name="filePath">Destination path where the downloaded file must be written to</param>
        /// <param name="completionTimeSecs">The time to complete the download</param>
        /// <param name="isForeground">Whether the download is in foreground (faster)</param>
        /// <returns>The download instance</returns>
        public static DODownload DownloadFileToDestPath(DOFile file, string filePath, string callerName,
            int completionTimeSecs = 5 * 60, bool isForeground = false)
        {
            var ranges = new DODownloadRanges();
            return DownloadFileToDestPath(file, ranges, filePath, callerName, completionTimeSecs, isForeground);
        }

        /// <summary>
        /// Downloads the specified partial file to specified the path and returns a DODownload object
        /// </summary>
        /// <param name="file"></param>
        /// <param name="ranges"></param>
        /// <param name="filePath"></param>
        /// <param name="completionTimeSecs"></param>
        /// <param name="isForeground"></param>
        /// <returns></returns>
        public static DODownload DownloadFileToDestPath(DOFile file, DODownloadRanges ranges, string filePath,
            string callerName, int completionTimeSecs = 5 * 60, bool isForeground = false)
        {
            var download = new DODownload(file, callerName, filePath);
            if (isForeground)
            {
                download.SetForeground();
            }

            download.Start(ranges);
            download.WaitUntilCompletion(completionTimeSecs);
            return download;
        }

        public void SetCostFlags(DODownloadCostPolicy policy)
        {
            ClientDownload.SetProperty(DODownloadProperty.CostPolicy, (uint)policy);
        }

        public void SetNoCallbacks()
        {
            ClientDownload.SetProperty(DODownloadProperty.CallbackInterface, null);
            ResetHandlerState();
        }

        public void SetUri(string newUri)
        {
            ClientDownload.SetProperty(DODownloadProperty.Uri, newUri);
        }

        public void SetContentSize(UInt64 size)
        {
            ClientDownload.SetProperty(DODownloadProperty.TotalSizeBytes, size);
        }

        /// <summary>
        /// Get decryption info from file data and give it to the download object.
        /// </summary>
        public void SetDecryptionInfo()
        {
            //ClientDownload.SetProperty(DODownloadProperty.DecryptionInfo, fileData.DOTestFile.DecryptionInfo());
        }

        //public void SetPhfInfo(IClientPhfInfo phfInfo)
        //{
        //    ClientDownload.SetProperty(DODownloadProperty.IntegrityCheckInfo, phfInfo.ToJsonString());
        //    ClientDownload.SetProperty(DODownloadProperty.IntegrityCheckMandatory, phfInfo.IsIntegrityCheckMandatory);
        //}

        /// <summary>
        /// Set ForegroundPriority to true. IDODownload defaults to background.
        /// </summary>
        public void SetForeground()
        {
            ClientDownload.SetProperty(DODownloadProperty.ForegroundPriority, true);
        }

        /// <summary>
        /// Set ForegroundPriority to false. IDODownload defaults to background without an explicit property for it.
        /// </summary>
        public void SetBackground()
        {
            ClientDownload.SetProperty(DODownloadProperty.ForegroundPriority, false);
        }

        public bool IsForeground()
        {
            ClientDownload.GetProperty(DODownloadProperty.ForegroundPriority, out object value);
            return (bool)value;
        }

        public bool IsBackground()
        {
            return !IsForeground();
        }

        //public void SetDataStream(ISequentialStream stream)
        //{
        //    // Without UnknownWrapper dosvc receives VT_DISPATCH and returns E_INVALIDARG.
        //    // This isn't required when building in razzle (os.2020). The interop
        //    // has InterfaceType "InterfaceIsIUnknown" which should be sufficient.
        //    ClientDownload.SetProperty(DODownloadProperty.StreamInterface, (stream != null) ? new UnknownWrapper(stream) : null);
        //}

        public void SetNoProgressTimeout(uint timeoutSecs)
        {
            ClientDownload.SetProperty(DODownloadProperty.NoProgressTimeoutSeconds, timeoutSecs);
        }

        public void SetCorrelationVector(string cv)
        {
            ClientDownload.SetProperty(DODownloadProperty.CorrelationVector, cv);
        }

        public uint GetNoProgressTimeout()
        {
            ClientDownload.GetProperty(DODownloadProperty.NoProgressTimeoutSeconds, out var val);
            return (uint)val;
        }

        public ulong GetTotalSizeBytes()
        {
            ClientDownload.GetProperty(DODownloadProperty.TotalSizeBytes, out var val);
            return Convert.ToUInt64(val);
        }

        public void SetHandler(DODownloadCallback handler)
        {
            Handler = handler;
            // Without UnknownWrapper dosvc receives VT_DISPATCH and returns E_INVALIDARG.
            // This isn't required when building in razzle (os.2020). The interop
            // has InterfaceType "InterfaceIsIUnknown" which should be sufficient.
            ClientDownload.SetProperty(DODownloadProperty.CallbackInterface, (handler != null) ? new UnknownWrapper(handler) : null);
        }

        public DODownloadState GetState()
        {
            return GetStatus().State;
        }

        public DO_DOWNLOAD_STATUS GetStatus()
        {
            ClientDownload.GetStatus(out DO_DOWNLOAD_STATUS status);
            return status;
        }

        public void Start()
        {
            Start(new DODownloadRanges());
        }

        public void Start(DODownloadRanges rangesInfo)
        {
            if (rangesInfo == null)
            {
                ClientDownload.Start(IntPtr.Zero);
            }
            else
            {
                // Marshal the ranges in place by hand-assembling a DO_DOWNLOAD_RANGES_INFO structure
                // in CoTaskMemAlloc'd memory. We need to take into account struct member alignment here.
                // The first member RangeCount is a UInt32 which will occupy 4bytes on x86 but 8bytes on x64 (4bytes padding).
                // After RangeCount is an array of DO_DOWNLOAD_RANGE structs which is nothing but 2 UInt64 numbers
                // so they are naturally aligned on both x86 and x64 machines as long as RangeCount is constructed properly.
                int elemSize = Marshal.SizeOf<DO_DOWNLOAD_RANGE>();
                int ptrSize = Marshal.SizeOf<IntPtr>(); // platform specific size for the first member (RangeCount)
                IntPtr coRangesInfo = Marshal.AllocCoTaskMem(ptrSize + (elemSize * rangesInfo.Count));
                IntPtr coRanges = coRangesInfo + ptrSize;
                try
                {
                    Marshal.WriteInt32(coRangesInfo, rangesInfo.Count);
                    for (int i = 0; i < rangesInfo.Count; i++)
                    {
                        Marshal.StructureToPtr(rangesInfo.Collection[i], coRanges + (elemSize * i), false);
                    }
                    Console.WriteLine($"Download {Id}: starting with {rangesInfo.Count} ranges");
                    ClientDownload.Start(coRangesInfo);
                }
                finally
                {
                    Marshal.FreeCoTaskMem(coRangesInfo);
                }
            }
        }

        public void Resume()
        {
            Console.WriteLine($"Download {Id}: resuming");
            ResetHandlerState();
            ClientDownload.Start(IntPtr.Zero);
        }

        public void Pause()
        {
            Console.WriteLine($"Download {Id}: pausing");
            ClientDownload.Pause();
        }

        public void Abort()
        {
            Console.WriteLine($"Download {Id}: aborting");
            ClientDownload.Abort();
        }

        public void Finalize2()
        {
            Console.WriteLine($"Download {Id}: finalizing");
            ClientDownload.Finalize2();
        }

        /// <summary>
        /// Finalizes the download and verifies the download is finalized and persisted
        /// </summary>
        public void FinalizeAndVerify()
        {
            Finalize2();
        }

        public void StartAndWaitUntilCompletion(DODownloadRanges rangeInfo, int completionTimeSecs, bool finalize = true)
        {
            Start(rangeInfo);
            WaitUntilCompletion(completionTimeSecs, finalize);
        }

        public void StartAndWaitUntilCompletion(int completionTimeSecs, bool finalize = true)
        {
            Start();
            WaitUntilCompletion(completionTimeSecs, finalize);
        }

        public void ResumeAndWaitUntilCompletion(int waitTimeSecs, bool finalize = true)
        {
            Resume();
            WaitUntilCompletion(waitTimeSecs, finalize);
        }

        public void StartAndWaitUntilTransferring(int waitTimeSecs = 15, DODownloadRanges rangeInfo = null)
        {
            var ranges = !DODownloadRanges.IsNullOrEmpty(rangeInfo) ? rangeInfo : new DODownloadRanges();
            Start(ranges);
            WaitUntilTransferring(waitTimeSecs);
        }

        public void WaitUntilCompletion(int completionTimeSecs, bool finalize = true)
        {
            Console.WriteLine($"Download {Id}: waiting until completion");
            Handler.WaitForState(DODownloadState.Transferred, completionTimeSecs, this, new[] { DODownloadState.Paused });
            if (finalize)
            {
                Finalize2();
            }
        }

        /// <summary>
        /// Wait until the download is in transferring state.
        /// </summary>
        /// <param name="waitTimeSecs">How long we should wait</param>
        /// <param name="isFromPaused">Whether we started from a paused state. In that case, we won't bail out if the state is paused.</param>
        public void WaitUntilTransferring(int waitTimeSecs, bool isFromPaused = false)
        {
            Handler.WaitForState(DODownloadState.Transferring, waitTimeSecs, this,
                (isFromPaused ? new DODownloadState[] { } : new[] { DODownloadState.Paused }));
        }

        public void WaitUntilTransferred(int waitTimeSecs)
        {
            Console.WriteLine($"Download {Id}: waiting until transferred state");
            Handler.WaitForState(DODownloadState.Transferred, waitTimeSecs, this,
                new DODownloadState[] { DODownloadState.Paused });
        }

        /// <summary>
        /// Use this to clear any state from the callback handler. Useful if we need to clear out
        /// any reported errors before resuming a job.
        /// </summary>
        public void ResetHandlerState()
        {
            Handler.Reset();
        }

        public int ErrorCode()
        {
            return Handler.GetLastDownloadStatus().Error;
        }

        public int ExtendedErrorCode()
        {
            return Handler.GetLastDownloadStatus().ExtendedError;
        }

        public bool IsStatusComplete()
        {
            return Handler.GetDownloadCompleteStatus();
        }

        public bool IsStatusError()
        {
            return (ErrorCode() != 0);
        }

        public bool IsStatusExtendedError()
        {
            return (ExtendedErrorCode() != 0);
        }

        /// <summary>
        /// Creates a DO Download and assigns a callback handler to it.
        /// Also instantiates other private data members.
        /// </summary>
        private void CommonInitNewDownload(DOFile file, string callerName)
        {
            SetDownload(CreateDownload(callerName));
            if (!string.IsNullOrEmpty(file.Id))
            {
                ClientDownload.SetProperty(DODownloadProperty.ContentId, file.Id);
            }
            ClientDownload.SetProperty(DODownloadProperty.Uri, file.Url);
            //SetHandler(new DODownloadCallback(Id.ToString()));
        }

        private void SetDownload(IDODownload download, PInvoke.RPC_C_IMP impLevel = PInvoke.RPC_C_IMP.IMPERSONATE)
        {
            PInvoke.AllowImpersonation<IDODownload>(download, impLevel);
            ClientDownload = download;
            ClientDownload.GetProperty(DODownloadProperty.Id, out object valId);
            Id = new Guid((string)valId);
        }

        private static IDODownload CreateDownload(string callerName)
        {
            IDODownload download = null;
            IDOManager manager = null;
            try
            {
                manager = CreateDOManager();
                manager.CreateDownload(out download);
            }
            finally
            {
                if (manager != null)
                {
                    Marshal.FinalReleaseComObject(manager);
                }
            }
            download.SetProperty(DODownloadProperty.DisplayName, callerName);
            return download;
        }

        public static IDOManager CreateDOManager()
        {
            var manager = (IDOManager)PInvoke.GetComObject(Constants.CLSID_DOManager, Constants.IID_DOManager);
            return manager;
        }
    }
}
