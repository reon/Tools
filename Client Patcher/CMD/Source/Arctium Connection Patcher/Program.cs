using System;
using System.IO;
using System.Threading;

namespace ArctiumConnectionPatcher
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Please, drag the file over this tool...");
                Thread.Sleep(4000);
                return;
            }

            // Original data
            byte[] LegacyRoutingTableData = 
            {
                0x6F, 0x8E, 0xFF, 0xDE, 0x9D, 0x79, 0xFF, 0x97, 0x59, 0xFF, 0xDF, 0x73, 0xFD, 0xFC, 0x2F, 0x9C,
                0xFB, 0xED, 0x1B, 0xF5, 0x1F, 0xFF, 0xDA, 0x9F, 0xFF, 0xFF, 0x7F, 0xCF, 0xFF, 0xDF, 0xB6, 0xFB,
                0xED, 0xFF, 0xFE, 0x7F, 0xFC, 0xEF, 0xF5, 0xDE, 0xFF, 0xBF, 0xD7, 0xBF, 0xEB, 0xC7, 0xFF, 0xD9,
                0xFF, 0xAB, 0xD9, 0xFF, 0xDB, 0x1F, 0xFD, 0xA7, 0xFD, 0xFE, 0xDB, 0x6D, 0xFE, 0xBE, 0xFF, 0xF7,
                0xDF, 0xBE, 0xFD, 0x6D, 0xF3, 0x3D, 0xAB, 0x3F, 0xBD, 0xFB, 0xFF, 0xBF, 0xFF, 0xBF, 0xC3, 0xFF,
                0xE9, 0x6F, 0x7F, 0xDD, 0x39, 0xE7, 0xF9, 0xF9, 0x67, 0xBF, 0x7F, 0xDF, 0x7B, 0xFE, 0xBB, 0xFF,
                0xF5, 0x7F, 0x3F, 0xBE, 0xFF, 0xF7, 0x77, 0xFF, 0xE4, 0x5E, 0xB5, 0xEF, 0xBF, 0xFE, 0xBE, 0xE7,
                0xAA, 0xD7, 0x3F, 0xDB, 0x7D, 0xFF, 0x76, 0xFD, 0xBF, 0xED, 0xFF, 0x6E, 0xFD, 0xE8, 0xD8, 0x77,
                0x3A, 0x04, 0x7D, 0xF9, 0xDB, 0xB2, 0xA8, 0xE6, 0x9E, 0xB0, 0x1F, 0xF4, 0xE2, 0xBB, 0xA4, 0xC4,
                0x41, 0x5C, 0xC9, 0x65, 0x4E, 0x70, 0xCD, 0xE2, 0x0B, 0xEF, 0x76, 0xAF, 0xE5, 0x95, 0xC1, 0xD9,
                0xB8, 0xF8, 0x5A, 0x68, 0xFB, 0x23, 0xAE, 0xFA, 0xA6, 0x09, 0x60, 0x58, 0x9F, 0x56, 0xCD, 0x02,
                0xA1, 0xA3, 0x6E, 0x30, 0x46, 0x83, 0x8F, 0x3F, 0xB1, 0x2E, 0x17, 0x9B, 0xCF, 0x78, 0xFE, 0x07,
                0xB8, 0xAC, 0x94, 0xB2, 0x0C, 0xF2, 0x67, 0x53, 0x49, 0xDA, 0xE7, 0xEB, 0xF4, 0x15, 0xD1, 0xBD,
                0xF5, 0xCB, 0x05, 0x24, 0xC8, 0xFF, 0x00, 0x22, 0x26, 0x97, 0xF1, 0x13, 0x83, 0xD2, 0xE5, 0xE9,
                0x9A, 0x1E, 0x6E, 0xEA, 0xEA, 0x88, 0x6F, 0x50, 0x7E, 0x96, 0xF6, 0x80, 0x9F, 0x2D, 0x3C, 0x89,
                0x04, 0x34, 0x42, 0x1C, 0xE4, 0x37, 0x18, 0xBF, 0xF0, 0x2E, 0x72, 0xC4, 0xF2, 0x28, 0xD7, 0xFC,
                0xAC, 0x8D, 0x1B, 0xCE, 0x7A, 0xBE, 0x79, 0x81, 0x82, 0x50, 0xC0, 0x39, 0x11, 0x95, 0x33, 0x27,
                0x16, 0x10, 0xE4, 0x70, 0x29, 0x97, 0x2B, 0x0C, 0x58, 0x92, 0x2A, 0x08, 0x91, 0x1D, 0x0D, 0xAD,
                0xFF, 0x48, 0xCE, 0xD0, 0xE1, 0x3D, 0xF3, 0x19, 0x6A, 0x4E, 0xF8, 0x1A, 0x03, 0x78, 0xE3, 0x3D,
                0x0F, 0x62, 0x55, 0x96, 0x6F, 0xB4, 0x60, 0xF5, 0x49, 0x48, 0xB7, 0x28, 0xB0, 0xDB, 0x5F, 0xA0,
                0x90, 0xD4, 0x47, 0xE7, 0x43, 0x2A, 0x71, 0xE3, 0x34, 0x6B, 0xD3, 0x51, 0xCA, 0x8C, 0x35, 0xB1,
                0xC6, 0x4C, 0x14, 0x9C, 0x64, 0x1D, 0x15, 0x94, 0x06, 0x9A, 0xF3, 0x03, 0x7F, 0x61, 0x69, 0x5C,
                0x4D, 0xC2, 0x26, 0x0A, 0x63, 0x0B, 0x6D, 0x53, 0x44, 0x65, 0xBA, 0x66, 0x3A, 0x22, 0x4A, 0x25,
                0x8E, 0x93, 0xA5, 0xF0, 0x57, 0x7A, 0x68, 0x6D, 0x85, 0xDE, 0x4A, 0x8F, 0x41, 0x8A, 0x0F, 0x5E,
                0xDD, 0xC5, 0x84, 0xBE, 0xD3, 0x77, 0x7D, 0xAB, 0x1E, 0x5D, 0x20, 0xAA, 0xDE, 0x8B, 0xC8, 0x75,
                0x43, 0x6C, 0xB6, 0x56, 0xED, 0x16, 0x4B, 0x51, 0x54, 0x73, 0x07, 0xF1, 0x5B, 0xC5, 0xE0, 0x59,
                0x75, 0xAE, 0xA1, 0xDC, 0x47, 0x31, 0x40, 0x99, 0x21, 0x4F, 0x74, 0xFB, 0x82, 0x46, 0xBA, 0x30,
                0xB3, 0x80, 0x61, 0x7C, 0x98, 0x8C, 0xF7, 0xA2, 0xEF, 0x3C, 0xC9, 0xED, 0xFD, 0x40, 0x1B, 0x9D,
                0xF6, 0x4F, 0x24, 0xEC, 0xF7, 0x63, 0xB6, 0x9D, 0xBC, 0x3B, 0x31, 0x5B, 0x13, 0x0E, 0x5A, 0x99,
                0x45, 0x74, 0x38, 0xE6, 0x25, 0x8B, 0xA7, 0x6A, 0xA6, 0xAB, 0x4C, 0xE0, 0x37, 0x0D, 0xFA, 0xA3,
                0xB4, 0x8E, 0x93, 0x17, 0x01, 0x73, 0x90, 0xA4, 0x11, 0x4D, 0xEB, 0x3B, 0x7B, 0xD6, 0x42, 0x36,
                0x18, 0xD5, 0x29, 0x32, 0x5E, 0x9B, 0x92, 0x33, 0x57, 0xD4, 0x9C, 0xFC, 0x12, 0x62, 0x39, 0xCA
            };
            byte[] SendData1 = { 0x52, 0x0C };
            byte[] SendData2 = { 0x52, 0x0D };
            byte[] SendData3 = { 0x12, 0x04 };
            byte[] CommsHandlerData = { 0x74 };
            byte[] emailData = { 0x74 };
            
            // Patched data
            byte[] patchedJump = { 0xEB };
            byte[] patchedWord = { 0x00, 0x00 };
            byte[] LegacyRoutingTableBytes = new byte[512];

            // Program start
            string wowBinary = args[0];

            Console.WriteLine("Arctium World of Warcraft - Mist of Pandaria v5.1.0a(16357) Client Patcher\n");
            Console.WriteLine("This patch will allow packet communication between server and client login to private server");
            Console.WriteLine("REMEMBER: Email as account name will be required to login!!!\n");
            Console.WriteLine("Choose an action for your WoW client binary");
            Console.WriteLine("-----------------------------------------");
            Console.WriteLine("1: Patch 32 bits Client");
            Console.WriteLine("2: Patch 64 bits Client");
            Console.WriteLine("3: Restore 32 bits Client");
            Console.WriteLine("4: Restore 64 bits Client");
            Console.WriteLine("0: Exit.\n");

            var option = Convert.ToByte(Console.ReadLine());
            Console.WriteLine(option);

            switch (option)
            {
                case 0: // Exit program
                    Environment.Exit(0);
                    break;
                case 1: // Patch x86 client
                    Console.WriteLine("Patching 32bits Client [{0}]...", wowBinary);
                    CheckIfFilePatched((int)Offsetsx32.emailOffset, patchedJump, wowBinary);
                    CheckIfFileOriginal((int)Offsetsx32.emailOffset, emailData, wowBinary);
                    CreateNewBinary(ref wowBinary, "x32_patched");
                    PatchBinary((int)Offsetsx32.emailOffset, patchedJump, wowBinary);

                    Console.WriteLine("Patching receive function...");
                    PatchBinary((int)Offsetsx32.CommsHandlerOffset, patchedJump, wowBinary);

                    Console.WriteLine("Patching send function...");
                    PatchBinary((int)Offsetsx32.SendOffset, patchedWord, wowBinary);
                    PatchBinary((int)Offsetsx32.SendOffset2, patchedWord, wowBinary);
                    PatchBinary((int)Offsetsx32.SendOffset3, patchedWord, wowBinary);

                    Console.WriteLine("Patching legacy routing table...");
                    PatchBinary((int)Offsetsx32.LegacyRoutingTableOffset, LegacyRoutingTableBytes, wowBinary);

                    Console.WriteLine("Exit in 5 seconds...");
                    Thread.Sleep(5000);
                    Environment.Exit(0);
                    break;
                case 2: // Patch x64 client
                    Console.WriteLine("Patching 64bits Client [{0}]...", wowBinary);
                    CheckIfFilePatched((int)Offsetsx64.emailOffset, patchedJump, wowBinary);
                    CheckIfFileOriginal((int)Offsetsx64.emailOffset, emailData, wowBinary);
                    CreateNewBinary(ref wowBinary, "x64_patched");
                    PatchBinary((int)Offsetsx64.emailOffset, patchedJump, wowBinary);

                    Console.WriteLine("Patching receive function...");
                    PatchBinary((int)Offsetsx64.CommsHandlerOffset, patchedJump, wowBinary);

                    Console.WriteLine("Patching send function...");
                    PatchBinary((int)Offsetsx64.SendOffset, patchedWord, wowBinary);
                    PatchBinary((int)Offsetsx64.SendOffset2, patchedWord, wowBinary);
                    PatchBinary((int)Offsetsx64.SendOffset3, patchedWord, wowBinary);

                    Console.WriteLine("Patching legacy routing table...");
                    PatchBinary((int)Offsetsx64.LegacyRoutingTableOffset, LegacyRoutingTableBytes, wowBinary);

                    Console.WriteLine("Exit in 5 seconds...");
                    Thread.Sleep(5000);
                    Environment.Exit(0);
                    break;
                case 3: // Restore x86 client
                    Console.WriteLine("Restoring 32bits Client [{0}]...", wowBinary);
                    CheckIfFilePatched((int)Offsetsx32.emailOffset, emailData, wowBinary);
                    CheckIfFileOriginal((int)Offsetsx32.emailOffset, patchedJump, wowBinary);
                    CreateNewBinary(ref wowBinary, "x32_restored");
                    PatchBinary((int)Offsetsx32.emailOffset, emailData, wowBinary);

                    Console.WriteLine("Restoring receive function...");
                    PatchBinary((int)Offsetsx32.CommsHandlerOffset, CommsHandlerData, wowBinary);

                    Console.WriteLine("Restoring send function...");
                    PatchBinary((int)Offsetsx32.SendOffset, SendData1, wowBinary);
                    PatchBinary((int)Offsetsx32.SendOffset2, SendData2, wowBinary);
                    PatchBinary((int)Offsetsx32.SendOffset3, SendData3, wowBinary);

                    Console.WriteLine("Restoring legacy routing table...");
                    PatchBinary((int)Offsetsx32.LegacyRoutingTableOffset, LegacyRoutingTableData, wowBinary);

                    Console.WriteLine("Exit in 5 seconds...");
                    Thread.Sleep(5000);
                    Environment.Exit(0);
                    break;
                case 4: // Restore x64 client
                    Console.WriteLine("Restoring 64bits Client [{0}]...", wowBinary);
                    CheckIfFilePatched((int)Offsetsx64.emailOffset, emailData, wowBinary);
                    CheckIfFileOriginal((int)Offsetsx64.emailOffset, patchedJump, wowBinary);
                    CreateNewBinary(ref wowBinary, "x64_restored");
                    PatchBinary((int)Offsetsx64.emailOffset, emailData, wowBinary);

                    Console.WriteLine("Restoring receive function...");
                    PatchBinary((int)Offsetsx64.CommsHandlerOffset, CommsHandlerData, wowBinary);

                    Console.WriteLine("Restoring send function...");
                    PatchBinary((int)Offsetsx64.SendOffset, SendData1, wowBinary);
                    PatchBinary((int)Offsetsx64.SendOffset2, SendData2, wowBinary);
                    PatchBinary((int)Offsetsx64.SendOffset3, SendData3, wowBinary);

                    Console.WriteLine("Restoring legacy routing table...");
                    PatchBinary((int)Offsetsx64.LegacyRoutingTableOffset, LegacyRoutingTableData, wowBinary);

                    Console.WriteLine("Exit in 5 seconds...");
                    Thread.Sleep(5000);
                    Environment.Exit(0);
                    break;
                default:
                    break;
            }

            Console.ReadKey();
        }

        static void CreateNewBinary(ref string file, string type)
        {
            Console.WriteLine("Creating backup from {0}...", file);
            File.Copy(file, "WoW_tmp", true);
            Console.WriteLine("Done");

            string newFileName = "WoW" + type + ".exe";
            if (File.Exists(newFileName))
                File.Delete(newFileName);

            Console.WriteLine("Create new binary...");

            File.Move(file, newFileName);
            File.Move("WoW_tmp", file);

            file = newFileName;
            Console.WriteLine("Done");
        }

        static void CheckIfFilePatched(int offset, byte[] pBytes, string file)
        {
            BinaryReader wowReader = new BinaryReader(File.Open(file, FileMode.Open, FileAccess.Read));
            wowReader.BaseStream.Seek(offset, SeekOrigin.Begin);

            if (wowReader.ReadByte() == pBytes[0])
            {
                Console.WriteLine("{0} already patched/restored!!!", file);
                wowReader.Close();

                Console.WriteLine("Exit in 5 seconds...");
                Thread.Sleep(4000);
                Environment.Exit(0);
            }
            wowReader.Close();
        }

        static void CheckIfFileOriginal(int offset, byte[] pBytes, string file)
        {
            BinaryReader wowReader = new BinaryReader(File.Open(file, FileMode.Open, FileAccess.Read));
            wowReader.BaseStream.Seek(offset, SeekOrigin.Begin);

            if (wowReader.ReadByte() != pBytes[0])
            {
                Console.WriteLine("{0} is not the expected file!!!", file);
                wowReader.Close();

                Console.WriteLine("Exit in 5 seconds...");
                Thread.Sleep(4000);
                Environment.Exit(0);
            }
            wowReader.Close();
        }

        static void PatchBinary(int offset, byte[] pBytes, string file)
        {
            BinaryWriter wowWriter = new BinaryWriter(File.Open(file, FileMode.Open, FileAccess.ReadWrite));
            wowWriter.BaseStream.Seek((int)offset, SeekOrigin.Begin);
            wowWriter.Write(pBytes);
            wowWriter.Close();

            Console.WriteLine("Done.");
        }
    }
}
