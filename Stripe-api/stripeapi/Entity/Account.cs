using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace stripeapi.Entity
{
    /// <summary>
    /// 用户表
    /// </summary>
    public class Account : BasePoco
    {
        public enum AccountTypeEnum
        {
            [Display(Name = "正式账号")]
            PUB,
            [Display(Name = "测试账号")]
            PRI
        }

        public enum AccountStatusEnum
        {
            [Display(Name = "解冻")]
            PUB,
            [Display(Name = "冻结")]
            PRI
        }

        [Display(Name = "第三方平台Id")]
        public string ThirdpartyId { get; set; }

        [Display(Name = "用户账号")]
        public string AccountName { get; set; }

        [Display(Name = "用户密码")]
        public string Password { get; set; }

        [Display(Name = "用户名称")]
        public string AccountTitle { get; set; }

        [Display(Name = "账号类型")]
        public AccountTypeEnum AccountType { get; set; }

        [Display(Name = "会员等级")]
        public Guid? VipLevelId { get; set; }       

        [Display(Name = "所属代理")]
        public Guid? AgentUserId { get; set; }

        [Display(Name = "邀请码")]
        public string Code { get; set; }

        [Display(Name = "下级邀请人数")]
        public int InvitationSum { get; set; }

        [Display(Name = "下级充值玩家总数")]
        public int InvitationPaySum { get; set; }

        [Display(Name = "钱包类型")]
        public Guid? WalletTypeId { get; set; }

        [Display(Name = "游戏余额")]
        [Column(TypeName = "decimal(10,2)")]
        public decimal Balance { get; set; }

        [Display(Name = "累计充值")]
        [Column(TypeName = "decimal(10,2)")]
        public decimal AddUpPay { get; set; }

        [Display(Name = "累计提现")]
        [Column(TypeName = "decimal(10,2)")]
        public decimal AddUpWithdraw { get; set; }

        [Display(Name = "累计投注")]
        [Column(TypeName = "decimal(10,2)")]
        public decimal AddUpBet { get; set; }

        [Display(Name = "注册时间")]
        public DateTime? AddTime { get; set; }

        [Display(Name = "注册IP")]
        public string AddIP { get; set; }

        [Display(Name = "最近上线时间")]
        public DateTime? LoginTime { get; set; }

        [Display(Name = "最后登录IP")]
        public string LoginIP { get; set; }

        [Display(Name = "手机号")]
        public string Phone { get; set; }

        [Display(Name = "邮箱")]
        public string Mail { get; set; }

        [Display(Name = "虚拟币钱包地址")]
        public string VirtualWallet { get; set; }

        [Display(Name = "用户状态")]
        public AccountStatusEnum AccountStatus { get; set; }

        [Display(Name = "所在游戏ID")]
        public string InGameID { get; set; }

        [Display(Name = "是否在线")]
        public string IsLoggedIn { get; set; }

        [Display(Name = "备注")]
        public string Remark { get; set; }

        [Display(Name = "用户客户端设置语言")]
        public string Language { get; set; }

        [Display(Name = "用户客户端设置皮肤")]
        public string Skin { get; set; }

        [Display(Name = "是否已计算邀请注册佣金")]
        public bool IsValid { get; set; }

        [Display(Name = "调游戏组渠道新增用户账号获取的id")]
        public int? ChannelAccountId { get; set; }

        public string StripeConnectAccountId { get; set; }
    }
}
