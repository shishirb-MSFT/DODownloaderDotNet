// COM Interop C# classes for accessing Delivery Optimization API.
// https://learn.microsoft.com/en-us/windows/win32/delivery_optimization/do-reference

using System;
using System.Runtime.InteropServices;

namespace DODownloader
{
    public class Constants
    {
        public static readonly Guid CLSID_DOManager = new Guid("5b99fa76-721c-423c-adac-56d03c8a8007");
        public static readonly Guid IID_DOManager = new Guid("400E2D4A-1431-4C1A-A748-39CA472CFDB1");
    }

    [ComImport]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [GuidAttribute("00000100-0000-0000-C000-000000000046")]
    public interface IEnumUnknown
    {
        void Next(uint celt, [Out, MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.IUnknown, SizeParamIndex = 0)] object[] rgelt, ref uint celtFetched);
        void Skip(uint celt);
        void Reset();
        void Clone(out IEnumUnknown enumUnknown);
    }

    public enum DODownloadCostPolicy
    {
        Always = 0,    // download runs regardless of the cost
        Unrestricted,  // download runs unless imposes costs or traffic limits
        Standard,      // download runs unless neither subject to a surcharge nor near exhaustion
        NoRoaming,     // download runs unless that connectivity is subject to roaming surcharges
        NoSurcharge,   // download runs unless subject to a surcharge
        NoCellular,    // download runs unless network is on cellular
    }

    public enum DODownloadState
    {
        Created = 0,     // download object is created but hasn’t been started yet
        Transferring,    // download is in progress
        Transferred,     // download is transferred (can start again)
        Finalized,       // download is finalized (cannot be started again)
        Aborted,         // download is aborted
        Paused           // download has been paused on demand or due to (transient) error
    }

    public enum DODownloadProperty
    {
        Id = 0,                    // [VT_BSTR] read-only ID to uniquely identify the download
        Uri,                       // [VT_BSTR] remote URI path (required)
        ContentId,                 // [VT_BSTR] download content ID
        DisplayName,               // [VT_BSTR] download display name
        LocalPath,                 // [VT_BSTR] Local path (may not exist, and DO will attempt to create it under the caller's token)
        HttpCustomHeaders,         // [VT_BSTR] custom HTTP request headers
        CostPolicy,                // [VT_UI4] one of the DODownloadCostPolicy values
        SecurityFlags,             // [VT_UI4] WinHTTP security flags
        CallbackFreqPercent,       // [VT_UI4] callback frequency calls based on percentage
        CallbackFreqSeconds,       // [VT_UI4] callback frequency calls based on seconds (default: 1 second)
        NoProgressTimeoutSeconds,  // [VT_UI4] DO timeout length for no download progress (min value: 60 sec and 0 is DO default)
        ForegroundPriority,        // [VT_BOOL] foreground download (background is the default)
        BlockingMode,              // [VT_BOOL] blocking mode - Start() will block until download is complete/error
        CallbackInterface,         // [VT_UNKNOWN] IDODownloadStatusCallback* for download callbacks
        StreamInterface,           // [VT_UNKNOWN] IStream* for direct streaming
        SecurityContext,           // [VT_ARRAY | VT_UI1] safe array of bytes of a serialized CERT_CONTEXT
        NetworkToken,              // [VT_BOOL] network token to be used during HTTP operations (set false to clear the token)
        CorrelationVector,         // [VT_BSTR] CV
        DecryptionInfo,            // [VT_BSTR] decryption info [can't be read], serialized json
        IntegrityCheckInfo,        // [VT_BSTR] PHF [can't be read], serialized json
        IntegrityCheckMandatory,   // [VT_BOOL] PHF boolean
        TotalSizeBytes,            // [VT_UI8] download size
        DisallowOnCellular,        // [VT_BOOL] don't download on cellular, regardless of cost policy
        HttpCustomAuthHeaders,     // [VT_BSTR] custom HTTPS headers used when challenged
        HttpAllowSecureToNonSecureRedirect, // [VT_BOOL] https to http redirection (default: FALSE)
        NonVolatile,                // [VT_BOOL] save download info to registry (default: FALSE)
    }

    public enum DODownloadPropertyEx
    {
        UpdateId = 0,              // [VT_BSTR] internal update ID
        CorrelationVector,         // [VT_BSTR] CV
        DecryptionInfo,            // [VT_BSTR] decryption info [can't be read]
        IntegrityCheckInfo,        // [VT_BSTR] PHF
        IntegrityCheckMandatory,   // [VT_BOOL] PHF boolean
        TotalSizeBytes,            // [VT_UI8] download size
        TempLocalFileUsage,        // [VT_BOOL] temp local file usage vs. download filename (default is true)
    }

    [StructLayoutAttribute(LayoutKind.Sequential)]
    public struct DO_DOWNLOAD_RANGE
    {
        public UInt64 Offset;
        public UInt64 Length;
    }

    [StructLayoutAttribute(LayoutKind.Sequential)]
    public struct DO_DOWNLOAD_RANGES_INFO
    {
        public uint RangeCount;
        public DO_DOWNLOAD_RANGE[] Ranges;
    }

    [StructLayoutAttribute(LayoutKind.Sequential)]
    public struct DO_DOWNLOAD_STATUS
    {
        public UInt64 BytesTotal;
        public UInt64 BytesTransferred;
        public DODownloadState State;
        public int Error;
        public int ExtendedError;
    }

    [StructLayoutAttribute(LayoutKind.Sequential)]
    public struct DO_DOWNLOAD_ENUM_CATEGORY
    {
        public DODownloadProperty Property;
        [MarshalAs(UnmanagedType.LPWStr)]
        public string Value;
    }

    [InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIUnknown)]
    [GuidAttribute("FBBD7FC0-C147-4727-A38D-827EF071EE77")]
    [ComImportAttribute()]
    public interface IDODownload
    {
        void Start(IntPtr ranges); // ranges is a marshaled DO_DOWNLOAD_RANGES_INFO
        void Pause();
        void Abort();
        void Finalize2(); // Note: Finalize() collides with Object.Finalize()
        void GetStatus(out DO_DOWNLOAD_STATUS status);
        void GetProperty(DODownloadProperty propId, out object propVal);
        void SetProperty(DODownloadProperty propId, ref object propVal);
    }

    [InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIUnknown)]
    [GuidAttribute("D166E8E3-A90E-4392-8E87-05E996D3747D")]
    [ComImportAttribute()]
    public interface IDODownloadStatusCallback
    {
        void OnStatusChange([MarshalAs(UnmanagedType.Interface)] IDODownload download, ref DO_DOWNLOAD_STATUS status);
    }

    [InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIUnknown)]
    [GuidAttribute("400E2D4A-1431-4C1A-A748-39CA472CFDB1")]
    [ComImportAttribute()]
    public interface IDOManager
    {
        void CreateDownload([MarshalAs(UnmanagedType.Interface)] out IDODownload download);
        void EnumDownloads(IntPtr enumCategory, [MarshalAs(UnmanagedType.Interface)] out IEnumUnknown ppEnum);
    }
}
