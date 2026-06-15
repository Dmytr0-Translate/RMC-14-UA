using System.Collections.Generic;
using Robust.Shared.Serialization;

namespace Content.Shared._Sich.Terminal;

[Serializable, NetSerializable]
public enum SichTerminalUiKey : byte
{
    Key
}

[Serializable, NetSerializable]
public sealed class SichTerminalState : BoundUserInterfaceState
{
    public readonly bool IsInput;
    public readonly List<string> Messages;
    public readonly string? AuthorizedName;

    public SichTerminalState(bool isInput, List<string> messages, string? authorizedName)
    {
        IsInput = isInput;
        Messages = messages;
        AuthorizedName = authorizedName;
    }
}

[Serializable, NetSerializable]
public sealed class SichTerminalSendMessage : BoundUserInterfaceMessage
{
    public readonly string Message;

    public SichTerminalSendMessage(string message)
    {
        Message = message;
    }
}
