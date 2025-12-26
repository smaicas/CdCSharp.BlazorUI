using CdCSharp.BlazorUI.Core.Css;
using FluentAssertions;

namespace CdCSharp.BlazorUI.Tests.Integration.Tests.Core.Extensions;

[Trait("Extensions", "NumberExtensions")]
public class NumberExtensionsTests
{
    [Theory(DisplayName = "EnsureRange_Byte_Max_ClampsToRange")]
    [InlineData((byte)10, (byte)5, (byte)5)]
    [InlineData((byte)3, (byte)5, (byte)3)]
    [InlineData((byte)0, (byte)5, (byte)0)]
    public void EnsureRange_Byte_Max_ClampsToRange(byte input, byte max, byte expected)
    {
        // Act
        byte result = input.EnsureRange(max);

        // Assert
        result.Should().Be(expected);
    }

    [Theory(DisplayName = "EnsureRange_Byte_MinMax_ClampsToRange")]
    [InlineData((byte)10, (byte)2, (byte)8, (byte)8)]
    [InlineData((byte)5, (byte)2, (byte)8, (byte)5)]
    [InlineData((byte)1, (byte)2, (byte)8, (byte)2)]
    public void EnsureRange_Byte_MinMax_ClampsToRange(byte input, byte min, byte max, byte expected)
    {
        // Act
        byte result = input.EnsureRange(min, max);

        // Assert
        result.Should().Be(expected);
    }

    [Theory(DisplayName = "EnsureRange_Int_Max_ClampsToRange")]
    [InlineData(10, 5, 5)]
    [InlineData(3, 5, 3)]
    [InlineData(-5, 10, 0)]
    [InlineData(0, 5, 0)]
    public void EnsureRange_Int_Max_ClampsToRange(int input, int max, int expected)
    {
        // Act
        int result = input.EnsureRange(max);

        // Assert
        result.Should().Be(expected);
    }

    [Theory(DisplayName = "EnsureRange_Int_MinMax_ClampsToRange")]
    [InlineData(10, 2, 8, 8)]
    [InlineData(5, 2, 8, 5)]
    [InlineData(1, 2, 8, 2)]
    [InlineData(-5, -10, 0, -5)]
    public void EnsureRange_Int_MinMax_ClampsToRange(int input, int min, int max, int expected)
    {
        // Act
        int result = input.EnsureRange(min, max);

        // Assert
        result.Should().Be(expected);
    }

    [Theory(DisplayName = "EnsureRangeToByte_ConvertsAndClampsToByteRange")]
    [InlineData(100, 100)]
    [InlineData(255, 255)]
    [InlineData(256, 255)]
    [InlineData(0, 0)]
    [InlineData(-10, 0)]
    [InlineData(500, 255)]
    public void EnsureRangeToByte_ConvertsAndClampsToByteRange(int input, byte expected)
    {
        // Act
        byte result = input.EnsureRangeToByte();

        // Assert
        result.Should().Be(expected);
    }
}