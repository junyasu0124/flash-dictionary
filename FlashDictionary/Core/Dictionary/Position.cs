using System.Diagnostics;

namespace FlashDictionary.Core.Dictionary;

[DebuggerDisplay("{Offset.ToString()} ({Length.ToString()})")]
internal readonly struct Position
{
    public Position(long offset, long length)
    {
        Offset = offset;
        Length = length;
    }
    public Position(int offset, int length)
    {
        Offset = offset;
        Length = length;
    }

    public readonly long Offset { get; }
    public readonly long Length { get; }

    public void Deconstruct(out long offset, out long length)
    {
        offset = Offset;
        length = Length;
    }
}
