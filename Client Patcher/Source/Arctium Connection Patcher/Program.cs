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
                return;

            string wowBinary = args[0];

            Console.WriteLine("Choose a patch for your WoW binary: \n\n");
            Console.WriteLine("0: Exit.\n");
            Console.WriteLine("1: Allow packet communication between server and client login to private server. Email as account name is required for this!!!");

            var option = Convert.ToByte(Console.ReadLine());
            Console.WriteLine(option);

            CreateNewBinary(ref wowBinary);

            switch (option)
            {
                case 0:
                    Environment.Exit(0);
                    break;
                case 1:
                {
                    byte[] patchedBytes = { 0xEB };
                    PatchBinary(Offsets.emailOffset, patchedBytes, wowBinary);

                    Console.WriteLine("Patching send function...");

                    byte[] SendBytes = new byte[2];
                    PatchBinary(Offsets.SendOffset, SendBytes, wowBinary);
                    PatchBinary(Offsets.SendOffset2, SendBytes, wowBinary);
                    PatchBinary(Offsets.SendOffset3, SendBytes, wowBinary);

                    Console.WriteLine("Patching legacy routing table...");
                    byte[] LegacyRoutingTableBytes = new byte[512];
                    PatchBinary(Offsets.LegacyRoutingTableOffset, LegacyRoutingTableBytes, wowBinary);

                    Console.WriteLine("Patching receive function...");
                    byte[] CommsHandlerBytes = { 0xEB };
                    PatchBinary(Offsets.CommsHandlerOffset, CommsHandlerBytes, wowBinary);

                    Console.WriteLine("Exit in 5 seconds...");
                    Thread.Sleep(5000);
                    Environment.Exit(0);
                    break;
                }
                default:
                    break;
            }

            Console.ReadKey();
        }

        static void CreateNewBinary(ref string file)
        {
            Console.WriteLine("Creating backup from {0}...", file);
            File.Copy(file, "WoW.bak", true);
            Console.WriteLine("Done");

            if (File.Exists("WoW_Patched.exe"))
                File.Delete("WoW_Patched.exe");

            Console.WriteLine("Create new binary...");

            File.Move(file, "WoW_Patched.exe");
            File.Move("WoW.bak", file);

            file = "WoW_Patched.exe";
            Console.WriteLine("Done");
        }

        static void PatchBinary(Offsets offset, byte[] pBytes, string file)
        {
            BinaryReader wowReader = new BinaryReader(File.Open(file, FileMode.Open, FileAccess.Read));
            wowReader.BaseStream.Seek((int)Offsets.emailOffset, SeekOrigin.Begin);

            if (wowReader.ReadByte() == pBytes[0])
            {
                Console.WriteLine("{0} already patched!!!", file);
                wowReader.Close();

                Console.WriteLine("Exit in 5 seconds...");
                Thread.Sleep(5000);
                Environment.Exit(0);
            }
            else
            {
                wowReader.Close();

                BinaryWriter wowWriter = new BinaryWriter(File.Open(file, FileMode.Open, FileAccess.ReadWrite));
                wowWriter.BaseStream.Seek((int)offset, SeekOrigin.Begin);
                wowWriter.Write(pBytes);
                wowWriter.Close();

                Console.WriteLine("Done.");
            }
        }
    }
}
