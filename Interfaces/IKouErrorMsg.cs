using JetBrains.Annotations;

namespace Koubot.Tool.Interfaces
{
    /// <summary>
    /// Kou环境下的错误信息接口（指示该类具有错误信息提供）
    /// </summary>
    public interface IKouErrorMsg
    {
        /// <summary>
        /// 错误信息
        /// </summary>
        public string ErrorMsg { get; set; }
    }

    /// <summary>
    /// IKouErrorMsg接口的拓展方法（当升级到.net standard 2.1后可迁移到接口默认实现）
    /// </summary>
    public static class IKouErrorMsgExtension
    {
        /// <summary>
        /// 快速打包错误（需要实现<see cref="IKouErrorMsg"/>接口）
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="errorMsg">错误内容</param>
        /// <returns>必定返回false用于快速return</returns>
        //[ContractAnnotation("=>false")]
        public static bool ReturnError([CanBeNull] this IKouErrorMsg obj, string errorMsg = null)
        {
            if (obj == null) return false;
            obj.ErrorMsg = errorMsg;
            return false;
        }
        /// <summary>
        /// 检查是否有错误
        /// </summary>
        /// <param name="errorObject"></param>
        /// <returns></returns>
        public static bool HasError(this IKouErrorMsg errorObject) => !string.IsNullOrWhiteSpace(errorObject.ErrorMsg);
    }
}