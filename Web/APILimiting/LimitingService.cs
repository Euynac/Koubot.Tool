using System.Collections.Generic;

namespace Koubot.Tool.Web.APILimiting
{
    /// <summary>
    /// 限流模式
    /// </summary>
    public enum LimitingType
    {
        /// <summary>
        /// 令牌桶模式，适用于常有高数量突发请求的，即可能会有达到最大QPS的情况
        /// </summary>
        TokenBucket,
        /// <summary>
        /// 漏桶模式，不管多少请求处理速度都一定，比较平滑，不会达到最大QPS，但平均处理速率是基于QPS的
        /// </summary>
        LeakyBucket,
    }
    /// <summary>
    /// 限制了长度的队列，多了则无法添加，且会返回false的加入队列错误的返回值
    /// </summary>
    internal class LimitedQueue<T> : Queue<T>
    {
        public int Limit { get; set; }

        public LimitedQueue(int limit) : base(limit)
        {
            this.Limit = limit;
        }
        public LimitedQueue() : this(0)
        {

        }

        public new bool Enqueue(T item)
        {
            if (Limit > 0 && this.Count >= this.Limit)
            {
                return false;
            }
            base.Enqueue(item);
            return true;
        }
    }

    /// <summary>
    /// 创建指定限流服务的工厂
    /// </summary>
    public class LimitingFactory
    {
        /// <summary>
        /// 创建限流服务对象
        /// </summary>
        /// <param name="limitingType">限流服务类型</param>
        /// <param name="maxQPS">最大QPS 每秒查询率/访问量（QPS，Queries-per-second）是对一个特定的查询服务器在规定时间内所处理流量多少的衡量标准。</param>
        /// <param name="limitSize">桶最大同时请求容量 默认是每秒最大请求容量，即QPS</param>
        /// <returns></returns>
        public static ILimitingService Build(LimitingType limitingType, int maxQPS, int limitSize)
        {
            if (maxQPS > 0 && limitSize > 0)
            {
                switch (limitingType)
                {
                    case LimitingType.TokenBucket:
                        return new TokenBucketLimitingService(maxQPS, limitSize);
                    case LimitingType.LeakyBucket:
                        return new LeakyBucketLimitingService(maxQPS, limitSize);
                    default:
                        break;
                }
            }
            return null;
        }

        public static ILimitingService Build(LimitingType limitingType, int masQPS)
        {
            return Build(limitingType, masQPS, masQPS);
        }
    }
}
