using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;

namespace FlashDictionary.Util.Collections;

class WebSortedDictionary
{
  public class SortedDictionary<TKey, TValue> : IEnumerable<KeyValuePair<TKey, TValue>> where TKey : struct, IComparable<TKey>
  {
    private const bool ColorRed = true;
    private const bool ColorBlack = false;

    public enum TreeRotation : byte
    {
      Left,
      LeftRight,
      Right,
      RightLeft
    }

    private Node? root;

    private int count;
    private int version;

    public bool Reverse { get; }

    public SortedDictionary()
    {
      Reverse = false;
    }
    public SortedDictionary(bool reverse)
    {
      Reverse = reverse;
    }

    public int Count => count;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int Compare(TKey item1, TKey item2)
    {
      return Reverse ? item2.CompareTo(item1) : item1.CompareTo(item2);
    }


    public bool Add(TKey key, TValue value) => AddCore(key, value, false);
    public bool AddOrChangeValue(TKey key, TValue value) => AddCore(key, value, true);

    public bool AddCore(TKey key, TValue value, bool changeMode)
    {
      if (root == null)
      {
        // The tree is empty and this is the first item.
        root = new Node(key, value, ColorBlack);
        count = 1;
        version++;
        return true;
      }

      // Search for a node at bottom to insert the new node.
      // If we can guarantee the node we found is not a 4-node, it would be easy to do insertion.
      // We split 4-nodes along the search path.
      Node? current = root;
      Node? parent = null;
      Node? grandParent = null;
      Node? greatGrandParent = null;

      // Even if we don't actually add to the set, we may be altering its structure (by doing rotations and such).
      // So update `_version` to disable any instances of Enumerator/TreeSubSet from working on it.
      version++;

      int order = 0;
      while (current != null)
      {
        order = Compare(key, current.Key);
        if (order == 0)
        {
          // We could have changed root node to red during the search process.
          // We need to set it to black before we return.
          root.SetBlack();
          if (changeMode)
          {
            current.Value = value;
            return true;
          }
          else
          {
            return false;
          }
        }

        // Split a 4-node into two 2-nodes.
        if (current.Is4Node)
        {
          current.Split4Node();
          // We could have introduced two consecutive red nodes after split. Fix that by rotation.
          if (Node.IsNonNullRed(parent))
          {
            InsertionBalance(current, ref parent!, grandParent!, greatGrandParent!);
          }
        }

        greatGrandParent = grandParent;
        grandParent = parent;
        parent = current;
        current = (order < 0) ? current.Left : current.Right;
      }

      // We're ready to insert the new node.
      Node node = new(key, value, ColorRed);
      if (order > 0)
      {
        parent!.Right = node;
      }
      else
      {
        parent!.Left = node;
      }

      // The new node will be red, so we will need to adjust colors if its parent is also red.
      if (parent.IsRed)
      {
        InsertionBalance(node, ref parent!, grandParent!, greatGrandParent!);
      }

      // The root node is always black.
      root.SetBlack();
      ++count;
      return true;
    }

    public void RemoveOrUnder(TKey orUnder)
    {
      while (true)
      {
        Node? current = root;
        if (current == null)
          return;
        while (current.Left != null)
        {
          current = current.Left;
        }
        var firstKey = current.Key;

        int order = Compare(orUnder, firstKey);
        if (order < 0)
          return;

        Remove(firstKey);
      }
    }


    public bool Remove(TKey key)
    {
      if (root == null)
        return false;

      // Search for a node and then find its successor.
      // Then copy the item from the successor to the matching node, and delete the successor.
      // If a node doesn't have a successor, we can replace it with its left child (if not empty),
      // or delete the matching node.
      //
      // In top-down implementation, it is important to make sure the node to be deleted is not a 2-node.
      // Following code will make sure the node on the path is not a 2-node.

      // Even if we don't actually remove from the set, we may be altering its structure (by doing rotations
      // and such). So update our version to disable any enumerators/subsets working on it.
      version++;

      Node? current = root;
      Node? parent = null;
      Node? grandParent = null;
      Node? match = null;
      Node? parentOfMatch = null;
      bool foundMatch = false;
      while (current != null)
      {
        if (current.Is2Node)
        {
          // Fix up 2-node
          if (parent == null)
          {
            // `current` is the root. Mark it red.
            current.SetRed();
          }
          else
          {
            Node sibling = parent.GetSibling(current);
            if (sibling.IsRed)
            {
              // If parent is a 3-node, flip the orientation of the red link.
              // We can achieve this by a single rotation.
              // This case is converted to one of the other cases below.
              if (parent.Right == sibling)
              {
                parent.RotateLeft();
              }
              else
              {
                parent.RotateRight();
              }

              parent.SetRed();
              sibling.SetBlack(); // The red parent can't have black children.
                                  // `sibling` becomes the child of `grandParent` or `root` after rotation. Update the link from that node.
              ReplaceChildOrRoot(grandParent, parent, sibling);
              // `sibling` will become the grandparent of `current`.
              grandParent = sibling;
              if (parent == match)
              {
                parentOfMatch = sibling;
              }

              sibling = parent.GetSibling(current);
            }

            if (sibling.Is2Node)
            {
              parent.Merge2Nodes();
            }
            else
            {
              // `current` is a 2-node and `sibling` is either a 3-node or a 4-node.
              // We can change the color of `current` to red by some rotation.
              Node newGrandParent = parent.Rotate(parent.GetRotation(current, sibling))!;

              newGrandParent.Color = parent.Color;
              parent.SetBlack();
              current.SetRed();

              ReplaceChildOrRoot(grandParent, parent, newGrandParent);
              if (parent == match)
              {
                parentOfMatch = newGrandParent;
              }
            }
          }
        }

        // We don't need to compare after we find the match.
        int order = foundMatch ? -1 : Compare(key, current.Key);
        if (order == 0)
        {
          // Save the matching node.
          foundMatch = true;
          match = current;
          parentOfMatch = parent;
        }

        grandParent = parent;
        parent = current;
        // If we found a match, continue the search in the right sub-tree.
        current = order < 0 ? current.Left : current.Right;
      }

      // Move successor to the matching node position and replace links.
      if (match != null)
      {
        ReplaceNode(match, parentOfMatch!, parent!, grandParent!);
        --count;
      }

      root?.SetBlack();
      return foundMatch;
    }

    public Enumerator GetEnumerator() => new(this);

    IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator() => GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    private void InsertionBalance(Node current, ref Node parent, Node grandParent, Node greatGrandParent)
    {
      ArgumentNullException.ThrowIfNull(parent);
      ArgumentNullException.ThrowIfNull(grandParent);

      bool parentIsOnRight = grandParent.Right == parent;
      bool currentIsOnRight = parent.Right == current;

      Node newChildOfGreatGrandParent;
      if (parentIsOnRight == currentIsOnRight)
      {
        // Same orientation, single rotation
        newChildOfGreatGrandParent = currentIsOnRight ? grandParent.RotateLeft() : grandParent.RotateRight();
      }
      else
      {
        // Different orientation, double rotation
        newChildOfGreatGrandParent = currentIsOnRight ? grandParent.RotateLeftRight() : grandParent.RotateRightLeft();
        // Current node now becomes the child of `greatGrandParent`
        parent = greatGrandParent;
      }

      // `grandParent` will become a child of either `parent` of `current`.
      grandParent.SetRed();
      newChildOfGreatGrandParent.SetBlack();

      ReplaceChildOrRoot(greatGrandParent, grandParent, newChildOfGreatGrandParent);
    }

    private void ReplaceChildOrRoot(Node? parent, Node child, Node newChild)
    {
      if (parent != null)
      {
        parent.ReplaceChild(child, newChild);
      }
      else
      {
        root = newChild;
      }
    }

    private void ReplaceNode(Node match, Node parentOfMatch, Node successor, Node parentOfSuccessor)
    {
      ArgumentNullException.ThrowIfNull(match);

      if (successor == match)
      {
        // This node has no successor. This can only happen if the right child of the match is null.
        successor = match.Left!;
      }
      else
      {
        ArgumentNullException.ThrowIfNull(parentOfSuccessor);

        successor.Right?.SetBlack();

        if (parentOfSuccessor != match)
        {
          // Detach the successor from its parent and set its right child.
          parentOfSuccessor.Left = successor.Right;
          successor.Right = match.Right;
        }

        successor.Left = match.Left;
      }

      if (successor != null)
      {
        successor.Color = match.Color;
      }

      ReplaceChildOrRoot(parentOfMatch, match, successor!);
    }

    public TKey GetFirstKey()
    {
      if (root == null)
        return default;

      Node current = root;
      while (current.Left != null)
      {
        current = current.Left;
      }

      return current.Key;
    }

    public TKey GetLastKey()
    {
      if (root == null)
        return default;

      Node current = root;
      while (current.Right != null)
      {
        current = current.Right;
      }

      return current.Key;
    }

    public static int Log2(int value) => BitOperations.Log2((uint)value);


    public sealed class Node(TKey key, TValue value, bool isRed)
    {
      public static bool IsNonNullBlack(Node? node) => node != null && node.IsBlack;

      public static bool IsNonNullRed(Node? node) => node != null && node.IsRed;

      public static bool IsNullOrBlack(Node? node) => node == null || node.IsBlack;

      public TKey Key { get; set; } = key;

      public TValue Value { get; set; } = value;

      public Node? Left { get; set; }

      public Node? Right { get; set; }

      public bool Color { get; set; } = isRed;

      public bool IsBlack => !Color;

      public bool IsRed => Color;

      public bool Is2Node => IsBlack && IsNullOrBlack(Left) && IsNullOrBlack(Right);

      public bool Is4Node => IsNonNullRed(Left) && IsNonNullRed(Right);

      public void SetBlack() => Color = ColorBlack;

      public void SetRed() => Color = ColorRed;



      public TreeRotation GetRotation(Node current, Node sibling)
      {
        bool currentIsLeftChild = Left == current;
        return IsNonNullRed(sibling.Left) ?
            (currentIsLeftChild ? TreeRotation.RightLeft : TreeRotation.Right) :
            (currentIsLeftChild ? TreeRotation.Left : TreeRotation.LeftRight);
      }

      public Node GetSibling(Node node)
      {
        return node == Left ? Right! : Left!;
      }
      public void Split4Node()
      {
        SetRed();
        Left!.SetBlack();
        Right!.SetBlack();
      }

      public Node? Rotate(TreeRotation rotation)
      {
        Node removeRed;
        switch (rotation)
        {
          case TreeRotation.Right:
            removeRed = Left!.Left!;
            removeRed.SetBlack();
            return RotateRight();
          case TreeRotation.Left:
            removeRed = Right!.Right!;
            removeRed.SetBlack();
            return RotateLeft();
          case TreeRotation.RightLeft:
            return RotateRightLeft();
          case TreeRotation.LeftRight:
            return RotateLeftRight();
          default:
            return null;
        }
      }
      public Node RotateLeft()
      {
        Node child = Right!;
        Right = child.Left;
        child.Left = this;
        return child;
      }

      public Node RotateLeftRight()
      {
        Node child = Left!;
        Node grandChild = child.Right!;

        Left = grandChild.Right;
        grandChild.Right = this;
        child.Right = grandChild.Left;
        grandChild.Left = child;
        return grandChild;
      }

      public Node RotateRight()
      {
        Node child = Left!;
        Left = child.Right;
        child.Right = this;
        return child;
      }

      public Node RotateRightLeft()
      {
        Node child = Right!;
        Node grandChild = child.Left!;

        Right = grandChild.Left;
        grandChild.Left = this;
        child.Left = grandChild.Right;
        grandChild.Right = child;
        return grandChild;
      }

      public void Merge2Nodes()
      {
        // Combine two 2-nodes into a 4-node.
        SetBlack();
        Left!.SetRed();
        Right!.SetRed();
      }

      public void ReplaceChild(Node child, Node newChild)
      {
        if (Left == child)
        {
          Left = newChild;
        }
        else
        {
          Right = newChild;
        }
      }
    }

    public struct Enumerator : IEnumerator<KeyValuePair<TKey, TValue>>, IEnumerator, ISerializable, IDeserializationCallback, IEquatable<Enumerator>
    {
      private readonly SortedDictionary<TKey, TValue> _tree;
      private readonly int _version;

      private readonly Stack<Node> _stack;
      private Node? _current;

      public Enumerator(SortedDictionary<TKey, TValue> orderboard)
      {
        _tree = orderboard;
        _version = orderboard.version;

        // 2 log(n + 1) is the maximum height.
        _stack = new Stack<Node>(2 * Log2(orderboard.Count + 1));
        _current = null;

        Initialize();
      }

      void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
      {
        throw new PlatformNotSupportedException();
      }

      void IDeserializationCallback.OnDeserialization(object? sender)
      {
        throw new PlatformNotSupportedException();
      }


      private void Initialize()
      {
        _current = null;
        Node? node = _tree.root;
        while (node != null)
        {
          _stack.Push(node);
          node = node.Left;
        }
      }

      public bool MoveNext()
      {
        // Make sure that the underlying subset has not been changed since

        if (_version != _tree.version)
        {
          throw new InvalidOperationException("SR.InvalidOperation_EnumFailedVersion");
        }

        if (_stack.Count == 0)
        {
          _current = null;
          return false;
        }

        _current = _stack.Pop();
        Node? node = _current.Right;
        while (node != null)
        {
          _stack.Push(node);
          node = node.Left;
        }
        return true;
      }

      public readonly void Dispose() { }

      public readonly KeyValuePair<TKey, TValue> Current
      {
        get
        {
          if (_current != null)
          {
            return new KeyValuePair<TKey, TValue>(_current.Key, _current.Value);
          }
          return default; // Should only happen when accessing Current is undefined behavior
        }
      }

      readonly object? IEnumerator.Current
      {
        get
        {
          if (_current == null)
          {
            throw new InvalidOperationException("SR.InvalidOperation_EnumOpCantHappen");
          }

          return _current.Key;
        }
      }

      internal readonly bool NotStartedOrEnded => _current == null;

      internal void Reset()
      {
        if (_version != _tree.version)
        {
          throw new InvalidOperationException("SR.InvalidOperation_EnumFailedVersion");
        }

        _stack.Clear();
        Initialize();
      }

      void IEnumerator.Reset() => Reset();

      bool IEquatable<SortedDictionary<TKey, TValue>.Enumerator>.Equals(SortedDictionary<TKey, TValue>.Enumerator other)
      {
        throw new NotImplementedException();
      }
    }
  }
}
