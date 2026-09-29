using System;

namespace SwipeClean.Cleaning
{
    /// <summary>Allocation-free FIFO queue with explicit overflow reporting.</summary>
    public sealed class BrushCommandQueue
    {
        private readonly BrushCommand[] _items;
        private int _head;
        private int _tail;

        public int Capacity => _items.Length;
        public int Count { get; private set; }

        public BrushCommandQueue(int capacity = 128)
        {
            if (capacity < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(capacity));
            }

            _items = new BrushCommand[capacity];
        }

        public bool TryEnqueue(in BrushCommand command)
        {
            if (Count == Capacity)
            {
                return false;
            }

            _items[_tail] = command;
            _tail = (_tail + 1) % Capacity;
            Count++;
            return true;
        }

        public bool TryDequeue(out BrushCommand command)
        {
            if (Count == 0)
            {
                command = default;
                return false;
            }

            command = _items[_head];
            _head = (_head + 1) % Capacity;
            Count--;
            return true;
        }

        public void Clear()
        {
            _head = 0;
            _tail = 0;
            Count = 0;
        }
    }
}

