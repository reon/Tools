using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
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
            Console.ForegroundColor = ConsoleColor.Cyan;

            Console.WriteLine("_____________World of Warcraft_____________");
            Console.WriteLine("    __                                     ");
            Console.WriteLine("    / |                     ,              ");
            Console.WriteLine("---/__|---)__----__--_/_--------------_--_-");
            Console.WriteLine("  /   |  /   ) /   ' /    /   /   /  / /  )");
            Console.WriteLine("_/____|_/_____(___ _(_ __/___(___(__/_/__/_");
            Console.WriteLine();

            Console.ForegroundColor = ConsoleColor.Gray;

            Console.WriteLine("Searching for available ClientDB (dbc & db2) files...");

            cascHandler = CASCHandler.OpenLocalStorage(Environment.CurrentDirectory + "\\");

            var fileList = new List<string>();

            var wowBin = "Wow.exe";
            var wowTBin = "WowT.exe";
            var wowXBin = "World of Warcraft";
            var bin = "";

            if (File.Exists(wowBin))
                bin = wowBin;
            else if (File.Exists(wowTBin))
                bin = wowTBin;
            else if (File.Exists(wowXBin))
                bin = wowXBin;
            else
            {
                Console.WriteLine("No valid World of Warcraft version found.");
                Console.ReadKey();

                return;
            }

            // Get dbc files from wow bin
            using (var sr = new StreamReader(Environment.CurrentDirectory + "/" + bin))
            {
                var text = sr.ReadToEnd();

                foreach (Match dbc in Regex.Matches(text, @"DBFilesClient\\([A-Za-z0-9\-_]+)\.(dbc|db2)"))
                    fileList.Add(dbc.Value.Replace(@"\\", @"\"));
            }

            // not in wow bin
            fileList.Add(@"DBFilesClient\WorldSafeLocs.dbc");
            fileList.Add(@"DBFilesClient\AttackAnimKits.dbc");
            fileList.Add(@"DBFilesClient\AttackAnimTypes.dbc");
            fileList.Add(@"DBFilesClient\WowError_Strings.dbc");

            Console.WriteLine("Found {0} available ClientDB files.", fileList.Count);

            root = cascHandler.LoadListFile(fileList);

            Console.WriteLine("Getting available locales...");

            var locales = new Dictionary<string, LocaleFlags>();

            if (args.Length == 1)
                locales.Add(args[0], (LocaleFlags)Enum.Parse(typeof(LocaleFlags), args[0]));
            else
            {
                var buildInfo = File.ReadAllText("./.build.info").Split(new[] { '|' })[19];
                var buildInfoLocales = Regex.Matches(buildInfo, " ([A-Za-z]{4}) speech");


                foreach (Match m in buildInfoLocales)
                {
                    var flagString = m.Value.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)[0];
                    var localFlag = (LocaleFlags)Enum.Parse(typeof(LocaleFlags), flagString);

                    if (!locales.ContainsKey(flagString))
                    {
                        locales.Add(flagString, localFlag);

                        Console.WriteLine($"Found locale '{flagString}'.");
                    }
                }
            }

            Directory.CreateDirectory("./Arctium/ClientDB/SQL");
            Directory.CreateDirectory("./Arctium/ClientDB/Files");

            /// Files
            Console.WriteLine("Extracting files...");

            var fileCtr = 0;
            var fileErrors = new List<string>();

            foreach (var file in root.GetFiles())
            {
                var nameOnly = file.FullName.Replace(@"\\", "").Replace(@"DBFilesClient\", "");

                foreach (var locale in locales)
                {
                    var dbStream = cascHandler.SaveFileTo(file.FullName, "./", locale.Value);

                    Console.Write($"Writing file {nameOnly} ({locale.Key})...");

                    if (dbStream != null)
                    {
                        Directory.CreateDirectory($"./Arctium/ClientDB/Files/{locale.Key}");

                        File.WriteAllBytes($"./Arctium/ClientDB/Files/{locale.Key}/{nameOnly}", dbStream.ToArray());

                        Console.ForegroundColor = ConsoleColor.Green;

                        Console.Write("Done.");
                        Console.WriteLine();

                        fileCtr++;
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Red;

                        Console.WriteLine("Error.");

                        fileErrors.Add($"{nameOnly} ({locale.Key})");
                    }

                    Console.ForegroundColor = ConsoleColor.Gray;
                }
            }

            Console.WriteLine($"Extracted {fileCtr} files.");
            Console.WriteLine();

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

            Console.WriteLine($"Found {existingStructList.Count} structures.");
            Console.WriteLine();

            #pragma warning disable CS0618
            AppDomain.CurrentDomain.AppendPrivatePath(Environment.CurrentDirectory);
            #pragma warning restore CS0618

            Console.WriteLine("Generating SQL data...");
            Console.WriteLine();

            var generatedTables = new List<string>();
            var counter = 0;

            var noLocaleMSSQL = new StreamWriter($"./Arctium/ClientDB/SQL/DataDB.MSSQL.sql");
            var noLocaleMYSQL = new StreamWriter($"./Arctium/ClientDB/SQL/DataDB.MYSQL.sql");

            foreach (var locale in locales)
            {
                var localeMSSQL = new StreamWriter($"./Arctium/ClientDB/SQL/{locale.Key}_DataDB.MSSQL.sql");
                var localeMYSQL = new StreamWriter($"./Arctium/ClientDB/SQL/{locale.Key}_DataDB.MYSQL.sql");

                foreach (var file in existingStructList)
                {
                    var nameOnly = file.Replace(@"\\", "").Replace(@"DBFilesClient\", "");
                    var dbStream = new MemoryStream(File.ReadAllBytes($"./Arctium/ClientDB/Files/{locale.Key}/{nameOnly}"));

                    if (dbStream != null)
                    {
                        nameOnly = nameOnly.Replace(@".dbc", "").Replace(@".db2", "");

                        var ccp = new CSharpCodeProvider();
                        var paramss = new CompilerParameters();

                        paramss.GenerateExecutable = false;
                        paramss.GenerateInMemory = true;

                        paramss.ReferencedAssemblies.Add("ClientDBExtractor.exe");

                        var file1 = File.ReadAllText(structsPath + nameOnly + ".cs");
                        var code = new[] { file1 };

                        var result = ccp.CompileAssemblyFromSource(paramss, code);
                        var type = result.CompiledAssembly.GetTypes()[0];
                        var hasStringProperties = type.GetProperties().Any(p => p.PropertyType == typeof(string));

                        var pluralized = nameOnly.Replace(@".dbc", "").Replace(@".db2", "").Pluralize();

                        if (hasStringProperties)
                            pluralized = pluralized + "_" + locale.Key;

                        if (!generatedTables.Contains(pluralized))
                        {
                            generatedTables.Add(pluralized);

                            if (hasStringProperties)
                                Console.Write($"Generating SQL data for {pluralized} ({locale.Key})...");
                            else
                                Console.Write($"Generating SQL data for {pluralized}...");

                            var dbTable = DBReader.Read(dbStream, type);

                            if (hasStringProperties)
                            {
                                localeMYSQL.Write(GenerateMYSQLData(nameOnly, pluralized, dbTable));
                                localeMSSQL.Write(GenerateMSSQLData(pluralized, dbTable));
                            }
                            else
                            {
                                noLocaleMYSQL.Write(GenerateMYSQLData(nameOnly, pluralized, dbTable));
                                noLocaleMSSQL.Write(GenerateMSSQLData(pluralized, dbTable));
                            }

                            counter++;

                            Console.ForegroundColor = ConsoleColor.Green;

                            Console.Write("Done.");
                            Console.WriteLine();
                        }
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Red;

                        Console.WriteLine($"./Arctium/ClientDB/Files/{locale.Key}/{nameOnly} doesn't exist.");
                    }

                    Console.ForegroundColor = ConsoleColor.Gray;
                }

                localeMSSQL.Dispose();
                localeMYSQL.Dispose();
            }

            noLocaleMSSQL.Dispose();
            noLocaleMYSQL.Dispose();

            Console.WriteLine("Generated Sql data for {0} ClientDB(s).", existingStructList.Count);

            Console.ForegroundColor = ConsoleColor.Red;

            Console.WriteLine();
            Console.WriteLine("ERROS WHILE EXTRACTING:");

            foreach (var s in fileErrors)
                Console.WriteLine(s);

            Console.ForegroundColor = ConsoleColor.Gray;

            Console.WriteLine();
            Console.WriteLine("Press any key to exit...");

            Console.ReadKey(true);
        }

        static string GenerateMYSQLData(string nameOnly, string pluralized, DataTable dbTable)
        {
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
                        {
                            typeName = "tinyint(3) unsigned NOT NULL";
                        }
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
                        {
                            typeName = "tinyint(3) unsigned NOT NULL";
                        }
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

            for (int i = 0; i < dbTable.Rows.Count; i++)
            {
                var r = dbTable.Rows[i];

                sbData.Append("(");

                foreach (var d in r.ItemArray)
                    sbData.Append(string.Format("'{0}', ", d));

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

            sbData.Replace("', ),", "'),");
            sbData.Replace("', );", "');");

            sbData.AppendLine();

            return sbData.ToString();
        }

        static string GenerateMSSQLData(string pluralized, DataTable dbTable)
        {
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

            for (int i = 0; i < dbTable.Rows.Count; i++)
            {
                var r = dbTable.Rows[i];

                sbData.Append("(");

                foreach (var d in r.ItemArray)
                    sbData.Append(string.Format("'{0}', ", d));

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

            sbData.Replace("', ),", "'),");
            sbData.Replace("', );", "');");

            sbData.AppendLine("");

            sbScheme.Append(sbData.ToString());

            sbScheme.AppendLine(string.Format("ALTER TABLE [dbo].[{0}] ADD PRIMARY KEY ([{1}])", pluralized, dbTable.Columns[0].ColumnName));
            sbScheme.AppendLine("GO");

            sbData.AppendLine("");

            return sbScheme.ToString();
        }
    }
}
