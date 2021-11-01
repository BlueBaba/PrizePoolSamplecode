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
using System.Collections.Generic;
using Enterprise.Shared.Utilities;
using System.ComponentModel;
using Enterprise.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.Azure.ServiceBus;

namespace App.Commands
{
    public class GetCustomerSavingsPlansQuery : IRequest<List<SavingsModel>>
    {
        [JsonIgnore]
        public string ProfileId { get; set; }
    }
    public class GetCustomerSavingsPlansQueryHandler : IRequestHandler<GetCustomerSavingsPlansQuery, List<SavingsModel>>
    {      
        private readonly ILogger<GetCustomerSavingsPlansQueryHandler> _logger;
        private readonly IUnitOfWork _uow;
        private readonly IBusControl _bus;
        private readonly IConfiguration _config;
        IDistributedCache _cache;

        public GetCustomerSavingsPlansQueryHandler(
               IConfiguration config,
            IBusControl bus,
             IUnitOfWork uow,
             ILogger<GetCustomerSavingsPlansQueryHandler> logger,
             IDistributedCache cache)
        {        
            _uow = uow;
            _config = config;
            _logger = logger;
            _bus = bus;
            _cache = cache;
        }

        public async Task<List<SavingsModel>> Handle(GetCustomerSavingsPlansQuery request, CancellationToken cancellationToken)
        {
            var repo = _uow.GetRepositoryAsync<SavingsRequest>();

            var savingsPlans = (await repo.GetListAsync(x => x.ProfileId == request.ProfileId, include: i => i.Include(c => c.Plan).Include(c => c.SavingsSchedules))).Items.Select(s => new SavingsModel
            {
                Id = s.Id,
                SavingsName = s.SavingsName,

                Balance = s.SavingsBalance,
                Status = s.SavingsStatus.GetDescription(),
                PlanName = s.Plan.PlanName,                
                SavingsCode = s.SavingsCode,
                SavingsFrequency = s.SavingsFrequency,
                TargetSavingsAmount = s.SavingsFrequencyDebitAmount,              
                InterestRate = s.InterestRate,
                InterestRateCalulation = s.InterestRateCalulation,
                InterestRateCalulationDesc = s.InterestRateCalulation.GetDescription(),
                StartDate = s.StartDate,
                EstimatedReturn = s.EstimatedReturn,
                MaturityDate = s.ExpectedMaturityDate,
                RegularDeductionAmount = s.SavingsFrequencyDebitAmount,
                SavingsBalance = s.SavingsBalance,
                isFixed = s.Plan.isFixed,
                PlanTypeDescription = s.Plan.PlanType.GetDescription(),                
                Schedules = s.SavingsSchedules?.Select(c => new ScheduleModel()
                {
                    AmountPaid = c.AmountPaid,
                    ExpectedAmount = c.ExpectedAmount,
                    NoOfContributorsPaid = 0,
                    ScheduleDate = c.ScheduleDate
                })?.ToList(),
                PlanType = s.Plan.PlanType.GetDescription()
            }).ToList();

            return savingsPlans;           

        }

    }



}
