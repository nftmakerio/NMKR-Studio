using System;

namespace NMKR.Shared.Classes.Blockfrost;

public class BlockfrostException : Exception
{
    public BlockfrostException(string Message) : base(Message)
    {
    }
}