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
using App.Utilities;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace App.Commands
{
    public class CreateFixedSavingsCommand : IRequest<GenericMessage<SavingsCreationResponseModel>>
    {
        public long SavingsPlanId { get; set; }
        [Required]
        public string SavingsName { get; set; }

        [Required]
        public decimal Amount { get; set; }

        [Required]
        public SavingsFrequencyEnum SavingsFrequency { get; set; }

        public string EmailAddress { get; internal set; }
        public string ProfileId { get; internal set; }

        [Required]
        public int HowLong { get; set; }
       
        [JsonIgnore]
        internal TokenData TokenData { get; set; }

        public string Autorenewal { get; set; }

        public string Autoclose { get; set; }
      

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


    public class CreateB2BFixedSavingsCommand : CreateFixedSavingsCommand
    {
        public new string ProfileId { get; set; }
        public new string EmailAddress { get; set; }
    }

    public class CreateFixedSavingsCommandHandler : IRequestHandler<CreateFixedSavingsCommand, GenericMessage<SavingsCreationResponseModel>>
    {
      
        private readonly ILogger<CreateFixedSavingsCommandHandler> _logger;
        private readonly IUnitOfWork _uow;
        private readonly IBusControl _bus;
        private readonly IConfiguration _config;
        IDistributedCache _cache;

        public CreateFixedSavingsCommandHandler(
               IConfiguration config,
            IBusControl bus,
             IUnitOfWork uow,
             ILogger<CreateFixedSavingsCommandHandler> logger,
             IDistributedCache cache)
        {
            
            _uow = uow;
            _config = config;
            _logger = logger;
            _bus = bus;
            _cache = cache;
        }

        public async Task<GenericMessage<SavingsCreationResponseModel>> Handle(CreateFixedSavingsCommand request, CancellationToken cancellationToken)
        {
            FinacleTopupAcctOpeningRequest myFinTopupAcctOpnReq = new FinacleTopupAcctOpeningRequest();
            TopupDepositAcctOpeningResp myFinTopupAcctOpnResp = new TopupDepositAcctOpeningResp();

            FinacleFDAcctOpeningRequest myFinFDAcctOpnReq = new FinacleFDAcctOpeningRequest();
            FinacleFDAcctOpeningResponse myFinFDAcctOpnResp = new FinacleFDAcctOpeningResponse();

            BalanceInquiryRequest myFinAcctBalReq = new BalanceInquiryRequest();
            BalanceInquiryResponse myFinAcctBalResp = new BalanceInquiryResponse();           

            if (request.SavingsFrequency != SavingsFrequencyEnum.Fixed)
            {
                return new GenericMessage<SavingsCreationResponseModel>()
                {
                    IsSuccessful = false,
                    Message = "Invalid savings frequency option indicated for fixed deposit" //
                };
            }

            if ((request.HowLong != 30) && (request.HowLong != 60) && (request.HowLong != 90) && (request.HowLong != 180) && (request.HowLong != 365)) 
            {
                return new GenericMessage<SavingsCreationResponseModel>()
                {
                    IsSuccessful = false,
                    Message = "Invalid minimum days for fixed deposit" //
                };
            }
            else
            {
                myFinTopupAcctOpnReq.debitAccountNumber = request.PaymentInstrumentReference; //CBA account number to debit
            }

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

            SavingsPlanEnum _myplantype;
            var repo = _uow.GetRepositoryAsync<SavingsRequest>();
            var planrepo = _uow.GetRepositoryAsync<SavingsPlan>();

            var retPlan = planrepo.GetAsync(x => (x.PlanType == SavingsPlanEnum.FIXED_TARGET_SAVINGS || x.PlanType ==  SavingsPlanEnum.FIXED_DEPOSIT) && x.Id ==  request.SavingsPlanId).Select(s => new {
                s.PlanName,
                s.PlanType,
                s.InterestRate,
                s.InterestRateCalulation                               
            }).FirstOrDefault();

            var retSavReq = repo.GetAsync(x => x.ProfileId == request.ProfileId).Select(s => new {
                s.ProfileId,
                s.SavingsPlanId,
                s.SavingsName,
                s.Id,
                s.EmailAddress
            }).ToList();


            if (retPlan == null)
            {
                return new GenericMessage<SavingsCreationResponseModel>()
                {
                    StatusCode=ErrorCodes.Savings_Plan_Not_Valid,
                    IsSuccessful = false,
                    Message = "The selected savings plan is not valid."
                };
            }
            else
            {
                _myplantype = retPlan.PlanType;
            }

            if (retSavReq.Count > 0)
            {
                foreach (var item in retSavReq) // Loop through List with foreach
                {
                    if (item.SavingsName == request.SavingsName)
                    {
                        return new GenericMessage<SavingsCreationResponseModel>()
                        {
                            StatusCode = ErrorCodes.Savings_Exist_For_Same_Administrator,
                            IsSuccessful = false,
                            Message = "Fixed deposit name exists for this profile/user"
                        };
                    }

                    if ((item.SavingsPlanId == request.SavingsPlanId) && (item.SavingsName == request.SavingsName))
                    {
                        return new GenericMessage<SavingsCreationResponseModel>()
                        {
                            StatusCode = ErrorCodes.Savings_Plan_Already_Exist,
                            IsSuccessful = false,
                            Message = "Specified FD plan and name already exist for the user"
                        };
                    }

                }
            }

            if ((request.SavingsFrequency != SavingsFrequencyEnum.Fixed)) //Fixed = 0, Daily = 1, Weekly = 2, //Monthly = 3
            {
                return new GenericMessage<SavingsCreationResponseModel>()
                {
                    IsSuccessful = false,
                    Message = "Invalid savings frequency"
                };
            }

            var jsonrequestbody = JsonConvert.SerializeObject(request);
            _logger.LogInformation($"Fixed deposit request body received after successful validation  - : {jsonrequestbody}");

            //Get customer's account balance
            _logger.LogInformation($"Fixed deposit - About calling finacle for customer's account balance");
            string acct_balance_url = _config.GetSection("MySettings").GetSection("FIAccount_Balance__url").Value;
            string acct_balance_key = _config.GetSection("MySettings").GetSection("FIBalanceInquiry__uniquekey").Value;
            string acct_balance_response = "";
            string customer_acct_bal = "";
            myFinAcctBalReq.accountNumber = request.PaymentInstrumentReference; //the account number
            myFinAcctBalReq.uniqueKey = acct_balance_key;

            var acctbaljson = JsonConvert.SerializeObject(myFinAcctBalReq);

            var acctbalancehandler = new HttpClientHandler();
            using (var acctbalanceclient = new HttpClient(acctbalancehandler))
            {
                var data = new StringContent(acctbaljson, Encoding.UTF8, "application/json");
                acctbalanceclient.Timeout = TimeSpan.FromMinutes(5);
                acctbalanceclient.BaseAddress = new Uri(acct_balance_url);
                acctbalanceclient.DefaultRequestHeaders.Accept.Clear();
                acctbalanceclient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                _logger.LogInformation($"Fixed deposit - Calling to get customer account balance json request : {acctbaljson}");
                _logger.LogInformation($"Fixed deposit - Calling to fetch account balance via : {acctbalanceclient.BaseAddress.AbsolutePath + " -- " + acctbalanceclient.BaseAddress.AbsoluteUri}");
                
                HttpResponseMessage response = await acctbalanceclient.PostAsync(acct_balance_url, data);
                if (response.IsSuccessStatusCode)
                {
                    acct_balance_response = response.Content.ReadAsStringAsync().Result;
                    _logger.LogInformation($"Fixed deposit - Received account balance response from  + {acctbalanceclient.BaseAddress.AbsoluteUri} +  :  + { acct_balance_response}");

                    BalanceInquiryResponse myBalInqJsonResp = JsonConvert.DeserializeObject<BalanceInquiryResponse>(acct_balance_response);
                    customer_acct_bal = myBalInqJsonResp.availableBalance;

                    var cust_avail_bal = Convert.ToDecimal(customer_acct_bal);

                    if ((myBalInqJsonResp.responseMessage.Trim().ToLower() == "approved") && (cust_avail_bal < request.Amount))
                    {
                        return new GenericMessage<SavingsCreationResponseModel>()
                        {
                            IsSuccessful = false,
                            Message = "Amount to be fixed must be greater than balance on account"
                        };
                    }

                    else if ((myBalInqJsonResp.responseMessage.Trim().ToLower() != "approved"))
                    {
                        return new GenericMessage<SavingsCreationResponseModel>()
                        {
                            IsSuccessful = false,
                            Message = myBalInqJsonResp.responseMessage
                        };
                    }

                }
                else
                {
                    acct_balance_response = response.Content.ReadAsStringAsync().Result;
                    _logger.LogInformation($"Fixed deposit - Received account balance response from  + {acctbalanceclient.BaseAddress.AbsoluteUri} +  :  + { acct_balance_response}");

                    BalanceInquiryResponse myBalInqJsonResp = JsonConvert.DeserializeObject<BalanceInquiryResponse>(acct_balance_response);

                    return new GenericMessage<SavingsCreationResponseModel>()
                    {
                        IsSuccessful = false,
                        Message = myBalInqJsonResp.responseMessage
                    };
                }
            }            
            //Get customer's account balance ends here


            //Get customer's CIF
            string custcifurl = _config.GetSection("MySettings").GetSection("CustomerProfileCIF_url").Value;
            string custCif = "";
            string custBVN = "";
            string myCustProfileJsonResponse = "";

            var handler = new HttpClientHandler();
            using (var client = new HttpClient(handler))
            {
                client.BaseAddress = new Uri(custcifurl + request.EmailAddress);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                _logger.LogInformation($"Fixed deposit - Calling to fetch customer CIF via : {client.BaseAddress.AbsolutePath + " -- " + client.BaseAddress.AbsoluteUri}");
                //GET Method  
                HttpResponseMessage response = await client.GetAsync(client.BaseAddress);
                if (response.IsSuccessStatusCode)
                {
                    myCustProfileJsonResponse = response.Content.ReadAsStringAsync().Result;
                    _logger.LogInformation($"Fixed deposit - Received customer info : {myCustProfileJsonResponse} from {client.BaseAddress.AbsolutePath + " -- " + client.BaseAddress.AbsoluteUri}");

                    CustProfileResp myCustProfileDeserializedJsonResp = JsonConvert.DeserializeObject<CustProfileResp>(myCustProfileJsonResponse);
                    custCif = myCustProfileDeserializedJsonResp.custprof.customerCif;
                    custBVN = myCustProfileDeserializedJsonResp.custprof.bvn;

                    _logger.LogInformation($"Fixed deposit - Received customer CIF : {custCif}");
                }
                else
                {
                    myCustProfileJsonResponse = response.Content.ReadAsStringAsync().Result;
                    _logger.LogInformation($"Fixed deposit - Received customer CIF : {myCustProfileJsonResponse}");
                    CustProfileResp myCustProfileDeserializedJsonResp = JsonConvert.DeserializeObject<CustProfileResp>(myCustProfileJsonResponse);

                    return new GenericMessage<SavingsCreationResponseModel>()
                    {
                        IsSuccessful = false,
                        Message = myCustProfileDeserializedJsonResp.error
                    };
                }
            }

            //Get Finacle opened account 
            _logger.LogInformation($"Fixed deposit - Attempt reading finacle account opening parameters from appsettings");
            string FIServices_TopupDeposit_Account_Opening_url = _config.GetSection("MySettings").GetSection("FIServices_TopupDeposit_Account_Opening_url").Value;
            _logger.LogInformation($"Fixed deposit -  Finacle Topup deposit account opening url : {FIServices_TopupDeposit_Account_Opening_url}");
            string mysolid = _config.GetSection("MySettings").GetSection("solId").Value;
            _logger.LogInformation($"Fixed deposit -  settings solid : {mysolid}");
            string myintroducerId = _config.GetSection("MySettings").GetSection("introducerId").Value;
            _logger.LogInformation($"Fixed deposit -  settings introducerId : {myintroducerId}");
            string myappid = _config.GetSection("MySettings").GetSection("appId").Value;
            _logger.LogInformation($"Fixed deposit -  settings appId : {myappid}");
            string myaccountManagerId = _config.GetSection("MySettings").GetSection("accountManagerId").Value;
            _logger.LogInformation($"Fixed deposit -  settings account manager Id : {myaccountManagerId}");
            String dateInString = DateTime.Now.ToString("dd-MM-yyyy");

            string myopenEffectiveDate = DateTime.Now.ToString("dd-MM-yyyy");
            _logger.LogInformation($"Fixed deposit -  settings effective date : {myopenEffectiveDate}");

            myFinTopupAcctOpnReq.accountManagerId = myaccountManagerId;
            myFinTopupAcctOpnReq.AppId = myappid;
            myFinTopupAcctOpnReq.customerId = custCif;
           
            if (request.SavingsFrequency == SavingsFrequencyEnum.Fixed) //Fixed = 0, Daily = 1, Weekly = 2, //Monthly = 3
            {

                int fdMonths = 0;
                fdMonths = (int) SavingsCalculation.GetFixedSavingsDaisInMonths(request.HowLong.ToString());
                myFinTopupAcctOpnReq.depositPeriodMonths = Convert.ToString(fdMonths);
                myFinTopupAcctOpnReq.depositPeriodDays = "0";
            }

            _logger.LogInformation($"Fixed deposit - About calling finacle for effective rate for FD");
            string TermDepositRate_url = _config.GetSection("MySettings").GetSection("Term_Deposit_Rate_url").Value;
            string fdrate_response = "";
            string effective_rate = "";

            var fdratehandler = new HttpClientHandler();
            using (var fdrateclient = new HttpClient(fdratehandler))
            {
                fdrateclient.BaseAddress = new Uri(TermDepositRate_url);
                fdrateclient.DefaultRequestHeaders.Accept.Clear();
                fdrateclient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                _logger.LogInformation($"Fixed deposit - Calling to fetch effective rate via : {fdrateclient.BaseAddress.AbsolutePath + " -- " + fdrateclient.BaseAddress.AbsoluteUri}");

                //GET Method  
                HttpResponseMessage response = await fdrateclient.GetAsync(fdrateclient.BaseAddress);
                if (response.IsSuccessStatusCode)
                {
                    fdrate_response = response.Content.ReadAsStringAsync().Result;
                    _logger.LogInformation($"Fixed deposit - Received effective rate response from  + {fdrateclient.BaseAddress.AbsoluteUri} +  :  + { fdrate_response}");

                    TermDepositRate myEffectiveRateDeserializedJsonResp = JsonConvert.DeserializeObject<TermDepositRate>(fdrate_response);
                    effective_rate = myEffectiveRateDeserializedJsonResp.effectiveRate;

                    var finalfdRate = Convert.ToDecimal(retPlan.InterestRate) - Convert.ToDecimal(effective_rate);                    
                    myFinTopupAcctOpnReq.depositRate = Math.Round(finalfdRate, 1).ToString();
                }
                else
                {
                    fdrate_response = response.Content.ReadAsStringAsync().Result;
                    _logger.LogInformation($"Fixed deposit - Received effective rate response from  + {fdrateclient.BaseAddress.AbsoluteUri} +  :  + { fdrate_response}");

                    TermDepositRate myEffectiveRateDeserializedJsonResp = JsonConvert.DeserializeObject<TermDepositRate>(fdrate_response);

                    return new GenericMessage<SavingsCreationResponseModel>()
                    {
                        IsSuccessful = false,
                        Message = myEffectiveRateDeserializedJsonResp.errorResponse
                    };
                }
            }

            myFinTopupAcctOpnReq.depositAmount = Convert.ToString(request.Amount);
            myFinTopupAcctOpnReq.IntroducerId = myintroducerId;
            myFinTopupAcctOpnReq.openEffectiveDate = myopenEffectiveDate;
            myFinTopupAcctOpnReq.solId = mysolid;
            string Fixed_Deposit__url = _config.GetSection("MySettings").GetSection("Fixed_Deposit__url").Value;
            _logger.LogInformation($"Fixed deposit -  Finacle Fixed deposit account opening url : {Fixed_Deposit__url}");            

            string myfdsolid = _config.GetSection("MySettings").GetSection("fdsolId").Value;
            _logger.LogInformation($"Fixed deposit -  settings solid : {myfdsolid}");
            string myfdintroducerId = _config.GetSection("MySettings").GetSection("fdintroducerId").Value;
            _logger.LogInformation($"Fixed deposit -  settings introducerId : {myfdintroducerId}");
            string myfdappid = _config.GetSection("MySettings").GetSection("fdappId").Value;
            _logger.LogInformation($"Fixed deposit -  settings appId : {myfdappid}");
            string myfdaccountManagerId = _config.GetSection("MySettings").GetSection("fdaccountManagerId").Value;
            _logger.LogInformation($"Fixed deposit -  settings account manager Id : {myfdaccountManagerId}");
            String fddateInString = DateTime.Now.ToString("dd-MM-yyyy");
            string myfdglSubHeadCode = _config.GetSection("MySettings").GetSection("fdglSubHeadCode").Value;
            _logger.LogInformation($"Fixed deposit -  settings fd glsubheadcode : {myfdglSubHeadCode}");

            string myfdschemeCode = _config.GetSection("MySettings").GetSection("fdschemeCode").Value;
            _logger.LogInformation($"Fixed deposit -  settings fd schemecode : {myfdschemeCode}");
            string myfdschemeType = _config.GetSection("MySettings").GetSection("fdschemeType").Value;
            _logger.LogInformation($"Fixed deposit -  settings fd schemetype : {myfdschemeType}");
            string myfduniqueKey = _config.GetSection("MySettings").GetSection("fduniqueKey").Value;
            _logger.LogInformation($"Fixed deposit -  settings fd uniquekey : {myfduniqueKey}");

            myFinFDAcctOpnReq.accountPrefinterest = effective_rate;
            myFinFDAcctOpnReq.appId = myfdappid;
            myFinFDAcctOpnReq.autoCloseOnMaturityFlg = request.Autoclose;
            myFinFDAcctOpnReq.autoRenewalFlag = request.Autorenewal;
            myFinFDAcctOpnReq.bvn = custBVN;
            myFinFDAcctOpnReq.cifId = custCif;
            myFinFDAcctOpnReq.debitAccountNumber = request.PaymentInstrumentReference;
            myFinFDAcctOpnReq.depositAmount = Convert.ToString(request.Amount);
            myFinFDAcctOpnReq.depositPeriodDays = Convert.ToString(request.HowLong);
            myFinFDAcctOpnReq.glSubHeadCode = myfdglSubHeadCode;
            myFinFDAcctOpnReq.introducerCode = myfdintroducerId;
            myFinFDAcctOpnReq.repaymentAccount = request.PaymentInstrumentReference;
            myFinFDAcctOpnReq.schemeCode = myfdschemeCode;
            myFinFDAcctOpnReq.schemeType = myfdschemeType;
            myFinFDAcctOpnReq.uniqueKey = myfduniqueKey;

            var json = JsonConvert.SerializeObject(myFinFDAcctOpnReq);
            var requestTime = DateTime.Now;
            string fin_fdacct_opn_respStr = "";
            string retfdAcctOpenCBA_Accountno = "";            

            var handlerAcctOpn = new HttpClientHandler();
            using (var fdacctopnclient = new HttpClient(handlerAcctOpn))
            {
                var data = new StringContent(json, Encoding.UTF8, "application/json");
                requestTime = DateTime.Now;
                fdacctopnclient.Timeout = TimeSpan.FromMinutes(5);
                fdacctopnclient.BaseAddress = new Uri(Fixed_Deposit__url);
                fdacctopnclient.DefaultRequestHeaders.Accept.Clear();
                fdacctopnclient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                _logger.LogInformation($"Fixed deposit - Calling to get Fixed Deposit_Account_Opening json request : {json}");
                _logger.LogInformation($"Fixed deposit - Calling to get Fixed Deposit_Account_Opening via : {fdacctopnclient.BaseAddress.AbsolutePath + " -- " + fdacctopnclient.BaseAddress.AbsoluteUri}");
                HttpResponseMessage httpResponseMsg = await fdacctopnclient.PostAsync(Fixed_Deposit__url, data);

                if (httpResponseMsg.IsSuccessStatusCode)
                {
                    fin_fdacct_opn_respStr = httpResponseMsg.Content.ReadAsStringAsync().Result;
                    _logger.LogInformation($"Fixed deposit - Received FixedDeposit_Account_Opening : {fin_fdacct_opn_respStr}");

                    myFinFDAcctOpnResp = JsonConvert.DeserializeObject<FinacleFDAcctOpeningResponse>(fin_fdacct_opn_respStr);

                    if (myFinFDAcctOpnResp.message.ToUpper().Contains("SUCCESS")) //AccountOpenedSuccessfully
                    {
                        retfdAcctOpenCBA_Accountno = myFinFDAcctOpnResp.accoutNumber; 
                        _logger.LogInformation($"Fixed deposit - CBA account returned to be logged in local DB : {retfdAcctOpenCBA_Accountno}");

                        SavingsRequest savingsRequest = new SavingsRequest()
                        {
                            SavingsFrequencyDebitAmount = request.Amount,
                            InterestRate = retPlan.InterestRate, 
                            SavingsPlanId = request.SavingsPlanId,
                            ProfileId = request.ProfileId,
                            EstimatedReturn = 0,
                            CIF = custCif,
                            SavingsBalance = (((request.Amount > 0)) ? request.Amount : 0),
                            CBSBalance = (((request.Amount > 0)) ? request.Amount : 0),
                            SavingsFrequency = request.SavingsFrequency,
                            SavingsCode = Guid.NewGuid().ToString(),
                            SavingsCoreBankingAccount = myFinFDAcctOpnResp.accoutNumber,

                            NoOfInstallments = request.HowLong,
                            SavingsName = request.SavingsName,
                            StartDate = DateTime.Now,

                            InterestRateCalulation = retPlan.InterestRateCalulation,
                            TimeStamp = DateTime.Now,
                            
                            SavingsStatus = SavingsStatusEnum.Pending_Debit,
                            PaymentInstrumentReference = request.PaymentInstrumentReference,
                            PaymentInstrumentType = request.PaymentInstrumentType,
                            EmailAddress = request.EmailAddress,
                            ContributorEmailAddress = request.EmailAddress,
                        };

                        savingsRequest.ExpectedMaturityDate = SavingsCalculation.CalculateMaturityDate(savingsRequest);
                        savingsRequest.ProjectedInterest = 0;

                        if (_myplantype == SavingsPlanEnum.FIXED_TARGET_SAVINGS)
                        {
                            savingsRequest.SavingsSchedules = new System.Collections.Generic.List<SavingsSchedule>();
                            savingsRequest.SavingsSchedules.Add(new SavingsSchedule()
                            {
                                ScheduleIdentifier = 1,
                                EmailAddress = request.EmailAddress,
                                SavingsCode = savingsRequest.SavingsCode,
                                TransactionReference = "",
                                AmountPaid = 0,
                                ExpectedAmount = request.Amount,
                                ScheduleDate = savingsRequest.StartDate,
                                TimeStamp = DateTime.Now,
                            });
                            await repo.AddAsync(savingsRequest);
                            _uow.SaveChanges();
                        }
                        else if (_myplantype == SavingsPlanEnum.FIXED_DEPOSIT)
                        {
                            await repo.AddAsync(savingsRequest);
                            _uow.SaveChanges();
                        }

                            return new GenericMessage<SavingsCreationResponseModel>()
                        {
                            IsSuccessful = true,
                            Message = "Savings is requested successfuly",
                            Result = new SavingsCreationResponseModel()
                            {
                                ExpectedCompletionDate = savingsRequest.ExpectedMaturityDate,
                                SavingsCode = savingsRequest.SavingsCode
                            },
                            ResponseCode = ErrorCodes.Successful
                        };

                    }
                    else
                    {
                        return new GenericMessage<SavingsCreationResponseModel>()
                        {
                            IsSuccessful = true,
                            Message = myFinTopupAcctOpnResp.description
                        };
                    }

                }
                else
                {
                    fin_fdacct_opn_respStr = httpResponseMsg.Content.ReadAsStringAsync().Result;
                    _logger.LogInformation($"Fixed deposit - Received FixedDeposit_Account_Opening : {fin_fdacct_opn_respStr}");
                    myFinFDAcctOpnResp = JsonConvert.DeserializeObject<FinacleFDAcctOpeningResponse>(fin_fdacct_opn_respStr);
                    return new GenericMessage<SavingsCreationResponseModel>()
                    {
                        IsSuccessful = false,
                        Message = "Unsuccessful http status response for Fixed deposit account opening" // "Exception during Topup deposit account opening"
                    };
                }
            }

        }
    

    }



}
