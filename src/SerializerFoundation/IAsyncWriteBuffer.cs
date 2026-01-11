namespace SerializerFoundation;

public interface IAsyncWriteBuffer : IAsyncDisposable
{
    bool TryGetSpan(int sizeHint, out Span<byte> span);
    bool TryGetReference(int sizeHint, ref byte reference);
    ValueTask EnsureBufferAsync(int sizeHint, CancellationToken cancellationToken = default);
    void Advance(int bytesWritten);
    long BytesWritten { get; }
    ValueTask DisposeAsync(CancellationToken cancellationToken);
}
