using System.Text;

namespace SPOrchestratorAPI.Helpers
{
    /// <summary>
    /// Un wrapper de Stream que reenvía todas las operaciones al stream original y copia todo lo escrito en un MemoryStream interno para capturar el contenido.
    /// </summary>
    public class ResponseCaptureStream : Stream
    {
        private readonly Stream _innerStream;
        private readonly MemoryStream _copyStream = new MemoryStream();

        public ResponseCaptureStream(Stream innerStream)
        {
            _innerStream = innerStream ?? throw new ArgumentNullException(nameof(innerStream));
        }

        /// <summary>
        /// Obtiene el contenido capturado en forma de texto (UTF8).
        /// </summary>
        public string GetCapturedText()
        {
            return Encoding.UTF8.GetString(_copyStream.ToArray());
        }

        public override bool CanRead => _innerStream.CanRead;
        public override bool CanSeek => _innerStream.CanSeek;
        public override bool CanWrite => _innerStream.CanWrite;
        public override long Length => _innerStream.Length;
        public override long Position 
        { 
            get => _innerStream.Position; 
            set { _innerStream.Position = value; _copyStream.Position = value; } 
        }

        public override void Flush()
        {
            _innerStream.Flush();
            _copyStream.Flush();
        }

        public override async Task FlushAsync(CancellationToken cancellationToken)
        {
            await _innerStream.FlushAsync(cancellationToken);
            await _copyStream.FlushAsync(cancellationToken);
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return _innerStream.Read(buffer, offset, count);
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            _copyStream.Seek(offset, origin);
            return _innerStream.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            _innerStream.SetLength(value);
            _copyStream.SetLength(value);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            _innerStream.Write(buffer, offset, count);
            _copyStream.Write(buffer, offset, count);
        }

        public override async Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            await _innerStream.WriteAsync(buffer, offset, count, cancellationToken);
            await _copyStream.WriteAsync(buffer, offset, count, cancellationToken);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _copyStream.Dispose();
                // No se debe disponer _innerStream, ya que lo administra el framework.
            }
            base.Dispose(disposing);
        }
    }
}
