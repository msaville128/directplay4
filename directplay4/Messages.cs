using System;
using System.Runtime.InteropServices;

namespace DirectPlay4;

// Only fixed-length data are defined here!
// Variable-length data are appended at the end of the message.

public interface ICommand<T> where T : unmanaged, ICommand<T>
{
    public static abstract short CommandId { get; }
}

// https://learn.microsoft.com/en-us/openspecs/windows_protocols/mc-dpl4cs/0f4f646d-9327-44a9-bb4c-2fd72df2e95d
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public unsafe struct SOCKADDR_IN
{
    public short Family;
    public ushort Port;
    public fixed byte Address[4];
    fixed byte Padding[8];
}

// https://learn.microsoft.com/en-us/openspecs/windows_protocols/mc-dpl4cs/caf0ddbe-d56d-474f-9c2a-f47c84cc0da9
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public unsafe struct DPSP_MSG_HEADER
{
    public static readonly int SignatureOffset = sizeof(int) + sizeof(SOCKADDR_IN);

    public int Mixed;
    public SOCKADDR_IN SockAddr;
    public fixed byte Magic[4];
    public short CommandId;
    public short Version;

    public uint Token
    {
        readonly get => ((uint)Mixed & 0xFFF00000) >> 20;
        set => Mixed = (int)(((uint)Mixed & 0x000FFFFF) | ((value & 0xFFF) << 20));
    }

    public int Size
    {
        readonly get => Mixed & 0x000FFFFF;
        set => Mixed = (int)((Mixed & 0xFFF00000) | ((uint)value & 0x000FFFFF));
    }

    public bool HasValidMagic =>
    (
        Magic[0] == 'p' &&
        Magic[1] == 'l' &&
        Magic[2] == 'a' &&
        Magic[3] == 'y'
    );
}

// https://learn.microsoft.com/en-us/openspecs/windows_protocols/mc-dpl4cs/8743fe84-59ab-4e98-b0a0-362aa8ce9b1d
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public unsafe struct DPSESSIONDESC2
{
    public int StructSize;
    public FLAGS Flags;
    public Guid Instance;
    public Guid Application;
    public int MaxPlayers;
    public int CurrentPlayers;
    public int SessionName;
    public int Password;
    public fixed int Reserved[2];
    public fixed int ApplicationDefined[4];

    [Flags]
    public enum FLAGS : int
    {
        NEW_PLAYERS_DISABLED = 1,
        MIGRATE_HOST = 4,
        NO_MESSAGE_ID = 8,
        JOIN_DISABLED = 32,
        KEEP_ALIVE = 64,
        NO_DATA_MESSAGES = 128,
        SECURE_SERVER = 256,
        PRIVATE = 512,
        PASSWORD_REQUIRED = 1024,
        MULTICAST_SERVER = 2048,
        CLIENT_SERVER = 4096,
        DIRECT_PLAY_PROTOCOL = 8192,
        NO_PRESERVE_ORDER = 16384,
        OPTIMIZE_LATENCY = 32768,
    }
}

// https://learn.microsoft.com/en-us/openspecs/windows_protocols/mc-dpl4cs/9f61f223-88e8-4436-88ed-62b68ea23c86
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct DPSP_MSG_ENUMSESSIONSREPLY : ICommand<DPSP_MSG_ENUMSESSIONSREPLY>
{
    public static short CommandId => 1;

    public DPSESSIONDESC2 SessionDesc;
    public int NameOffset;
}

// https://learn.microsoft.com/en-us/openspecs/windows_protocols/mc-dpl4cs/998a213f-f3d4-4613-92b9-41c1739bfcf5
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct DPSP_MSG_ENUMSESSIONS : ICommand<DPSP_MSG_ENUMSESSIONS>
{
    public static short CommandId => 2;

    public Guid Application;
    public int PasswordOffset;
    public FLAGS Flags;

    [Flags]
    public enum FLAGS : int
    {
        AVAILABLE = 1,
        ALL = 2,
        PASSWORD_REQUIRED = 64,
    }
}
