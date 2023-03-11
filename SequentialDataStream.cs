using System;
using System.Runtime.InteropServices;

namespace DODownloader
{
    internal class SequentialStreamReceiver : IStream
    {
        public ulong TotalBytesReceived { get; private set; }
        public int TotalCallsReceived { get; private set; }

        public SequentialStreamReceiver()
        {
            TotalBytesReceived = 0;
            TotalCallsReceived = 0;
        }

        // ** IStream methods **
        // DO client calls only the Write method, so that's the only one implemented here.

        public void Read([Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1)] byte[] pv, uint cb, out uint pcbRead)
        {
            throw new NotImplementedException();
        }

        public virtual void Write([MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1)] byte[] pv, uint cb, IntPtr pcbWritten)
        {
            TotalBytesReceived += cb;
            ++TotalCallsReceived;
            Console.WriteLine($"Data stream received = {cb} bytes, total = {TotalBytesReceived} bytes");
            if (pcbWritten != IntPtr.Zero)
            {
                Marshal.WriteInt64(pcbWritten, cb);
            }
        }

        public void Seek(long dlibMove, uint dwOrigin, IntPtr plibNewPosition)
        {
            throw new NotImplementedException();
        }

        public void SetSize(long libNewSize)
        {
            throw new NotImplementedException();
        }

        public void CopyTo(IStream pstm, long cb, out long pcbRead, IntPtr pcbWritten)
        {
            throw new NotImplementedException();
        }

        public void Commit(uint grfCommitFlags)
        {
            throw new NotImplementedException();
        }

        public void Revert()
        {
            throw new NotImplementedException();
        }

        public void LockRegion(long libOffset, long cb, uint dwLockType)
        {
            throw new NotImplementedException();
        }

        public void UnlockRegion(long libOffset, long cb, uint dwLockType)
        {
            throw new NotImplementedException();
        }

        public void Stat(out System.Runtime.InteropServices.ComTypes.STATSTG pstatstg, uint grfStatFlag)
        {
            throw new NotImplementedException();
        }

        public void Clone(out IStream ppstm)
        {
            throw new NotImplementedException();
        }
    }
}
