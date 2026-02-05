using System;
using System.Collections.Generic;

class PriorityQueue<T> where T : IComparable<T>
{
    #region property

    private T[] _data;
    public int Count { get; private set; }
    public int Capacity { get; private set; }

    #endregion
    #region 생성자

    public PriorityQueue()
    {
        Count = 0;
        Capacity = 1;
        _data = new T[Capacity];
    }

    public PriorityQueue(int capacity)
    {
        Count = 0;
        Capacity = capacity;
        _data = new T[Capacity];
    }

    #endregion
    #region private func

    private void Expand()
    {
        T[] newData = new T[Capacity * 2];

        for (int i = 0; i < Count; i++)
        {
            newData[i] = _data[i];
        }

        _data = newData;
        Capacity *= 2;
    }

    private void Swap(ref T left, ref T right)
    {
        T temp = left;
        left = right;
        right = temp;
    }

    #endregion
    #region public func

    public void Enqueue(T value)
    {
        // 배열이 꽉 찼을 경우 확장
        if (Count >= Capacity)
        {
            Expand();
        }

        // 데이터 추가
        _data[Count] = value;
        Count++;

        // 데이터 정렬(힙 트리 구조 유지)
        int now = Count - 1;
        while (now > 0)
        {
            int parent = (now - 1) / 2;
            if (_data[now].CompareTo(_data[parent]) < 0)
            {
                break;
            }

            Swap(ref _data[now], ref _data[parent]);
            now = parent;
        }
    }

    public T Dequeue()
    {
        if (Count == 0)
        {
            throw new IndexOutOfRangeException();
        }

        // 루트 노드 값 추출, 마지막 노드와 교환 후 제거
        T result = _data[0];
        _data[0] = _data[Count - 1];
        _data[Count - 1] = default(T);
        Count--;

        // 데이터 정렬(힙 트리 구조 유지)
        int now = 0;
        while (now < Count)
        {
            // 힙 트리 구조의 자식 노드 인덱스 규칙
            int left = now * 2 + 1;
            int right = now * 2 + 2;

            int next = now;

            if (left < Count && _data[next].CompareTo(_data[left]) < 0)
            {
                next = left;
            }

            if (right < Count && _data[next].CompareTo(_data[right]) < 0)
            {
                next = right;
            }

            if (next == now)
            {
                break;
            }

            Swap(ref _data[now], ref _data[next]);
            now = next;
        }
        return result;
    }

    public T Peek()
    {
        if (Count == 0)
        {
            throw new IndexOutOfRangeException();
        }

        return _data[0];
    }

    #endregion
}