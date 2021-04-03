using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Text;

namespace Oldmansoft.Identity
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

        private readonly MemoryCache Memory = new MemoryCache(new MemoryCacheOptions());

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
            if (Memory.TryGetValue(name, out int cache))
            {
                var tryWrongCount = cache;
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
            var oldValue = Memory.GetOrCreate(name, entry =>
            {
                entry.SetAbsoluteExpiration(DateTime.Now.AddSeconds(LockSecond));
                return 1;
            });

            Memory.Set(name, oldValue + 1, DateTime.Now.AddSeconds(LockSecond));
        }
    }
}
