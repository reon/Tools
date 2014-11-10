using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using ClientDBExtractor.Reader;
using Microsoft.CSharp;

namespace ClientDBExtractor
{
    class Program
    {
        static CASCHandler cascHandler;
        static CASCFolder root;

        static void Main(string[] args)
        {
            Console.WriteLine("Searching for available ClientDB (dbc & db2) files...");

            cascHandler = CASCHandler.OpenLocalStorage(Environment.CurrentDirectory + "\\");

            var fileList = new List<string>();

            // Get dbc files from wow bin
            using (var sr = new StreamReader(Environment.CurrentDirectory + "\\Wow.exe"))
            {
                var text = sr.ReadToEnd();
                var dbcFiles = Regex.Matches(text, @"DBFilesClient\\([A-Za-z0-9\-_]+)\.dbc");
                var db2Files = Regex.Matches(text, @"DBFilesClient\\([A-Za-z0-9\-_]+)\.db2");

                foreach (Match dbc in dbcFiles)
                    fileList.Add(dbc.Value.Replace(@"\\", @"\"));
                foreach (Match db2 in db2Files)
                    fileList.Add(db2.Value.Replace(@"\\", @"\"));
            }

            // not in wow bin
            fileList.Add(@"DBFilesClient\WorldSafeLocs.dbc");
            fileList.Add(@"DBFilesClient\AttackAnimKits.dbc");
            fileList.Add(@"DBFilesClient\AttackAnimTypes.dbc");
            fileList.Add(@"DBFilesClient\WowError_Strings.dbc");

            Console.WriteLine("Found {0} available ClientDB files.", fileList.Count);
            Console.WriteLine("Getting structures...");
            var existingStructList = new List<string>();
            var structsPath = Environment.CurrentDirectory + "/Structures/";
            var structureNames = Directory.GetFiles(structsPath);
            var structureNameList = new List<string>(structureNames.Length);

            foreach (var s in structureNames)
                structureNameList.Add(Path.GetFileNameWithoutExtension(s));

            foreach (var s in fileList)
            {
                var nameOnly = s.Replace(@"\\", "").Replace(@"DBFilesClient\", "").Replace(@".dbc", "").Replace(@".db2", "");

                if (structureNameList.Contains(nameOnly))
                    existingStructList.Add(s);
            }

            Console.WriteLine("Found {0} structures.", existingStructList.Count);
            root = cascHandler.LoadListFile(existingStructList);

            LocaleFlags locale = LocaleFlags.enUS;
            if (args.Length == 1)
                locale = (LocaleFlags)Enum.Parse(typeof(LocaleFlags), args[0]);

            AppDomain.CurrentDomain.AppendPrivatePath(Environment.CurrentDirectory);
            var counter = 0;
            if (!Directory.Exists("./DBFilesClient"))
                Directory.CreateDirectory("./DBFilesClient");

            Console.WriteLine("Creating DataDB.MySql.sql data...");

            if (File.Exists("./DBFilesClient/DataDB.MySql.sql"))
                File.Delete("./DBFilesClient/DataDB.MySql.sql");

            using (var datadbsql = new StreamWriter("./DBFilesClient/DataDB.MySql.sql", true))
            {

                foreach (var file in root.GetFiles())
                {
                    {
                        Console.WriteLine("Extracting '{0}'...", file.FullName);

                        var dbStream = cascHandler.SaveFileTo(file.FullName, "./", locale);
                        if (dbStream != null)
                        {
                            var nameOnly = file.FullName.Replace(@"\\", "").Replace(@"DBFilesClient\", "").Replace(@".dbc", "").Replace(@".db2", "");
                            var pluralized = nameOnly.Pluralize();
                            var ccp = new CSharpCodeProvider();
                            var paramss = new CompilerParameters();
                            paramss.GenerateExecutable = false;
                            paramss.GenerateInMemory = true;

                            paramss.ReferencedAssemblies.Add("ClientDBExtractor.exe");

                            var file1 = File.ReadAllText(structsPath + nameOnly + ".cs");
                            var code = new[] { file1 };

                            var result = ccp.CompileAssemblyFromSource(paramss, code);
                            var type = result.CompiledAssembly.GetTypes()[0];

                            var dbTable = DBReader.Read(dbStream, type);

                            // create scheme
                            var sbScheme = new StringBuilder();
                            sbScheme.AppendLine(string.Format("DROP TABLE IF EXISTS `{0}`;", pluralized));
                            sbScheme.AppendLine(string.Format("CREATE TABLE `{0}` (", pluralized));

                            foreach (DataColumn c in dbTable.Columns)
                            {
                                var typeName = "";

                                switch (c.DataType.Name)
                                {
                                    case "SByte":
                                    case "Byte":
                                        typeName = "tinyint(3) unsigned NOT NULL";
                                        break;
                                    case "Int32":
                                        if (c.ColumnName == "Race" || c.ColumnName == "RaceId" || c.ColumnName == "Class" || c.ColumnName == "ClassId"
                                                || c.ColumnName == "Sex" || c.ColumnName == "Gender")
                                            typeName = "tinyint(3) unsigned NOT NULL";
                                        else if (c.ColumnName == "ID" || c.ColumnName == "Id" || c.ColumnName == nameOnly + "Id" || c.ColumnName == nameOnly + "ID"
                                            || c.ColumnName == "SpellId"
                                            || c.ColumnName == "Spell" || c.ColumnName.EndsWith("Mask"))
                                        {
                                            typeName = "int(11) unsigned NOT NULL";
                                        }

                                        else
                                            typeName = "int(11) NOT NULL";

                                        if ((pluralized == "ChrClasses" && c.ColumnName == "Id") || (pluralized == "ChrRaces" && c.ColumnName == "Id"))
                                            typeName = "tinyint(3) unsigned NOT NULL";

                                        break;
                                    case "UInt32":
                                        if (c.ColumnName == "Race" || c.ColumnName == "RaceId" || c.ColumnName == "Class" || c.ColumnName == "ClassId"
                                                || c.ColumnName == "Sex" || c.ColumnName == "Gender")
                                            typeName = "tinyint(3) unsigned NOT NULL";
                                        else
                                            typeName = "int(11) unsigned NOT NULL";

                                        if ((pluralized == "ChrClasses" && c.ColumnName == "Id") || (pluralized == "ChrRaces" && c.ColumnName == "Id"))
                                            typeName = "tinyint(3) unsigned NOT NULL";
                                        break;
                                    case "Int64":
                                        if (c.ColumnName == "ID" || c.ColumnName == "Id" || c.ColumnName == nameOnly + "Id" || c.ColumnName == nameOnly + "ID")

                                            typeName = "bigint(20) NOT NULL";
                                        else
                                            typeName = "bigint(20) unsigned NOT NULL";
                                        break;
                                    case "UInt64":
                                        typeName = "bigint(20) unsigned NOT NULL";
                                        break;
                                    case "Single":
                                        typeName = "float NOT NULL";
                                        break;
                                    case "Boolean":
                                        typeName = "tinyint(1) NOT NULL";
                                        break;
                                    case "String":
                                        typeName = "text";
                                        break;
                                    default:
                                        break;
                                }

                                sbScheme.AppendLine(string.Format("  `{0}` {1},", c.ColumnName, typeName));
                            }

                            sbScheme.AppendLine(string.Format("  PRIMARY KEY (`{0}`)", dbTable.Columns[0].ColumnName));
                            sbScheme.AppendLine(") ENGINE=InnoDB DEFAULT CHARSET=utf8;");
                            sbScheme.AppendLine("");


                            var sbData = new StringBuilder();
                            sbData.AppendLine(string.Format("INSERT INTO `{0}` VALUES ", pluralized));

                            var insertctr = 0;
                            // append rows
                            for (int i = 0; i < dbTable.Rows.Count; i++)
                            {
                                var r = dbTable.Rows[i];

                                sbData.Append("(");

                                foreach (var d in r.ItemArray)
                                {
                                    sbData.Append(string.Format("'{0}', ", d));
                                }

                                if (i == dbTable.Rows.Count - 1)
                                {
                                    sbData.AppendLine(");");
                                }
                                else if (insertctr == 100)
                                {
                                    sbData.AppendLine(");");
                                    sbData.AppendLine(string.Format("INSERT INTO `{0}` VALUES ", pluralized));

                                    insertctr = 0;

                                }
                                else
                                    sbData.AppendLine("),");

                                ++insertctr;
                            }

                            sbData = sbData.Replace("', ),", "'),").Replace("', );", "');");
                            sbData.AppendLine("");

                            sbScheme.Append(sbData.ToString());

                            datadbsql.Write(sbScheme.ToString());

                            counter++;

                            Console.WriteLine("Done.");
                        }
                    }
                }
            }

            /// MSSQL
            Console.WriteLine("Creating DataDB.MSSql.sql data...");

            if (File.Exists("./DBFilesClient/DataDB.MSSql.sql"))
                File.Delete("./DBFilesClient/DataDB.MSSql.sql");

            using (var datadbsql = new StreamWriter("./DBFilesClient/DataDB.MSSql.sql", true))
            {

                foreach (var file in root.GetFiles())
                {
                    {
                        Console.WriteLine("Extracting '{0}'...", file.FullName);

                        var dbStream = cascHandler.SaveFileTo(file.FullName, "./", locale);
                        if (dbStream != null)
                        {
                            var nameOnly = file.FullName.Replace(@"\\", "").Replace(@"DBFilesClient\", "").Replace(@".dbc", "").Replace(@".db2", "");
                            var pluralized = nameOnly.Pluralize();
                            var ccp = new CSharpCodeProvider();
                            var paramss = new CompilerParameters();
                            paramss.GenerateExecutable = false;
                            paramss.GenerateInMemory = true;

                            paramss.ReferencedAssemblies.Add("ClientDBExtractor.exe");

                            var file1 = File.ReadAllText(structsPath + nameOnly + ".cs");
                            var code = new[] { file1 };

                            var result = ccp.CompileAssemblyFromSource(paramss, code);
                            var type = result.CompiledAssembly.GetTypes()[0];

                            var dbTable = DBReader.Read(dbStream, type);

                            // create scheme
                            var sbScheme = new StringBuilder();
                            sbScheme.AppendLine(string.Format("DROP TABLE [dbo].[{0}]", pluralized));
                            sbScheme.AppendLine(string.Format("GO"));
                            sbScheme.AppendLine(string.Format("CREATE TABLE [dbo].[{0}] (", pluralized));

                            foreach (DataColumn c in dbTable.Columns)
                            {
                                var typeName = "";

                                switch (c.DataType.Name)
                                {
                                    case "SByte":
                                    case "Byte":
                                        typeName = "tinyint NOT NULL DEFAULT ((0))";
                                        break;
                                    case "Int32":
                                        typeName = "int NOT NULL DEFAULT ((0))";
                                        break;
                                    case "UInt32":
                                        typeName = "bigint NOT NULL DEFAULT ((0))";
                                        break;
                                    case "Int64":
                                        typeName = "bigint NOT NULL DEFAULT ((0))";
                                        break;
                                    case "UInt64":
                                        typeName = "bigint NOT NULL DEFAULT ((0))";
                                        break;
                                    case "Single":
                                        typeName = "real NOT NULL DEFAULT ((0))";
                                        break;
                                    case "Boolean":
                                        typeName = "tinyint NOT NULL DEFAULT ((0))";
                                        break;
                                    case "String":
                                        typeName = "nvarchar(MAX) NULL DEFAULT (N'')";
                                        break;
                                    default:
                                        break;
                                }

                                sbScheme.AppendLine(string.Format("  [{0}] {1},", c.ColumnName, typeName));
                            }

                            sbScheme.Remove(sbScheme.Length - 3, 1);
                            sbScheme.AppendLine(")");

                            sbScheme.AppendLine("GO");
                            sbScheme.AppendLine("");

                            var sbData = new StringBuilder();
                            sbData.AppendLine(string.Format("INSERT INTO [{0}] VALUES ", pluralized));

                            var insertctr = 0;
                            // append rows
                            for (int i = 0; i < dbTable.Rows.Count; i++)
                            {
                                var r = dbTable.Rows[i];

                                sbData.Append("(");

                                foreach (var d in r.ItemArray)
                                {
                                    sbData.Append(string.Format("'{0}', ", d));
                                }

                                if (i == dbTable.Rows.Count - 1)
                                {
                                    sbData.AppendLine(");");
                                }
                                else if (insertctr == 100)
                                {
                                    sbData.AppendLine(");");
                                    sbData.AppendLine("GO");
                                    sbData.AppendLine(string.Format("INSERT INTO [{0}] VALUES ", pluralized));

                                    insertctr = 0;

                                }
                                else
                                    sbData.AppendLine("),");

                                ++insertctr;
                            }

                            sbData = sbData.Replace("', ),", "'),").Replace("', );", "');");
                            sbData.AppendLine("");

                            sbScheme.Append(sbData.ToString());

                            sbScheme.AppendLine(string.Format("ALTER TABLE [dbo].[{0}] ADD PRIMARY KEY ([{1}])", pluralized, dbTable.Columns[0].ColumnName));
                            sbScheme.AppendLine("GO");
                            sbData.AppendLine("");

                            datadbsql.Write(sbScheme.ToString());

                            counter++;

                            Console.WriteLine("Done.");
                        }
                    }
                }
            }

            Console.WriteLine("Generated Sql data for {0} ClientDB(s).", existingStructList.Count);
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey(true);
        }
    }
}
