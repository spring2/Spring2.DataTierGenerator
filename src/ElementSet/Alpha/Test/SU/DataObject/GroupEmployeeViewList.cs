using System;

using Spring2.DataTierGenerator.Attribute;

namespace StampinUp.DataObject
{

    /// <summary>
    /// GroupEmployeeViewData generic collection
    /// </summary>
    public class GroupEmployeeViewList : System.Collections.CollectionBase
    {

	[Generate]
	public static readonly GroupEmployeeViewList UNSET = new GroupEmployeeViewList(true);
	[Generate]
	public static readonly GroupEmployeeViewList DEFAULT = new GroupEmployeeViewList(true);

	[Generate]
	private Boolean immutable = false;

	[Generate]
	private GroupEmployeeViewList (Boolean immutable)
	{
	    this.immutable = immutable;
	}

	[Generate]
	public GroupEmployeeViewList()
	{
	}

	// Indexer implementation.
	[Generate]
	public GroupEmployeeViewData this[int index]
	{
	    get
	    {
		return (GroupEmployeeViewData) List[index];
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
	public void Add(GroupEmployeeViewData value)
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
	public Boolean Contains(GroupEmployeeViewData value)
	{
	    return List.Contains(value);
	}

	[Generate]
	public Int32 IndexOf(GroupEmployeeViewData value)
	{
	    return List.IndexOf(value);
	}

	[Generate]
	public void Insert(Int32 index, GroupEmployeeViewData value)
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
	public void Remove(GroupEmployeeViewData value)
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
		if (o is GroupEmployeeViewData)
		{
		    Add((GroupEmployeeViewData)o);
		}
		else
		{
		    throw new System.InvalidCastException("object in list could not be cast to GroupEmployeeViewData");
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
