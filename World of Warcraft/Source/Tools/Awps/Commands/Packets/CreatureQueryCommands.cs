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
using System.Threading;
using Awps.Attributes;
using Awps.Misc;
using Awps.Structures;

namespace Awps.Commands.Packets
{
    public class CreatureQueryCommands
    {
        static uint CreatureId = 1;

        [ConsoleCommand("Creature", "Usage: Creature #count (Requests the #count amount of Creature.adb requests to the server.)")]
        public static void Creature(string[] args)
        {
            var count = Command.Read<int>(args, 0);

            if (count > 0)
            {
                for (var i = 0; i < count; i++)
                {
                    Thread.Sleep(5);

                    // DBQueryBulk = 0x138B
                    var creature = new PacketWriter(0x138B);

                    creature.Write(0x0C9D6B6B3u);

                    creature.PutBits(1, 13);
                    creature.Flush();

                    creature.Write(new SmartGuid());
                    creature.Write(CreatureId++);

                    SendCDataStore.Send(creature);
                }

                Console.WriteLine("Creature command execution finished.");
            }
        }
    }
}
