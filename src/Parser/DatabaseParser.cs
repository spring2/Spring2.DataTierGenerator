using System;

using System.Collections;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Text;
using System.Xml;

using Spring2.DataTierGenerator;
using Spring2.DataTierGenerator.Element;

namespace Spring2.DataTierGenerator.Parser {
    /// <summary>
    /// Summary description for DatabaseParser.
    /// </summary>
    public class DatabaseParser : ParserSkeleton, IParser {
	
	private String connectionString;

	public DatabaseParser(Configuration options, String connectionString) {
	    this.connectionString = connectionString;
	}

	private ArrayList GetEntities(XmlDocument doc, SqlConnection connection, ArrayList sqlentities, ParserValidationDelegate vd) {
	    ArrayList entities = new ArrayList();

	    if (doc != null) {
		entities.AddRange(Entity.ParseFromXml(options, doc, sqltypes, types, sqlentities, vd));

		//		if (options.AutoDiscoverProperties) {
		//		    foreach (Entity entity in entities) {
		//			entity.Fields = GetFields(entity, connection, doc, sqltypes, types);
		//		    }
		//		}
	    }

	    //	    if (options.AutoDiscoverEntities) {
	    //		// Get a list of the entities in the database
	    //		DataTable objDataTable = new DataTable();
	    //		SqlDataAdapter objDataAdapter = new SqlDataAdapter("SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_CATALOG = '" + connection.Database + "'", connection);
	    //		objDataAdapter.Fill(objDataTable);
	    //		foreach (DataRow row in objDataTable.Rows) {
	    //		    if (row["TABLE_TYPE"].ToString() == "BASE TABLE" && row["TABLE_NAME"].ToString() != "dtproperties") {
	    //			if (Entity.FindEntityBySqlEntity(entities, row["TABLE_NAME"].ToString()) == null) {
	    //			    Entity entity = new Entity();
	    //			    entity.Name = row["TABLE_NAME"].ToString();
	    //			    entity.SqlEntity.Name = row["TABLE_NAME"].ToString();
	    //			    if (options.UseViews) {
	    //				entity.SqlEntity.View = "vw" + entity.SqlEntity.Name;
	    //			    }
	    //			    entity.Fields = GetFields(entity, connection, doc, sqltypes, types);
	    //			    entities.Add(entity);
	    //			}
	    //		    }
	    //		}	    
	    //	    }

	    return entities;
	}


	private ArrayList GetDatabases(XmlDocument doc, ParserValidationDelegate vd) {
	    ArrayList list = new ArrayList();

	    if (doc != null) {
		list.AddRange(Database.ParseFromXml(options, doc, sqltypes, types, vd));
	    }
	    return list;
	}



	//	private ArrayList GetSqlEntities(XmlDocument doc, SqlConnection connection) {
	//	    ArrayList entities = new ArrayList();
	//
	//	    if (doc != null) {
	//		entities.AddRange(SqlEntity.ParseFromXml(options, doc, sqltypes, types));
	//
	////		if (options.AutoDiscoverProperties) {
	////		    foreach (Entity entity in entities) {
	////			entity.Fields = GetFields(entity, connection, doc, sqltypes, types);
	////		    }
	////		}
	//	    }
	//	    return entities;
	//	}

	private ArrayList GetFields(Entity entity, SqlConnection connection, XmlDocument doc, Hashtable sqltypes, Hashtable types) {
	    ArrayList fields = entity.Fields;

	    Boolean foundNewProperties=false;
	    if (entity.SqlEntity.AutoDiscoverProperties) {
		DataTable columns = GetTableColumns(entity.SqlEntity, connection);
		foreach (DataRow objDataRow in columns.Rows) {
		    if (objDataRow["COLUMN_COMPUTED"].ToString() == "0") {
			if (entity.SqlEntity.FindColumnByName(objDataRow["COLUMN_NAME"].ToString()) == null) {
			    Field field = new Field();
			    field.Name = objDataRow["COLUMN_NAME"].ToString();
			    field.Column.Name = field.Name;

			    field.Column.SqlType.Name = objDataRow["DATA_TYPE"].ToString();
			    // if the sql type is defined, default to all values defined in it
			    if (sqltypes.ContainsKey(field.Column.SqlType.Name)) {
				field.Column.SqlType = (SqlType)((SqlType)sqltypes[field.Column.SqlType.Name]).Clone();
				if (types.Contains(field.Column.SqlType.Type)) {
				    field.Type = (Spring2.DataTierGenerator.Element.Type)((Spring2.DataTierGenerator.Element.Type)types[field.Column.SqlType.Type]).Clone();
				} else {
				    Console.Out.WriteLine("Type " + field.Column.SqlType.Type + " was not defined");
				}
			    } else {
				Console.Out.WriteLine("SqlType " + field.Column.SqlType.Name + " was not defined");
			    }

			    field.Column.SqlType.Length = objDataRow["CHARACTER_MAXIMUM_LENGTH"].ToString().Length > 0 ? (Int32)objDataRow["CHARACTER_MAXIMUM_LENGTH"] : (Int32)(Int16)objDataRow["COLUMN_LENGTH"];
			    if (!System.DBNull.Value.Equals(objDataRow["NUMERIC_PRECISION"])) field.Column.SqlType.Precision = (Int32)(Byte)objDataRow["NUMERIC_PRECISION"];
			    if (!System.DBNull.Value.Equals(objDataRow["NUMERIC_SCALE"])) field.Column.SqlType.Scale = (Int32)objDataRow["NUMERIC_SCALE"];
			    field.Column.Identity = objDataRow["IsIdentity"].ToString() == "1";
			    //			    field.IsPrimaryKey = objDataRow["IsPrimaryKey"].ToString() == "1";
			    field.Column.RowGuidCol = objDataRow["IsRowGuidCol"].ToString() == "1";
			    //			    field.IsForeignKey = objDataRow["IsForeignKey"].ToString() == "1";
			    //			    field.IsViewColumn = objDataRow["IsViewColumn"].ToString() == "1";

			    // Check for unicode columns
			    if (field.Column.SqlType.Name.ToLower() == "nchar" || field.Column.SqlType.Name.ToLower() == "nvarchar" || field.Column.SqlType.Name.ToLower() == "ntext") {
				field.Column.SqlType.Length = field.Column.SqlType.Length / 2;
			    }
					
			    // Check for text or ntext columns, which require a different length from what SQL Server reports
			    if (field.Column.SqlType.Name.ToLower() == "text") {
				field.Column.SqlType.Length = 2147483647;
			    } else if (field.Column.SqlType.Name.ToLower() == "ntext") {
				field.Column.SqlType.Length = 1073741823;
			    }
					
			    // Append the array to the array list
			    fields.Add(field);

			    if (!foundNewProperties) {
				Console.Out.WriteLine(entity.ToXml());
				foundNewProperties=true;
			    }
			    Console.Out.WriteLine("\t" + field.ToXml(true));

			}
		    }
		}
	    }
	    if (foundNewProperties) {
		Console.Out.WriteLine("</entity>\n");
	    }

	    return fields;
	}

	private DataTable GetTableColumns(SqlEntity sqlentity, SqlConnection connection) {
	    String sql = "	SELECT	INFORMATION_SCHEMA.COLUMNS.COLUMN_NAME, \n";
	    sql = sql + " 		INFORMATION_SCHEMA.COLUMNS.DATA_TYPE, \n";
	    sql = sql + " 		INFORMATION_SCHEMA.COLUMNS.CHARACTER_MAXIMUM_LENGTH, \n";
	    sql = sql + " 		INFORMATION_SCHEMA.COLUMNS.NUMERIC_SCALE, \n";
	    sql = sql + " 		INFORMATION_SCHEMA.COLUMNS.NUMERIC_PRECISION, \n";
	    sql = sql + " 		systypes.length AS COLUMN_LENGTH, \n";
	    sql = sql + " 		syscolumns.iscomputed AS COLUMN_COMPUTED, \n";
	    sql = sql + "		'0' IsViewColumn, \n";
	    sql = sql + "		coalesce(VC.colid, 1000+ORDINAL_POSITION) COLUMN_ID, \n";
	    sql = sql + "		ColumnProperty(OBJECT_ID(INFORMATION_SCHEMA.COLUMNS.TABLE_NAME), INFORMATION_SCHEMA.COLUMNS.COLUMN_NAME, 'IsIdentity') AS IsIdentity, \n";
	    sql = sql + "		ColumnProperty(OBJECT_ID(INFORMATION_SCHEMA.COLUMNS.TABLE_NAME), INFORMATION_SCHEMA.COLUMNS.COLUMN_NAME, 'IsRowGuidCol') AS IsRowGuidCol, \n";
	    sql = sql + " 		INFORMATION_SCHEMA.COLUMNS.IS_NULLABLE \n";
	    //	    sql = sql + "		case when INFORMATION_SCHEMA.COLUMNS.COLUMN_NAME in (SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.CONSTRAINT_COLUMN_USAGE INNER JOIN INFORMATION_SCHEMA.TABLE_CONSTRAINTS ON INFORMATION_SCHEMA.CONSTRAINT_COLUMN_USAGE.CONSTRAINT_NAME = INFORMATION_SCHEMA.TABLE_CONSTRAINTS.CONSTRAINT_NAME WHERE INFORMATIoN_SCHEMA.TABLE_CONSTRAINTS.TABLE_NAME = INFORMATION_SCHEMA.COLUMNS.TABLE_NAME AND CONSTRAINT_TYPE = 'FOREIGN KEY') then 1 else 0 end IsForeignKey, \n";
	    //	    sql = sql + "		case when INFORMATION_SCHEMA.COLUMNS.COLUMN_NAME in (SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.CONSTRAINT_COLUMN_USAGE INNER JOIN INFORMATION_SCHEMA.TABLE_CONSTRAINTS ON INFORMATION_SCHEMA.CONSTRAINT_COLUMN_USAGE.CONSTRAINT_NAME = INFORMATION_SCHEMA.TABLE_CONSTRAINTS.CONSTRAINT_NAME WHERE INFORMATIoN_SCHEMA.TABLE_CONSTRAINTS.TABLE_NAME = INFORMATION_SCHEMA.COLUMNS.TABLE_NAME AND CONSTRAINT_TYPE = 'PRIMARY KEY') then 1 else 0 end IsPrimaryKey \n";
	    sql = sql + " 	FROM INFORMATION_SCHEMA.COLUMNS \n";
	    sql = sql + "  	INNER JOIN systypes ON INFORMATION_SCHEMA.COLUMNS.DATA_TYPE = systypes.name \n";
	    sql = sql + "  	INNER JOIN syscolumns ON INFORMATION_SCHEMA.COLUMNS.COLUMN_NAME = syscolumns.name  AND syscolumns.id = OBJECT_ID('" + sqlentity.Name + "') \n";
	    sql = sql + "	left join syscolumns vc on INFORMATION_SCHEMA.COLUMNS.COLUMN_NAME = vc.name AND vc.id = OBJECT_ID('" + sqlentity.Name + "') \n";
	    sql = sql + "  	WHERE INFORMATION_SCHEMA.COLUMNS.TABLE_NAME = '" + sqlentity.Name + "' \n";

	    // if basing data objects on views, get additional fields found in the corresponding view (by naming convention of vw + tablename) -- should be configuration option
	    if (sqlentity.UseViews && sqlentity.View.Length>1) {
		sql = sql + "union \n";
		sql = sql + "	SELECT	INFORMATION_SCHEMA.COLUMNS.COLUMN_NAME, \n";
		sql = sql + " 		INFORMATION_SCHEMA.COLUMNS.DATA_TYPE, \n";
		sql = sql + " 		INFORMATION_SCHEMA.COLUMNS.CHARACTER_MAXIMUM_LENGTH, \n";
		sql = sql + " 		INFORMATION_SCHEMA.COLUMNS.NUMERIC_SCALE, \n";
		sql = sql + " 		INFORMATION_SCHEMA.COLUMNS.NUMERIC_PRECISION, \n";
		sql = sql + "  		systypes.length AS COLUMN_LENGTH, \n";
		sql = sql + "  		syscolumns.iscomputed AS COLUMN_COMPUTED, \n";
		sql = sql + " 		'1' IsViewColumn, \n";
		sql = sql + "		ORDINAL_POSITION COLUMN_ID, \n";
		sql = sql + "		ColumnProperty(OBJECT_ID(INFORMATION_SCHEMA.COLUMNS.TABLE_NAME), INFORMATION_SCHEMA.COLUMNS.COLUMN_NAME, 'IsIdentity') AS IsIdentity, \n";
		sql = sql + "		ColumnProperty(OBJECT_ID(INFORMATION_SCHEMA.COLUMNS.TABLE_NAME), INFORMATION_SCHEMA.COLUMNS.COLUMN_NAME, 'IsRowGuidCol') AS IsRowGuidCol, \n";
		sql = sql + " 		INFORMATION_SCHEMA.COLUMNS.IS_NULLABLE \n";
		//		sql = sql + "		case when INFORMATION_SCHEMA.COLUMNS.COLUMN_NAME in (SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.CONSTRAINT_COLUMN_USAGE INNER JOIN INFORMATION_SCHEMA.TABLE_CONSTRAINTS ON INFORMATION_SCHEMA.CONSTRAINT_COLUMN_USAGE.CONSTRAINT_NAME = INFORMATION_SCHEMA.TABLE_CONSTRAINTS.CONSTRAINT_NAME WHERE INFORMATIoN_SCHEMA.TABLE_CONSTRAINTS.TABLE_NAME = INFORMATION_SCHEMA.COLUMNS.TABLE_NAME AND CONSTRAINT_TYPE = 'FOREIGN KEY') then 1 else 0 end IsForeignKey, \n";
		//		sql = sql + "		case when INFORMATION_SCHEMA.COLUMNS.COLUMN_NAME in (SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.CONSTRAINT_COLUMN_USAGE INNER JOIN INFORMATION_SCHEMA.TABLE_CONSTRAINTS ON INFORMATION_SCHEMA.CONSTRAINT_COLUMN_USAGE.CONSTRAINT_NAME = INFORMATION_SCHEMA.TABLE_CONSTRAINTS.CONSTRAINT_NAME WHERE INFORMATIoN_SCHEMA.TABLE_CONSTRAINTS.TABLE_NAME = INFORMATION_SCHEMA.COLUMNS.TABLE_NAME AND CONSTRAINT_TYPE = 'PRIMARY KEY') then 1 else 0 end IsPrimaryKey \n";
		sql = sql + " 	FROM INFORMATION_SCHEMA.COLUMNS \n";
		sql = sql + " 	INNER JOIN systypes ON INFORMATION_SCHEMA.COLUMNS.DATA_TYPE = systypes.name \n";
		sql = sql + " 	INNER JOIN syscolumns ON INFORMATION_SCHEMA.COLUMNS.COLUMN_NAME = syscolumns.name \n";
		sql = sql + " 	WHERE INFORMATION_SCHEMA.COLUMNS.TABLE_NAME = '" + sqlentity.View + "' AND syscolumns.id = OBJECT_ID('" + sqlentity.View + "') \n";
		sql = sql + " 	and INFORMATION_SCHEMA.COLUMNS.COLUMN_NAME not in (select INFORMATION_SCHEMA.COLUMNS.COLUMN_NAME from INFORMATION_SCHEMA.COLUMNS where INFORMATION_SCHEMA.COLUMNS.TABLE_NAME = '" + sqlentity.Name + "') \n";
	    }
	    sql = sql + "order by column_id \n";

	    // Fill the dataset with the information for the current table
	    DataTable table = new DataTable();
	    SqlDataAdapter adapter = new SqlDataAdapter(sql, connection);
	    adapter.Fill(table);
	    return table;
	}

	private ArrayList DiscoverSqlEntities(SqlConnection connection) {
	    ArrayList list = new ArrayList();

	    // Get a list of the entities in the database
	    DataTable table = new DataTable();
	    //SqlDataAdapter adapter = new SqlDataAdapter("SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_CATALOG = '" + connection.Database + "'", connection);
	    SqlDataAdapter adapter = new SqlDataAdapter("SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_CATALOG = '" + connection.Database + "' and TABLE_NAME in ('Address','ErrorLog','Firm','Function' ,'GroupOrderDetail' ,'GroupOrders','GroupOrderVendors' ,'OrderAllocation' ,'OrderDelivery' ,'OrderDetail' ,'OrderDetailOption' ,'Orders' ,'OrderVendor' ,'OrdPmt' ,'pmt_type' ,'SecurityGroup' ,'SecurityGroupFunction' ,'ServiceArea','Users' ,'FirmLocation' ,'FirmLocRule' ,'FirmRule')", connection);
	    //'Address','ErrorLog','Firm','Function' ,'GroupOrderDetail' ,'GroupOrders','GroupOrderVendors' ,'OrderAllocation' ,'OrderDelivery' ,'OrderDetail' ,'OrderDetailOption' ,'Orders' ,'OrderVendor' ,'OrdPmt' ,'pmt_type' ,'SecurityGroup' ,'SecurityGroupFunction' ,'ServiceArea','Users' ,'FirmLocation' ,'FirmLocRule' ,'FirmRule'
	    //SqlDataAdapter adapter = new SqlDataAdapter("SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_CATALOG = '" + connection.Database + "' and TABLE_NAME in ('Users')", connection);

	    adapter.Fill(table);
	    //form.Maximum= table.Rows.Count +1;

	    foreach (DataRow row in table.Rows) {
		if (row["TABLE_TYPE"].ToString() == "BASE TABLE" && row["TABLE_NAME"].ToString() != "dtproperties") {
		    SqlEntity sqlentity = new SqlEntity();
		    sqlentity.Name = row["TABLE_NAME"].ToString();
		    sqlentity.View = "vw" + sqlentity.Name;
		    if (sqlentity.Name.Equals("Users")) {
			sqlentity.View = "vwUser";
		    }
		    if (sqlentity.Name.Equals("Orders")) {
			sqlentity.View = "vwOrder";
		    }
		    sqlentity.Columns = DiscoverColumns(sqlentity, connection);
		    sqlentity.Constraints = DiscoverConstraints(sqlentity, connection);
		    sqlentity.Indexes = DiscoverIndexes(sqlentity, connection);
		    list.Add(sqlentity);
		    //form.Step();
		}
	    }	    

	    return list;
	}

	private ArrayList DiscoverConstraints(SqlEntity sqlentity, SqlConnection connection) {
	    ArrayList list = new ArrayList();

	    DataTable table = new DataTable();
	    String sql = "SELECT c.*, coalesce((i.status & 16),0) ClusteredIndex, coalesce(rt.TABLE_NAME,'') ForeignEntity, coalesce(cc.CHECK_CLAUSE, '') CheckClause ";
	    sql += "FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS c ";
	    sql += "left join sysindexes i on c.CONSTRAINT_NAME=i.name ";
	    sql += "left join INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS r on c.CONSTRAINT_NAME=r.CONSTRAINT_NAME ";
	    sql += "left join INFORMATION_SCHEMA.TABLE_CONSTRAINTS rt on r.UNIQUE_CONSTRAINT_NAME=rt.CONSTRAINT_NAME ";
	    sql += "left join INFORMATION_SCHEMA.CHECK_CONSTRAINTS cc on c.CONSTRAINT_NAME=cc.CONSTRAINT_NAME ";
	    sql += "where c.table_name='" + sqlentity.Name + "' ";

	    SqlDataAdapter adapter = new SqlDataAdapter(sql, connection);
	    adapter.Fill(table);
	    
	    foreach (DataRow row in table.Rows) {
		Spring2.DataTierGenerator.Element.Constraint constraint = new Spring2.DataTierGenerator.Element.Constraint();
		constraint.Name = row["CONSTRAINT_NAME"].ToString();
		constraint.Type = row["CONSTRAINT_TYPE"].ToString();
		constraint.Clustered = Int32.Parse(row["ClusteredIndex"].ToString())!=0;
		constraint.ForeignEntity = row["ForeignEntity"].ToString();
		constraint.CheckClause = row["CheckClause"].ToString();

		if (constraint.Type.ToUpper().Equals("PRIMARY KEY") || constraint.Type.ToUpper().Equals("UNIQUE")) {
		    constraint.Columns = DiscoverPrimaryKeyColumns(sqlentity, constraint, connection);
		}
		if (constraint.Type.ToUpper().Equals("FOREIGN KEY")) {
		    constraint.Columns = DiscoverForeignKeyColumns(sqlentity, constraint, connection);
		}

		list.Add(constraint);
	    }	    
	    return list;
	}

	private ArrayList DiscoverPrimaryKeyColumns(SqlEntity sqlentity, Spring2.DataTierGenerator.Element.Constraint constraint, SqlConnection connection) {
	    ArrayList list = new ArrayList();

	    DataTable table = new DataTable();
	    String sql = "SELECT * ";
	    sql += "FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE  ";
	    sql += "where table_name='" + sqlentity.Name + "' and constraint_name='" + constraint.Name + "'  ";
	    sql += "order by ORDINAL_POSITION  ";
	    SqlDataAdapter adapter = new SqlDataAdapter(sql, connection);
	    adapter.Fill(table);
	    
	    foreach (DataRow row in table.Rows) {
		Column column = new Column();
		column.Name = row["COLUMN_NAME"].ToString();
		list.Add(column);
	    }

	    return list;
	}

	private ArrayList DiscoverForeignKeyColumns(SqlEntity sqlentity, Spring2.DataTierGenerator.Element.Constraint constraint, SqlConnection connection) {
	    ArrayList list = new ArrayList();

	    String s = "select 1 ORDINAL_POSITION, c.name COLUMN_NAME, rc.name FOREIGN_COLUMN ";
	    s += "from syscolumns c ";
	    s += "left join sysreferences r on c.id = r.fkeyid and c.colid=r.fkey1 and r.constid=object_id('" + constraint.Name + "') ";
	    s += "left join syscolumns rc on rc.id=r.rkeyid and rc.colid=r.rkey1 ";
	    s += "where c.id = object_id('" + sqlentity.Name + "') and r.constid is not null ";

	    DataTable table = new DataTable();
	    String sql = s;
	    for (int i=2; i<=16; i++) {
		sql += " union ";
		sql += s.Replace("1", i.ToString());
	    }
	    SqlDataAdapter adapter = new SqlDataAdapter(sql, connection);
	    adapter.Fill(table);

	    foreach (DataRow row in table.Rows) {
		Column column = new Column();
		column.Name = row["COLUMN_NAME"].ToString();
		column.ForeignColumn = row["FOREIGN_COLUMN"].ToString();
		list.Add(column);
	    }

	    return list;
	}

	private ArrayList DiscoverIndexes(SqlEntity sqlentity, SqlConnection connection) {
	    ArrayList list = new ArrayList();

	    DataTable table = new DataTable();

	    String sql = "declare @objid int \n";
	    sql += "set @objid = object_id('" + sqlentity.Name + "') \n";
	    sql += "select INDEXPROPERTY(@objid, name, 'IsUnique') IsUnique, INDEXPROPERTY(@objid, name, 'IsClustered') IsClustered, name \n";
	    sql += "from sysindexes \n";
	    sql += "where id = @objid and indid > 0 and indid < 255 and (status & 64)=0 and name not in (select name from sysobjects) \n";
	    sql += "order by indid \n";

	    SqlDataAdapter adapter = new SqlDataAdapter(sql, connection);
	    adapter.Fill(table);

	    foreach (DataRow row in table.Rows) {
		Index index = new Index();
		index.Name = row["NAME"].ToString();
		index.Clustered = !row["IsClustered"].ToString().Equals("0");
		index.Unique = !row["IsUnique"].ToString().Equals("0");
		index.Columns = DiscoverIndexColumns(sqlentity, index, connection);
		list.Add(index);
	    }

	    return list;
	}

	private ArrayList DiscoverIndexColumns(SqlEntity sqlentity, Index index, SqlConnection connection) {
	    ArrayList list = new ArrayList();

	    DataTable table = new DataTable();

	    String sql = "DECLARE @indid smallint, \n";
	    sql += "	@indname sysname,  \n";
	    sql += "   	@indkey int,  \n";
	    sql += "	@name varchar(30) \n";
	    sql += " \n";
	    sql += "SET NOCOUNT ON \n";
	    sql += " \n";
	    sql += "set @name='" + sqlentity.Name + "' \n";
	    sql += "set @indname='" + index.Name + "' \n";
	    sql += " \n";
	    sql += "select @indid=indid from sysindexes where id=object_id(@name) and name=@indname \n";
	    sql += "   \n";
	    sql += "	create table #spindtab \n";
	    sql += "	( \n";
	    sql += "		TABLE_NAME			sysname	collate database_default NULL, \n";
	    sql += "		INDEX_NAME			sysname	collate database_default NULL, \n";
	    sql += "		COLUMN_NAME			sysname	collate database_default NULL, \n";
	    sql += "		SORT_DIRECTION			varchar(50) NULL, \n";
	    sql += "		ORDINAL_POSITION		int \n";
	    sql += "	) \n";
	    sql += " \n";
	    sql += "     SET @indkey = 1 \n";
	    sql += "     WHILE @indkey <= 16 and INDEX_COL(@name, @indid, @indkey) is not null \n";
	    sql += "      BEGIN \n";
	    sql += "	insert into #spindtab(table_name, index_name, column_name, ordinal_position, sort_direction) values(@name, @indname, INDEX_COL(@name, @indid, @indkey), @indkey, case when indexkey_property(object_id(@name), @indid, 1, 'isdescending')=1 then 'DESC' else '' end) \n";
	    sql += "        SET @indkey = @indkey + 1 \n";
	    sql += "      END \n";
	    sql += " \n";
	    sql += "select * from #spindtab order by ordinal_position \n";
	    sql += " \n";
	    sql += "drop table #spindtab \n";
	    sql += " \n";
	    sql += "SET NOCOUNT OFF \n";

	    SqlDataAdapter adapter = new SqlDataAdapter(sql, connection);
	    adapter.Fill(table);

	    foreach (DataRow row in table.Rows) {
		Column column = new Column();
		column.Name = row["COLUMN_NAME"].ToString();
		column.SortDirection = row["SORT_DIRECTION"].ToString();
		list.Add(column);
	    }

	    return list;
	}

	private ArrayList DiscoverColumns(SqlEntity sqlentity, SqlConnection connection) {
	    ArrayList list = new ArrayList();

	    //	    Boolean foundNewProperties=false;
	    DataTable columns = GetTableColumns(sqlentity, connection);
	    foreach (DataRow row in columns.Rows) {
		if (row["COLUMN_COMPUTED"].ToString() == "0") {
		    Column column = new Column();
		    column.Name = row["COLUMN_NAME"].ToString();

		    column.SqlType.Name = row["DATA_TYPE"].ToString();
		    // if the sql type is defined, default to all values defined in it
		    if (sqltypes.ContainsKey(column.SqlType.Name)) {
			column.SqlType = (SqlType)((SqlType)sqltypes[column.SqlType.Name]).Clone();
		    } else {
			Console.Out.WriteLine("SqlType " + column.SqlType.Name + " was not defined");
		    }

		    column.SqlType.Length = row["CHARACTER_MAXIMUM_LENGTH"].ToString().Length > 0 ? (Int32)row["CHARACTER_MAXIMUM_LENGTH"] : (Int32)(Int16)row["COLUMN_LENGTH"];
		    if (!System.DBNull.Value.Equals(row["NUMERIC_PRECISION"])) column.SqlType.Precision = (Int32)(Byte)row["NUMERIC_PRECISION"];
		    if (!System.DBNull.Value.Equals(row["NUMERIC_SCALE"])) column.SqlType.Scale = (Int32)row["NUMERIC_SCALE"];
		    column.Identity = row["IsIdentity"].ToString() == "1";
		    column.RowGuidCol = row["IsRowGuidCol"].ToString() == "1";
		    column.ViewColumn = row["IsViewColumn"].ToString() == "1";
		    column.Required = row["IS_NULLABLE"].ToString().Trim().ToUpper().Equals("NO");

		    // Check for unicode columns
		    if (column.SqlType.Name.ToLower() == "nchar" || column.SqlType.Name.ToLower() == "nvarchar" || column.SqlType.Name.ToLower() == "ntext") {
			column.SqlType.Length = column.SqlType.Length / 2;
		    }
					
		    // Check for text or ntext columns, which require a different length from what SQL Server reports
		    if (column.SqlType.Name.ToLower() == "text") {
			column.SqlType.Length = 2147483647;
		    } else if (column.SqlType.Name.ToLower() == "ntext") {
			column.SqlType.Length = 1073741823;
		    }

		    // Append the array to the array list
		    list.Add(column);

		    //		    if (!foundNewProperties) {
		    //			Console.Out.WriteLine(sqlentity.ToXml());
		    //			foundNewProperties=true;
		    //		    }
		    //		    Console.Out.WriteLine("\t" + column.ToXml(true));
		    //Application.DoEvents();
		}
	    }
	    //	    if (foundNewProperties) {
	    //		Console.Out.WriteLine("</entity>\n");
	    //	    }

	    return list;
	}


    }
}
