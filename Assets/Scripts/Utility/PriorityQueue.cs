using System;
using System.Collections.Generic;


/// <summary>
/// 基于MS的优先队列实现
/// </summary>
/// <typeparam name="T"></typeparam>
public class PriorityQueue<T> {
    // Fields
    private IComparer<T> Comparer_;
    private int Count_;
    private T[] Heap_;
    private const int DefaultCapacity = 6;

    // Methods
    public PriorityQueue( int capacity, IComparer<T> comparer ) {
        this.Heap_ = new T[(capacity > 0) ? capacity : 6];
        this.Count_ = 0;
        this.Comparer_ = comparer;
    }

    // Properties
    public int Count {
        get {
            return this.Count_;
        }
    }

    public T Top {
        get {
            return this.Heap_[0];
        }
    }

    private static int HeapLeftChild( int i ) {
        return ((i * 2) + 1);
    }

    private static int HeapParent( int i ) {
        return ((i - 1) / 2);
    }

    private static int HeapRightFromLeft( int i ) {
        return (i + 1);
    }

    public void Pop() {
        if( this.Count_ > 1 ) {
            int i = 0;
            for( int j = PriorityQueue<T>.HeapLeftChild( i ); j < this.Count_; j = PriorityQueue<T>.HeapLeftChild( i ) ) {
                int index = PriorityQueue<T>.HeapRightFromLeft( j );
                int num4 = ((index < this.Count_) && (this.Comparer_.Compare( this.Heap_[index], this.Heap_[j] ) < 0)) ? index : j;
                this.Heap_[i] = this.Heap_[num4];
                i = num4;
            }
            this.Heap_[i] = this.Heap_[this.Count_ - 1];
        }
        this.Count_--;
    }

    public void Push( T value ) {
        if( this.Count_ == this.Heap_.Length ) {
            T[] localArray = new T[this.Count_ * 2];
            for( int j = 0; j < this.Count_; j++ ) {
                localArray[j] = this.Heap_[j];
            }
            this.Heap_ = localArray;
        }
        int i = this.Count_;
        while( i > 0 ) {
            int index = PriorityQueue<T>.HeapParent( i );
            if( this.Comparer_.Compare( value, this.Heap_[index] ) >= 0 ) {
                break;
            }
            this.Heap_[i] = this.Heap_[index];
            i = index;
        }
        this.Heap_[i] = value;
        this.Count_++;
    }

    public void Clear() {
        this.Count_ = 0;
    }
}

