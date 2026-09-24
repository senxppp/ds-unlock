using System;
using System.Threading;

namespace DSBait
{
    // Tiny idle process used as a process-name decoy ("Cyberpunk2077.exe").
    // Does absolutely nothing except exist. No window, ~0% CPU.
    internal static class Dummy
    {
        private static void Main()
        {
            Thread.Sleep(int.MaxValue);
        }
    }
}
