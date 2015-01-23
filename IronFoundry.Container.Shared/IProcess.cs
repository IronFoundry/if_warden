﻿using System;
using System.Diagnostics;

namespace IronFoundry.Warden.Utilities
{
    // BR: Move this to IronFoundry.Container.Shared
    public interface IProcess : IDisposable
    {
        int ExitCode { get; }
        IntPtr Handle { get; }
        int Id { get; }
        
        long PrivateMemoryBytes { get; }

        event EventHandler Exited;
        event EventHandler<ProcessDataReceivedEventArgs> OutputDataReceived;
        event EventHandler<ProcessDataReceivedEventArgs> ErrorDataReceived;

        void Kill();
        void WaitForExit();
        bool WaitForExit(int milliseconds);

        void RequestExit();
    }
}