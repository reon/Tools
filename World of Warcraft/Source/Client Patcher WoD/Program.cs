/*
 * Copyright (C) 2012-2014 Arctium Emulation <http://arctium.org>
 * 
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */

using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using Connection_Patcher.Constants;

namespace Connection_Patcher
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length >= 1)
            {
                {
                    Console.WriteLine("Arctium Connection Patcher");
                    Console.WriteLine("Press Enter to patch...");
                    Console.ReadKey(true);

                    var commonAppData = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
                    var modulePath = commonAppData + "/" + "Blizzard Entertainment/Battle.net/Cache/8f/52/8f52906a2c85b416a595702251570f96d3522f39237603115f2f1ab24962043c.auth";

                    if (!File.Exists(modulePath))
                    {
                        Console.WriteLine("Base module doesn't exist, downloading it...");

                        if (!Directory.Exists(commonAppData + "/" + "Blizzard Entertainment/Battle.net/Cache/8f"))
                        {
                            Directory.CreateDirectory(commonAppData + "/" + "Blizzard Entertainment/Battle.net/Cache/8f");
                            Directory.CreateDirectory(commonAppData + "/" + "Blizzard Entertainment/Battle.net/Cache/8f/52");
                        }
                        else if (!Directory.Exists(commonAppData + "/" + "Blizzard Entertainment/Battle.net/Cache/8f/52"))
                            Directory.CreateDirectory(commonAppData + "/" + "Blizzard Entertainment/Battle.net/Cache/8f/52");


                        var webClient = new WebClient();

                        webClient.DownloadFileCompleted += (o, e) =>
                        {
                            Patch(args, modulePath, commonAppData);
                        };

                        webClient.DownloadFileAsync(new Uri("http://xx.depot.battle.net:1119/8f52906a2c85b416a595702251570f96d3522f39237603115f2f1ab24962043c.auth"), modulePath);

                        Console.WriteLine("Done.");
                    }
                    else
                        Patch(args, modulePath, commonAppData);
                }

                Thread.Sleep(5000);
                Environment.Exit(0);
            }
        }

        static void Patch(string[] args, string modulePath, string commonAppData)
        {
            using (var patcher = new Patcher(args[0]))
            {
                switch (patcher.Type)
                {
                    case BinaryTypes.Pe32:
                        patcher.Patch(Patches.Windows.x86.BNet, Patterns.Windows.x86.BNet);
                        patcher.Patch(Patches.Windows.x86.Send, Patterns.Windows.x86.Send);
                        patcher.Patch(Patches.Windows.x86.Signature, Patterns.Windows.x86.Signature);

                        patcher.Binary = patcher.Binary.Replace(".exe", "") + "_Patched.exe";

                        patcher.Finish();

                        Console.WriteLine("Patching module...");

                        using (var patcher2 = new Patcher(modulePath))
                        {
                            patcher2.Patch(Patches.Windows.x86.Password, Patterns.Windows.x86.Password);

                            if (!Directory.Exists(commonAppData + "/" + "Blizzard Entertainment/Battle.net/Cache"))
                                Directory.CreateDirectory(commonAppData + "/" + "Blizzard Entertainment/Battle.net/Cache");

                            if (!Directory.Exists(commonAppData + "/" + "Blizzard Entertainment/Battle.net/Cache/2e"))
                            {
                                Directory.CreateDirectory(commonAppData + "/" + "Blizzard Entertainment/Battle.net/Cache/2e");
                                Directory.CreateDirectory(commonAppData + "/" + "Blizzard Entertainment/Battle.net/Cache/2e/6d");
                            }
                            else if (!Directory.Exists(commonAppData + "/" + "Blizzard Entertainment/Battle.net/Cache/2e/6d"))
                                Directory.CreateDirectory(commonAppData + "/" + "Blizzard Entertainment/Battle.net/Cache/2e/6d");

                            patcher2.Binary = commonAppData + "/" + "Blizzard Entertainment/Battle.net/Cache/2e/6d/" + Helper.GetFileChecksum(patcher2.binary) + ".auth";

                            patcher2.Finish();
                        }

                        Console.WriteLine("Patching module finished.");

                        if (args.Length == 1 || (args.Length == 2 && args[1] == "true"))
                        {
                            Console.WriteLine("Adding host rewrite...");

                            var system32 = Environment.GetFolderPath(Environment.SpecialFolder.System);
                            var path = Path.Combine(system32, @"drivers\etc\hosts");

                            var exists = false;

                            using (var sr = new StreamReader(path))
                            {
                                while (!sr.EndOfStream)
                                {
                                    var line = sr.ReadLine();

                                    if (line == "127.0.0.1 arctium.logon.battle.net")
                                    {
                                        exists = true;
                                        break;
                                    }
                                }
                            }

                            if (!exists)
                            {
                                using (var stream = new StreamWriter(path, true, Encoding.UTF8))
                                {
                                    stream.WriteLine("");
                                    stream.WriteLine("127.0.0.1 arctium.logon.battle.net");
                                }
                            }

                            Console.WriteLine("Host rewrite successfully added.");
                        }

                        break;
                    case BinaryTypes.Pe64:

                        //patcher.Binary = patcher.Binary.Replace(".exe", "") + "_Patched.exe";
                        // 
                        //patcher.Finish();
                        break;
                    case BinaryTypes.Mach32:

                        //patcher.Binary = patcher.Binary + " Patched";
                        // 
                        //patcher.Finish();
                        break;
                    case BinaryTypes.Mach64:

                        //patcher.Binary = patcher.Binary + " Patched";
                        //                              
                        //patcher.Finish();
                        break;
                    default:
                        throw new NotSupportedException("Type: " + patcher.Type + " not supported!");
                }
            }

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Successfully created your patched binaries.");

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Patching done.");

            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("Writing patched files...");
        }
    }
}
