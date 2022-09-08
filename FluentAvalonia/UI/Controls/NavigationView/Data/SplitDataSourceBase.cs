using Avalonia.Collections;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace FluentAvalonia.UI.Controls;

internal abstract class SplitDataSourceBase<T, TVectorID, AttachedDataType>
{
    public SplitDataSourceBase(int vectorIdSize)
    {
        splitVectors = new SplitVector<T, TVectorID>[vectorIdSize];
    }

    public TVectorID GetVectorIDForItem(int index)
    {
        Debug.Assert(index >= 0 && index < RawDataSize);
        return flags[index];
    }

    public AttachedDataType AttachedData(int index)
    {
        Debug.Assert(index >= 0 && index < RawDataSize);
        return _attachedData[index];
    }

    public void AttachedData(int index, AttachedDataType attachedData)
    {
        Debug.Assert(index >= 0 && index < RawDataSize);
        _attachedData[index] = attachedData;
    }

    public void ResetAttachedData(AttachedDataType attachedData)
    {
        for (int i = 0; i < RawDataSize; i++)
        {
            _attachedData[i] = attachedData;
        }
    }

    public SplitVector<T, TVectorID> GetVectorForItem(int index)
    {
        if (index >= 0 && index < RawDataSize)
        {
            return splitVectors[(int)(object)flags[index]];
        }
        return null;
    }

    public void MoveItemsToVector(TVectorID newVectorID)
    {
        MoveItemsToVector(0, RawDataSize, newVectorID);
    }

    public void MoveItemsToVector(int start, int end, TVectorID newVectorID)
    {
        Debug.Assert(start >= 0 && end <= RawDataSize);
        for (int i = start; i < end; i++)
        {
            MoveItemToVector(i, newVectorID);
        }
    }

    public void MoveItemToVector(int index, TVectorID newVectorID)
    {
        Debug.Assert(index >= 0 && index < RawDataSize);
        if (!flags[index].Equals(newVectorID))
        {
            // remove from the old vector
            var sv = GetVectorForItem(index);
            if (sv != null)
                sv.RemoveAt(index);

            // change flag
            flags[index] = newVectorID;

            // insert item to vector which matches with the newVectorID
            var toVector = splitVectors[(int)(object)newVectorID];
            if (toVector != null)
            {
                int pos = GetPreferIndex(index, newVectorID);
                var value = GetAt(index);
                toVector.InsertAt(pos, index, value);
            }
        }
    }

    public abstract int IndexOf(T value);
    public abstract T GetAt(int index);
    public abstract int Size { get; }
    protected abstract TVectorID DefaultVectorIDOnInsert { get; }
    protected abstract AttachedDataType DefaultAttachedData { get; }

    protected int IndexOfImpl(T value, TVectorID vectorID)
    {
        int indexInOriginalVector = IndexOf(value);
        int index = -1;
        if (indexInOriginalVector != -1)
        {
            var vector = GetVectorForItem(indexInOriginalVector);
            if (vector != null && vector.GetVectorIDForItem().Equals(vectorID))
            {
                index = vector.IndexFromIndexInOriginalVector(indexInOriginalVector);
            }
        }
        return index;
    }

    protected void InitializeSplitVectors(params SplitVector<T, TVectorID>[] vectors)
    {
        foreach (var vector in vectors)
        {
            splitVectors[(int)(object)vector.GetVectorIDForItem()] = vector;
        }
    }

    protected SplitVector<T, TVectorID> GetVector(TVectorID vectorID)
    {
        return splitVectors[(int)(object)vectorID];
    }

    protected void OnClear()
    {
        // Clear all vectors
        foreach (var vector in splitVectors)
        {
            if (vector != null)
            {
                vector.Clear();
            }
        }
        flags.Clear();
        _attachedData.Clear();
    }

    protected void OnRemoveAt(int startIndex, int count)
    {
        for (int i = startIndex + count - 1; i >= startIndex; i--)
        {
            OnRemoveAt(i);
        }
    }

    protected void OnInsertAt(int startIndex, int count)
    {
        for (int i = startIndex; i < startIndex + count; i++)
        {
            OnInsertAt(i);
        }
    }

    protected int RawDataSize => flags.Count;

    protected void SyncAndInitVectorFlagsWithID(TVectorID defaultID, AttachedDataType defaultAttachedData)
    {
        // Initialize the flags
        for (int i = 0; i < Size; i++)
        {
            flags.Add(defaultID);
            _attachedData.Add(defaultAttachedData);
        }
    }

    protected void Clear()
    {
        OnClear();
    }

    private void OnRemoveAt(int index)
    {
        var vectorID = flags[index];

        // Update mapping on all Vectors and Remove Item on vectorID vector;
        foreach (var vector in splitVectors)
        {
            if (vector != null)
            {
                vector.OnRawDataRemove(index, vectorID);
            }
        }
        flags.RemoveAt(index);
        _attachedData.RemoveAt(index);
    }

    private void OnReplace(int index)
    {
        var splitVector = GetVectorForItem(index);
        if (splitVector != null)
        {
            var value = GetAt(index);
            splitVector.Replace(index, value);
        }
    }

    private void OnInsertAt(int index)
    {
        var vectorID = DefaultVectorIDOnInsert;
        var defaultAttachedData = DefaultAttachedData;
        var preferIndex = GetPreferIndex(index, vectorID);
        var data = GetAt(index);

        // Update mapping on all Vectors and Insert Item on vectorID vector;
        foreach (var vector in splitVectors)
        {
            if (vector != null)
            {
                vector.OnRawDataInsert(preferIndex, index, data, vectorID);
            }
        }

        flags.Insert(index, vectorID);
        _attachedData.Insert(index, defaultAttachedData);
    }

    private int GetPreferIndex(int index, TVectorID vectorID)
    {
        return RangeCount(0, index, vectorID);
    }

    private int RangeCount(int start, int end, TVectorID vectorID)
    {
        int count = 0;
        for (int i = start; i < end; i++)
        {
            if (flags[i].Equals(vectorID))
            {
                count++;
            }
        }
        return count;
    }

    // length is the same as data source, and used to identify which SplitVector it belongs to.
    List<TVectorID> flags = new List<TVectorID>();
    List<AttachedDataType> _attachedData = new List<AttachedDataType>();
    SplitVector<T, TVectorID>[] splitVectors;
}

internal class SplitVector<T, TVectorId>
{
    public SplitVector(TVectorId id, Func<T, int> indexOfFunction)
    {
        _vectorID = id;
        indexFunctionFromDataSource = indexOfFunction;

        vector = new AvaloniaList<T>();
    }

    public TVectorId GetVectorIDForItem()
    {
        return _vectorID;
    }

    public IList<T> Vector => vector;

    public void OnRawDataRemove(int indexInOriginalVector, TVectorId vectorID)
    {
        if (_vectorID.Equals(vectorID))
        {
            RemoveAt(indexInOriginalVector);
        }

        for (int i = 0; i < indicesInOriginalVector.Count; i++)
        {
            if (indicesInOriginalVector[i] > indexInOriginalVector)
            {
                indicesInOriginalVector[i]--;
            }
        }
    }

    public void OnRawDataInsert(int preferIndex, int indexInOriginalVector, T value, TVectorId vectorID)
    {
        for (int i = 0; i < indicesInOriginalVector.Count; i++)
        {
            if (indicesInOriginalVector[i] >= indexInOriginalVector) // WinUI #5558
            {
                indicesInOriginalVector[i]++;
            }
        }
        if (_vectorID.Equals(vectorID))
        {
            InsertAt(preferIndex, indexInOriginalVector, value);
        }
    }

    public void InsertAt(int preferIndex, int indexInOriginalVector, T value)
    {
        Debug.Assert(preferIndex >= 0);
        Debug.Assert(indexInOriginalVector >= 0);
        vector.Insert(preferIndex, value);
        indicesInOriginalVector.Insert(preferIndex, indexInOriginalVector);
    }

    public void Replace(int indexInOriginalVector, T value)
    {
        Debug.Assert(indexInOriginalVector >= 0);

        var index = IndexFromIndexInOriginalVector(indexInOriginalVector);
        var vect = vector;
        vect.RemoveAt(index);
        vect.Insert(index, value);
    }

    public void Clear()
    {
        vector.Clear();
        indicesInOriginalVector.Clear();
    }

    public void RemoveAt(int indexInOriginalVector)
    {
        Debug.Assert(indexInOriginalVector >= 0);
        var index = IndexFromIndexInOriginalVector(indexInOriginalVector);
        Debug.Assert(index < indicesInOriginalVector.Count);
        vector.RemoveAt(index);
        indicesInOriginalVector.RemoveAt(index);
    }

    public int IndexOf(T value)
    {
        int indexInOriginalVector = indexFunctionFromDataSource(value);
        return IndexFromIndexInOriginalVector(indexInOriginalVector);
    }

    public int IndexToIndexInOriginalVector(int index)
    {
        Debug.Assert(index >= 0 && index < Size);
        return indicesInOriginalVector[index];
    }

    public int IndexFromIndexInOriginalVector(int indexInOriginalVector)
    {
        var pos = indicesInOriginalVector.IndexOf(indexInOriginalVector);
        if (pos != -1)
        {
            return pos;
        }
        return pos;
    }

    public int Size => indicesInOriginalVector.Count;

    TVectorId _vectorID;
    IList<T> vector;
    List<int> indicesInOriginalVector = new List<int>();
    Func<T, int> indexFunctionFromDataSource;
}
