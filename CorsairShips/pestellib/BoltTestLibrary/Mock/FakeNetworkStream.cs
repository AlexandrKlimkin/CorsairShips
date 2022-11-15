using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace BoltTestLibrary.Mock
{
    public class FakeNetworkStream : Stream
    {
        public FakeNetworkStream RemotePCStream { get; private set; }
        
        private bool closed = false;
        
        private readonly Queue<byte> data;

        public override bool CanRead => true;

        public override bool CanSeek => false;

        public override bool CanWrite => true;

        public override long Length => throw new NotImplementedException();

        public override long Position { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        
        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotImplementedException();
        }

        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }

        private FakeNetworkStream(FakeNetworkStream remote)
        {
            this.data = new Queue<byte>();
            RemotePCStream = remote;
        }

        public FakeNetworkStream()
        {
            this.data = new Queue<byte>();
            RemotePCStream = new FakeNetworkStream(this);
        }
        
        public override void Flush()
        {
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (closed)
            {
                throw new Exception("connection closed");
            }

            var result = 0;
            for (var i = offset; i < offset + count; i++)
            {
                if (data.Count > 0)
                {
                    result++;
                    buffer[i] = data.Dequeue();
                } 
                else
                {
                    break;
                }
            }
            return result;
        }

        public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            while (data.Count == 0)
            {
                if (closed)
                {
                    throw new Exception("connection closed");
                }
                await Task.Delay(30);
            }
            return Read(buffer, offset, count);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            if (RemotePCStream != null)
            {
                RemotePCStream.DoWrite(buffer, offset, count);
            }
        }

        private void DoWrite(byte[] buffer, int offset, int count)
        {
            for (var i = offset; i < offset + count; i++)
            {
                data.Enqueue(buffer[i]);
            }            
        }

        public override void Close()
        {
            base.Close();
            closed = true;
        }
    }
}
