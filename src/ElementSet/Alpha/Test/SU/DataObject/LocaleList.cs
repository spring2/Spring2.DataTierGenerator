using System;

using Spring2.DataTierGenerator.Attribute;

namespace StampinUp.DataObject
{

    /// <summary>
    /// LocaleData generic collection
    /// </summary>
    public class LocaleList : System.Collections.CollectionBase
    {

	[Generate]
	public static readonly LocaleList UNSET = new LocaleList(true);
	[Generate]
	public static readonly LocaleList DEFAULT = new LocaleList(true);

	[Generate]
	private Boolean immutable = false;

	[Generate]
	private LocaleList (Boolean immutable)
	{
	    this.immutable = immutable;
	}

	[Generate]
	public LocaleList()
	{
	}

	// Indexer implementation.
	[Generate]
	public LocaleData this[int index]
	{
	    get
	    {
		return (LocaleData) List[index];
	    }
	    set
	    {
		if (!immutable)
		{
		    List[index] = value;
		}
		else
		{
		    throw new System.Data.ReadOnlyException();
		}
	    }
	}

	[Generate]
	public void Add(LocaleData value)
	{
	    if (!immutable)
	    {
		List.Add(value);
	    }
	    else
	    {
		throw new System.Data.ReadOnlyException();
	    }
	}

	[Generate]
	public Boolean Contains(LocaleData value)
	{
	    return List.Contains(value);
	}

	[Generate]
	public Int32 IndexOf(LocaleData value)
	{
	    return List.IndexOf(value);
	}

	[Generate]
	public void Insert(Int32 index, LocaleData value)
	{
	    if (!immutable)
	    {
		List.Insert(index, value);
	    }
	    else
	    {
		throw new System.Data.ReadOnlyException();
	    }
	}

	[Generate]
	public void Remove(int index)
	{
	    if (!immutable)
	    {
		if (index > Count - 1 || index < 0)
		{
		    throw new IndexOutOfRangeException();
		}
		else
		{
		    List.RemoveAt(index);
		}
	    }
	    else
	    {
		throw new System.Data.ReadOnlyException();
	    }
	}

	[Generate]
	public void Remove(LocaleData value)
	{
	    if (!immutable)
	    {
		List.Remove(value);
	    }
	    else
	    {
		throw new System.Data.ReadOnlyException();
	    }
	}

	[Generate]
	public void AddRange(System.Collections.IList list)
	{
	    foreach(Object o in list)
	    {
		if (o is LocaleData)
		{
		    Add((LocaleData)o);
		}
		else
		{
		    throw new System.InvalidCastException("object in list could not be cast to LocaleData");
		}
	    }
	}

	[Generate]
	public Boolean IsDefault
	{
	    get
	    {
		return Object.ReferenceEquals(this, DEFAULT);
	    }
	}

	[Generate]
	public Boolean IsUnset
	{
	    get
	    {
		return Object.ReferenceEquals(this, UNSET);
	    }
	}

	[Generate]
	public Boolean IsValid
	{
	    get
	    {
		return !(IsDefault || IsUnset);
	    }
	}


    }
}
