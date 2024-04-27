using FlashDictionary.Util;
using System;
using System.Collections.Generic;

namespace FlashDictionary.Core.Dictionary;

internal class KeyComparer : IComparer<string>
{
  /// <inheritdoc cref="IComparer{T}.Compare"/>"
  public static int StaticCompare(string? x, string? y)
  {
    if (x == null)
    {
      return y == null ? 0 : -1;
    }
    else if (y == null)
    {
      return 1;
    }

    var loweredX = x.ToLower();
    var loweredY = y.ToLower();

    for (int i = 0; i < Math.Min(x.Length, y.Length); i++)
    {
      if ((loweredX[i]).IsSmallAlphabet())
      {
        if (loweredY[i].IsSmallAlphabet() == false)
        {
          return 1;
        }
      }
      else if (loweredY[i].IsSmallAlphabet())
      {
        return -1;
      }

      if (loweredX[i] < loweredY[i])
      {
        return -1;
      }
      else if (loweredX[i] > loweredY[i])
      {
        return 1;
      }
    }
    return x.CompareTo(y);
  }

  public int Compare(string? x, string? y)
  {
    return StaticCompare(x, y);
  }
}
