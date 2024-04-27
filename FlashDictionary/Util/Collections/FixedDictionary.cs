using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace FlashDictionary.Util.Collections;

/// <summary>
/// A dictionary with limited capacity. When the capacity is exceeded, the oldest element is removed.
/// </summary>
/// <param name="capacity">A capacity of the dictionary.</param>
internal class FixedDictionary<TKey, TValue>(int capacity) where TKey : notnull
{
  public int Capacity { get; } = capacity;
  private readonly SortedDictionary<TKey, LinkedListNode<KeyValuePair<TKey, TValue>>> dictionary = [];
  private readonly LinkedList<KeyValuePair<TKey, TValue>> linkedList = new();

  public void Add(TKey key, TValue value)
  {
    if (dictionary.ContainsKey(key))
    {
      Remove(key);
    }

    LinkedListNode<KeyValuePair<TKey, TValue>> newNode = linkedList.AddLast(new KeyValuePair<TKey, TValue>(key, value));
    dictionary.Add(key, newNode);

    if (linkedList.Count > Capacity)
    {
      RemoveOldest();
    }
  }

  public bool TryGetValue(TKey key, [NotNullWhen(true)] out TValue? value)
  {
    if (dictionary.TryGetValue(key, out var node))
    {
      value = node.Value.Value!;
      return true;
    }
    else
    {
      value = default;
      return false;
    }
  }

  public IEnumerable<KeyValuePair<TKey, TValue>> GetLatestValues(int maxCount)
  {
    LinkedListNode<KeyValuePair<TKey, TValue>>? node = linkedList.Last;
    while (node != null && maxCount-- > 0)
    {
      yield return node.Value;
      node = node.Previous;
    }
  }

  public bool Remove(TKey key)
  {
    if (dictionary.TryGetValue(key, out LinkedListNode<KeyValuePair<TKey, TValue>>? node))
    {
      dictionary.Remove(key);
      linkedList.Remove(node);
      return true;
    }
    else
    {
      return false;
    }
  }

  private void RemoveOldest()
  {
    LinkedListNode<KeyValuePair<TKey, TValue>>? oldestNode = linkedList.First;
    if (oldestNode == null)
      return;
    dictionary.Remove(oldestNode.Value.Key);
    linkedList.RemoveFirst();
  }

  public void Clear()
  {
    dictionary.Clear();
    linkedList.Clear();
  }
}
