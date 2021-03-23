using RedisHelp;
using SQ.Base.Queue;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace JTServer.Worker
{
    public class StringSetWorker<T> : IWorkItem<RedisHelper>
    {
        private string key;
        private T value;
        TimeSpan expiry;

        public StringSetWorker(string key, T value, TimeSpan expiry)
        {
            this.key = key;
            this.value = value;
            this.expiry = expiry;
        }

        public void Dispose()
        {
        }

        public void Execute(RedisHelper tag, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            tag.StringSet(key, value, expiry);
        }
    }
}
