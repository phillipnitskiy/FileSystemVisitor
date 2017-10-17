using System;
using System.IO;

namespace Module_1.Task_1
{
    public class DirectoryEventArgs : EventArgs
    {
        public string Path { get; set; }

        public bool Stop { get; set; }
        public bool Exclude { get; set; }
    }
}
