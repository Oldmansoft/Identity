using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Oldmansoft.Identity.Domain
{
    public abstract class Account
    {
        /// <summary>
        /// 序号
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 成员序号
        /// </summary>
        public Guid? MemberId { get; set; }

        /// <summary>
        /// 成员类型
        /// </summary>
        public int MemberType { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreatedTime { get; set; }

        /// <summary>
        /// 创建帐号
        /// </summary>
        public Account()
        {
            CreatedTime = DateTime.Now;
        }

        protected abstract byte[] GetPasswordHash();

        protected abstract void SetPasswordHash(byte[] hash);

        public abstract List<Guid> GetRoleIds();

        public abstract void SetRoleIds(Guid[] roleIds);

        /// <summary>
        /// 检测密码
        /// </summary>
        /// <param name="doubleSHA256Hash">双重 SHA256 十六进制散列</param>
        /// <param name="seed">加密种子</param>
        /// <returns></returns>
        public bool CheckPassword(string doubleSHA256Hash, string seed)
        {
            if (string.IsNullOrEmpty(doubleSHA256Hash)) throw new ArgumentNullException("doubleSHA256Hash");
            if (string.IsNullOrEmpty(seed)) throw new ArgumentNullException("seed");

            string passwordHash = BitConverter.ToString(GetPasswordHash()).Replace("-", "");
            byte[] source = Encoding.UTF8.GetBytes(string.Format("{0}{1}", passwordHash, seed));

            byte[] hash = source.GetSHA256Hash();
            return BitConverter.ToString(hash).Replace("-", "") == doubleSHA256Hash.ToUpper();
        }

        /// <summary>
        /// 检测密码
        /// </summary>
        /// <param name="passwordSHA256Hash">密码 SHA256 十六进制散列</param>
        /// <returns></returns>
        public bool CheckPasswordHash(string passwordSHA256Hash)
        {
            return BitConverter.ToString(GetPasswordHash()).Replace("-", "") == passwordSHA256Hash.ToUpper();
        }

        /// <summary>
        /// 设置密码
        /// </summary>
        /// <param name="passwordSHA256Hash">密码 SHA256 十六进制散列</param>
        public void SetPasswordHash(string passwordSHA256Hash)
        {
            if (passwordSHA256Hash == null || passwordSHA256Hash.Length == 0) throw new ArgumentNullException();
            if (passwordSHA256Hash.Length % 2 == 1) throw new ArgumentException("不是标准的十六进制字符串", "passwordHash");
            var hash = new byte[passwordSHA256Hash.Length / 2];
            for (var i = 0; i < hash.Length; i++)
            {
                hash[i] = Convert.ToByte(passwordSHA256Hash.Substring(i * 2, 2), 16);
            }
            SetPasswordHash(hash);
        }

        /// <summary>
        /// 是否已经绑定
        /// </summary>
        /// <returns></returns>
        public bool IsBound()
        {
            return MemberId.HasValue;
        }

        /// <summary>
        /// 绑定到会员
        /// </summary>
        /// <param name="memberId"></param>
        /// <param name="memberType"></param>
        /// <returns></returns>
        public bool Bind(Guid memberId, int memberType)
        {
            if (!IsBound())
            {
                MemberId = memberId;
                MemberType = memberType;
                return true;
            }
            return false;
        }

        /// <summary>
        /// 解除绑定
        /// </summary>
        /// <param name="memberId"></param>
        /// <returns></returns>
        public bool Unbind(Guid memberId)
        {
            if (MemberId == memberId)
            {
                MemberId = null;
                MemberType = 0;
                return true;
            }
            return false;
        }

        /// <summary>
        /// 是否是系统内属
        /// </summary>
        /// <returns></returns>
        public bool IsSystem()
        {
            return MemberType == -1;
        }
    }
}
