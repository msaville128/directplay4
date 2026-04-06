using System;
using System.Runtime.InteropServices;

namespace DirectPlay4;

// Only fixed-length data are defined here!
// Variable-length data are appended at the end of the message.

interface ICommand<T> where T : unmanaged, ICommand<T>
{
    public static abstract int CommandId { get; }
}

// https://learn.microsoft.com/en-us/openspecs/windows_protocols/mc-dpl4cs/0f4f646d-9327-44a9-bb4c-2fd72df2e95d
[StructLayout(LayoutKind.Sequential, Pack = 1)]
unsafe struct SOCKADDR_IN
{
    public short Family;
    public ushort Port;
    public fixed byte Address[4];
    fixed byte Padding[8];
}

// https://learn.microsoft.com/en-us/openspecs/windows_protocols/mc-dpl4cs/caf0ddbe-d56d-474f-9c2a-f47c84cc0da9
[StructLayout(LayoutKind.Sequential, Pack = 1)]
unsafe struct DPSP_MSG_HEADER
{
    public int Mixed;
    public SOCKADDR_IN SockAddr;
    public fixed byte Magic[4];
    public short CommandId;
    public short Version;

    public readonly uint Token => ((uint)Mixed & 0xFFF00000) >> 20;
    public readonly int Size => Mixed & 0x000FFFFF;

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
unsafe struct DPSESSIONDESC2
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

// https://learn.microsoft.com/en-us/openspecs/windows_protocols/mc-dpl4cs/f9651796-2e3c-4864-903d-f5bed9db5f9f
[StructLayout(LayoutKind.Sequential, Pack = 1)]
struct DPSECURITYDESC
{
    public int Size;
    public int Flags;
    public int SSPIProvider;
    public int CAPIProvider;
    public int CAPIProviderType;
    public int EncryptionAlgorithm;
}

// https://learn.microsoft.com/en-us/openspecs/windows_protocols/mc-dpl4cs/b557f8cc-683d-4198-9d96-5c303d7456cb
[StructLayout(LayoutKind.Sequential, Pack = 1)]
struct DPLAYI_PACKEDPLAYER
{
    public int Size;
    public FLAGS Flags;
    public int PlayerId;
    public int ShortNameLength;
    public int LongNameLength;
    public int ServiceProviderDataSize;
    public int PlayerDataSize;
    public int NumberOfPlayers;
    public int SystemPlayerId;
    public int FixedSize;
    public int PlayerVersion;
    public int ParentId;

    [Flags]
    public enum FLAGS : int
    {
        SERVER_PLAYER = 1,
        NAME_SERVER = 2,
        PLAYER_IN_GROUP = 4,
        LOCAL = 8,
    }
}

// https://learn.microsoft.com/en-us/openspecs/windows_protocols/mc-dpl4cs/59101f7c-5cee-4490-891c-ec799089eb55
[StructLayout(LayoutKind.Sequential, Pack = 1)]
struct DPLAYI_SUPERPACKEDPLAYER
{
    public int Size;
    public FLAGS Flags;
    public int PlayerId;
    public PLAYER_INFO_MASK PlayerInfoMask;
    public int VersionOrSystemPlayerId;

    [Flags]
    public enum PLAYER_INFO_MASK : int
    {
        SHORT_NAME = 1,
        LONG_NAME = 2,
        SP_DATA_LENGTH_1_BYTE = 4,
        SP_DATA_LENGTH_2_BYTE = 8,
        SP_DATA_LENGTH_4_BYTE = 12,
        PLAYER_DATA_LENGTH_1_BYTE = 16,
        PLAYER_DATA_LENGTH_2_BYTE = 32,
        PLAYER_DATA_LENGTH_4_BYTE = 48,
        PLAYER_COUNT_1_BYTE = 64,
        PLAYER_COUNT_2_BYTE = 128,
        PLAYER_COUNT_4_BYTE = 192,
        PARENT_ID = 256,
        SHORTCUT_COUNT_1_BYTE = 512,
        SHORTCUT_COUNT_2_BYTE = 1024,
        SHORTCUT_COUNT_4_BYTE = 1536,
    }

    [Flags]
    public enum FLAGS : int
    {
        SERVER_PLAYER = 1,
        NAME_SERVER = 2,
        PLAYER_IN_GROUP = 4,
        LOCAL = 8,
    }
}

// https://learn.microsoft.com/en-us/openspecs/windows_protocols/mc-dpl4cs/9f61f223-88e8-4436-88ed-62b68ea23c86
[StructLayout(LayoutKind.Sequential, Pack = 1)]
struct DPSP_MSG_ENUMSESSIONSREPLY : ICommand<DPSP_MSG_ENUMSESSIONSREPLY>
{
    public static int CommandId => 1;

    public DPSESSIONDESC2 SessionDesc;
    public int NameOffset;
}

// https://learn.microsoft.com/en-us/openspecs/windows_protocols/mc-dpl4cs/998a213f-f3d4-4613-92b9-41c1739bfcf5
[StructLayout(LayoutKind.Sequential, Pack = 1)]
struct DPSP_MSG_ENUMSESSIONS : ICommand<DPSP_MSG_ENUMSESSIONS>
{
    public static int CommandId => 2;

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

// https://learn.microsoft.com/en-us/openspecs/windows_protocols/mc-dpl4cs/2d1f82b1-552c-45ef-8814-b95a6891dd62
[StructLayout(LayoutKind.Sequential, Pack = 1)]
struct DPSP_MSG_REQUESTPLAYERID : ICommand<DPSP_MSG_REQUESTPLAYERID>
{
    public static int CommandId => 5;

    public FLAGS Flags;

    [Flags]
    public enum FLAGS : int
    {
        SERVER_PLAYER = 1,
        LOCAL = 4,
    }
}

// https://learn.microsoft.com/en-us/openspecs/windows_protocols/mc-dpl4cs/44e7485d-4567-411b-bbe6-b778246f8167
[StructLayout(LayoutKind.Sequential, Pack = 1)]
struct DPSP_MSG_REQUESTPLAYERREPLY : ICommand<DPSP_MSG_REQUESTPLAYERREPLY>
{
    public static int CommandId => 7;

    public int PlayerId;
    public DPSECURITYDESC SecDesc;
    public int SSPIProviderOffset;
    public int CAPIProviderOffset;
    public int Result;
}

// https://learn.microsoft.com/en-us/openspecs/windows_protocols/mc-dpl4cs/3ad794ac-e734-48ea-b3f1-639103d9c3c3
[StructLayout(LayoutKind.Sequential, Pack = 1)]
struct DPSP_MSG_ADDFORWARDREQUEST : ICommand<DPSP_MSG_ADDFORWARDREQUEST>
{
    public static int CommandId => 19;

    public int IdTo;
    public int PlayerId;
    public int GroupId;
    public int CreateOffset;
    public int PasswordOffset;
}

// https://learn.microsoft.com/en-us/openspecs/windows_protocols/mc-dpl4cs/2f253701-52af-4f7f-8e7e-3d48c191cf89
[StructLayout(LayoutKind.Sequential, Pack = 1)]
struct DPSP_MSG_SUPERENUMPLAYERSREPLY : ICommand<DPSP_MSG_SUPERENUMPLAYERSREPLY>
{
    public static int CommandId => 41;

    public int PlayerCount;
    public int GroupCount;
    public int PackedOffset;
    public int ShortcutCount;
    public int DescriptionOffset;
    public int NameOffset;
    public int PasswordOffset;
}
