namespace SerializerFoundation;

public interface IAsyncReadBuffer : IAsyncDisposable
{
    bool TryGetSpan(int byteCount, out ReadOnlySpan<byte> span);
    ValueTask EnsureBufferAsync(int byteCount, CancellationToken cancellationToken);
    void Advance(int bytesConsumed);
}
