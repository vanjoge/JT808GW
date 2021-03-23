using RedisHelp;
using SQ.Base.Queue;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace JTServer.Worker
{
    public class HashSetAllWorker : IWorkItem<RedisHelper>
    {
        private string key;
        private HashEntry[] value;

        public HashSetAllWorker(string key, HashEntry[] value)
        {
            this.key = key;
            this.value = value;
        }

        public void Dispose()
        {
        }

        public void Execute(RedisHelper tag, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            tag.HashSetAll(key, value);
        }
    }
}
