using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Havensread.Connector;

public static class Worker
{
    public sealed class Names
    {
        public const string BookIngestionWorker = nameof(BookIngestionWorker);
    }

    public enum State : byte
    {
        Initialized,
        Running,
        Stopped,
    }

    public record struct Data(string Name, State State, DateTimeOffset LastCommandTime);
}
