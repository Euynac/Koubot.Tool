using System;

using System.Threading;
using System.Threading.Tasks;

namespace Koubot.Tool.Web.APILimiting
{
    /// <summary>
    /// 漏桶算法限流服务，按照平均速率进行处理请求，不适合突发高数量请求
    /// </summary>
    public class LeakyBucketLimitingService : ILimitingService
    {
        /// <summary>
        /// 请求类，放入桶中等待被处理
        /// </summary>
        class RequestObject
        {
            public bool HasHandle { get; set; }
        }
        /// <summary>
        /// 每秒最大访问量，也是用于求漏桶的速率
        /// </summary>
        public int MaxQPS { get; }
        /// <summary>
        /// 桶最大容量，多余的请求会被丢弃或挂起。也是能进行最大并行数量，但是在突发高流除了第一次能瞬间装满水桶（瞬间并发所有桶容量），以后再来的请求会按照速率进行处理
        /// </summary>
        public int LimitSize { get; }
        /// <summary>
        /// 固定容量的桶
        /// </summary>
        private readonly LimitedQueue<RequestObject> limitedQueue = null;
        /// <summary>
        /// 用于指示漏桶什么时候停止
        /// </summary>
        private readonly CancellationTokenSource cancellationToken;
        private Task task = null;
        /// <summary>
        /// 用于加锁
        /// </summary>
        private readonly object lockObject = new object();

        public LeakyBucketLimitingService(int maxQPS, int limitSize)
        {
            LimitSize = limitSize;
            MaxQPS = maxQPS;
            limitedQueue = new LimitedQueue<RequestObject>(limitSize);
            cancellationToken = new CancellationTokenSource();
            task = Task.Factory.StartNew(TokenProcess, cancellationToken.Token);
        }

        private void TokenProcess()
        {
            int sleep = 1000 / MaxQPS;
            if (sleep == 0) sleep = 1;
            sleep += 1;//不能卡那么准
            DateTime start = DateTime.Now;
            while (cancellationToken.Token.IsCancellationRequested == false)
            {
                lock (lockObject)
                {
                    if (limitedQueue.Count > 0)
                    {
                        var request = limitedQueue.Dequeue();
                        request.HasHandle = true;
                    }
                }

                if (DateTime.Now - start < TimeSpan.FromMilliseconds(sleep))
                {
                    int newSleep = sleep - (int)(DateTime.Now - start).TotalMilliseconds;
                    if (newSleep >= 0) Thread.Sleep(newSleep);
                }
                start = DateTime.Now;
            }
        }

        public void Dispose()
        {
            cancellationToken.Cancel();
        }

        public bool Request()
        {
            //if (limitedQueue.Count < LimitSize)//感觉并不是真正的漏桶算法，这样直接返回true和令牌桶一样了，应该是被处理的时候才会返回true
            //{
            //    lock (lockObject)
            //    {
            //        if(limitedQueue.Count < LimitSize)
            //        {
            //            return limitedQueue.Enqueue(new object());
            //        }
            //    }
            //}
            lock (lockObject)
            {
                if (limitedQueue.Count == 0) return true;
            }
            return false;
        }


        public async Task<bool> RequestWithRetryAsync(int retryCount = 10)
        {
            return await Task.Factory.StartNew(() => RequestWithRetry(retryCount));
        }

        public bool RequestWithRetry(int retryCount)
        {
            if (retryCount < 0) return false;
            RequestObject requestObject = new RequestObject();
            bool isInBucket = false;
            while (retryCount != 0)
            {

                if (limitedQueue.Count < LimitSize && !isInBucket)
                {
                    lock (lockObject)
                    {
                        if (limitedQueue.Count < LimitSize)
                        {
                            limitedQueue.Enqueue(requestObject);
                            isInBucket = true;
                        }
                    }
                }
                if (requestObject.HasHandle) return true;
                Thread.Sleep(System.Math.Max(0, limitedQueue.Count * 1000 / MaxQPS));
                retryCount--;
            }
            return false;
        }
    }
}
