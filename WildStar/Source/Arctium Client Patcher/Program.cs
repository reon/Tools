/*
 * Copyright (C) 2012-2013 Arctium Emulation <http://arctium.org>
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

namespace Arctium_Client_Patcher
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 1)
            {
                Console.WriteLine("Please enter a command:");
                Console.WriteLine("watermark: Removes your account id watermark from background.");
                Console.WriteLine();

                var command = Console.ReadLine();
                var patcher = new Patcher(args[0]);

                switch (command)
                {
                    case "watermark":
                        var offsets = new long[2] { 0xA61F, 0xE5EB };
                        var isAMD64 = Helper.IsAMD64(args[0]);
                        var offset = isAMD64 ? offsets[1] : offsets[0];

                        Console.WriteLine("Type: {0}", isAMD64 ? "AMD64" : "I386");

                        if (patcher.Initialized && patcher.CheckAddress(offset, 0xEB))
                            patcher.Patch(offset, 0xEB);

                        break;
                    default:
                        Console.WriteLine("Not supported command!");
                        break;
                }
            }
            else
                Console.WriteLine("Please Drag 'n Drop your WildStar.exe");

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey(true);
        }
    }
}
