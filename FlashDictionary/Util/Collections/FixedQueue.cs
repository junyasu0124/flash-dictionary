using System.Collections;
using System.Collections.Generic;

namespace FlashDictionary.Util.Collections;

internal class FixedQueue<T>(int capacity) : IEnumerable<T>
{
  private readonly Queue<T> queue = new(capacity);
  public int Capacity { get; private set; } = capacity;

  public void Enqueue(T item)
  {
    if (queue.Count == Capacity)
    {
      queue.Dequeue();
    }
    queue.Enqueue(item);
  }

  public T Dequeue()
  {
    return queue.Dequeue();
  }

  public Queue<T>.Enumerator GetEnumerator()
  {
    return queue.GetEnumerator();
  }
  IEnumerator<T> IEnumerable<T>.GetEnumerator()
  {
    return queue.GetEnumerator();
  }
  IEnumerator IEnumerable.GetEnumerator()
  {
    return queue.GetEnumerator();
  }
}
