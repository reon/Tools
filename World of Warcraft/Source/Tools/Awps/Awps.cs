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
using Awps.Commands;
using Awps.Hooks;
using Awps.Log;

namespace Awps
{
    public class Awps
    {
        public static PacketLog bnetLogger;
        public static PacketLog wowLogger;

        public static BNetReceiveHook bnetReceive;
        public static BNetSendHook bnetSend;
        public static ReceiveHook receive;
        public static SendHook send;

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
                Console.Write("{0} ", Globals.Version);
                Console.WriteLine("(client detected: {0} {1})\n", Helper.GetClientBuildVersion(), ((Environment.Is64BitProcess) ? "x64" : "x86"));
                Console.WriteLine("Please enter a command.");
                Console.WriteLine("Available commands are: 'start', 'stop'.\n");

                CommandManager.InitCommands();
            }

            return 0;
        }
    }
}
