using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Text;
using System.Threading.Tasks;

namespace WebManApplication
{
    /// <summary>
    /// 密码守卫
    /// </summary>
    public class PasswordGuard
    {
        /// <summary>
        /// 唯一实例
        /// </summary>
        public static readonly PasswordGuard Instance = new PasswordGuard();

        private readonly MemoryCache Memory = new MemoryCache("Identity");

        /// <summary>
        /// 重试次数
        /// </summary>
        public int CanTryCount { get; set; }

        /// <summary>
        /// 锁定秒数
        /// </summary>
        public int LockSecond { get; set; }

        private PasswordGuard()
        {
            CanTryCount = 3;
            LockSecond = 10;
        }

        /// <summary>
        /// 是否锁定
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public bool IsLockedOut(string name)
        {
            var cacheItem = Memory.GetCacheItem(name);
            if (cacheItem != null)
            {
                var tryWrongCount = (int)cacheItem.Value;
                if (tryWrongCount >= CanTryCount)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 设置计数
        /// </summary>
        /// <param name="name"></param>
        public void SetCount(string name)
        {
            var oldValue = Memory.AddOrGetExisting(name, 1, DateTime.Now.AddSeconds(LockSecond));
            if (oldValue != null)
            {
                Memory.Set(name, (int)oldValue + 1, DateTime.Now.AddSeconds(LockSecond));
            }
        }
    }
}
