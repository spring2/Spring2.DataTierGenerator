using System;
using System.Collections;
using System.Text;
using System.Xml;

namespace Spring2.DataTierGenerator {
    public class Entity : Spring2.Core.DataObject.DataObject, ICloneable {

	protected String name = String.Empty;
	protected String sqlObject = String.Empty;
	protected String sqlView = String.Empty;
	protected ArrayList fields = new ArrayList();
	private ArrayList finders = new ArrayList();

	public String Name {
	    get { return this.name; }
	    set { this.name = value; }
	}

	public String SqlObject {
	    get { return this.sqlObject; }
	    set { this.sqlObject = value; }
	}

	public String SqlView {
	    get { return this.sqlView; }
	    set { this.sqlView = value; }
	}

	public ArrayList Fields {
	    get { return this.fields; }
	    set { this.fields = value; }
	}

	public ArrayList Finders {
	    get { return this.finders; }
	    set { this.finders = value; }
	}

	public String ToXml() {
	    StringBuilder sb = new StringBuilder();
	    sb.Append("<entity name=\"").Append(name).Append("\"");
	    sb.Append(" sqlobject=\"").Append(sqlObject).Append("\"");
	    sb.Append(" sqlview=\"").Append(sqlView).Append("\"");
	    sb.Append(" />");

	    return sb.ToString();
	}


	public static Entity FindEntityBySqlObject(ArrayList entities, String sqlObject) {
	    foreach (Entity entity in entities) {
		if (entity.SqlObject == sqlObject) {
		    return entity;
		}
	    }
	    return null;
	}

	public static ArrayList ParseFromXml(Configuration options, XmlDocument doc, Hashtable sqltypes, Hashtable types) {
	    ArrayList entities = new ArrayList();
	    XmlNodeList elements = doc.DocumentElement.GetElementsByTagName("entity");
	    foreach (XmlNode node in elements) {
		Entity entity = new Entity();
		entity.Name = node.Attributes["name"].Value;
		if (node.Attributes["sqlobject"] != null) {
		    entity.SqlObject = node.Attributes["sqlobject"].Value;
		    if (options.UseViews) {
			entity.SqlView = "vw" + entity.SqlObject;
		    }
		}
		if (node.Attributes["sqlview"] != null) {
		    entity.SqlView = node.Attributes["sqlview"].Value;
		}
		entity.Fields = Field.ParseFromXml(doc, entity, sqltypes, types);
		entity.Finders = Finder.ParseFromXml(node, entity);
		entities.Add(entity);
	    }
	    return entities;
	}

	public Boolean HasUpdatableFields() {
	    Boolean has = false;
	    foreach (Field field in fields) {
		if (!field.IsIdentity && !field.IsPrimaryKey && !field.IsRowGuidCol) {
		    has=true;	
		}
	    }
	    return has;
	}

	public IList GetPrimaryKeyColumns() {
	    ArrayList list = new ArrayList();
	    Field id = GetIdentityColumn();
	    if (id != null && id.Name.Length>0) {
		list.Add(id);
	    } else {
		foreach (Field field in fields) {
		    if (field.IsPrimaryKey) {
			list.Add(field);
		    }
		}
	    }
	    return list;
	}

	// static helper method
	public Field GetIdentityColumn() {
	    foreach (Field field in fields) {
		if (field.IsIdentity) {
		    return field;
		}
	    }
	    return new Field();   // this should not return this - should return null
	}

	public Field FindFieldBySqlName(String name) {
	    foreach (Field field in fields) {
		if (field.SqlName == name) {
		    return field;
		}
	    }
	    return null;
	}

	public Field FindFieldByName(String name) {
	    foreach (Field field in fields) {
		if (field.Name == name) {
		    return field;
		}
	    }
	    return null;
	}

	public Object Clone() {
	    return MemberwiseClone();
	}


    }
}