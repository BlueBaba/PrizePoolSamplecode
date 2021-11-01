using App.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace App.Entities
{
    public class SavingsCalculation
    {
        private static readonly DateTime UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        public static long GetCurrentUnixTimestampMillis()
        {
            DateTime localDateTime, univDateTime;
            localDateTime = DateTime.Now;
            univDateTime = localDateTime.ToUniversalTime();
            return (long)(univDateTime - UnixEpoch).TotalMilliseconds;
        }
        public static DateTime CalculateMaturityDate(SavingsRequest savingsRequest)
        {
            return savingsRequest.SavingsFrequency switch
            {
                SavingsFrequencyEnum.Fixed => savingsRequest.StartDate.AddDays(savingsRequest.NoOfInstallments),
                SavingsFrequencyEnum.Daily => savingsRequest.StartDate.AddDays(savingsRequest.NoOfInstallments),
                SavingsFrequencyEnum.Weekly => savingsRequest.StartDate.AddDays(savingsRequest.NoOfInstallments * 7),
                SavingsFrequencyEnum.Monthly => savingsRequest.StartDate.AddMonths(savingsRequest.NoOfInstallments),
                _ => throw new Exception($"Period not captured. ({savingsRequest.SavingsFrequency})"),
            };
        }

        public static DateTime CalculateCollectionDate(DateTime StartDate, SavingsFrequencyEnum frequency, int Order)
        {
            if (Order < 1) Order = 1;
            return frequency switch
            {
                SavingsFrequencyEnum.Daily => StartDate.AddDays(Order - 1),
                SavingsFrequencyEnum.Weekly => StartDate.AddDays(Order - 1 * 7),
                SavingsFrequencyEnum.Monthly => StartDate.AddMonths(Order - 1),
                _ => throw new Exception("Period not captured."),
            };
        }

        public static FixedSavingsMonthsEnum GetFixedSavingsDaisInMonths(string noOfDays)
        {
            FixedSavingsMonthsEnum Months = new FixedSavingsMonthsEnum();

            if (Convert.ToInt32(noOfDays) == 30) 
            {
                Months = FixedSavingsMonthsEnum.Thirtydays;
            }
            else if (Convert.ToInt32(noOfDays) == 60) 
            {
                Months = FixedSavingsMonthsEnum.Sixtydays;
            }
            else if (Convert.ToInt32(noOfDays) == 90) 
            {
                Months = FixedSavingsMonthsEnum.Ninetydays;
            }
            else if (Convert.ToInt32(noOfDays) == 180) 
            {
                Months = FixedSavingsMonthsEnum.OneEigthydays;
            }
            else if (Convert.ToInt32(noOfDays) == 365) 
            {
                Months = FixedSavingsMonthsEnum.ThreeSixtyFivedays;
            }
            return Months;
        }

        public decimal getNow(SavingsRequest savingsRequest)
        {

            return savingsRequest.InterestRateCalulation switch
            {
                InterestRateCalulation.PerDay => 0,
                InterestRateCalulation.PerMonth => 0,
                InterestRateCalulation.PerAnnum => 0,
                _ => 0,
            };

        }

    }
}
