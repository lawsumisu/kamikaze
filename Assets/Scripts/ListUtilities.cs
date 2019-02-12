using System.Collections.Generic;
using UnityEngine;

namespace ListUtilities{

    /// <summary>
    /// A Data Structure designed for maintaining a set number of elements in the order that they are added.
    /// When the n+1th element is added to a list of size n, the "window" of accessible elements is shifted
    /// by 1, resulting in the removal of the 1st element. This window-shift occurs in O(1) time.
    /// This also means that (for a full array) the least recently added element is always at index
    /// "0" and the most recently added element is always at index "size-1".
    /// </summary>
    public class WindowArray<T> {
        private T[] array;
        private int windowIndex = 0;
        private int size;

        //Constructors
        public WindowArray(int size) {
            this.size = size;
            array = new T[size];

        }
        public WindowArray(T[] array) {
            this.array = array;
            size = array.Length;
        }
        //Methods
        public void Add(T item) {
            array[windowIndex % size] = item;
            windowIndex++;

        }

        /// <summary>
        /// Gets the element that was most recently added to this array.
        /// </summary>
        /// <returns>The last.</returns>
        public T GetLast() {
            return this[size - 1];
        }
        //Extra methods to make use similar to arrays.
        public static implicit operator WindowArray<T>(T[] array) {
            return new WindowArray<T>(array);
        }
        public List<T> ToList() {
            List<T> output = new List<T>();
            for (int i = 0; i < size; i++) {
                output.Add(this[i]);
            }
            return output;
        }
        public T this[int i] {
            get {
                int ii = i % size;
                if (windowIndex < size) {
                    return array[(ii + size) % size];
                }
                else {
                    return array[(windowIndex + ii + size) % size];
                }
            }
            set {
                int ii = i % size;
                if (windowIndex < size) {
                    array[(ii + size) % size] = value;
                }
                else {
                    array[(windowIndex + ii + size) % size] = value;
                }
            }
        }
        public int Length {
            get { return Mathf.Min(windowIndex, size); }
        }

        public int MaxSize {
            get { return size; }
        }

        public void Fill(T item) {
            for (int i = 0; i < size; i++) {
                Add(item);
            }
        }
    }

}
