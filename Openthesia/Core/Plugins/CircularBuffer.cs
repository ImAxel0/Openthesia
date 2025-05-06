using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Openthesia.Core.Plugins;

public class CircularBuffer
{
    private readonly float[] _buffer;
    private int _head;
    private int _tail;
    private int _count;
    public int Count => _count;

    public CircularBuffer(int size)
    {
        _buffer = new float[size];
    }

    public void Write(float[] data, int offset, int count)
    {
        for (int i = 0; i < count; i++)
        {
            _buffer[_head] = data[offset + i];
            _head = (_head + 1) % _buffer.Length;

            if (_count < _buffer.Length)
                _count++;
            else
                _tail = (_tail + 1) % _buffer.Length; // Overwrite oldest data
        }
    }

    public int Read(float[] output, int offset, int count)
    {
        int readCount = Math.Min(count, _count);
        for (int i = 0; i < readCount; i++)
        {
            output[offset + i] = _buffer[_tail];
            _tail = (_tail + 1) % _buffer.Length;
            _count--;
        }
        return readCount;
    }
}
