using System;
using System.Data;
using System.IO;
using System.Collections;
using System.Text;

namespace Spring2.DataTierGenerator {
    /// <summary>
    /// Generates stored procedures and associated data access code for the specified database.
    /// </summary>
    public class DAOGenerator : GeneratorBase {

	/// <summary>
	/// Contructor for the Generator class.
	/// </summary>
	/// <param name="strConnectionString">Connecion string to a SQL Server database.</param>
	public DAOGenerator(Configuration options, StreamWriter writer, Entity entity, ArrayList fields) : base(options, writer, entity, fields) {
	}
		

	/// <summary>
	/// Creates a C# data access class for all of the table's stored procedures.
	/// </summary>
	/// <param name="table">Name of the table the class should be generated for.</param>
	/// <param name="fields">ArrayList object containing one or more Field objects as defined in the table.</param>
	public void CreateDataAccessClass() {
	    StreamWriter	objStreamWriter;
	    StringBuilder	sb;
	    string			strFileName;
			
	    sb = new StringBuilder(4096);

	    // Create the header for the class
	    sb.Append("using System;\n");
	    sb.Append("using System.Data;\n");
	    sb.Append("using System.Data.SqlClient;\n");
	    sb.Append("using System.Configuration;\n");
	    sb.Append("using System.Collections;\n");
	    sb.Append("using Spring2.Core.DAO;\n");
	    sb.Append("using ").Append(options.GetDONameSpace(null)).Append(";\n");

		Hashtable namespaces = new Hashtable();
		foreach (Field field in fields) {
		    TypeData type = (TypeData)entity.Types[field.Type.Name];
		    if (!type.Package.Equals(String.Empty) && !namespaces.Contains(type.Package)) {
			namespaces.Add(type.Package, type.Package);
		    }		
		}
		foreach (Object o in namespaces.Keys) {
		    sb.Append("using ").Append(o.ToString()).Append(";\n");
		}

	    sb.Append("\n");
	    sb.Append("namespace " + options.GetDAONameSpace(entity.Name) + " {\n");
	    sb.Append("\tpublic class " + options.GetDAOClassName(entity.Name) + " : Spring2.Core.DAO.EntityDAO {\n");

	    //sb.Append("\n\t\tprivate readonly String TABLE=\"").Append(table).Append("\";\n\n");
	    sb.Append("\n\t\tprivate readonly String VIEW=\"vw").Append(entity.Name).Append("\";\n\n");

	    CreateDAOListMethods(sb);

	    // Append the access methods
	    CreateInsertMethod(sb);
	    sb.Append("\n\n");
	    CreateUpdateMethod(sb);
	    sb.Append("\n\n");
	    CreateDeleteMethods(sb);

	    if (options.GenerateSelectStoredProcs) {
		sb.Append("\n\n");
		CreateSelectMethods(sb);
	    }
		
	    // Close out the class and namespace
	    sb.Append("\t}\n");
	    sb.Append("}\n");

	    // Create the output stream
	    strFileName = options.RootDirectory + options.DaoClassDirectory + "\\" + options.GetDAOClassName(entity.Name) + ".cs";
	    if (File.Exists(strFileName))
		File.Delete(strFileName);
	    objStreamWriter = new StreamWriter(strFileName);
	    objStreamWriter.Write(sb.ToString());
	    objStreamWriter.Close();
	    objStreamWriter = null;
	    sb = null;
	}

		
	/// <summary>
	/// Creates a string that represents the insert functionality of the data access class.
	/// </summary>
	/// <param name="table">Name of the table the data access class is for.</param>
	/// <param name="fields">ArrayList object containing one or more Field objects as defined in the table.</param>
	/// <param name="sb">StreamBuilder object that the resulting string should be appended to.</param>
	private void CreateInsertMethod(StringBuilder sb) {
	    Field	objField;
	    int			intIndex;
	    bool		blnReturnVoid;
			
	    // Append the method header
	    sb.Append("\t\t/// <summary>\n");
	    sb.Append("\t\t/// Inserts a record into the " + entity.Name + " table.\n");
	    sb.Append("\t\t/// </summary>\n");
	    sb.Append("\t\t/// <param name=\"\"></param>\n");
			
	    // Determine the return type of the insert function
	    blnReturnVoid = true;
	    for (intIndex = 0; intIndex < fields.Count; intIndex++) {
		objField = (Field)fields[intIndex];
		if (objField.IsIdentity) {
		    sb.Append("\t\tpublic Int32 Insert(");
		    blnReturnVoid = false;
		    break;
		} else if (objField.IsRowGuidCol) {
		    sb.Append("\t\tpublic Guid Insert(");
		    blnReturnVoid = false;
		    break;
		}
	    }
			
	    if (blnReturnVoid)
		sb.Append("\t\tpublic void Insert(");
			
	    // Append the method call parameters
	    /*			for (intIndex = 0; intIndex < fields.Count; intIndex++) {
			    objField = (Field)fields[intIndex];
			    if (objField.IsIdentity == false && objField.IsRowGuidCol == false) {
				strMethodParameter = CreateMethodParameter(objField);
				sb.Append(strMethodParameter);
				sb.Append(", ");
			    }
			    objField = null;
			}
	    */			

	    // Append the method call parameters - data object
	    sb.Append(entity.Name).Append("Data data, ");

	    // Append the connection string parameter
	    sb.Append("SqlConnection connection");
			
	    // Append the method header
	    sb.Append(") {\n");
			
	    // Append the variable declarations
	    //sb.Append("\t\t\tSqlConnection	objConnection;\n");
	    sb.Append("\t\t\tSqlCommand cmd;\n");
	    sb.Append("\n");

	    // Append the try block
	    //            sb.Append("\t\t\ttry {\n");

	    sb.Append(GetCreateCommandSection(options.GetProcName(entity.Name, "Insert")));
			
	    sb.Append("\t\t\t\tSqlParameter rv = cmd.Parameters.Add(\"RETURN_VALUE\", SqlDbType.Int);\n");
	    sb.Append("\t\t\t\trv.Direction = ParameterDirection.ReturnValue;\n");

	    // Append the parameter appends  ;)
	    sb.Append("\t\t\t\t//Create the parameters and append them to the command object\n");
	    for (intIndex = 0; intIndex < fields.Count; intIndex++) {
		objField = (Field)fields[intIndex];
		if (!objField.IsViewColumn) {
		    if (objField.IsIdentity || objField.IsRowGuidCol) {
			//sb.Append("\t\t\t\t" + objField.CreateSqlParameter(true, true));
		    } else {
			sb.Append("\t\t\t\t" + objField.CreateSqlParameter(false, true));
		    }
		}
		objField = null;
	    }
	    sb.Append("\n");

	    // Append the execute statement
	    sb.Append("\t\t\t\t// Execute the query\n");
	    sb.Append("\t\t\t\tcmd.ExecuteNonQuery();\n");
			
	    // Append the parameter value extraction
	    for (intIndex = 0; intIndex < fields.Count; intIndex++) {
		objField = (Field)fields[intIndex];
		if (objField.IsIdentity || objField.IsRowGuidCol) {
		    sb.Append("\n\t\t\t\t// Set the output paramter value(s)\n");
		    if (objField.IsIdentity)
			//sb.Append("\t\t\t\treturn Int32.Parse(cmd.Parameters[\"@" + objField.Name.Replace(" ", "_") + "\"].Value.ToString());\n");
			sb.Append("\t\t\t\treturn Int32.Parse(cmd.Parameters[\"RETURN_VALUE\"].Value.ToString());\n");
		    else
			sb.Append("\t\t\t\treturn Guid.NewGuid(cmd.Parameters[\"@" + objField.Name.Replace(" ", "_") + "\"].Value.ToString());\n");
		}
		objField = null;
	    }
			
	    // Append the catch block
	    //            sb.Append("\t\t\t} catch (Exception objException) {\n");
	    //            sb.Append("\t\t\t\tthrow (new Exception(\"" + entity.Name.Replace(" ", "_") + ".Insert\\n\\n\" + objException.Message));\n");
	    //            sb.Append("\t\t\t}\n");
			
	    // Append the method footer
	    sb.Append("\t\t}\n");
	}


	/// <summary>
	/// Creates a string that represents the update functionality of the data access class.
	/// </summary>
	/// <param name="table">Name of the table the data access class is for.</param>
	/// <param name="fields">ArrayList object containing one or more Field objects as defined in the table.</param>
	/// <param name="sb">StreamBuilder object that the resulting string should be appended to.</param>
	private void CreateUpdateMethod(StringBuilder sb) {
	    Field objField;
	    Field objNewField;
	    Field objOldField;
	    //string		strMethodParameter;
	    int			intIndex;
			
	    // Append the method header
	    sb.Append("\t\t/// <summary>\n");
	    sb.Append("\t\t/// Updates a record in the " + entity.Name + " table.\n");
	    sb.Append("\t\t/// </summary>\n");
	    sb.Append("\t\t/// <param name=\"\"></param>\n");
	    sb.Append("\t\tpublic void Update(");
			
	    // Append the method call parameters
	    /*			for (intIndex = 0; intIndex < fields.Count; intIndex++) {
			    objField = (Field)fields[intIndex];
			    if (objField.IsPrimaryKey && objField.IsIdentity == false && objField.IsRowGuidCol == false) {
				objOldField = objField.Copy();
				objOldField.Name = "Old" + objOldField.Name;
				strMethodParameter = CreateMethodParameter(objOldField);
				sb.Append(strMethodParameter);
				sb.Append(", ");
					
				objNewField = objField.Copy();
				objNewField.Name = "New" + objNewField.Name;
				strMethodParameter = CreateMethodParameter(objNewField);
				sb.Append(strMethodParameter);
				sb.Append(", ");
			    } else {
				strMethodParameter = CreateMethodParameter(objField);
				sb.Append(strMethodParameter);
				sb.Append(", ");
			    }
			    objField = null;
			}
	    */
	    // Append the method call parameters - data object
	    sb.Append(entity.Name).Append("Data data, ");

			
	    // Append the connection string parameter
	    sb.Append("SqlConnection connection");
			
	    // Append the method header
	    sb.Append(") {\n");
			
	    // Append the variable declarations
	    //sb.Append("\t\t\tSqlConnection	objConnection;\n");
	    sb.Append("\t\t\tSqlCommand cmd;\n");
	    sb.Append("\n");

	    // Append the try block
	    //            sb.Append("\t\t\ttry {\n");

	    sb.Append(GetCreateCommandSection(options.GetProcName(entity.Name, "Update")));
			
	    // Append the parameter appends  ;)
	    sb.Append("\t\t\t\t//Create the parameters and append them to the command object\n");
	    for (intIndex = 0; intIndex < fields.Count; intIndex++) {
		objField = (Field)fields[intIndex];
		if (!objField.IsViewColumn) {
		    //                    if (objField.IsPrimaryKey && objField.IsIdentity == false && objField.IsRowGuidCol == false) {
		    //                        objOldField = objField.Copy();
		    //                        objOldField.Name = "Old" + objOldField.Name;
		    //                        sb.Append("\t\t\t\t" + objOldField.CreateSqlParameter(false, true));
		    //					
		    //                        objNewField = objField.Copy();
		    //                        objNewField.Name = "New" + objNewField.Name;
		    //                        sb.Append("\t\t\t\t" + objNewField.CreateSqlParameter(false, true));
		    //                    } else {
		    sb.Append("\t\t\t\t" + objField.CreateSqlParameter(false, true));
		    //                    }
		}
		objField = null;
	    }
	    sb.Append("\n");
			
	    // Append the execute statement
	    sb.Append("\t\t\t\t// Execute the query\n");
	    sb.Append("\t\t\t\tcmd.ExecuteNonQuery();\n");
			
	    // Append the catch block
	    //            sb.Append("\t\t\t} catch (Exception objException) {\n");
	    //            sb.Append("\t\t\t\tthrow (new Exception(\"" + entity.Name.Replace(" ", "_") + ".Update\\n\\n\" + objException.Message));\n");
	    //            sb.Append("\t\t\t}\n");
			
	    // Append the method footer
	    sb.Append("\t\t}\n");
	}


	/// <summary>
	/// Creates a string that represents the delete functionality of the data access class.
	/// </summary>
	/// <param name="table">Name of the table the data access class is for.</param>
	/// <param name="fields">ArrayList object containing one or more Field objects as defined in the table.</param>
	/// <param name="sb">StreamBuilder object that the resulting string should be appended to.</param>
	private void CreateDeleteMethods(StringBuilder sb) {
	    Field	objField;
	    int			intIndex;
	    string		strColumnName;
	    string		strPrimaryKeyList;
	    ArrayList	arrKeyList;
			
	    // Create the array list of key fields
	    strPrimaryKeyList = "";
	    arrKeyList = new ArrayList();
	    for (intIndex = 0; intIndex < fields.Count; intIndex++) {
		objField = (Field)fields[intIndex];
		if (objField.IsPrimaryKey) {
		    arrKeyList.Add(objField);
		    strPrimaryKeyList += objField.Name.Replace(" ", "_") + "_";
		}
		objField = null;
	    }
			
	    // Trim off the last underscore
	    if (strPrimaryKeyList.Length > 0)
		strPrimaryKeyList = strPrimaryKeyList.Substring(0, strPrimaryKeyList.Length - 1);

	    /*********************************************************************************************************/
	    // Create the remaining select functions based on identity columns or uniqueidentifiers
	    for (intIndex = 0; intIndex < fields.Count; intIndex++) {
		objField = (Field)fields[intIndex];
			
		if (objField.IsIdentity || (options.GenerateProcsForForeignKey && (objField.IsRowGuidCol || objField.IsPrimaryKey || objField.IsForeignKey))) {
		    strColumnName = objField.Name.Substring(0, 1).ToUpper() + objField.Name.Substring(1);
					
		    String methodName = "Delete" + strColumnName.Replace(" ", "_");
		    // if this option is on, only generate the PK 
		    if (options.GenerateOnlyPrimaryDeleteStoredProc) {
			arrKeyList.Clear();
			methodName = "Delete";
		    }

		    // Append the method header
		    sb.Append("\t\t/// <summary>\n");
		    sb.Append("\t\t/// Deletes a record from the " + entity.Name + " table by " + objField.Name + ".\n");
		    sb.Append("\t\t/// </summary>\n");
		    sb.Append("\t\t/// <param name=\"\"></param>\n");
		    //sb.Append("\t\tpublic void DeleteBy" + strColumnName.Replace(" ", "_") + "(" + objField.CreateMethodParameter() + ", SqlConnection connection) {\n");
		    sb.Append("\t\tpublic void " + methodName + "(" + objField.CreateMethodParameter() + ", SqlConnection connection) {\n");
					
		    // Append the variable declarations
		    //                    sb.Append("\t\t\tSqlConnection	objConnection;\n");
		    sb.Append("\t\t\tSqlCommand cmd;\n");
		    sb.Append("\n");

		    // Append the try block
		    //                    sb.Append("\t\t\ttry {\n");

		    sb.Append(GetCreateCommandSection(options.GetProcName(entity.Name, methodName)));

		    // Append the parameters
		    sb.Append("\t\t\t\t// Create and append the parameters\n");
		    sb.Append("\t\t\t\t" + objField.CreateSqlParameter(false, false));
		    sb.Append("\n");

		    // Append the execute statement
		    sb.Append("\t\t\t\t// Execute the query and return the result\n");
		    sb.Append("\t\t\t\tcmd.ExecuteNonQuery();\n");
					
		    // Append the catch block
		    //                    sb.Append("\t\t\t} catch (Exception objException) {\n");
		    //                    sb.Append("\t\t\t\tthrow (new Exception(\"" + table.Replace(" ", "_") + "." + methodName + "\\n\\n\" + objException.Message));\n");
		    //                    sb.Append("\t\t\t}\n");
					
		    // Append the method footer
		    if (arrKeyList.Count > 0)
			sb.Append("\t\t}\n\n\n");
		    else
			sb.Append("\t\t}\n\n");
				
		    objField = null;
		}
	    }

	    /*********************************************************************************************************/
	    // Create the select functions based on a composite primary key
	    if (arrKeyList.Count > 1) {
		// Append the method header
		sb.Append("\t\t/// <summary>\n");
		sb.Append("\t\t/// Deletes a record from the " + entity.Name + " table by a composite primary key.\n");
		sb.Append("\t\t/// </summary>\n");
		sb.Append("\t\t/// <param name=\"\"></param>\n");

		String methodName = "";
		if (options.GenerateOnlyPrimaryDeleteStoredProc) {
		    methodName = "Delete";
		} else {
		    methodName = "DeleteBy" + strPrimaryKeyList;
		}
				
		sb.Append("\t\tpublic void " + methodName + "(");
		for (intIndex = 0; intIndex < arrKeyList.Count; intIndex++) {
		    objField = (Field)arrKeyList[intIndex];
		    sb.Append(objField.CreateMethodParameter() + ", ");
		}
		sb.Append("SqlConnection connection) {\n");
				
		// Append the variable declarations
		//                sb.Append("\t\t\tSqlConnection	objConnection;\n");
		sb.Append("\t\t\tSqlCommand cmd;\n");
		sb.Append("\n");

		// Append the try block
		//                sb.Append("\t\t\ttry {\n");

		sb.Append(GetCreateCommandSection(options.GetProcName(entity.Name, methodName)));

		// Append the parameters
		sb.Append("\t\t\t\t// Create and append the parameters\n");
		for (intIndex = 0; intIndex < arrKeyList.Count; intIndex++) {
		    objField = (Field)arrKeyList[intIndex];
		    sb.Append("\t\t\t\t" + objField.CreateSqlParameter(false, false));
		    objField = null;
		}
		sb.Append("\n");

		// Append the execute statement
		sb.Append("\t\t\t\t// Execute the query and return the result\n");
		sb.Append("\t\t\t\tcmd.ExecuteNonQuery();\n");
				
		// Append the catch block
		//                sb.Append("\t\t\t} catch (Exception objException) {\n");
		//                sb.Append("\t\t\t\tthrow (new Exception(\"" + entity.Name.Replace(" ", "_") + "." + methodName + "\\n\\n\" + objException.Message));\n");
		//                sb.Append("\t\t\t}\n");
				
		// Append the method footer
		sb.Append("\t\t}\n\n\n");
			
		objField = null;
	    }
	}


	/// <summary>
	/// Creates a string that represents the select functionality of the data access class.
	/// </summary>
	/// <param name="table">Name of the table the data access class is for.</param>
	/// <param name="fields">ArrayList object containing one or more Field objects as defined in the table.</param>
	/// <param name="sb">StreamBuilder object that the resulting string should be appended to.</param>
	private void CreateSelectMethods(StringBuilder sb) {
	    Field	objField;
	    int			intIndex;
	    string		strColumnName;
	    string		strPrimaryKeyList;
	    ArrayList	arrKeyList;
			
	    // Create the array list of key fields
	    strPrimaryKeyList = "";
	    arrKeyList = new ArrayList();
	    for (intIndex = 0; intIndex < fields.Count; intIndex++) {
		objField = (Field)fields[intIndex];
		if (objField.IsPrimaryKey) {
		    arrKeyList.Add(objField);
		    strPrimaryKeyList += objField.Name.Replace(" ", "_") + "_";
		}
		objField = null;
	    }
			
	    // Trim off the last underscore
	    if (strPrimaryKeyList.Length > 0)
		strPrimaryKeyList = strPrimaryKeyList.Substring(0, strPrimaryKeyList.Length - 1);

	    /*********************************************************************************************************/
	    // Create the initial "select all" function
			
	    // Append the method header
	    sb.Append("\t\t/// <summary>\n");
	    sb.Append("\t\t/// Selects a record from the " + entity.Name + " table.\n");
	    sb.Append("\t\t/// </summary>\n");
	    sb.Append("\t\t/// <param name=\"\"></param>\n");
	    sb.Append("\t\tpublic SqlDataReader Select(SqlConnection connection) {\n");
			
	    // Append the variable declarations
	    //            sb.Append("\t\t\tSqlConnection	objConnection;\n");
	    sb.Append("\t\t\tSqlCommand cmd;\n");
	    sb.Append("\n");

	    // Append the try block
	    sb.Append("\t\t\ttry {\n");

	    sb.Append(GetCreateCommandSection(options.GetProcName(entity.Name, "Select")));

	    // Append the execute statement
	    sb.Append("\t\t\t\t// Execute the query and return the result\n");
	    sb.Append("\t\t\t\treturn cmd.ExecuteReader(CommandBehavior.CloseConnection);\n");
			
	    // Append the catch block
	    sb.Append("\t\t\t} catch (Exception objException) {\n");
	    sb.Append("\t\t\t\tthrow (new Exception(\"" + entity.Name.Replace(" ", "_") + ".Select\\n\\n\" + objException.Message));\n");
	    sb.Append("\t\t\t}\n");
			
	    // Append the method footer
	    sb.Append("\t\t}\n\n\n");

	    /*********************************************************************************************************/
	    // Create the remaining select functions based on identity columns or uniqueidentifiers
	    for (intIndex = 0; intIndex < fields.Count; intIndex++) {
		objField = (Field)fields[intIndex];
			
		if (objField.IsIdentity || objField.IsRowGuidCol || objField.IsPrimaryKey || objField.IsForeignKey) {
		    strColumnName = objField.Name.Substring(0, 1).ToUpper() + objField.Name.Substring(1);
				
		    // Append the method header
		    sb.Append("\t\t/// <summary>\n");
		    sb.Append("\t\t/// Selects a record from the " + entity.Name + " table by " + objField.Name + ".\n");
		    sb.Append("\t\t/// </summary>\n");
		    sb.Append("\t\t/// <param name=\"\"></param>\n");
		    sb.Append("\t\tpublic SqlDataReader SelectBy" + strColumnName.Replace(" ", "_") + "(" + objField.CreateMethodParameter() + ", SqlConnection connection) {\n");
					
		    // Append the variable declarations
		    //                    sb.Append("\t\t\tSqlConnection	objConnection;\n");
		    sb.Append("\t\t\tSqlCommand cmd;\n");
		    sb.Append("\n");

		    // Append the try block
		    sb.Append("\t\t\ttry {\n");

		    sb.Append(GetCreateCommandSection(options.GetProcName(entity.Name, options.GetProcName(entity.Name, "SelectBy" + strColumnName.Replace(" ", "_")))));

		    // Append the parameters
		    sb.Append("\t\t\t\t// Create and append the parameters\n");
		    sb.Append("\t\t\t\t" + objField.CreateSqlParameter(false, false));
		    sb.Append("\n");

		    // Append the execute statement
		    sb.Append("\t\t\t\t// Execute the query and return the result\n");
		    sb.Append("\t\t\t\treturn cmd.ExecuteReader(CommandBehavior.CloseConnection);\n");
					
		    // Append the catch block
		    sb.Append("\t\t\t} catch (Exception objException) {\n");
		    sb.Append("\t\t\t\tthrow (new Exception(\"" + entity.Name.Replace(" ", "_") + ".SelectBy" + strColumnName.Replace(" ", "_") + "\\n\\n\" + objException.Message));\n");
		    sb.Append("\t\t\t}\n");
					
		    // Append the method footer
		    if (arrKeyList.Count > 0)
			sb.Append("\t\t}\n\n\n");
		    else
			sb.Append("\t\t}\n\n");
				
		    objField = null;
		}
	    }

	    /*********************************************************************************************************/
	    // Create the select functions based on a composite primary key
	    if (arrKeyList.Count > 1) {
		// Append the method header
		sb.Append("\t\t/// <summary>\n");
		sb.Append("\t\t/// Selects a record from the " + entity.Name + " table by a composite primary key.\n");
		sb.Append("\t\t/// </summary>\n");
		sb.Append("\t\t/// <param name=\"\"></param>\n");
				
		sb.Append("\t\tpublic SqlDataReader SelectBy" + strPrimaryKeyList + "(");
		for (intIndex = 0; intIndex < arrKeyList.Count; intIndex++) {
		    objField = (Field)arrKeyList[intIndex];
		    sb.Append(objField.CreateMethodParameter() + ", ");
		}
		sb.Append("SqlConnection connection) {\n");
				
		// Append the variable declarations
		//                sb.Append("\t\t\tSqlConnection	objConnection;\n");
		sb.Append("\t\t\tSqlCommand cmd;\n");
		sb.Append("\n");

		// Append the try block
		sb.Append("\t\t\ttry {\n");

		sb.Append(GetCreateCommandSection(options.GetProcName(entity.Name, options.GetProcName(entity.Name, "SelectBy" + strPrimaryKeyList))));

		// Append the parameters
		sb.Append("\t\t\t\t// Create and append the parameters\n");
		for (intIndex = 0; intIndex < arrKeyList.Count; intIndex++) {
		    objField = (Field)arrKeyList[intIndex];
		    sb.Append("\t\t\t\t" + objField.CreateSqlParameter(false, false));
		    objField = null;
		}
		sb.Append("\n");

		// Append the execute statement
		sb.Append("\t\t\t\t// Execute the query and return the result\n");
		sb.Append("\t\t\t\treturn cmd.ExecuteReader(CommandBehavior.CloseConnection);\n");
				
		// Append the catch block
		sb.Append("\t\t\t} catch (Exception objException) {\n");
		sb.Append("\t\t\t\tthrow (new Exception(\"" + entity.Name.Replace(" ", "_") + ".SelectBy" + strPrimaryKeyList + "\\n\\n\" + objException.Message));\n");
		sb.Append("\t\t\t}\n");
				
		// Append the method footer
		sb.Append("\t\t}\n\n\n");
			
		objField = null;
	    }
	}
		
	private void CreateDAOListMethods(StringBuilder sb) {
			
	    // Append the method header
	    sb.Append("\t\tprotected override String GetViewName() { \n");
	    sb.Append("\t\t	return VIEW; \n");
	    sb.Append("\t\t} \n");
	    sb.Append("\t\t \n");

	    // GetList method
	    sb.Append("\t\tpublic override ICollection GetList(IWhere whereClause, IOrderBy orderByClause, SqlConnection connection) { \n");
	    sb.Append("\t\t	SqlDataReader dataReader = GetListReader(whereClause, orderByClause, connection); \n");
	    sb.Append("\t\t	 \n");
	    sb.Append("\t\t	ArrayList list = new ArrayList(); \n");
	    sb.Append("\t\t	while (dataReader.Read()) { \n");
	    sb.Append("\t\t		list.Add(GetDataObjectFromReader(dataReader)); \n");
	    sb.Append("\t\t	} \n");
	    sb.Append("\t\t	dataReader.Close(); \n");
	    sb.Append("\t\t	return list; \n");
	    sb.Append("\t\t} \n");

	    // Load
	    sb.Append("\t\tpublic ").Append(options.GetDOClassName(entity.Name)).Append(" Load(Int32 id, SqlConnection connection) {\n");
	    sb.Append("\t\t	SqlDataReader dataReader = GetListReader(new WhereClause(\"").Append(Field.GetIdentityColumn(fields).Name).Append("\", id), null, connection);\n");
	    sb.Append("\t\t    \n");
	    sb.Append("\t\t	dataReader.Read();\n");
	    sb.Append("\t\t	return GetDataObjectFromReader(dataReader);\n");
	    sb.Append("\t\t}\n");
	    sb.Append("\n");			

	    // GetDataObjectFromReader
	    sb.Append("\t\tprivate ").Append(options.GetDOClassName(entity.Name)).Append(" GetDataObjectFromReader(SqlDataReader dataReader) {\n");
	    sb.Append("\t\t	").Append(options.GetDOClassName(entity.Name)).Append(" data = new ").Append(options.GetDOClassName(entity.Name)).Append("();\n");

	    foreach (Field field in fields) {
		    sb.Append("\t\t\tdata.").Append(field.GetMethodFormat()).Append(" = "); //dataReader.IsDBNull(dataReader.GetOrdinal(\"").Append(field.Name).Append("\")) ? ").Append(field.DataTypeClass).Append(".UNSET : ").Append("new ").Append(field.DataTypeClass).Append("(dataReader.Get").Append(field.ReaderType).Append("(").Append("dataReader.GetOrdinal(\"").Append(field.Name).Append("\")").Append("));\n");
		    //TypeData type = (TypeData)entity.Types[field.Type.Name];
		    //SqlTypeData sqltype = (SqlTypeData)entity.SqlTypes[field.SqlType.Name];
		    if (field.Type.ConvertFromSqlTypeFormat.Length >0) {
			sb.Append(String.Format(field.Type.ConvertFromSqlTypeFormat, "data", field.GetMethodFormat(), String.Format(field.SqlType.ReaderMethodFormat, "dataReader", field.GetMethodFormat()), "dataReader"));
		    } else {
			sb.Append(String.Format(field.SqlType.ReaderMethodFormat, "dataReader", field.GetMethodFormat()));
		    }
		    sb.Append(";\n");

		//				if (options.UseDataTypes) {
		//					switch(objField.SqlType.ToLower()) {
		//						case "image":
		//							sb.Append("\t\t\t//dataReader.GetBytes(").Append("dataReader.GetOrdinal(\"").Append(objField.Name).Append("\")").Append(", 0, data.").Append(objField.Name).Append(", 0, ").Append(objField.Length).Append(");\n");
		//							break;
		//						default: 
		//							if (objField.Constructor.Length>0) {
		//								sb.Append("\t\t\tdata.").Append(objField.GetMethodFormat()).Append(" = dataReader.IsDBNull(dataReader.GetOrdinal(\"").Append(objField.Name).Append("\")) ? ").Append(objField.DataTypeClass).Append(".UNSET : ").Append("").Append(objField.DataTypeClass).Append(".").Append(objField.Constructor).Append("(dataReader.Get").Append(objField.ReaderType).Append("(").Append("dataReader.GetOrdinal(\"").Append(objField.Name).Append("\")").Append("));\n");
		//							} else {
		//								sb.Append("\t\t\tdata.").Append(objField.GetMethodFormat()).Append(" = dataReader.IsDBNull(dataReader.GetOrdinal(\"").Append(objField.Name).Append("\")) ? ").Append(objField.DataTypeClass).Append(".UNSET : ").Append("new ").Append(objField.DataTypeClass).Append("(dataReader.Get").Append(objField.ReaderType).Append("(").Append("dataReader.GetOrdinal(\"").Append(objField.Name).Append("\")").Append("));\n");
		//							}
		//							break;
		//					}
		//				} else {
		//					switch (objField.SqlType.ToLower()) {
		//						case "bytes":
		//							sb.Append("\t\t\t//dataReader.GetBytes(").Append("dataReader.GetOrdinal(\"").Append(objField.Name).Append("\")").Append(", 0, data.").Append(objField.Name).Append(", 0, ").Append(objField.Length).Append(");\n");
		//							break;
		//						default: 
		//							sb.Append("\t\t\tdata.").Append(objField.Name).Append(" = ").Append("dataReader.Get").Append(objField.ReaderType).Append("(").Append("dataReader.GetOrdinal(\"").Append(objField.Name).Append("\")").Append(");\n");
		//							break;
		//					}
		//				}
		//				objField = null;
	    }
	    /*
	    sb.Append("\t\t	if (dataReader.IsDBNull(7)) { \n");
	    sb.Append("\t\t		data.LastUpdateUser = \"\";\n");
	    sb.Append("\t\t	} else {\n");
	    sb.Append("\t\t		data.LastUpdateUser = dataReader.GetString(7);\n");
	    sb.Append("\t\t	}\n");
	    */
	    sb.Append("\t\t\n");
	    sb.Append("\t\t	return data;\n");
	    sb.Append("\t\t}\n");
	    sb.Append("\n");			
	}


	private String GetCreateCommandSection(String procName) {
	    StringBuilder sb = new StringBuilder();
	    // Append the connection object creation
	    //                    sb.Append("\t\t\t\t// Create and open the database connection\n");
	    //                    sb.Append("\t\t\t\tobjConnection = new SqlConnection(ConfigurationSettings.AppSettings[\"ConnectionString\"]);\n");
	    //                    sb.Append("\t\t\t\tobjConnection.Open();\n");
	    //                    sb.Append("\n");
					
	    // Append the command object creation
	    sb.Append("\t\t\t\t// Create and execute the command\n");
	    //                    sb.Append("\t\t\t\tcmd = new SqlCommand();\n");
	    //					sb.Append("\t\t\t\tcmd.Connection = objConnection;\n");
	    sb.Append("\t\t\t\tcmd = GetSqlCommand(connection, \"" + procName + "\", CommandType.StoredProcedure);\n");

	    //sb.Append("\t\t\t\tcmd.CommandText = \"" + options.GetProcName(entity.Name, "DeleteBy" + strColumnName.Replace(" ", "_")) + "\";\n");
	    //sb.Append("\t\t\t\tcmd.CommandText = \"" + procName + "\";\n");
	    //sb.Append("\t\t\t\tcmd.CommandType = CommandType.StoredProcedure;\n");
	    sb.Append("\n");
	    return sb.ToString();
	}

    }
}
