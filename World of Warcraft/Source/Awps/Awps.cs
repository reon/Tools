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
using System.Diagnostics;
using System.Threading;
using Awps.Log;

namespace Awps
{
    public class Awps
    {
        static ReceiveHook receive;
        static SendHook send;

        public static int EntryPoint(string args)
        {
            Process.EnterDebugMode();
            Helper.InitializeConsole();
            Memory.Initialize();

            if (Memory.IsInitialized)
            {
                Console.WriteLine("___________________________________________");
                Console.WriteLine("    __                                     ");
                Console.WriteLine("    / |                     ,              ");
                Console.WriteLine("---/__|---)__----__--_/_--------------_--_-");
                Console.WriteLine("  /   |  /   ) /   ' /    /   /   /  / /  )");
                Console.WriteLine("_/____|_/_____(___ _(_ __/___(___(__/_/__/_");
                Console.WriteLine("___________________________________________");
                Console.WriteLine("http://arctium.org\n");

                Console.WriteLine("{0}\n", Globals.Version);
                Console.WriteLine("Please enter a command.");
                Console.WriteLine("Available commands are: 'start'\n");

                ReadCommands();
            }

            return 0;
        }

        public static void ReadCommands()
        {
            while (true)
            {
                Thread.Sleep(1);

                Console.WriteLine("AWPS >> ");

                var command = Console.ReadLine().ToLower();

                switch (command)
                {
                    case "start":
                        PacketLog.Initialize("PacketDumps", "Dump");

                        Console.WriteLine("Starting Arctium WoW Packet Sniffer...");

                        if (!PacketLog.IsRunning)
                        {
                            if (receive == null)
                                receive = new ReceiveHook();
                            else
                                receive.Start();

                            if (send == null)
                                send = new SendHook();
                            else
                                send.Start();

                            PacketLog.IsRunning = true;
                        }

                        break;
                    case "stop":
                        receive.Remove();
                        send.Remove();

                        PacketLog.IsRunning = false;

                        break;
                    default:
                        Console.WriteLine("Command '{0}' not supported!", command);
                        ReadCommands();
                        break;
                }
            }
        }
    }
}
