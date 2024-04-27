using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace FlashDictionary.Core.Dictionary;

[DebuggerDisplay("{(Second.HasValue ? First.ToString() + Second.Value.ToString() : First.ToString())}")]
internal readonly struct CharPair : IComparable
{
  [Obsolete("Default constructor is not supported.", true)]
  [ExcludeFromCodeCoverage]
  [EditorBrowsable(EditorBrowsableState.Never)]
  public CharPair()
  {
    throw new NotSupportedException();
  }
  public CharPair(char first, char second)
  {
    First = char.ToLower(first);
    Second = char.ToLower(second);
  }
  public CharPair(char first)
  {
    First = char.ToLower(first);
  }
  /// <summary>
  /// If <paramref name="string"/> is a single character, <see cref="Second"/> will be <see langword="null"/>, otherwise the first character of <paramref name="string"/> will be <see cref="First"/> and the second character will be <see cref="Second"/>.
  /// </summary>
  /// <param name="string"><see cref="CharPair"/> creation source string.</param>
  /// <exception cref="ArgumentNullException">Thrown when <paramref name="string"/> is <see langword="null"/> or empty.</exception>
  public CharPair(string @string)
  {
    if (string.IsNullOrEmpty(@string))
      throw new ArgumentNullException(nameof(@string));

    if (@string.Length == 1)
    {
      First = char.ToLower(@string[0]);
    }
    else
    {
      First = char.ToLower(@string[0]);
      Second = char.ToLower(@string[1]);
    }
  }

  public readonly char First { get; }
  public readonly char? Second { get; } = null;

  public override readonly string ToString()
  {
    if (Second.HasValue)
      return new string([First, Second.Value]);
    else
      return First.ToString();
  }

  public static bool operator ==(CharPair arg1, CharPair arg2)
  {
    return arg1.First == arg2.First && arg1.Second == arg2.Second;
  }
  public static bool operator !=(CharPair arg1, CharPair arg2)
  {
    return arg1.First != arg2.First || arg1.Second != arg2.Second;
  }
  public static bool operator ==(CharPair charPair, string @string)
  {
    return @string.Length == 2 && charPair.First == @string[0] && charPair.Second == @string[1];
  }
  public static bool operator !=(CharPair charPair, string @string)
  {
    return @string.Length != 2 || charPair.First != @string[0] || charPair.Second != @string[1];
  }

  public bool Equals(CharPair obj)
  {
    return this == obj;
  }
  public bool Equals(CharPair? obj)
  {
    return this == obj;
  }
  public override bool Equals(object? obj)
  {
    return obj is CharPair charPair && this == charPair;
  }

  public override int GetHashCode()
  {
    if (Second.HasValue)
      return HashCode.Combine(First, Second.Value);
    else
      return First.GetHashCode();
  }

  public int CompareTo(CharPair charPair)
  {
    if (Second.HasValue)
    {
      if (First != charPair.First)
      {
        return First.CompareTo(charPair.First);
      }
      return Second.Value.CompareTo(charPair.Second);
    }
    else
    {
      if (charPair.Second.HasValue && First == charPair.First)
        return -1;
      else
        return First.CompareTo(charPair.First);
    }
  }
  public int CompareTo(object? obj)
  {
    if (obj == null)
    {
      return 1;
    }

    if (obj is CharPair otherPair)
    {
      return CompareTo(otherPair);
    }

    throw new ArgumentException("Object must be of type CharPair.");
  }
}
