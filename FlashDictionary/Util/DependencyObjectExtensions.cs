using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FlashDictionary.Util;

internal static class DependencyObjectExtensions
{
  /// <summary>
  /// Get children dependency objects.
  /// </summary>
  internal static IEnumerable<DependencyObject> Children(this DependencyObject obj)
  {
    ArgumentNullException.ThrowIfNull(obj);

    var count = VisualTreeHelper.GetChildrenCount(obj);
    if (count == 0)
      yield break;

    for (int i = 0; i < count; i++)
    {
      var child = VisualTreeHelper.GetChild(obj, i);
      if (child is not null)
        yield return child;
    }
  }

  /// <summary>
  /// Get descendant dependency objects.
  /// </summary>
  internal static IEnumerable<DependencyObject> Descendants(this DependencyObject obj)
  {
    ArgumentNullException.ThrowIfNull(obj);

    foreach (var child in obj.Children())
    {
      yield return child;
      foreach (var grandChild in child.Descendants())
        yield return grandChild;
    }
  }

  /// <summary>
  /// Get children dependency objects of a specific type.
  /// </summary>
  internal static IEnumerable<T> Children<T>(this DependencyObject obj) where T : DependencyObject
  {
    return obj.Children().OfType<T>();
  }
  /// <summary>
  /// Get the first child dependency object of a specific type.
  /// </summary>
  /// <returns>If no child wa found, return <see langword="null"/>.</returns>
  internal static T? ChildrenFirstOrDefault<T>(this DependencyObject obj, Func<T, bool> predicate) where T : DependencyObject
  {
    var items = obj.Children().OfType<T>();
    foreach (var item in items)
    {
      if (predicate(item))
        return item;
    }
    return null;
  }

  /// <summary>
  /// Get descendant dependency objects of a specific type.
  /// </summary>
  internal static IEnumerable<T> Descendants<T>(this DependencyObject obj) where T : DependencyObject
  {
    return obj.Descendants().OfType<T>();
  }
  /// <summary>
  /// Get the first descendant dependency object of a specific type.
  /// </summary>
  /// <returns>If no descendant was found, return <see langword="null"/>.</returns>
  internal static T? DescendantsFirstOrDefault<T>(this DependencyObject obj) where T : DependencyObject
  {
    var items = obj.Descendants().OfType<T>();
    if (items is null || !items.Any())
      return null;
    return items.First();
  }
  /// <summary>
  /// Get the first descendant dependency object of a specific type that satisfies the condition.
  /// </summary>
  /// <returns>If no descendant was found, return <see langword="null"/>.</returns>
  internal static T? DescendantsFirstOrDefault<T>(this DependencyObject obj, Func<T, bool> predicate) where T : DependencyObject
  {
    var items = obj.Descendants().OfType<T>();
    if (items is null)
      return null;
    foreach (var item in items)
    {
      if (predicate(item))
        return item;
    }
    return null;
  }
}
