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
using Awps.Attributes;
using Awps.Hooks;
using Awps.Log;
using Awps.Misc;

namespace Awps.Commands
{
    class HookCommands
    {
        [ConsoleCommand("start", "")]
        public static void Start(string[] args)
        {
            var name = "";

            if (args.Length == 1)
            {
                name = Command.Read<string>(args, 0);
                name = name.Replace(@"""", "");
            }

            Awps.bnetLogger = new PacketLog();
            Awps.bnetLogger.Initialize("PacketDumps", name != "" ? name : "BNetDump");

            Awps.wowLogger = new PacketLog();
            Awps.wowLogger.Initialize("PacketDumps", name != "" ? name : "WoWDump");

            Console.WriteLine("Starting Arctium WoW Packet Sniffer...");

            if (!Awps.bnetLogger.IsRunning)
            {
                if (Awps.bnetReceive == null)
                    Awps.bnetReceive = new BNetReceiveHook();
                else
                    Awps.bnetReceive.Start();

                if (Awps.bnetSend == null)
                    Awps.bnetSend = new BNetSendHook();
                else
                    Awps.bnetSend.Start();

                Awps.bnetLogger.IsRunning = true;
            }

            if (!Awps.wowLogger.IsRunning)
            {
                if (Awps.receive == null)
                    Awps.receive = new ReceiveHook();
                else
                    Awps.receive.Start();

                if (Awps.send == null)
                    Awps.send = new SendHook();
                else
                    Awps.send.Start();

                Awps.wowLogger.IsRunning = true;
            }
        }

        [ConsoleCommand("stop", "")]
        public static void Stop(string[] args)
        {
            Awps.bnetReceive.Remove();
            Awps.bnetSend.Remove();
            Awps.receive.Remove();
            Awps.send.Remove();

            Awps.bnetLogger.IsRunning = false;
            Awps.wowLogger.IsRunning = false;
        }

        [ConsoleCommand("send", "")]
        public static void Send(string[] args)
        {
            if (Environment.Is64BitProcess)
                SendCDataStore.Initialize();
        }
    }
}
