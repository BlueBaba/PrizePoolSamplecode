using App.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace App.Models
{
    public class SavingsCreationResponseModel
    {
        public string SavingsCode { get; set; }
        public DateTime ExpectedCompletionDate { get; set; }
    }

    /// <summary> Newly Created 
    public class TopupSavingsResponseModel
    {
        public string SavingsCode { get; set; }
        public DateTime ExpectedCompletionDate { get; set; }
    }

    public class WithdrawalSavingsResponseModel
    {
        public string SavingsCode { get; set; }
        public DateTime ExpectedCompletionDate { get; set; }
    }

    public class MakeInternalPaymentResponseModel
    {
        public string transactionReference { get; set; }
        public string transactionId { get; set; }
        public double balance { get; set; }
        public string responseCode { get; set; }
        public string responseMessage { get; set; }
    }


    public class ContributorModel
    {
        //public String CIF { get; set; }
        public String ProfileId { get; set; }
        public int Order { get; set; }
        internal bool IsAdmin { get; set; } = false;
        public string ContributorName { get; set; }
        public string EmailAddress { get; set; }

        [Required]
        public new PaymentInstrumentType PaymentInstrumentType { get; set; }

        [Required]
        public new string PaymentInstrumentReference { get; set; }
    }

    public class SavingsModel
    {
        public string SavingsName { get; set; }
        public decimal Balance { get; set; }
        public decimal InterestRate { get; set; }
        public InterestRateCalulation InterestRateCalulation { get; set; }
        public String InterestRateCalulationDesc { get; set; }

        public String Status { get; set; }

        public String Message { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime MaturityDate { get; set; }
        public decimal EstimatedReturn { get; set; }
        public decimal RegularDeductionAmount { get; set; }
        public decimal SavingsBalance { get; set; }
        public bool isFixed { get; set; }
        public string PlanType { get; set; }
        public List<ScheduleModel> Schedules { get; set; }
        public long Id { get; set; }
        public string PlanName { get; set; }
        public string SavingsCode { get; set; }

        public SavingsFrequencyEnum SavingsFrequency { get; set; }
        public decimal TargetSavingsAmount { get; set; } 
        public string PlanTypeDescription { get; set; }
    }

    public enum PaymentInstrumentType
    {
        Card = 1,
        Account = 2
    }

    public enum SavingsFrequencyEnum
    {
        Fixed = 0, 
        Daily = 1,
        Weekly = 2,
        Monthly = 3
    }

    public enum FixedSavingsMonthsEnum
    {   
        //30, 60, 90 and 365 days tenure   
        Thirtydays = 1,
        Sixtydays = 2,
        Ninetydays = 3,
        OneEigthydays = 6,
        ThreeSixtyFivedays = 12,
    }

    public enum SavingsStatusEnum
    {
        [Description("Pending Debit")]
        Pending_Debit = 1,//This only applies to fixed
        [Description("Pending Start Date")]
        Pending_StartDate = 2,
        [Description("Running")]
        Running = 3,
        [Description("Paused")]
        Paused = 4,
        [Description("Closed")]
        Closed = 5
    }

    public class ScheduleModel
    {
        public decimal ExpectedAmount { get; set; }
        public int NoOfContributorsPaid { get; set; }
        public decimal AmountPaid { get; set; }
        public DateTime ScheduleDate { get; set; }
        public int ReceivingOrder { get; set; }
    }

}
