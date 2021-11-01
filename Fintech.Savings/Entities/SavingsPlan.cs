using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using App.Commands;

namespace App.Entities
{
    [Table("SavingsPlan")]
    public class SavingsPlan
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }
        public string PlanName { get; set; } 
        public string PlanCode { get; set; } 
        public bool isFixed { get; set; }
        public SavingsPlanEnum PlanType { get; set; }
        public decimal InterestRate { get; set; }
        public InterestRateCalulation InterestRateCalulation { get; set; }
        public string Description { get;  set; }
    }

    public enum SavingsPlanEnum
    {
        [Description("Flexible")]
        FLEXIBLE=0,
        [Description("Target Savings")]
        FIXED_TARGET_SAVINGS =1,
        [Description("Fixed Deposit")]
        FIXED_DEPOSIT =2
    }

}