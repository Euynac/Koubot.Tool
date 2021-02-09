using System;

namespace Koubot.Tool.General
{
    /// <summary>
    /// 泛型工具类
    /// </summary>
    public static class GenericsTool
    {
        /// <summary>
        /// 将Predicate转化为对应的Func
        /// </summary>
        /// <param name="predicate"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static Func<T, bool> PredicateConvertToFunc<T>(Predicate<T> predicate) => new(predicate);
        

        /// <summary>
        /// (in T, in TIn, out TR)类型 将Func第二个参数类型（TIn）支持协变，即将TIn转换为指定类型TOut（TIn需是TOut的子类）
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TIn"></typeparam>
        /// <typeparam name="TOut"></typeparam>
        /// <typeparam name="TR"></typeparam>
        /// <param name="func"></param>
        /// <returns></returns>
        public static Func<T, TOut, TR> ConvertFunc<T, TIn, TOut, TR>(Func<T, TIn, TR> func) where TIn : TOut
        {
            return (t, p) => func(t, (TIn)p);
        }

        /// <summary>
        /// (in TIn, out TR)类型 将Func第一个参数类型（TIn）支持协变，即将TIn转换为指定类型TOut（TIn需是TOut的子类）
        /// </summary>
        /// <typeparam name="TIn"></typeparam>
        /// <typeparam name="TOut"></typeparam>
        /// <typeparam name="TR"></typeparam>
        /// <param name="func"></param>
        /// <returns></returns>
        public static Func<TOut, TR> ConvertFunc<TIn, TOut, TR>(Func<TIn, TR> func) where TIn : TOut
        {
            return p => func((TIn)p);
        }
    }
}
