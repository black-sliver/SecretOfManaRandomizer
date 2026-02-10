using JetBrains.Annotations;
using SoMRandomizer.util;
using Xunit;

namespace SoMRandomizer.Tests;

[TestSubject(typeof(SomLz77))]
public class OpenWorldTest
{
    [Fact]
    public void TestEncode1Plus3()
    {
        var data = "\0\0\0\0"u8.ToArray();
        var windowOne = SomLz77.encodeSomLz77(data, 1);
        var windowFour = SomLz77.encodeSomLz77(data, 4);
        // window size lo
        // window size hi
        // len lo
        // len hi
        // 1 byte 0 (len of uncompressed - 1)
        // 1 byte 0 (uncompressed bytes)
        // 2 byte symbol to repeat the 0
        Assert.Equal(8, windowOne.Count);
        Assert.Equal(8, windowFour.Count);
        Assert.NotEqual(windowOne[0], windowFour[0]);
        Assert.Equal(windowOne[1], windowFour[1]);
        Assert.Equal(windowOne[2], windowFour[2]);
        Assert.Equal(windowOne[3], windowFour[3]);
        Assert.Equal(0, windowOne[4]);
        Assert.Equal(0, windowFour[4]);
        Assert.Equal(0, windowOne[5]);
        Assert.Equal(0, windowFour[5]);
        // window shift should make no difference here:
        Assert.Equal(0x80, windowOne[6]); // both offset and len value 0
        Assert.Equal(0x80, windowFour[6]); // both offset and len value 0
        Assert.Equal(0x00, windowOne[7]); // len value 0
        Assert.Equal(0x00, windowFour[7]); // len value 0
    }

    [Fact]
    public void TestEncode4Plus4()
    {
        var data = "\x01\x02\x03\x04\x05\x01\x02\x03\x04\x05"u8.ToArray();
        var windowOne = SomLz77.encodeSomLz77(data, 1);
        var windowFour = SomLz77.encodeSomLz77(data, 4);
        // window size lo
        // window size hi
        // len lo
        // len hi
        // 1 byte 4 (len of uncompressed - 1)
        // 5 byte 1, 2, 3, 4, 5 (uncompressed bytes)
        // 2 byte symbol to repeat the 1, 2, 3, 4, 5
        Assert.Equal(12, windowOne.Count);
        Assert.Equal(12, windowFour.Count);
        Assert.NotEqual(windowOne[0], windowFour[0]);
        Assert.Equal(windowOne[1], windowFour[1]);
        Assert.Equal(windowOne[2], windowFour[2]);
        Assert.Equal(windowOne[3], windowFour[3]);
        Assert.Equal(4, windowOne[4]);
        Assert.Equal(4, windowFour[4]);
        for (var i = 0; i < 5; i++)
        {
            Assert.Equal(data[i], windowOne[5 + i]);
            Assert.Equal(data[i], windowFour[5 + i]);
        }
        Assert.NotEqual(windowOne[10], windowFour[10]); // window shift differs
        Assert.Equal(windowOne[11], windowFour[11]);
    }
}
