using System.ComponentModel.DataAnnotations;

namespace stripeapi.Entity
{
    public class BasePoco: TopBasePoco
    {
        //
        // 摘要:
        //     CreateTime
        [Display(Name = "_Admin.CreateTime")]
        public DateTime? CreateTime { get; set; }

        //
        // 摘要:
        //     CreateBy
        [Display(Name = "_Admin.CreateBy")]
        [StringLength(50, ErrorMessage = "Validate.{0}stringmax{1}")]
        public string CreateBy { get; set; }

        //
        // 摘要:
        //     UpdateTime
        [Display(Name = "_Admin.UpdateTime")]
        public DateTime? UpdateTime { get; set; }

        //
        // 摘要:
        //     UpdateBy
        [Display(Name = "_Admin.UpdateBy")]
        [StringLength(50, ErrorMessage = "Validate.{0}stringmax{1}")]
        public string UpdateBy { get; set; }
    }
}
