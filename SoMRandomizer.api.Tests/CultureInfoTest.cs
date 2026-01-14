using System;
using System.Globalization;
using JetBrains.Annotations;
using Xunit;

namespace SoMRandomizer.api.Tests;

// SoMR Settings uses enUS for decimals, but we want to compile with Culture stripped
// so validate that all uses of invariant culture behaves as with original code
[TestSubject(typeof(OpenWorld))]
public class CultureInfoTest
{
    private static readonly CultureInfo Invariant = CultureInfo.InvariantCulture;
    private static readonly CultureInfo EnUs = new("en-US");

    [Fact]
    public void TestInvariantDoubleParseBehavesAsUs()
    {
        const string s1 = "1.23";
        const string s2 = "-12345.6";
        const string s3A = "1";
        const string s3B = "2";
        Assert.True(Math.Abs(Double.Parse(s1, Invariant) - Double.Parse(s1, EnUs)) < 0.01);
        Assert.True(Math.Abs(Double.Parse(s2, Invariant) - Double.Parse(s2, EnUs)) < 0.01);
        Assert.False(Math.Abs(Double.Parse(s3A, Invariant) - Double.Parse(s3B, EnUs)) < 0.01);
    }

    [Fact]
    public void TestInvariantDoubleToStringBehavesAsUs()
    {
        const double n1 = 1.23;
        const double n2 = -12345.6;
        const double n3A = 1;
        const double n3B = 2;
        Assert.Equal(n1.ToString(Invariant), n1.ToString(EnUs));
        Assert.Equal(n2.ToString(Invariant), n2.ToString(EnUs));
        Assert.NotEqual(n3A.ToString(Invariant), n3B.ToString(EnUs));
    }

    [Fact]
    public void TestInvariantIntParseBehavesAsUs()
    {
        const string s1 = "12345";
        const string s2 = "-123456";
        const string s3A = "1";
        const string s3B = "2";
        Assert.Equal(int.Parse(s1, Invariant), int.Parse(s1, EnUs));
        Assert.Equal(int.Parse(s2, Invariant), int.Parse(s2, EnUs));
        Assert.NotEqual(int.Parse(s3A, Invariant), int.Parse(s3B, EnUs));

        Exception invariantException = null;
        try
        {
            int.Parse("2.34", Invariant);
        }
        catch (Exception e)
        {
            invariantException = e;
        }

        Exception enUsException = null;
        try
        {
            int.Parse("2.34", Invariant);
        }
        catch (Exception e)
        {
            enUsException = e;
        }

        Assert.NotNull(invariantException);
        Assert.NotNull(enUsException);
        Assert.Equal(invariantException.Message, enUsException.Message);
    }

    [Fact]
    public void TestInvariantIntToStringBehavesAsUs()
    {
        const int n1 = 12345;
        const int n2 = -123456;
        const int n3A = 1;
        const int n3B = 2;
        Assert.Equal(n1.ToString(Invariant), n1.ToString(EnUs));
        Assert.Equal(n2.ToString(Invariant), n2.ToString(EnUs));
        Assert.NotEqual(n3A.ToString(Invariant), n3B.ToString(EnUs));
    }
}
