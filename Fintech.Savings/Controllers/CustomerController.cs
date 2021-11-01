using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Threading.Tasks;
using App.Commands;
using App.Models;
using Enterprise.Shared.Utilities;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace App.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CustomerController : ControllerBase
    {
        private readonly ILogger<CustomerController> _logger;
        private readonly IMediator _mediator;

        public String TraceId
        {
            get
            {
                return Activity.Current?.Id ?? HttpContext.TraceIdentifier;
            }
        }

        public CustomerController(IMediator mediator, ILogger<CustomerController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(List<SavingsPlanModel>), (int)HttpStatusCode.OK)]
        [Produces("application/json")]
        //[Authorize]
        [HttpGet, Route("savingsplans")]
        public async Task<IActionResult> GetSavingsPlanQuery()
        {
            try
            {
                var result = await _mediator.Send(new GetSavingsPlanQuery());
                _logger.LogInformation($"Savings plan query response  - : {result}");

                return Ok(result);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"TraceId: {TraceId}");
                return new BadRequestObjectResult(new ErrorResponseModel()
                {
                    TraceMessages = ex.Message,
                    Message = "An error occured while trying to perform operations",
                    RequestId = TraceId
                });

            }

        }


        [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(List<SavingsModel>), (int)HttpStatusCode.OK)]
        [Produces("application/json")]
        [HttpGet, Route("customersavings")]
        public async Task<IActionResult> GetCustomerSavingsPlansQuery(string profileId) 
        {
            try
            {
                var cspjsonrequestbody = JsonConvert.SerializeObject(profileId);
                _logger.LogInformation($"Customer savings plan query request body received at controller  - : {cspjsonrequestbody}");

                var result = await _mediator.Send(new GetCustomerSavingsPlansQuery() { ProfileId = profileId });

                _logger.LogInformation($"Customer savings plan query response  - : {result}");

                if (result.Count > 0)
                {
                    return Ok(result);
                }
                else
                {
                    var unfoundresp = new List<SavingsModel>();
                    SavingsModel mySavmodel = new SavingsModel();

                    mySavmodel.Status = "not found";
                    mySavmodel.Message = "Profile not found";

                    unfoundresp.Add(mySavmodel);
                    result = unfoundresp;

                    return Ok(result);
                }
                
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"TraceId: {TraceId}");
                return new BadRequestObjectResult(new ErrorResponseModel()
                {
                    TraceMessages = ex.Message,
                    Message = "An error occured while trying to perform operations",
                    RequestId = TraceId
                });

            }

        }


        [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(SavingsCreationResponseModel), (int)HttpStatusCode.OK)]
        [Produces("application/json")]
        [HttpPost, Route("createfixedsavingsforcustomer")]
        public async Task<IActionResult> CreateB2BFixedSavingsCommand([FromBody]CreateB2BFixedSavingsCommand command)
        {
            try
            {
                var fdjsonrequestbody = JsonConvert.SerializeObject(command);
                _logger.LogInformation($"Fixed deposit request body received at controller  - : {fdjsonrequestbody}");

                var newcommand = new CreateFixedSavingsCommand()
                {
                    ProfileId = command.ProfileId,
                    EmailAddress = command.EmailAddress,
                    Amount = command.Amount,
                    HowLong = command.HowLong,
                    PaymentInstrumentReference = command.PaymentInstrumentReference,
                    PaymentInstrumentType=command.PaymentInstrumentType,
                    SavingsFrequency = command.SavingsFrequency,
                    SavingsName = command.SavingsName,
                    SavingsPlanId = command.SavingsPlanId,

                    Autorenewal = command.Autorenewal,
                    Autoclose = command.Autoclose                    
                };

                if (command.ProfileId == command.PaymentInstrumentReference)
                {
                    _logger.LogInformation($"Fixed deposit response  - : { $"Message: = Invalid Profile and Instrument, TraceId: {TraceId}" }");
                    return new BadRequestObjectResult(new ErrorResponseModel() { Message = "Invalid Profile and Instrument", RequestId = TraceId });
                }

                var result = await _mediator.Send(newcommand);
                var resultjsonrequestbody = JsonConvert.SerializeObject(result);

                _logger.LogInformation($"Fixed deposit response  - : {resultjsonrequestbody}, TraceId: {TraceId}");

                if (result.IsSuccessful)
                {
                    return Ok(result.Result);
                }
                else
                {
                    return new BadRequestObjectResult(new ErrorResponseModel() { Message = result.Message, RequestId = TraceId });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"TraceId: {TraceId}");
                return new BadRequestObjectResult(new ErrorResponseModel()
                {
                    TraceMessages = ex.Message,
                    Message = "An error occured while trying to perform operations",
                    RequestId = TraceId
                });

            }

        }

        [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(SavingsCreationResponseModel), (int)HttpStatusCode.OK)]
        [Produces("application/json")]
        [HttpPost, Route("createtargetsavingsforcustomer")]
        public async Task<IActionResult> CreateB2BTargetSavingsCommand([FromBody]CreateB2BTargetSavingsCommand command)
        {
            try
            {
                var tsjsonrequestbody = JsonConvert.SerializeObject(command);
                _logger.LogInformation($"Target savings request body received at controller  - : {tsjsonrequestbody}");

                var newcommand = new CreateTargetSavingsCommand()
                {
                    ProfileId = command.ProfileId,                    
                    EmailAddress =command.EmailAddress,

                    PaymentInstrumentReference = command.PaymentInstrumentReference, //Source account number or card number
                    PaymentInstrumentType = command.PaymentInstrumentType,
                    NoOfInstallments =command.NoOfInstallments,
                    PreferedStartDate=command.PreferedStartDate,
                    RegularDebitAmount=command.RegularDebitAmount,
                    SavingsFrequency=command.SavingsFrequency,
                    SavingsName=command.SavingsName,
                    SavingsPlanId=command.SavingsPlanId
                };
                var result = await _mediator.Send(newcommand);                
                _logger.LogInformation($"Target savings response  - : {result}");

                if (result.IsSuccessful)
                {
                    return Ok(result.Result);
                }
                else
                {
                    return new BadRequestObjectResult(new ErrorResponseModel() { Message = result.Message, RequestId = TraceId });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"TraceId: {TraceId}");
                return new BadRequestObjectResult(new ErrorResponseModel()
                {
                    TraceMessages = ex.Message,
                    Message = "An error occured while trying to perform operations",
                    RequestId = TraceId
                });

            }

        }
        
    }
}









