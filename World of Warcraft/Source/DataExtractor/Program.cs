// Copyright (c) Arctium Emulation.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CASC_Lib.CASC.Constants;
using CASC_Lib.CASC.Handlers;
using CASC_Lib.Misc;
using DataExtractor.Maps.Defines;
using DataExtractor.Reader;
using Microsoft.CSharp;

namespace DataExtractor
{
    class Program
    {
        static CASCHandler cascHandler;
        static List<string> pluralizationExceptions = new List<string>
        {
            "gtNpcTotalHp"
        };

        static void Main(string[] args)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;

            Console.WriteLine(@"                   _   _                 ");
            Console.WriteLine(@"    /\            | | (_)                ");
            Console.WriteLine(@"   /  \   _ __ ___| |_ _ _   _ _ __ ___  ");
            Console.WriteLine(@"  / /\ \ | '__/ __| __| | | | | '_ ` _ \ ");
            Console.WriteLine(@" / ____ \| | | (__| |_| | |_| | | | | | |");
            Console.WriteLine(@"/_/    \_\_|  \___|\__|_|\__,_|_| |_| |_|");
            Console.WriteLine(@"           _                             ");
            Console.WriteLine(@"          |_._ _   | __|_ o _._          ");
            Console.WriteLine(@"          |_| | |_||(_||_ |(_| |         ");
            Console.WriteLine();

            Console.WriteLine($"{"www.arctium-emulation.com",33}");
            Console.WriteLine();

            Console.ForegroundColor = ConsoleColor.Gray;

            Console.WriteLine("Initializing CASC library...");

            cascHandler = new CASCHandler(Environment.CurrentDirectory);

            Console.WriteLine("Done.");

            ExtractClientDBData(args);

            Console.WriteLine("Done.");

            // We don't need them for now.
            //ExtractMapData();

            Console.WriteLine();
            Console.WriteLine("Press any key to exit...");

            Console.ReadKey(true);
        }

        static void ExtractMapData()
        {
            Console.WriteLine("Extracting map files...");

            Directory.CreateDirectory("./Project-WoW/Maps");

            var mapDBC = cascHandler.ReadFile(@"DBFilesClient\Map.dbc");
            var mapDBData = DBReader.Read(mapDBC, typeof(MapDB));

            Parallel.For(0, mapDBData.Rows.Count, i =>
            {
                var mapId = Convert.ToUInt16(mapDBData.Rows[i][0]);
                var mapName = mapDBData.Rows[i][1].ToString();
                var mapType = Convert.ToByte(mapDBData.Rows[i][5]);

                // Skip transport & garrison maps.
                if (mapType == 3 || mapType == 4)
                    return;

                var map = new Map { Id = mapId, Name = mapName };
                var mapReader = new MapReader();

                // Version 1
                // P A M 1 (50 41 4D 01 ) = MAP1
                mapReader.Write(new byte[] { 0x50, 0x41, 0x4D, 0x01 });
                mapReader.Write(map.Id, 11);
                mapReader.Write(Encoding.UTF8.GetBytes(mapName).Length, 7);
                mapReader.Write(Encoding.UTF8.GetBytes(mapName));

                mapReader.Flush();

                for (var j = 0; j < 64; j++)
                {
                    for (var k = 0; k < 64; k++)
                    {
                        var mapData = cascHandler.ReadFile($@"World\Maps\{mapName}\{mapName}_{j}_{k}.adt");

                        if (mapData != null)
                        {
                            mapReader.Initialize(mapData.ToArray());
                            mapReader.Read(map, k, j);
                        }
                    }
                }

                File.WriteAllBytes( $"./Project-WoW/Maps/{map.Id:0000}.map", mapReader.Finish(map).ToArray());

                Console.WriteLine($"Extraction of map '{mapName}' done.");
            });
        }

        static void ExtractClientDBData(string[] args)
        {
            Console.WriteLine("Searching for available ClientDB (dbc & db2) files...");

            var fileList = new List<string>();

            var wowBin = cascHandler.BasePath + "/Wow.exe";
            var wowBBin = cascHandler.BasePath + "/WowB.exe";
            var wowTBin = cascHandler.BasePath + "/WowT.exe";
            var wowXBin = cascHandler.BasePath + "/World of Warcraft";
            var bin = "";

            if (File.Exists(wowBin))
                bin = wowBin;
            else if (File.Exists(wowTBin))
                bin = wowTBin;
            else if (File.Exists(wowXBin))
                bin = wowXBin;
            else if (File.Exists(wowBBin))
                bin = wowBBin;
            else
            {
                Console.WriteLine("No valid World of Warcraft version found.");
                Console.ReadKey();

                return;
            }

            // Get dbc files from wow bin
            using (var sr = new StreamReader(bin))
            {
                var text = sr.ReadToEnd();

                foreach (Match dbc in Regex.Matches(text, @"DBFilesClient\\([A-Za-z0-9\-_]+)\.(dbc|db2)"))
                    fileList.Add(dbc.Value.Replace(@"\\", @"\"));
            }

            // not in wow bin
            fileList.Add(@"DBFilesClient\WorldSafeLocs.dbc");

            Console.WriteLine("Getting available locales...");

            var locales = new Dictionary<string, Locales>();

            if (args.Length == 1)
                locales.Add(args[0], (Locales)Enum.Parse(typeof(Locales), args[0]));
            else
            {
                var buildInfo = File.ReadAllText(cascHandler.BasePath + "/.build.info").Split(new[] { '|' })[21];
                var buildInfoLocales = Regex.Matches(buildInfo, " ([A-Za-z]{4}) speech");


                foreach (Match m in buildInfoLocales)
                {
                    var flagString = m.Value.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)[0];
                    var localFlag = (Locales)Enum.Parse(typeof(Locales), flagString);

                    if (!locales.ContainsKey(flagString))
                    {
                        locales.Add(flagString, localFlag);

                        Console.WriteLine($"Found locale '{flagString}'.");
                    }
                }
            }

            Directory.CreateDirectory("./Project-WoW/ClientDB/SQL");
            Directory.CreateDirectory("./Project-WoW/ClientDB/Files");

            /// Files
            Console.WriteLine("Extracting files...");

            var fileCtr = 0;
            var fileErrors = new List<string>();

            foreach (var file in fileList)
            {
                var nameOnly = file.Replace(@"\\", "").Replace(@"DBFilesClient\", "");

                foreach (var locale in locales)
                {
                    var dbStream = cascHandler.ReadFile(file, locale.Value);

                    Console.Write($"Writing file {nameOnly} ({locale.Key})...");

                    if (dbStream != null)
                    {
                        Directory.CreateDirectory($"./Project-WoW/ClientDB/Files/{locale.Key}");

                        Task.Run(() => FileWriter.WriteFile(dbStream, $"./Project-WoW/ClientDB/Files/{locale.Key}/{nameOnly}"));

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
            var structsPath = cascHandler.BasePath + "/Structures/";

            if (!Directory.Exists(structsPath))
                Directory.CreateDirectory(structsPath);

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

            var noLocaleMSSQL = new StreamWriter($"./Project-WoW/ClientDB/SQL/DataDB.MSSQL.sql");
            var noLocaleMYSQL = new StreamWriter($"./Project-WoW/ClientDB/SQL/DataDB.MYSQL.sql");

            foreach (var locale in locales)
            {
                var localeMSSQL = new StreamWriter($"./Project-WoW/ClientDB/SQL/{locale.Key}_DataDB.MSSQL.sql");
                var localeMYSQL = new StreamWriter($"./Project-WoW/ClientDB/SQL/{locale.Key}_DataDB.MYSQL.sql");

                foreach (var file in existingStructList)
                {
                    var nameOnly = file.Replace(@"\\", "").Replace(@"DBFilesClient\", "");
                    var path = $"./Project-WoW/ClientDB/Files/{locale.Key}/{nameOnly}";

                    if (!File.Exists(path))
                    {
                        Console.ForegroundColor = ConsoleColor.Red;

                        Console.WriteLine($"{path} doesn't exist.");
                        Console.WriteLine("Skip it.");

                        continue;
                    }

                    var dbStream = new MemoryStream(File.ReadAllBytes(path));

                    if (dbStream != null)
                    {
                        nameOnly = nameOnly.Replace(@".dbc", "").Replace(@".db2", "");

                        var ccp = new CSharpCodeProvider();
                        var paramss = new CompilerParameters();

                        paramss.GenerateExecutable = false;
                        paramss.GenerateInMemory = true;

                        paramss.ReferencedAssemblies.Add("DataExtractor.exe");

                        var file1 = File.ReadAllText(structsPath + nameOnly + ".cs");
                        var code = new[] { file1 };

                        var result = ccp.CompileAssemblyFromSource(paramss, code);
                        var type = result.CompiledAssembly.GetTypes()[0];
                        var hasStringProperties = type.GetProperties().Any(p => p.PropertyType == typeof(string));

                        var pluralized = nameOnly.Replace(@".dbc", "").Replace(@".db2", "");
                        
                        if (!pluralizationExceptions.Contains(pluralized))
                            pluralized = pluralized.Pluralize();

                        pluralized.Insert(0, pluralized[0].ToString().ToUpperInvariant());
                        pluralized.Remove(1);

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

            if (fileErrors.Count > 0)
            {
                Console.WriteLine("ERROS WHILE EXTRACTING:");

                foreach (var s in fileErrors)
                    Console.WriteLine(s);
            }

            Console.ForegroundColor = ConsoleColor.Gray;
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
                    case "Int16":
                        typeName = "smallint(5) NOT NULL";
                        break;
                    case "UInt16":
                        typeName = "smallint(5) unsigned NOT NULL";
                        break;
                    case "Int32":
                        if (c.ColumnName == "Race" || c.ColumnName == "RaceId" || c.ColumnName == "Class" || c.ColumnName == "ClassId"
                            || c.ColumnName == "Sex" || c.ColumnName == "Gender")
                        {
                            typeName = "tinyint(3) unsigned NOT NULL";
                        }
                        else if (c.ColumnName == "ID" || c.ColumnName == "Id" || c.ColumnName == nameOnly + "Id" || c.ColumnName == nameOnly + "ID"
                                 || c.ColumnName == "SpellId"
                                 || c.ColumnName == "Spell")
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
                    sbData.AppendFormat(CultureInfo.GetCultureInfo("en-US").NumberFormat, "'{0}', ", d.GetType() == typeof(string) ? d.ToString().Replace("\"", "\"\"").Replace("'", @"\'") : d);

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

            sbScheme.Append(sbData);

            return sbScheme.ToString();
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
                    sbData.AppendFormat(CultureInfo.GetCultureInfo("en-US").NumberFormat, "'{0}', ", d.GetType() == typeof(string) ? d.ToString().Replace("\"", "\"\"").Replace("'", @"\'") : d);

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
