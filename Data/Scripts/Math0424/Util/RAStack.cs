using System;
using System.Collections.Generic;
using System.Text;

namespace AnimationEngine.Utils
{
    internal class RAStack<T> : List<T>
    {
        public void Set(int i, T t)
        {
            this[Count - i - 1] = t;
        }

        public void Push(T t)
        {
            Add(t);
        }

        public T Pop(int i)
        {
            T val = this[Count - 1];
            if (i > 1)
            {
                RemoveRange(Count - i, i);
            } 
            else
            {
                RemoveAt(Count - 1);
            }
            return val;
        }

        public T Peek(int i)
        {
            return this[Count - i - 1];
        }

    }
}
