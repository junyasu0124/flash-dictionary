using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace FlashDictionary.Core.Dictionary;

internal static class CharExtension
{
  public static bool IsNull(this char value)
  {
    return value == '\u0000';
  }
}

[DebuggerDisplay("{this.ToString()}")]
[StructLayout(LayoutKind.Explicit)]
internal readonly struct TripleChar : IComparable, IEquatable<TripleChar>
{
  [FieldOffset(0)]
  private readonly char first;
  public char First { get => first; }

  [FieldOffset(2)]
  private readonly char second;
  public char Second { get => second; }

  [FieldOffset(4)]
  private readonly char third;
  public char Third { get => third; }

  // To prevent memory from containing unnecessary values
  [FieldOffset(6)]
  private readonly char dummy;


  [Obsolete("Default constructor is not supported.", true)]
  [ExcludeFromCodeCoverage]
  [EditorBrowsable(EditorBrowsableState.Never)]
  public TripleChar()
  {
    throw new NotSupportedException();
  }
  public TripleChar(char first, char second, char third)
  {
    this.first = char.ToLower(first);
    this.second = char.ToLower(second);
    this.third = char.ToLower(third);
  }
  public TripleChar(char first, char second)
  {
    this.first = char.ToLower(first);
    this.second = char.ToLower(second);
  }
  public TripleChar(char first)
  {
    this.first = char.ToLower(first);
  }


  public override readonly string ToString()
  {
    if (Second.IsNull())
    {
      return First.ToString();
    }
    else if (Third.IsNull())
    {
      return new string([First, Second]);
    }
    else
    {
      return new string([First, Second, Third]);
    }
  }

  public static bool operator ==(TripleChar arg1, TripleChar arg2)
  {
    return Unsafe.As<TripleChar, long>(ref arg1) == Unsafe.As<TripleChar, long>(ref arg2);
  }
  public static bool operator !=(TripleChar arg1, TripleChar arg2)
  {
    return !(arg1 == arg2);
  }
  public static bool operator ==(TripleChar charPair, string @string)
  {
    return @string.Length == 3 && charPair.First == @string[0] && charPair.Second == @string[1] && charPair.Third == @string[2];
  }
  public static bool operator !=(TripleChar charPair, string @string)
  {
    return !(charPair == @string);
  }

  public bool Equals(TripleChar obj)
  {
    return this == obj;
  }
  public bool Equals(TripleChar? obj)
  {
    return this == obj;
  }
  public override bool Equals(object? obj)
  {
    return obj is TripleChar tripleChar && this == tripleChar;
  }

  public override int GetHashCode()
  {
    return HashCode.Combine(First, Second, Third);
  }

  public int CompareTo(TripleChar charPair)
  {
    if (this == charPair)
    {
      return 0;
    }

    if (First == charPair.First)
    {
      if (Second == charPair.Second)
      {
        return Third.CompareTo(charPair.Third);
      }
      else
      {
        return Second.CompareTo(charPair.Second);
      }
    }
    else
    {
      return First.CompareTo(charPair.First);
    }
  }
  public int CompareTo(object? obj)
  {
    if (obj == null)
    {
      return 1;
    }

    if (obj is TripleChar otherPair)
    {
      return CompareTo(otherPair);
    }

    throw new ArgumentException("Object must be of type CharPair.");
  }
}

internal class TripleCharComparer : IComparer<TripleChar>
{
  public int Compare(TripleChar x, TripleChar y)
  {
    if (x == y)
    {
      return 0;
    }

    if (x.First == y.First)
    {
      if (x.Second == y.Second)
      {
        return x.Third.CompareTo(y.Third);
      }
      else
      {
        return x.Second.CompareTo(y.Second);
      }
    }
    else
    {
      return x.First.CompareTo(y.First);
    }
  }
}

internal class TripleCharEqualityComparer : IEqualityComparer<TripleChar>
{
  public bool Equals(TripleChar x, TripleChar y)
  {
    return x == y;
  }

  public int GetHashCode([DisallowNull] TripleChar obj)
  {
    return obj.GetHashCode();
  }
}
