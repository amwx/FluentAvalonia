using System;
using System.Collections.Generic;
using System.Linq;

namespace FluentAvalonia.UI.Controls;

public struct IndexPath : IComparable<IndexPath>, IEquatable<IndexPath>
{
    public static readonly IndexPath Unselected = default;

    public IndexPath(int index)
    {
        _path = new List<int>();
        _path.Add(index);
    }

    public IndexPath(int groupIndex, int itemIndex)
    {
        _path = new List<int>();
        _path.Add(groupIndex);
        _path.Add(itemIndex);
    }

    public IndexPath(IEnumerable<int> indices)
    {
        _path = new List<int>();
        if (indices != null)
        {
            for (int i = 0; i < indices.Count(); i++)
            {
                _path.Add(indices.ElementAt(i));
            }
        }
    }

    public static IndexPath CreateFrom(int index)
    {
        return new IndexPath(index);
    }

    public static IndexPath CreateFrom(int groupIndex, int itemIndex)
    {
        return new IndexPath(groupIndex, itemIndex);
    }

    public static IndexPath CreateFromIndices(IList<int> indices)
    {
        return new IndexPath(indices);
    }

    public int GetSize() => _path?.Count ?? 0;

    public int GetAt(int index) => _path?[index] ?? throw new IndexOutOfRangeException();

    public int CompareTo(IndexPath rhs)
    {
        int compareResult = 0;
        int lhsCount = GetSize();
        int rhsCount = rhs.GetSize();

        if (lhsCount == 0 || rhsCount == 0)
        {
            // one of the paths are empty, compare based on size
            compareResult = (lhsCount - rhsCount);
        }
        else
        {
            // both paths are non-empty, but can be of different size
            for (int i = 0; i < Math.Min(lhsCount, rhsCount); i++)
            {
                if (_path[i] < rhs._path[i])
                {
                    compareResult = -1;
                    break;
                }
                else if (_path[i] > rhs._path[i])
                {
                    compareResult = 1;
                    break;
                }
            }

            // if both match upto min(lhs...), compare based on size
            compareResult = compareResult == 0 ? (lhsCount - rhsCount) : compareResult;
        }

        if (compareResult != 0)
        {
            compareResult = compareResult > 0 ? 1 : -1;
        }

        return compareResult;
    }

    public override string ToString()
    {
        string result = "R";
        foreach (var index in _path)
        {
            result += $".{index}";
        }

        return result;
    }

    public bool IsValid()
    {
        for (int i = 0; i < _path.Count; i++)
        {
            if (_path[i] < 0)
            {
                return false;
            }
        }
        return true;
    }

    public IndexPath CloneWithChildIndex(int childIndex)
    {
        var newPath = new List<int>(_path);
        newPath.Add(childIndex);
        return new IndexPath(newPath);
    }

    public override int GetHashCode()
    {
        //adapted from original IndexPath Avalonia port
        var hashCode = -504981047;

        foreach (var i in _path)
            hashCode = hashCode * -1521134295 + i.GetHashCode();

        return hashCode;
    }

    public override bool Equals(object obj) => obj is IndexPath other && Equals(other);

    public bool Equals(IndexPath other) => CompareTo(other) == 0;

    //From original port
    public static bool operator <(IndexPath x, IndexPath y) { return x.CompareTo(y) < 0; }
    public static bool operator >(IndexPath x, IndexPath y) { return x.CompareTo(y) > 0; }
    public static bool operator <=(IndexPath x, IndexPath y) { return x.CompareTo(y) <= 0; }
    public static bool operator >=(IndexPath x, IndexPath y) { return x.CompareTo(y) >= 0; }
    public static bool operator ==(IndexPath x, IndexPath y) { return x.CompareTo(y) == 0; }
    public static bool operator !=(IndexPath x, IndexPath y) { return x.CompareTo(y) != 0; }
    public static bool operator ==(IndexPath? x, IndexPath? y) { return (x ?? default).CompareTo(y ?? default) == 0; }
    public static bool operator !=(IndexPath? x, IndexPath? y) { return (x ?? default).CompareTo(y ?? default) != 0; }

    private IList<int> _path;
}
