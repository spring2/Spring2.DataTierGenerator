using System;

using System.Collections;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Schema;

using Spring2.Core.Xml;

using Spring2.DataTierGenerator;
using Spring2.DataTierGenerator.Element;

namespace Spring2.DataTierGenerator.Parser {
    /// <summary>
    /// Summary description for XMLParser.
    /// </summary>
    public abstract class ParserSkeleton : IParser {

	protected Configuration options = new Configuration();
	protected IList entities = new ArrayList();
	protected IList enumtypes = new ArrayList();
	protected IList collections = new ArrayList();
	protected IList databases = new ArrayList();
	protected Hashtable types = new Hashtable();
	protected Hashtable sqltypes = new Hashtable();
	protected GeneratorElement generator = new GeneratorElement();
	protected ParserElement parser = new ParserElement();

	protected Boolean isValid = false;
	protected IList errors = new ArrayList();

	public Configuration Configuration {
	    get { return options; }
	}

	public Boolean IsValid {
	    get { return isValid; }
	}

	public Boolean HasWarnings {
	    get { return errors.Count>0; }
	}

	public IList Errors {
	    get { return errors; }
	}

	public String ErrorDescription {
	    get {
		String s = String.Empty;
		foreach(Object o in errors) {
		    s += o.ToString() + Environment.NewLine;
		}
		return s;
	    }
	}

	public IList Databases {
	    get { return (ArrayList)((ArrayList)databases).Clone(); }
	}

	public IList Entities {
	    get { return (ArrayList)((ArrayList)entities).Clone(); }
	}

	public IList Enums {
	    get { return (ArrayList)((ArrayList)enumtypes).Clone(); }
	}

	public IList Collections {
	    get { return (ArrayList)((ArrayList)collections).Clone(); }
	}

	public ICollection Types {
	    get { return types.Values; }
	}

	public ICollection SqlTypes {
	    get { return sqltypes.Values; }
	}

	public GeneratorElement Generator {
	    get { return generator; }
	}

	public ParserElement Parser {
	    get { return parser; }
	}

	/// <summary>
	/// Event handler for parser validation events
	/// </summary>
	/// <param name="args"></param>
	protected void ParserValidationEventHandler(ParserValidationArgs args) {
	    if (args.Severity.Equals(ParserValidationSeverity.ERROR)) {
		isValid = false;
	    }
	    errors.Add(args);
	}


	/// <summary>
	/// Post-parse validations
	/// </summary>
	/// <param name="vd"></param>
	protected void Validate(ParserValidationDelegate vd) {
	    //TODO: walk through collection to make sure that cross relations are correct.

	    foreach (DatabaseElement database in databases) {
		foreach(SqlEntityElement sqlentity in database.SqlEntities) {
		    if (sqlentity.GetPrimaryKeyColumns().Count==0 && (sqlentity.GenerateDeleteStoredProcScript || sqlentity.GenerateUpdateStoredProcScript || sqlentity.GenerateInsertStoredProcScript)) {
			vd(ParserValidationArgs.NewWarning("SqlEntity " + sqlentity.Name + " does not have any primary key columns defined."));
		    }

		    if (!sqlentity.HasUpdatableColumns() && sqlentity.GenerateUpdateStoredProcScript) {
			vd(ParserValidationArgs.NewWarning("SqlEntity " + sqlentity.Name + " does not have any editable columns and does not have generateupdatestoredprocscript=\"false\" specified."));
		    }

		}
	    }

	    // make sure that all columns are represented in entities
	    foreach(EntityElement entity in entities) {
		if (entity.SqlEntity.Name.Length>0) {
		    foreach(ColumnElement column in entity.SqlEntity.Columns) {
			if (entity.FindFieldByColumnName(column.Name)==null) {
			    vd(ParserValidationArgs.NewWarning("could not find field representing column " + column.Name + " in entity " + entity.Name + "."));
			}
		    }
		}
	    }
	}


    }
}
