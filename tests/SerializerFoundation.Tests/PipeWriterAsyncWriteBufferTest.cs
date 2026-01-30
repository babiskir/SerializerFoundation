using System;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Text;

namespace SerializerFoundation.Tests;

public class PipeWriterAsyncWriteBufferTest
{

}

public class PipeReaderAsyncReadBufferTest
{
    [Test]
    public async Task Foo()
    {
        MemoryStream stream = new MemoryStream(new byte[1000]);
        var p = PipeReader.Create(stream);

        var foo = await p.ReadAtLeastAsync(0);


    }
}
