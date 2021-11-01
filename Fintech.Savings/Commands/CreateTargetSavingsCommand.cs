using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Configuration;
using MassTransit;
using Newtonsoft.Json;
using Enterprise.Data;
using App.Entities;
using App.Models;
using Microsoft.Extensions.Caching.Distributed;
using Enterprise.Shared.Utilities;
using System.ComponentModel;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Hosting;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace App.Commands
{
    public class CreateTargetSavingsCommand : IRequest<GenericMessage<SavingsCreationResponseModel>>
    {
        private readonly IOptions<MySettings> appSettings;
        private IConfiguration _config { get; set; }
        private IWebHostEnvironment _host { get; set; }



        [Required]
        public long SavingsPlanId { get; set; }

        [Required]
        public string SavingsName { get; set; }
        public SavingsFrequencyEnum SavingsFrequency { get; set; }

        [Required]
        public decimal RegularDebitAmount { get; set; }

             
        [Required]
        public DateTime? PreferedStartDate { get; set; }

        [Required]       
        public int NoOfInstallments { get; set; }

        [Required]
        public string EmailAddress { get; internal set; }

        public string ProfileId { get; internal set; }

        [JsonIgnore]
        internal TokenData TokenData { get; set; }

        //
        /// <summary>
        /// 1 :Card
        /// 2: Account
        /// </summary>
        [Required]
        public PaymentInstrumentType PaymentInstrumentType { get; set; }

        /// <summary>
        /// When instrument type is 1 (Card) , specify card reference
        /// When instrument type is 2 (Account), specify account number
        /// </summary>
        [Required]
        public string PaymentInstrumentReference { get; set; }

    }
    public class CreateB2BTargetSavingsCommand: CreateTargetSavingsCommand
    {
        public new string ProfileId { get; set; }
        public new string EmailAddress { get;  set; }


        [Required]
        public new PaymentInstrumentType PaymentInstrumentType { get; set; }

        [Required]
        public new string PaymentInstrumentReference { get; set; }
    }

    public class CreateSavingsCommandHandler : IRequestHandler<CreateTargetSavingsCommand, GenericMessage<SavingsCreationResponseModel>>
    {
      
        private readonly ILogger<CreateSavingsCommandHandler> _logger;
        private readonly IUnitOfWork _uow;
        private readonly IBusControl _bus;
        private readonly IConfiguration _config;
        IDistributedCache _cache;

        public CreateSavingsCommandHandler(
               IConfiguration config,
            IBusControl bus,
             IUnitOfWork uow,
             ILogger<CreateSavingsCommandHandler> logger,
             IDistributedCache cache)
        {
            
            _uow = uow;
            _config = config;
            _logger = logger;
            _bus = bus;
            _cache = cache;
        }

        public async Task<GenericMessage<SavingsCreationResponseModel>> Handle(CreateTargetSavingsCommand request, CancellationToken cancellationToken)
        {
            decimal _periodicDebitAmount = 0;
            var repo = _uow.GetRepositoryAsync<SavingsRequest>();
            var planrepo = _uow.GetRepositoryAsync<SavingsPlan>();           

            var retPlan = planrepo.GetAsync(x => x.Id == request.SavingsPlanId).Select(s => new {
                s.InterestRate,s.InterestRateCalulation,s.PlanCode,s.PlanName,s.Description,s.Id }).FirstOrDefault();

            var retSavingsReq = repo.GetAsync(x => x.ProfileId == request.ProfileId).Select(s => new {s.ProfileId,
                s.Id,s.SavingsName,s.SavingsPlanId }).FirstOrDefault();


            if (request.PreferedStartDate == null || request.PreferedStartDate < DateTime.Today)
            {
                return new GenericMessage<SavingsCreationResponseModel>()
                {
                    IsSuccessful = false,
                    Message = "Specified prefered start date is invalid"
                };
            }

            if (request.SavingsFrequency == SavingsFrequencyEnum.Fixed) //Target savings not allowed for fixed frequency
            {
                return new GenericMessage<SavingsCreationResponseModel>()
                {
                    IsSuccessful = false,
                    Message = "Invalid savings frequency option indicated for target savings" //
                };
            }

            if (retPlan == null)
            {
                return new GenericMessage<SavingsCreationResponseModel>()
                {
                    IsSuccessful = false,
                    Message = "The selected savings plan is not valid."
                };
            }
            
            string allowedminimumNoOfInstallments = _config.GetSection("MySettings").GetSection("NoOfInstallments").Value;
            if ((request.NoOfInstallments <= Convert.ToInt16(allowedminimumNoOfInstallments))) 
            {
                return new GenericMessage<SavingsCreationResponseModel>()
                {
                    IsSuccessful = false,
                    Message = "NoOfInstallments must be greater that the minimum permissible value"
                };
            }

            if (retSavingsReq != null)
            {
                if (retSavingsReq.SavingsPlanId == request.SavingsPlanId && retSavingsReq.SavingsName == request.SavingsName && retSavingsReq.ProfileId == request.ProfileId)
                {
                    return new GenericMessage<SavingsCreationResponseModel>()
                    {
                        IsSuccessful = false,
                        Message = "Specified savings plan already exist for the user"
                    };
                }
            }

            if ((request.SavingsFrequency != SavingsFrequencyEnum.Daily) && (request.SavingsFrequency != SavingsFrequencyEnum.Weekly) && (request.SavingsFrequency != SavingsFrequencyEnum.Monthly)) //Fixed = 0, Daily = 1, Weekly = 2, //Monthly = 3
            {
                return new GenericMessage<SavingsCreationResponseModel>()
                {
                    IsSuccessful = false,
                    Message = "Invalid savings frequency" //should be 1,2 or 3
                };
            }

            FinacleTopupAcctOpeningRequest myFinTopupAcctOpnReq = new FinacleTopupAcctOpeningRequest();

            if (request.PaymentInstrumentType == PaymentInstrumentType.Account)
            {
                myFinTopupAcctOpnReq.debitAccountNumber = request.PaymentInstrumentReference; //CBA account number to debit
            }
            else
            {
                return new GenericMessage<SavingsCreationResponseModel>()
                {
                    IsSuccessful = false,
                    Message = "Invalid payment instrument type" 
                };
            }

            if (request.SavingsFrequency == SavingsFrequencyEnum.Monthly) //Weekly = 2, //Monthly = 3
            {
                myFinTopupAcctOpnReq.depositPeriodMonths = request.NoOfInstallments.ToString();
                myFinTopupAcctOpnReq.depositPeriodDays = "0";
            }

            if (request.SavingsFrequency == SavingsFrequencyEnum.Weekly) //Weekly = 2, //Monthly = 3
            {
                int divRest;
                divRest = request.NoOfInstallments / 7; 
                myFinTopupAcctOpnReq.depositPeriodDays = divRest.ToString();
                myFinTopupAcctOpnReq.depositPeriodMonths = "0";
            }
            if (request.SavingsFrequency == SavingsFrequencyEnum.Daily) //Daily = 1, Weekly = 2, //Monthly = 3
            {
                int divRest;
                divRest = request.NoOfInstallments; 
                myFinTopupAcctOpnReq.depositPeriodDays = divRest.ToString();
                myFinTopupAcctOpnReq.depositPeriodMonths = "0";
            }

            //Go ahead and create savings starts here
            //SavingsFrequency == Fixed = 0,  Daily = 1,   Weekly = 2,  Monthly = 3

            _periodicDebitAmount = Convert.ToDecimal(request.RegularDebitAmount / request.NoOfInstallments);

            SavingsRequest savingsRequest = new SavingsRequest()
            {
                SavingsPlanId = request.SavingsPlanId,
                SavingsFrequency = request.SavingsFrequency,
                NoOfInstallments = request.NoOfInstallments, 
                ProfileId = request.ProfileId,
                SavingsBalance = 0, 
                CBSBalance = 0,
                EstimatedReturn = 0,
                SavingsFrequencyDebitAmount = request.RegularDebitAmount,
                InterestRate = retPlan.InterestRate,
                InterestRateCalulation = retPlan.InterestRateCalulation,
                SavingsName = request.SavingsName,

                StartDate = request.PreferedStartDate ?? DateTime.Now,

                SavingsCode = Guid.NewGuid().ToString(),
                ContributorEmailAddress = request.EmailAddress,
                EmailAddress = request.EmailAddress,
                PaymentInstrumentType = request.PaymentInstrumentType,
                PaymentInstrumentReference = request.PaymentInstrumentReference,

                UserId = SavingsCalculation.GetCurrentUnixTimestampMillis(), 
                TimeStamp = DateTime.Now,
                SavingsStatus = SavingsStatusEnum.Running,
                SavingsSchedules = new System.Collections.Generic.List<SavingsSchedule>()
            };
            savingsRequest.ExpectedMaturityDate = SavingsCalculation.CalculateMaturityDate(savingsRequest);
            savingsRequest.ProjectedInterest = 0;

            decimal interestCalculation = request.RegularDebitAmount * (retPlan.InterestRate / 100);

            for (int order = 1; order <= request.NoOfInstallments; order++)
            {
                var CollectionDate = SavingsCalculation.CalculateCollectionDate(request.PreferedStartDate ?? DateTime.Now, request.SavingsFrequency, order);

                //SavingsFrequency == Fixed = 0,  Daily = 1,   Weekly = 2,  Monthly = 3
                savingsRequest.SavingsSchedules.Add(new SavingsSchedule()
                {
                    ScheduleIdentifier = order,
                    AmountPaid = 0,
                    SavingsCode = savingsRequest.SavingsCode,
                    EmailAddress = request.EmailAddress,
                    TransactionReference = "",
                    ExpectedAmount = _periodicDebitAmount,
                    ScheduleDate = CollectionDate,
                    TimeStamp = DateTime.Now
                });

            }
            await repo.AddAsync(savingsRequest);
            _uow.SaveChanges();

            return new GenericMessage<SavingsCreationResponseModel>()
            {
                IsSuccessful = true,
                Result = new SavingsCreationResponseModel()
                {
                    ExpectedCompletionDate = savingsRequest.ExpectedMaturityDate,
                    SavingsCode = savingsRequest.SavingsCode
                },
                Message = "Savings is requested successfuly"
            };
        }      

        public String ReadCache()
        {
            string value = _cache.GetString("CacheTime");
            if (value == null)
            {
                value = DateTime.Now.ToString();
                var options = new DistributedCacheEntryOptions();
                options.SetSlidingExpiration(TimeSpan.FromMinutes(1));
                _cache.SetString("CacheTime", value, options);
            }      
            return value;
        }
    }



}
