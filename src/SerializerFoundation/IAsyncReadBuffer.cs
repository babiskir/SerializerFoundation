namespace SerializerFoundation;

public interface IAsyncReadBuffer : IAsyncDisposable
{
    bool TryGetSpan(int sizeHint, out ReadOnlySpan<byte> span);
    bool TryGetReference(int sizeHint, ref byte reference);
    ValueTask EnsureBufferAsync(int sizeHint, CancellationToken cancellationToken);
    void Advance(int bytesConsumed);
    long BytesConsumed { get; }
}
