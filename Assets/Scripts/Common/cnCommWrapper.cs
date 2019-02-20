using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class cnCommWrapper
{
    [DllImport("cnCommWrapper")]
    public static extern IntPtr CreateComm();
    [DllImport("cnCommWrapper")]
    public static extern void DisposeComm(IntPtr comm);
    [DllImport("cnCommWrapper")]
    public static extern bool IsOpen(IntPtr comm);
    [DllImport("cnCommWrapper")]
    public static extern bool SetBufferSize(IntPtr comm, int dwInputSize, int dwOutputSize);
    [DllImport("cnCommWrapper")]
    public static extern void ClearInputBuffer(IntPtr comm);
    [DllImport("cnCommWrapper")]
    public static extern int GetInputSize(IntPtr comm);
    [DllImport("cnCommWrapper")]
    public static extern bool Open(IntPtr comm, int dwPort, string szSetStr);
    [DllImport("cnCommWrapper")]
    public static extern void Close(IntPtr comm);
    [DllImport("cnCommWrapper")]
    public static extern bool SetRTS(IntPtr comm, bool OnOrOff);
    [DllImport("cnCommWrapper")]
    public static extern int Read(IntPtr comm, byte[] Buffer, int dwBufferLength, int dwWaitTime = 10);
    [DllImport("cnCommWrapper")]
    public static extern int Write(IntPtr comm, byte[] Buffer, int dwBufferLength, int dwWaitTime = 20);
























}
