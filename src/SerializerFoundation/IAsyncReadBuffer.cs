namespace SerializerFoundation;

public interface IAsyncReadBuffer : IAsyncDisposable
{
    bool TryGetSpan(int byteCount, out ReadOnlySpan<byte> span);
    bool TryGetReference(int sizeHint, ref byte reference);
    ValueTask EnsureBufferAsync(int byteCount, CancellationToken cancellationToken);
    void Advance(int bytesConsumed);
    long BytesConsumed { get; }
}
