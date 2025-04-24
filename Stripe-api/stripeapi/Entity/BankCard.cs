using System.ComponentModel.DataAnnotations;

namespace stripeapi.Entity
{
    /// <summary>
    /// 用户银行卡信息
    /// </summary>
    public class BankCard : BasePoco
    {
        [Display(Name = "用户Id")]
        public Guid Uid { get; set; }

        [Display(Name = "银行卡类型Id")]
        public Guid? BankCardTypeId { get; set; }

        [Display(Name = "银行卡类型名称")]
        public string BankCardTypeName { get; set; }

        [Display(Name = "银行卡号")]
        public string CardNumber { get; set; }

        [Display(Name = "姓名")]
        public string CardHolder { get; set; }

        [Display(Name = "电话")]
        public string Phone { get; set; }

        [Display(Name = "电子邮箱")]
        public string Email { get; set; }
    }
}
