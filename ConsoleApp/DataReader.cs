using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

//All things considered the operations included in this algorithm could work on a proper SQL database.
//Using LINQ expresions and EF would be a lot better idea, however I can think of many situations, where the CSV format is the only way. 
namespace ConsoleApp
{

    public class DataReader
    {
        IEnumerable<ImportedObject> ImportedObjects;

        public void ImportAndPrintData(string fileToImport, bool printData = true)
        {
            //A type created for storing the table (TextFieldParser or CsvHelper would be probably a better idea here)
            ImportedObjects = new List<ImportedObject>() { new ImportedObject() };
            //Opening a stream to file
            using(var streamReader = new StreamReader(fileToImport))//if not .NET tool then resource management should be in place
            {
                //A helper variable to convey the CSV data
                var importedLines = new List<string>();
                while (!streamReader.EndOfStream)//until no end of file
                {
                    //save one line of CSV file
                    var line = streamReader.ReadLine();
                    //And add that line to the list 
                    importedLines.Add(line);
                }

                //deserialize the read file into ImportedObject type
                for (int i = 0; i < importedLines.Count; i++)
                {
                    var importedLine = importedLines[i];
                    if (importedLine != string.Empty)
                    {
                        var values = importedLine.Split(';');
                        var importedObject = new ImportedObject();
                        importedObject.Type = values[0];
                        importedObject.Name = values[1];
                        importedObject.schema = values[2];
                        importedObject.parentName = values[3];
                        importedObject.ParentType = values[4];
                        importedObject.DataType = values[5];
                        importedObject.IsNullable = values[6];
                        ((List<ImportedObject>)ImportedObjects).Add(importedObject);
                    }

                }
            }

            // clear and correct imported data
            // improved the null exception handling by adding the null conditional
            foreach (var importedObject in ImportedObjects)
            {
                importedObject.Type = importedObject.Type?.Trim().Replace(" ", "").Replace(Environment.NewLine, "").ToUpper();
                importedObject.Name = importedObject.Name?.Trim().Replace(" ", "").Replace(Environment.NewLine, "");
                importedObject.schema = importedObject.schema?.Trim().Replace(" ", "").Replace(Environment.NewLine, "");
                importedObject.parentName = importedObject.parentName?.Trim().Replace(" ", "").Replace(Environment.NewLine, "");
                importedObject.ParentType = importedObject.ParentType?.Trim().Replace(" ", "").Replace(Environment.NewLine, "");
            }

            // assign number of children
            // incressed the start index to skip the header line and null line
            for (int i = 2; i < ImportedObjects.Count(); i++)
            {
                var importedObject = ImportedObjects.ToArray()[i];//assign the first element of ImpoprtedObjects to local variable

                foreach (var impObj in ImportedObjects) //loop through elements of ImportedObjects
                {
                    //match the Parent.Type and Parent.Name
                    if (impObj.ParentType == importedObject.Type)
                    {
                        if (impObj.parentName == importedObject.Name)
                        {
                            //If both criteria are filled than increse the number of children
                            importedObject.numberOfChildren = 1 + importedObject.numberOfChildren;
                            //the importedObject variable should be saved somewhere?? otherwise it is useless
                            //added the assignment of the updated value to proper filed in the database object
                            ((List<ImportedObject>)ImportedObjects)[i].numberOfChildren = importedObject.numberOfChildren;
                        }
                    }
                }
            }
            //loop through database object and print only "DATABASE" type
            foreach (var database in ImportedObjects)
            {
                if (database.Type == "DATABASE")
                {
                    Console.WriteLine($"Database '{database.Name}' ({database.numberOfChildren} tables)");

                    // print all database's tables
                    foreach (var table in ImportedObjects)
                    {
                        //print only children
                        if (table.ParentType?.ToUpper() == database?.Type)
                        {
                            if (table.parentName == database.Name)
                            {
                                Console.WriteLine($"\tTable '{table.schema}.{table.Name}' ({table.numberOfChildren} columns)");

                                // print all table's columns
                                foreach (var column in ImportedObjects)
                                {
                                    if (column.ParentType?.ToUpper() == table?.Type)
                                    {
                                        if (column.parentName == table.Name)
                                        {
                                            Console.WriteLine($"\t\tColumn '{column.Name}' with {column.DataType} data type {(column.IsNullable == "1" ? "accepts nulls" : "with no nulls")}");
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            Console.ReadLine();
        }
    }

    class ImportedObject : ImportedObjectBaseClass
    {
        public string Name //Why? I would delete this and use the base class but it could be needed for some kind of naming convention or compatibility with other object types
        {
            get;
            set;
        }
        public string schema; //if those are variables and not properities they should be written with lowercase firs letter

        public string parentName;//although they probably should all be properties
        public string ParentType
        {
            get; set;
        }

        public string DataType { get; set; }
        public string IsNullable { get; set; }

        public double numberOfChildren;
    }

    class ImportedObjectBaseClass
    {
        public string Name { get; set; }
        public string Type { get; set; }
    }
}
