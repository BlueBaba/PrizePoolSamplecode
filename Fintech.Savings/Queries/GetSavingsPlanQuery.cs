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
using Enterprise.Shared;

namespace App.Commands
{
    public class GetSavingsPlanQuery : IRequest<List<SavingsPlanModel>>
    {
        public TokenData UserData { get;  set; }


    }


    public class SavingsPlanModel
    {
        public String PlanName { get; set; }
        public String PlanCode { get; set; }
        public bool isFixed { get; set; }
        public decimal InterestRate { get;  set; }
        public InterestRateCalulation InterestRateCalulation { get; set; }
        public String InterestRateCalulationDesc { get; set; }
        public long Id { get;  set; }
        public string Description { get;  set; }
        public string PlanTypeDescription { get; internal set; }
    }
    

    public class GetSavingsPlanQueryHandler : IRequestHandler<GetSavingsPlanQuery, List<SavingsPlanModel>>
    {
      
        private readonly ILogger<GetSavingsPlanQueryHandler> _logger;
        private readonly IUnitOfWork _uow;
        private readonly IBusControl _bus;
        private readonly IConfiguration _config;
        IDistributedCache _cache;

        public GetSavingsPlanQueryHandler(
               IConfiguration config,
            IBusControl bus,
             IUnitOfWork uow,
             ILogger<GetSavingsPlanQueryHandler> logger,
             IDistributedCache cache)
        {         
            _uow = uow;
            _config = config;
            _logger = logger;
            _bus = bus;
            _cache = cache;
        }

        public async Task<List<SavingsPlanModel>> Handle(GetSavingsPlanQuery request, CancellationToken cancellationToken)
        {
            var repo = _uow.GetRepositoryAsync<SavingsPlan>();


           var savingsPlans= repo.GetAsync(x =>true).Select(s => new SavingsPlanModel {
               Id=s.Id,
               InterestRate=s.InterestRate,
               Description= s.Description,
               isFixed= s.isFixed,
               InterestRateCalulation=s.InterestRateCalulation,
               InterestRateCalulationDesc=s.InterestRateCalulation.GetDescription(),
               PlanTypeDescription=s.PlanType.GetDescription(),
               PlanCode=s.PlanCode,
               PlanName=s.PlanName

            }).ToList() ;

            return savingsPlans;

        }


    }



}
