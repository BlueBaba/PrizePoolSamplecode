using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using App.Commands;
using App.Models;

namespace App.Entities
{
    [Table("SavingsRequest")]
    public class SavingsRequest
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }
        public long UserId { get; set; }
        public string CIF { get; set; }
        public string ProfileId { get; set; }
        public decimal SavingsFrequencyDebitAmount { get; set; }
        public string SavingsName { get; set; }
        public virtual SavingsPlan Plan { get; set; }
        [ForeignKey("SavingsPlan")]
        public long SavingsPlanId { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime ExpectedMaturityDate { get; set; }

        public decimal InterestRate { get; set; }
        public InterestRateCalulation InterestRateCalulation { get; set; }
        public SavingsFrequencyEnum SavingsFrequency { get; set; }
        public int NoOfInstallments { get; set; }
        public DateTime TimeStamp { get; set; }
        public decimal SavingsBalance { get; set; }
        public decimal EstimatedReturn { get; set; }

        public string SavingsCoreBankingAccount { get; set; }
        public decimal ProjectedInterest { get; set; }

        
        public string RolloverStatus { get; set; }
        public string AutoCloseOnMaturityFlg { get; set; }
        public string AutoRenewalFlag { get; set; }


        public virtual List<SavingsSchedule> SavingsSchedules { get; set; }
        public SavingsStatusEnum SavingsStatus { get;  set; }
        public string PaymentInstrumentReference { get;  set; }
        public PaymentInstrumentType PaymentInstrumentType { get;  set; }
        public string ContributorEmailAddress { get;  set; }
        public string SavingsCode { get;  set; }
        public string EmailAddress { get;  set; }
        public bool ForfeitInterest { get;  set; }
        public decimal CBSBalance { get;  set; }
        public decimal PenaltyApplied { get;  set; }
    }


    [Table("SavingsSchedule")]
    public class SavingsSchedule
    {

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }
        public DateTime ScheduleDate { get; set; }
        public DateTime? TimeStamp { get; set; }
        public decimal AmountPaid { get; set; }
        public decimal ExpectedAmount { get; set; }
        public int ScheduleIdentifier { get; set; }


        [ForeignKey("SavingsRequest")]
        public long SavingsRequestId { get; set; }
        public virtual SavingsRequest SavingsRequest { get; set; }
        public string SavingsCode { get; set; }
        public string EmailAddress { get; set; }
        public string TransactionReference { get; internal set; }
    }


    [Table("TopWithdrwalActivity")]
    public class TopWithdrwalActivity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        public string ProfileId { get; set; }
        public string SavingsCode { get; set; }
        public string TranType { get; set; }
        public decimal Amount { get; set; }
        public decimal Oldbalance { get; set; }
        public decimal Newbalance { get; set; }
        public string TransactionRef { get; set; }
        public string Description { get; set; }
        public string Accountnumber { get; set; }
        public string Responsecode { get; set; }
        public string Errorresponse { get; set; }
        public DateTime TransactionTimestamp { get; set; }      
    }


    [Table("AppClient")]
    public class AppClient
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }
        public string ClientName { get; set; }
        public string ClientCode { get; set; }
        public string ClientKey { get; set; }
        public DateTime CreatedDate { get; set; }
        public string Status { get; set; }
    }






    public enum RequestStatus
    {
        [Description("Pending")]
        Pending = 0,
        [Description("Accepted")]
        Accepted = 1,
        [Description("Rejected")]
        Rejected = 2

    }






}