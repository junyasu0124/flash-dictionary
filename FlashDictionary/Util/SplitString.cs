using System;
using System.Runtime.CompilerServices;

namespace FlashDictionary.Util;

internal static class Split
{
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool TrySplitIntoTwo(string source, char separator, out ReadOnlySpan<char> first, out ReadOnlySpan<char> second)
  {
    var index = source.IndexOf(separator, StringComparison.Ordinal);
    if (index == -1)
    {
      first = [];
      second = [];
      return false;
    }
    first = source.AsSpan(0, index);
    second = source.AsSpan(index + 1);
    return true;
  }
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool TrySplitIntoTwo(string source, char separator, out ReadOnlySpan<char> first, out string second)
  {
    var index = source.IndexOf(separator, StringComparison.Ordinal);
    if (index == -1)
    {
      first = [];
      second = string.Empty;
      return false;
    }
    first = source.AsSpan(0, index);
    second = source[(index + 1)..];
    return true;
  }
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool TrySplitIntoTwo(string source, char separator, out string first, out ReadOnlySpan<char> second)
  {
    var index = source.IndexOf(separator, StringComparison.Ordinal);
    if (index == -1)
    {
      first = string.Empty;
      second = [];
      return false;
    }
    first = source[..index];
    second = source.AsSpan(index + 1);
    return true;
  }
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool TrySplitIntoTwo(string source, char separator, out string first, out string second)
  {
    var index = source.IndexOf(separator, StringComparison.Ordinal);
    if (index == -1)
    {
      first = string.Empty;
      second = string.Empty;
      return false;
    }
    first = source[..index];
    second = source[(index + 1)..];
    return true;
  }
}
