using SerializerFoundation.Tests.Mini;
using System.Buffers;

namespace SerializerFoundation.Tests;

public static partial class MiniSerializer
{
    public static byte[] SerializeFixedSpanBuffer<T>(T value, IMiniSerializerProvider serializerProvider)
    {
        var serializer = serializerProvider.GetMiniSerializer<FixedSpanWriteBuffer, ReadOnlySpanReadBuffer, T>();

        Span<byte> buffer = new byte[65536];
        var writeBuffer = new FixedSpanWriteBuffer(buffer);
        try
        {
            serializer.Serialize(ref writeBuffer, value, default);
        }
        finally
        {
            writeBuffer.Dispose();
        }

        return buffer.Slice(0, (int)writeBuffer.BytesWritten).ToArray();
    }

    public static T DeserializeFixedSpanBuffer<T>(byte[] data, IMiniSerializerProvider serializerProvider)
    {
        var serializer = serializerProvider.GetMiniSerializer<FixedSpanWriteBuffer, ReadOnlySpanReadBuffer, T>();
        var readBuffer = new ReadOnlySpanReadBuffer(data);
        return serializer.Deserialize(ref readBuffer, default);
    }
}

public class FixedSpanBufferTest
{
    [Test]
    public async Task FullUse()
    {
        var bytes = new byte[100];
        var buffer = new FixedSpanWriteBuffer(bytes);

        var span = buffer.GetSpan(1);
        span.Length.IsEqualTo(100);

        span = buffer.GetSpan(10);
        span.Length.IsEqualTo(100);

        buffer.Advance(8);
        buffer.BytesWritten.IsEqualTo(8);

        span = buffer.GetSpan(20);
        span.Length.IsEqualTo(92);

        span = buffer.GetSpan(92);
        span.Length.IsEqualTo(92);
        buffer.Advance(92);
        buffer.BytesWritten.IsEqualTo(100);

        try
        {
            span = buffer.GetSpan(1);
            Assert.Fail("Expected exception was not thrown.");
        }
        catch (Exception ex)
        {
            await Assert.That(ex).IsTypeOf<InvalidOperationException>();
        }
    }

    [Test]
    public async Task OverAdvance()
    {
        var bytes = new byte[100];
        var buffer = new FixedSpanWriteBuffer(bytes);

        buffer.Advance(42);
        buffer.BytesWritten.IsEqualTo(42);

        var span = buffer.GetSpan(58); // ok
        span.Length.IsEqualTo(58);

        try
        {
            span = buffer.GetSpan(59); // ng
            Assert.Fail("Expected exception was not thrown.");
        }
        catch (Exception ex)
        {
            await Assert.That(ex).IsTypeOf<InvalidOperationException>();
        }
    }

    [Test]
    public async Task DiallowReturnZero()
    {
        var bytes = new byte[100];
        var buffer = new FixedSpanWriteBuffer(bytes);

        var span = buffer.GetSpan(100);
        span.Length.IsEqualTo(100);

        buffer.Advance(100);

        try
        {
            buffer.GetSpan(); // ng
            Assert.Fail("Expected exception was not thrown.");
        }
        catch (Exception ex)
        {
            await Assert.That(ex).IsTypeOf<InvalidOperationException>();
        }
    }

    [Test]
    public async Task DiallowReturnZero2()
    {
        var bytes = new byte[100];
        var buffer = new FixedSpanWriteBuffer(bytes);

        var span = buffer.GetSpan(100);
        span.Length.IsEqualTo(100);

        buffer.Advance(100);

        try
        {
            buffer.GetSpan(1); // ng
            Assert.Fail("Expected exception was not thrown.");
        }
        catch (Exception ex)
        {
            await Assert.That(ex).IsTypeOf<InvalidOperationException>();
        }
    }

    [Test]
    public async Task IntSerializer()
    {
        var expected = 123456789;
        var bytes = MiniSerializer.SerializeFixedSpanBuffer(expected, DefaultMiniSerializerProvider.Instance);
        await Assert.That(bytes).IsEquivalentTo(new byte[] { 21, 205, 91, 7 });
    }

    [Test]
    public async Task ArraySerializer()
    {
        int[] expected = [1, 10, 100, 1000, 10000];
        var bytes = MiniSerializer.SerializeFixedSpanBuffer(expected, DefaultMiniSerializerProvider.Instance);

        await Assert.That(bytes).IsEquivalentTo(new byte[]
        {
            5, 0, 0, 0,   // Length: 5
            1, 0, 0, 0,   // 1
            10, 0, 0, 0,  // 10
            100, 0, 0, 0, // 100
            232, 3, 0, 0, // 1000
            16, 39, 0, 0  // 10000
        });
    }

    [Test]
    public async Task ArraySerializerDeserialize()
    {
        {
            var expected = Enumerable.Range(1, 100).ToArray();
            var bytes = MiniSerializer.SerializeFixedSpanBuffer(expected, DefaultMiniSerializerProvider.Instance);

            var actual = MiniSerializer.DeserializeFixedSpanBuffer<int[]>(bytes, DefaultMiniSerializerProvider.Instance);

            await Assert.That(actual).IsEquivalentTo(expected);
        }
        {
            var expected = Enumerable.Range(1, 1000).ToArray();
            var bytes = MiniSerializer.SerializeFixedSpanBuffer(expected, DefaultMiniSerializerProvider.Instance);

            var actual = MiniSerializer.DeserializeFixedSpanBuffer<int[]>(bytes, DefaultMiniSerializerProvider.Instance);

            await Assert.That(actual).IsEquivalentTo(expected);
        }
    }

    [Test]
    public async Task GetReference_Basic()
    {
        var bytes = new byte[100];
        var buffer = new FixedSpanWriteBuffer(bytes);

        ref byte reference = ref buffer.GetReference(1);
        reference = 0xAB;
        buffer.Advance(1);
        
        AssertUtils.IsEqualTo(bytes[0], (byte)0xAB);
        AssertUtils.IsEqualTo(buffer.BytesWritten, 1);
    }

    [Test]
    public async Task GetReference_ThrowsWhenInsufficientSpace()
    {
        var bytes = new byte[10];
        var buffer = new FixedSpanWriteBuffer(bytes);
        buffer.Advance(10);

        try
        {
            buffer.GetReference(1);
            Assert.Fail("Expected exception was not thrown.");
        }
        catch (Exception ex)
        {
            await Assert.That(ex).IsTypeOf<InvalidOperationException>();
        }
    }

    [Test]
    public async Task GetSpan_WithZeroSizeHint_ReturnsRemainingBuffer()
    {
        var bytes = new byte[100];
        var buffer = new FixedSpanWriteBuffer(bytes);
        buffer.Advance(30);

        var span = buffer.GetSpan(0);
        await Assert.That(span.Length).IsEqualTo(70);
    }

    [Test]
    public async Task EmptyBuffer_ThrowsOnGetSpan()
    {
        var bytes = Array.Empty<byte>();
        var buffer = new FixedSpanWriteBuffer(bytes);

        try
        {
            buffer.GetSpan();
            Assert.Fail("Expected exception was not thrown.");
        }
        catch (Exception ex)
        {
            await Assert.That(ex).IsTypeOf<InvalidOperationException>();
        }
    }

    [Test]
    public async Task EmptyBuffer_ThrowsOnGetReference()
    {
        var bytes = Array.Empty<byte>();
        var buffer = new FixedSpanWriteBuffer(bytes);

        try
        {
            buffer.GetReference();
            Assert.Fail("Expected exception was not thrown.");
        }
        catch (Exception ex)
        {
            await Assert.That(ex).IsTypeOf<InvalidOperationException>();
        }
    }

    [Test]
    public async Task BytesWritten_InitiallyZero()
    {
        var bytes = new byte[100];
        var buffer = new FixedSpanWriteBuffer(bytes);
        await Assert.That(buffer.BytesWritten).IsEqualTo(0);
    }

    [Test]
    public async Task FlushAndDispose_DoNotThrow()
    {
        var bytes = new byte[100];
        var buffer = new FixedSpanWriteBuffer(bytes);
        buffer.GetSpan(10);
        buffer.Advance(10);

        buffer.Flush();
        buffer.Dispose();

        await Assert.That(buffer.BytesWritten).IsEqualTo(10);
    }

    [Test]
    public void FixedPointerWriteBuffer_Basic()
    {
        var bytes = new byte[100];
        unsafe
        {
            fixed (byte* ptr = bytes)
            {
                var buffer = new FixedPointerWriteBuffer(ptr, 100);

                var span = buffer.GetSpan(10);
                span.Length.IsEqualTo(100);

                buffer.Advance(10);
                buffer.BytesWritten.IsEqualTo(10);

                span = buffer.GetSpan(90);
                span.Length.IsEqualTo(90);
            }
        }
    }

    [Test]
    public void FixedPointerWriteBuffer_GetReference()
    {
        var bytes = new byte[100];
        unsafe
        {
            fixed (byte* ptr = bytes)
            {
                var buffer = new FixedPointerWriteBuffer(ptr, 100);

                ref byte reference = ref buffer.GetReference(1);
                reference = 0xCD;
                buffer.Advance(1);
            }
        }

        bytes[0].IsEqualTo((byte)0xCD);
    }

    [Test]
    public void FixedPointerWriteBuffer_ThrowsWhenInsufficientSpace()
    {
        var bytes = new byte[10];
        unsafe
        {
            fixed (byte* ptr = bytes)
            {
                var buffer = new FixedPointerWriteBuffer(ptr, 10);
                buffer.Advance(10);

                var throwed = false;
                try
                {
                    buffer.GetSpan(1);
                }
                catch (InvalidOperationException)
                {
                    throwed = true;
                }
                throwed.IsEqualTo(true);
            }
        }
    }
}
