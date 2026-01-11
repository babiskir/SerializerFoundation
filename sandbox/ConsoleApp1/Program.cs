

using SerializerFoundation;
using System.Buffers;
using System.IO.Pipelines;
using System.Runtime.CompilerServices;


Stream stream = new MemoryStream();
var pipeWriter = PipeWriter.Create(stream);


var writeBuffer = new PipeWriterAsyncWriteBuffer(pipeWriter);


ref byte p = ref Unsafe.NullRef<byte>();
writeBuffer.TryGetReference(4, ref p);


