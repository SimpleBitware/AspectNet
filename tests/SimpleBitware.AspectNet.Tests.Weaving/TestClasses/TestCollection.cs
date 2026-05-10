using SimpleBitware.AspectNet.Abstractions.Attributes;
using SimpleBitware.AspectNet.Tests.Weaving.Attributes;

namespace SimpleBitware.AspectNet.Tests.Weaving.TestClasses;

[RecordActivity]
public class TestCollection<T>
{
    private readonly List<T> items = [];

    [ExtendedRecordActivity]
    public T this[int index]
    {
        get
        {
            if (index < 0 || index >= items.Count)
                throw new ArgumentOutOfRangeException(nameof(index));
            
            return items[index];
        }
        set
        {
            if (index < 0 || index >= items.Count)
                throw new ArgumentOutOfRangeException(nameof(index));
            
            items[index] = value;
        }
    }

    public int Count => items.Count;

    [AspectNetExclude]
    public void Add(T item) => items.Add(item);

    public bool Remove(T item) => items.Remove(item);
}
