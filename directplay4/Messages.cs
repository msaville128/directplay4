using System;
using System.Runtime.InteropServices;

namespace DirectPlay4;

interface IMessage<T> where T : unmanaged, IMessage<T>, allows ref struct
{
    public static unsafe int Size => sizeof(T);
}

interface ICommand<T> : IMessage<T> where T : unmanaged, ICommand<T>, allows ref struct
{
    public static abstract int CommandId { get; }
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
unsafe ref struct SOCKADDR_IN : IMessage<SOCKADDR_IN>
{
    public short Family;
    public ushort Port;
    public fixed byte Address[4];
    fixed byte Padding[8];
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
ref struct DPSP_MSG_HEADER : IMessage<DPSP_MSG_HEADER>
{
    public int Mixed;
    public SOCKADDR_IN SockAddr;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
unsafe ref struct DPSP_MSG_ENVELOPE : IMessage<DPSP_MSG_ENVELOPE>
{
    public fixed byte Magic[4];
    public short CommandId;
    public short Version;

    public bool HasValidMagic =>
    (
        Magic[0] == 'p' &&
        Magic[1] == 'l' &&
        Magic[2] == 'a' &&
        Magic[3] == 'y'
    );
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
unsafe ref struct DPSESSIONDESC2 : IMessage<DPSESSIONDESC2>
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

[StructLayout(LayoutKind.Sequential, Pack = 1)]
ref struct DPSECURITYDESC : IMessage<DPSECURITYDESC>
{
    public int Size;
    public int Flags;
    public int SSPIProvider;
    public int CAPIProvider;
    public int CAPIProviderType;
    public int EncryptionAlgorithm;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
ref struct DPLAYI_PACKEDPLAYER : IMessage<DPLAYI_PACKEDPLAYER>
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

[StructLayout(LayoutKind.Sequential, Pack = 1)]
ref struct DPLAYI_SUPERPACKEDPLAYER : IMessage<DPLAYI_SUPERPACKEDPLAYER>
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

[StructLayout(LayoutKind.Sequential, Pack = 1)]
ref struct DPSP_MSG_ENUMSESSIONSREPLY : ICommand<DPSP_MSG_ENUMSESSIONSREPLY>
{
    public static int CommandId => 1;

    public DPSESSIONDESC2 SessionDesc;
    public int NameOffset;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
ref struct DPSP_MSG_ENUMSESSIONS : ICommand<DPSP_MSG_ENUMSESSIONS>
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
        PREVIOUS = 4,
        NO_REFRESH = 8,
        ASYNC = 16,
        STOP_ASYNC = 32,
        PASSWORD_REQUIRED = 64,
        RETURN_STATUS = 128,
    }
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
ref struct DPSP_MSG_REQUESTPLAYERID : ICommand<DPSP_MSG_REQUESTPLAYERID>
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

[StructLayout(LayoutKind.Sequential, Pack = 1)]
ref struct DPSP_MSG_REQUESTPLAYERREPLY : ICommand<DPSP_MSG_REQUESTPLAYERREPLY>
{
    public static int CommandId => 7;

    public int PlayerId;
    public DPSECURITYDESC SecDesc;
    public int SSPIProviderOffset;
    public int CAPIProviderOffset;
    public int Result;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
ref struct DPSP_MSG_ADDFORWARDREQUEST : ICommand<DPSP_MSG_ADDFORWARDREQUEST>
{
    public static int CommandId => 19;

    public int IdTo;
    public int PlayerId;
    public int GroupId;
    public int CreateOffset;
    public int PasswordOffset;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
ref struct DPSP_MSG_SUPERENUMPLAYERSREPLY : ICommand<DPSP_MSG_SUPERENUMPLAYERSREPLY>
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
