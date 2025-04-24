using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Stripe;
using stripeapi.Dtos;
using stripeapi.Entity;

namespace stripeapi.Controllers
{
    /// <summary>
    /// 提供Stripe Connect相关的API接口
    /// </summary>
    [Route("api/[controller]")]
    [Authorize] // 添加授权特性，要求用户必须登录
    public class ConnectController : ControllerBase
    {
        private readonly ApplicationDbContext _dbContext;

        public ConnectController(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        
        
        /// <summary>
        /// 创建Stripe Connect Custom账户
        /// </summary>
        /// <param name="request">账户创建请求</param>
        /// <returns>创建的Connect账户信息</returns>
        /// <remarks>
        /// 该API用于为用户创建Stripe Connect Custom账户。
        /// Custom账户由平台完全控制，用户无需直接与Stripe交互。
        /// 创建后还需要通过Account Links或OAuth流程完成账户验证。
        /// </remarks>
        [HttpPost("create-account")]
        public async Task<IActionResult> CreateConnectAccount([FromBody] ConnectAccountCreateRequest request)
        {
            try
            {
                //// 获取当前登录用户
                //string userId = GetCurrentUserId();
                if(!request.UserId.HasValue)
                {
                    throw new ArgumentNullException(nameof(request.UserId), "用户ID不能为空");
                }
                // 检查用户是否已有Connect账户
                var account = await _dbContext.Accounts.FirstOrDefaultAsync(a => a.ID == request.UserId.Value);
                if (account != null && !string.IsNullOrEmpty(account.StripeConnectAccountId))
                {
                    // 用户已有Connect账户，获取账户信息并直接返回
                    var existingService = new AccountService();
                    var existingAccount = await existingService.GetAsync(account.StripeConnectAccountId);
                    
                    // 获取关联的钱包账户
                    var existingProfitAccount = await _dbContext.ProfitAccounts.FirstOrDefaultAsync(a => a.UserId == request.UserId.Value.ToString()); // await _profitAccountService.GetByUserIdAsync(userId);

                    return Ok(new 
                    {
                        accountId = existingAccount.Id,
                        email = existingAccount.Email,
                        country = existingAccount.Country,
                        defaultCurrency = existingAccount.DefaultCurrency,
                        capabilities = existingAccount.Capabilities,
                        chargesEnabled = existingAccount.ChargesEnabled,
                        payoutsEnabled = existingAccount.PayoutsEnabled,
                        requirementsDisabled = existingAccount.Requirements.DisabledReason,
                        requirementsPending = existingAccount.Requirements.PendingVerification,
                        requirementsCurrentlyDue = existingAccount.Requirements.CurrentlyDue,
                        // 如果有钱包账户，也返回钱包信息
                        wallet = existingProfitAccount != null ? new {
                            balance = existingProfitAccount.WalletValue,
                            currency = existingProfitAccount.CurrencyCode
                        } : null,
                        // 添加标记表明这是已存在的账户
                        isExisting = true
                    });
                }
                
                // 使用用户信息构建Stripe Connect账户请求
                var options = new AccountCreateOptions
                {
                    Type = request.Type,
                    Country = request.Country,
                    Email = account.Mail, // 使用用户注册邮箱
                    DefaultCurrency = request.DefaultCurrency,
                    BusinessType = "individual", // 或 "company"，根据实际需要设置
                    
                    BusinessProfile = new AccountBusinessProfileOptions
                    {
                        Name = request.BusinessName,
                        Url = request.BusinessUrl,
                        ProductDescription = "软件开发与技术服务"
                    },
                    
                    Capabilities = new AccountCapabilitiesOptions
                    {
                        Transfers = new AccountCapabilitiesTransfersOptions
                        {
                            Requested = true
                        },
                        CardPayments = new AccountCapabilitiesCardPaymentsOptions
                        {
                            Requested = true
                        }
                    },
                    
                    TosAcceptance = request.TosAcceptance ? new AccountTosAcceptanceOptions
                    {
                        Date = DateTime.UtcNow,
                        Ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1"
                    } : null,
                    
                    Metadata = new Dictionary<string, string>
                    {
                        { "user_id",account.ID.ToString()  }
                    }
                };
                
                if (!string.IsNullOrEmpty(request.BusinessId))
                {
                    options.Metadata.Add("business_id", request.BusinessId);
                }
                
                // 调用Stripe API创建Connect账户
                var service = new AccountService();
                var stripeAccount = await service.CreateAsync(options);
                
                // 更新用户信息，关联Connect账户
                account.StripeConnectAccountId = stripeAccount.Id;
                _dbContext.Accounts.Update(account); // _accountService.UpdateAsync(account);
                
                // 为用户创建ProfitAccount(钱包账户)，如果不存在
                var profitAccount =  await _dbContext.ProfitAccounts.FirstOrDefaultAsync(x=>x.UserId==request.UserId.ToString());
                if (profitAccount == null)
                {
                    profitAccount = new ProfitAccount
                    {
                        UserId = account.ID.ToString(),
                        ConnectAccountId = stripeAccount.Id,
                        WalletValue = 0,
                        CurrencyCode = request.DefaultCurrency.ToLower()
                    };
                      _dbContext.ProfitAccounts.Update(profitAccount);
                }
                else
                {
                    // 如果已存在钱包账户，只更新关联的Connect账户ID
                    profitAccount.ConnectAccountId = stripeAccount.Id;
                    _dbContext.ProfitAccounts.Update(profitAccount);
                }
                
                return Ok(new 
                {
                    accountId = stripeAccount.Id,
                    email = stripeAccount.Email,
                    country = stripeAccount.Country,
                    defaultCurrency = stripeAccount.DefaultCurrency,
                    capabilities = stripeAccount.Capabilities,
                    chargesEnabled = stripeAccount.ChargesEnabled,
                    payoutsEnabled = stripeAccount.PayoutsEnabled,
                    requirementsDisabled = stripeAccount.Requirements.DisabledReason,
                    requirementsPending = stripeAccount.Requirements.PendingVerification,
                    requirementsCurrentlyDue = stripeAccount.Requirements.CurrentlyDue
                });
            }
            catch (StripeException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "内部服务器错误", details = ex.Message });
            }
        }
        
        /// <summary>
        /// 创建账户链接用于完成Connect账户验证
        /// </summary>
        /// <param name="refreshUrl">验证失败后的重定向URL</param>
        /// <param name="returnUrl">验证成功后的重定向URL</param>
        /// <returns>账户链接URL</returns>
        /// <remarks>
        /// 该API创建一个临时URL，引导用户完成身份验证和银行账户添加等流程。
        /// 该链接在创建后15分钟内有效，且只能使用一次。
        /// </remarks>
        [HttpPost("create-account-link")]
        public async Task<IActionResult> CreateAccountLink(
            [FromQuery] string refreshUrl,
            [FromQuery] string returnUrl,
            [FromQuery] Guid? userId)
        {
            try
            {
                // 获取当前登录用户
                if(!userId.HasValue)
                {
                    return BadRequest("用户ID不能为空");
                }

                // 查找用户关联的Connect账户
                var account = await _dbContext.Accounts.FirstOrDefaultAsync(x => x.ID == userId); 
                if (account == null || string.IsNullOrEmpty(account.StripeConnectAccountId))
                {
                    return BadRequest(new { error = "您还未创建Connect账户" });
                }
                
                var options = new AccountLinkCreateOptions
                {
                    Account = account.StripeConnectAccountId,
                    RefreshUrl = refreshUrl,
                    ReturnUrl = returnUrl,
                    Type = "account_onboarding",
                    Collect = "eventually_due"
                };
                
                var service = new AccountLinkService();
                var accountLink = await service.CreateAsync(options);
                
                return Ok(new { url = accountLink.Url });
            }
            catch (StripeException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "内部服务器错误", details = ex.Message });
            }
        }
        
        /// <summary>
        /// 向Connect账户转账(充值)
        /// </summary>
        /// <param name="request">转账请求</param>
        /// <returns>转账结果</returns>
        /// <remarks>
        /// 该API用于从平台账户向用户的Connect账户转账资金。
        /// 充值金额将实时同步至用户的ProfitAccount余额。
        /// </remarks>
        [HttpPost("create-transfer")]
        public async Task<IActionResult> CreateTransfer([FromBody] ConnectTransferRequest request)
        {
            try
            {
                if (!request.UserId.HasValue)
                {
                    return BadRequest("用户ID不能为空");
                }
                // 查找用户关联的Connect账户和钱包账户
                var account = await _dbContext.Accounts.FirstOrDefaultAsync(x => x.ID == request.UserId);
                if (account == null || string.IsNullOrEmpty(account.StripeConnectAccountId))
                {
                    return BadRequest(new { error = "您还未创建Connect账户" });
                }
                
                // 验证请求的目标账户是否为当前用户
                if (request.Destination != account.StripeConnectAccountId)
                {
                    return BadRequest(new { error = "只能向自己的账户充值" });
                }
                
                // 获取用户钱包账户
                var profitAccount = await _dbContext.ProfitAccounts.FirstOrDefaultAsync(x => x.UserId == request.UserId.Value.ToString());
                if (profitAccount == null)
                {
                    return BadRequest(new { error = "未找到您的钱包账户" });
                }
                
                // 调用Stripe API进行转账
                var options = new TransferCreateOptions
                {
                    Amount = request.Amount,
                    Currency = request.Currency,
                    Destination = request.Destination,
                    Description = request.Description ?? "游戏币充值",
                    SourceTransaction = request.SourceTransaction,
                    TransferGroup = request.TransferGroup,
                    Metadata = request.Metadata ?? new Dictionary<string, string> { { "user_id", request.UserId.Value.ToString() } }
                };
                
                var service = new TransferService();
                var transfer = await service.CreateAsync(options);
                
                // 创建充值记录
                var depositRecord = new DepositRecord
                {
                    UserId = request.UserId.Value.ToString(),
                    Amount = transfer.Amount,
                    CurrencyCode = transfer.Currency,
                    Status = DepositStatus.Success,
                    TransactionId = transfer.Id,
                    ProfitAccountId = profitAccount.ID,
                    Remark = transfer.Description,
                    CreateBy = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                };
                await _dbContext.DepositRecords.AddAsync(depositRecord);

                profitAccount.WalletValue+= depositRecord.Amount;
                _dbContext.ProfitAccounts.Update(profitAccount);
                
                return Ok(new
                {
                    transferId = transfer.Id,
                    amount = transfer.Amount,
                    currency = transfer.Currency,
                    destination = transfer.Destination,
                    created = transfer.Created,
                    reversed = transfer.Reversed,
                    sourceType = transfer.SourceType,
                    // 返回更新后的钱包余额
                    balance = profitAccount.WalletValue,
                    depositRecordId = depositRecord.ID
                });
            }
            catch (StripeException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "内部服务器错误", details = ex.Message });
            }
        }
        
        /// <summary>
        /// 从Connect账户提现到银行账户
        /// </summary>
        /// <param name="amount">提现金额（最小货币单位）</param>
        /// <param name="currency">货币类型，默认USD</param>
        /// <param name="destination">目标银行账户ID (可选)</param>
        /// <returns>提现结果</returns>
        /// <remarks>
        /// 该API用于从用户的Connect账户提现到已关联的银行账户。
        /// 提现金额将实时扣减用户的ProfitAccount余额。
        /// </remarks>
        [HttpPost("create-connected-payout")]
        public async Task<IActionResult> CreateConnectedPayout(
            [FromQuery] long amount,
            [FromQuery] string currency = "usd",
            [FromQuery] string? destination = null,
            [FromQuery] Guid? userId=null)
        {
            try
            {
                // 获取当前登录用户 
                if (!userId.HasValue)
                {
                    return BadRequest("用户ID不能为空");
                }
                // 查找用户关联的Connect账户和钱包账户
                var account = await _dbContext.Accounts.FirstOrDefaultAsync(x => x.ID == userId);
                if (account == null || string.IsNullOrEmpty(account.StripeConnectAccountId))
                {
                    return BadRequest(new { error = "您还未创建Connect账户" });
                }

                // 获取用户钱包账户
                var profitAccount = await _dbContext.ProfitAccounts.FirstOrDefaultAsync(x => x.UserId == userId.Value.ToString());
                if (profitAccount == null)
                {
                    return BadRequest(new { error = "未找到您的钱包账户" });
                }

                // 检查余额是否足够
                if (profitAccount.WalletValue < amount)
                {
                    return BadRequest(new { error = $"余额不足，当前余额: {profitAccount.WalletValue / 100.0m} {profitAccount.CurrencyCode.ToUpper()}" });
                }
                
                // 检查银行账户
                if (string.IsNullOrEmpty(destination))
                {
                    // 检查用户是否有默认银行账户
                    //var hasBankAccount = await CheckUserHasBankAccountAsync(account.StripeConnectAccountId);
                    //if (!hasBankAccount)
                    //{
                    //    return BadRequest(new { error = "您还未添加银行账户，请先添加提现银行卡" });
                    //}
                }
                
                // 调用Stripe API创建提现
                var options = new PayoutCreateOptions
                {
                    Amount = amount,
                    Currency = currency,
                    Destination = destination
                };
                
                var requestOptions = new RequestOptions
                {
                    StripeAccount = account.StripeConnectAccountId
                };
                
                var service = new PayoutService();
                var payout = await service.CreateAsync(options, requestOptions);
                
                // 创建提现记录
                var withdrawRecord = new WithdrawRecord
                {
                    UserId = userId.Value.ToString(),
                    Amount = payout.Amount,
                    CurrencyCode = payout.Currency,
                    Status = WithdrawStatus.Success,
                    PayoutId = payout.Id,
                    ProfitAccountId = profitAccount.ID,
                    BankAccountId = payout.Destination?.ToString() ?? "default",
                    CreateTime = DateTime.Now,
                };
                await _dbContext.WithdrawRecords.AddAsync(withdrawRecord);

                profitAccount.WalletValue -= withdrawRecord.Amount;
                _dbContext.ProfitAccounts.Update(profitAccount);
                return Ok(new
                {
                    payoutId = payout.Id,
                    amount = payout.Amount,
                    currency = payout.Currency,
                    arrivalDate = payout.ArrivalDate,
                    status = payout.Status,
                    destination = payout.Destination,
                    // 返回更新后的钱包余额
                    balance = profitAccount.WalletValue,
                    withdrawRecordId = withdrawRecord.ID
                });
            }
            catch (StripeException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "内部服务器错误", details = ex.Message });
            }
        }
        
        // 检查用户是否有银行账户
        //private async Task<bool> CheckUserHasBankAccountAsync(string connectAccountId)
        //{
        //    var externalAccountService = new ExternalAccountService();
        //    var options = new ExternalAccountListOptions
        //    {
        //        Object = "bank_account",
        //        Limit = 1
        //    };
            
        //    var requestOptions = new RequestOptions
        //    {
        //        StripeAccount = connectAccountId
        //    };
            
        //    var accounts = await externalAccountService.ListAsync(options, requestOptions);
        //    return accounts.Data.Count > 0;
        //}
        
        /// <summary>
        /// 处理Connect账户相关的Webhook事件
        /// </summary>
        /// <returns>处理结果</returns>
        /// <remarks>
        /// 该API用于接收和处理Stripe的Webhook事件通知，特别是Connect账户相关的事件。
        /// 事件处理中会同步更新用户钱包余额、充值和提现记录状态。
        /// </remarks>
        [HttpPost("webhook")]
        [AllowAnonymous] // Webhook不需要授权
        public async Task<IActionResult> HandleConnectWebhook()
        {
            try
            {
                var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
                
                // 获取Stripe签名，用于验证请求来源
                string stripeSignature = Request.Headers["Stripe-Signature"];
                
                // 替换为你在Stripe Dashboard中设置的Webhook密钥
                string webhookSecret = "whsec_your_connect_webhook_secret_key";
                
                // 构造Stripe事件
                var stripeEvent = EventUtility.ConstructEvent(
                    json,
                    stripeSignature,
                    webhookSecret
                );
                
                // 处理不同类型的Connect事件
                switch (stripeEvent.Type)
                {
                    case "account.updated":
                        var account = stripeEvent.Data.Object as stripeapi.Entity.Account;
                        await HandleAccountUpdatedAsync(account);
                        break;
                        
                    case "transfer.created":
                        var transfer = stripeEvent.Data.Object as Transfer;
                        await HandleTransferCreatedAsync(transfer);
                        break;
                        
                    case "transfer.paid":
                        var paidTransfer = stripeEvent.Data.Object as Transfer;
                        await HandleTransferPaidAsync(paidTransfer);
                        break;
                        
                    case "transfer.failed":
                        var failedTransfer = stripeEvent.Data.Object as Transfer;
                        await HandleTransferFailedAsync(failedTransfer);
                        break;
                        
                    case "payout.created":
                        var payout = stripeEvent.Data.Object as Payout;
                        await HandlePayoutCreatedAsync(payout);
                        break;
                        
                    case "payout.paid":
                        var paidPayout = stripeEvent.Data.Object as Payout;
                        await HandlePayoutPaidAsync(paidPayout);
                        break;
                        
                    case "payout.failed":
                        var failedPayout = stripeEvent.Data.Object as Payout;
                        await HandlePayoutFailedAsync(failedPayout);
                        break;
                        
                    case "payout.canceled":
                        var canceledPayout = stripeEvent.Data.Object as Payout;
                        await HandlePayoutCanceledAsync(canceledPayout);
                        break;
                }
                
                return Ok(new { received = true });
            }
            catch (StripeException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "内部服务器错误", details = ex.Message });
            }
        }
        
        // 处理账户更新事件
        private async Task HandleAccountUpdatedAsync(stripeapi.Entity.Account account)
        {
            // 更新系统中的Connect账户状态
            var user = await _dbContext.Accounts.FirstOrDefaultAsync(x => x.ID == account.ID);
            if (user != null)
            {
                
            }
        }
        
        // 处理转账创建事件
        private async Task HandleTransferCreatedAsync(Transfer transfer)
        {
            // 查找充值记录
            var depositRecord = await _dbContext.DepositRecords.FirstOrDefaultAsync(x => x.ID == Guid.Parse(transfer.Id)); 
            if (depositRecord != null)
            {
              
            }
        }
        
        // 处理转账成功事件
        private async Task HandleTransferPaidAsync(Transfer transfer)
        {
            // 更新充值记录状态
            var depositRecord = await _dbContext.DepositRecords.FirstOrDefaultAsync(x => x.ID == Guid.Parse(transfer.Id));
            if (depositRecord != null)
            {
                depositRecord.Status = DepositStatus.Success; //"paid";
                depositRecord.CreateTime = DateTime.Now;
                _dbContext.DepositRecords.Update(depositRecord);
            }
        }
        
        // 处理转账失败事件
        private async Task HandleTransferFailedAsync(Transfer transfer)
        {
            // 查找充值记录
            var depositRecord = await _dbContext.DepositRecords.FirstOrDefaultAsync(x => x.ID == Guid.Parse(transfer.Id));
            if (depositRecord != null)
            {
                depositRecord.Status = DepositStatus.Failed;
                depositRecord.CreateTime = DateTime.UtcNow;
                _dbContext.DepositRecords.Update(depositRecord);
           
                // 回滚钱包余额
                var profitAccount = await _dbContext.ProfitAccounts.FirstOrDefaultAsync(x => x.ID == depositRecord.ProfitAccountId);
                if (profitAccount != null)
                {
                    profitAccount.WalletValue -= depositRecord.Amount;
                    _dbContext.ProfitAccounts.Update(profitAccount);
                }
            }
        }
        
        // 处理提现创建事件
        private async Task HandlePayoutCreatedAsync(Payout payout)
        {
            var withdrawRecord = await _dbContext.WithdrawRecords.FirstOrDefaultAsync(x => x.ID == Guid.Parse(payout.Id));
            // 查找提现记录
          
            if (withdrawRecord != null)
            {
                withdrawRecord.Status = WithdrawStatus.Pending;
                withdrawRecord.CreateTime = DateTime.Now;
                _dbContext.WithdrawRecords.Update(withdrawRecord);
            }
        }
        
        // 处理提现成功事件
        private async Task HandlePayoutPaidAsync(Payout payout)
        {
            // 更新提现记录状态
            var withdrawRecord = await _dbContext.WithdrawRecords.FirstOrDefaultAsync(x => x.ID == Guid.Parse(payout.Id));
            if (withdrawRecord != null)
            {
                withdrawRecord.Status = WithdrawStatus.Success;
                withdrawRecord.CompletedAt = DateTime.Now;
                withdrawRecord.UpdateTime = DateTime.Now;
                _dbContext.WithdrawRecords.Update(withdrawRecord);
            }
        }
        
        // 处理提现失败事件
        private async Task HandlePayoutFailedAsync(Payout payout)
        {
            // 查找提现记录
            var withdrawRecord = await _dbContext.WithdrawRecords.FirstOrDefaultAsync(x => x.ID == Guid.Parse(payout.Id));
            if (withdrawRecord != null)
            {
                withdrawRecord.Status = WithdrawStatus.Failed;
                withdrawRecord.ErrorMessage = payout.FailureMessage;
                withdrawRecord.UpdateTime = DateTime.Now;
                _dbContext.WithdrawRecords.Update(withdrawRecord);
                
                // 回滚钱包余额
                var profitAccount = await _dbContext.ProfitAccounts.FirstOrDefaultAsync(x=>x.ID==withdrawRecord.ProfitAccountId);
                if (profitAccount != null)
                {
                    profitAccount.WalletValue -= withdrawRecord.Amount;
                    // 退款到钱包余额
                    _dbContext.ProfitAccounts.Update(profitAccount);
                }
            }
        }
        
        // 处理提现取消事件
        private async Task HandlePayoutCanceledAsync(Payout payout)
        {
            // 查找提现记录
            var withdrawRecord = await _dbContext.WithdrawRecords.FirstOrDefaultAsync(x => x.ID == Guid.Parse(payout.Id));
            if (withdrawRecord != null)
            {
                withdrawRecord.Status = WithdrawStatus.Rejected; // //"canceled";
                withdrawRecord.UpdateTime = DateTime.Now;
                _dbContext.WithdrawRecords.Update(withdrawRecord);
                
                // 回滚钱包余额
                var profitAccount = await _dbContext.ProfitAccounts.FirstOrDefaultAsync(x=>x.ID==withdrawRecord.ProfitAccountId);
                if (profitAccount != null)
                {
                    profitAccount.WalletValue -= withdrawRecord.Amount;
                    // 退款到钱包余额
                    _dbContext.ProfitAccounts.Update(profitAccount);
                }
            }
        }
        
        /// <summary>
        /// 获取Connect账户余额
        /// </summary>
        /// <returns>账户余额信息</returns>
        /// <remarks>
        /// 该API用于查询当前用户Connect账户的可用余额和待结算余额。
        /// 可用余额可以提现，待结算余额需要等待结算周期完成后才可以提现。
        /// </remarks>
        [HttpGet("balance")]
        public async Task<IActionResult> GetConnectBalance(Guid? userid)
        {
            try
            {
                // 获取当前登录用户
                if(!userid.HasValue)
                {
                    return BadRequest("用户ID不能为空");
                }

                // 查找用户关联的Connect账户
                var account = await _dbContext.Accounts.FirstOrDefaultAsync(x => x.ID == userid.Value);
                if (account == null || string.IsNullOrEmpty(account.StripeConnectAccountId))
                {
                    return BadRequest(new { error = "您还未创建Connect账户" });
                }
                
                var requestOptions = new RequestOptions
                {
                    StripeAccount = account.StripeConnectAccountId
                };
                
                var service = new BalanceService();
                var balance = await service.GetAsync(requestOptions);

                // 获取钱包账户余额
                var profitAccount = await _dbContext.ProfitAccounts.FirstOrDefaultAsync(x => x.UserId == userid.Value.ToString()); 
                
                var balanceInfo = new
                {
                    // 可用余额列表(可能包含多种货币)
                    available = balance.Available.Select(b => new
                    {
                        amount = b.Amount,
                        currency = b.Currency,
                        sourceTypes = b.SourceTypes
                    }).ToList(),
                    
                    // 待结算余额列表(可能包含多种货币)
                    pending = balance.Pending.Select(b => new
                    {
                        amount = b.Amount,
                        currency = b.Currency,
                        sourceTypes = b.SourceTypes
                    }).ToList(),
                    
                    // Connect账户ID
                    connectAccountId = account.StripeConnectAccountId,
                    
                    // 钱包余额信息
                    wallet = profitAccount != null ? new {
                        balance = profitAccount.WalletValue,
                        currency = profitAccount.CurrencyCode,
                        updatedAt = profitAccount.UpdateTime
                    } : null
                };

                return Ok(balanceInfo);
            }
            catch (StripeException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "内部服务器错误", details = ex.Message });
            }
        }
        
        /// <summary>
        /// 为Connect账户添加银行账户
        /// </summary>
        /// <param name="accountId">Connect账户ID</param>
        /// <param name="request">银行账户Token</param>
        /// <returns>创建的银行账户信息</returns>
        /// <remarks>
        /// 该API使用预先创建的银行账户Token为Connect账户添加银行账户。
        /// 这种方式更安全，因为敏感的银行卡信息由Stripe直接处理，不经过你的服务器。
        /// </remarks>
        [HttpPost("add-bank-account/{accountId}")]
        public async Task<IActionResult> AddBankAccount(string accountId, [FromBody] TokenRequest request)
        {
            try
            {
                // 验证参数
                if (string.IsNullOrEmpty(accountId))
                {
                    return BadRequest(new { error = "Connect账户ID不能为空" });
                }
                
                if (request == null || string.IsNullOrEmpty(request.TokenId))
                {
                    return BadRequest(new { error = "银行账户Token不能为空" });
                }
                
                // 记录输入参数
                Console.WriteLine($"添加银行账户 - AccountId: {accountId}, TokenId: {request.TokenId}");
                
                try
                {
                    // 正确使用AccountService来添加外部账户
                    var accountService = new AccountService();
                    
                    // 添加外部账户到Connect账户
                    var options = new AccountUpdateOptions
                    {
                        ExternalAccount = request.TokenId
                    };
                    
                    var account = await accountService.UpdateAsync(accountId, options);
                    
                    // 获取最新添加的银行账户
                    var bankAccount = account.ExternalAccounts?.Data
                        .FirstOrDefault(a => a.Object == "bank_account") as BankAccount;
                    
                    if (bankAccount != null)
                    {
                        return Ok(new
                        {
                            bankAccountId = bankAccount.Id,
                            country = bankAccount.Country,
                            currency = bankAccount.Currency,
                            last4 = bankAccount.Last4,
                            bankName = bankAccount.BankName,
                            status = bankAccount.Status,
                            default_for_currency = bankAccount.DefaultForCurrency,
                            connectAccountId = accountId
                        });
                    }
                    else
                    {
                        // 如果找不到银行账户但账户已更新，返回成功信息
                        return Ok(new
                        {
                            success = true,
                            message = "银行账户已添加，但无法获取详细信息",
                            accountId = account.Id
                        });
                    }
                }
                catch (StripeException ex)
                {
                    // 捕获并记录Stripe异常的详细信息
                    Console.WriteLine($"Stripe错误: 代码={ex.StripeError?.Code}, 类型={ex.StripeError?.Type}");
                    Console.WriteLine($"详细信息: {ex.Message}");
                    
                    return BadRequest(new { 
                        error = ex.Message,
                        code = ex.StripeError?.Code,
                        type = ex.StripeError?.Type
                    });
                }
            }
            catch (Exception ex)
            {
                // 记录详细错误信息
                Console.WriteLine($"添加银行账户一般错误: {ex.Message}");
                Console.WriteLine($"堆栈跟踪: {ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"内部错误: {ex.InnerException.Message}");
                }
                
                return StatusCode(500, new { error = "内部服务器错误", details = ex.Message });
            }
        }
        
        /// <summary>
        /// 创建银行账户Token
        /// </summary>
        /// <param name="request">银行账户信息</param>
        /// <returns>创建的银行账户Token</returns>
        /// <remarks>
        /// 该API用于创建银行账户Token，可以用于后续添加银行账户到Connect账户。
        /// 注意：生产环境中应该避免直接通过服务器传输银行账户信息，
        /// 应该使用Stripe.js在前端创建Token，以避免敏感信息通过你的服务器传输。
        /// </remarks>
        [HttpPost("create-bank-account-token")]
        public async Task<IActionResult> CreateBankAccountToken([FromBody] BankAccountTokenRequest request)
        {
            try
            {
                // 创建银行账户参数对象 - 这里使用正确的Stripe对象而不是字典
                var options = new TokenCreateOptions
                {
                    // 使用字符串指定参数
                    BankAccount = new TokenBankAccountOptions
                    {
                        Country = request.Country,
                        Currency = request.Currency,
                        AccountNumber = request.AccountNumber,
                        RoutingNumber = request.RoutingNumber,
                        AccountHolderName = request.AccountHolderName,
                        AccountHolderType = request.AccountHolderType
                    }
                };
                
                var service = new TokenService();
                var token = await service.CreateAsync(options);
                
                // 返回创建的Token
                return Ok(new
                {
                    tokenId = token.Id,
                    bankAccount = new
                    {
                        id = token.BankAccount.Id,
                        country = token.BankAccount.Country,
                        currency = token.BankAccount.Currency,
                        last4 = token.BankAccount.Last4,
                        bankName = token.BankAccount.BankName,
                        status = token.BankAccount.Status
                    }
                });
            }
            catch (StripeException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "内部服务器错误", details = ex.Message });
            }
        }
        
        /// <summary>
        /// 为Connect账户添加银行账户（简化版）
        /// </summary>
        /// <returns>创建的银行账户信息</returns>
        [HttpGet("simple-add-bank")]
        public async Task<IActionResult> AddBankAccountSimple(
            [FromQuery] string accountId,
            [FromQuery] string tokenId)
        {
            try
            {
                if (string.IsNullOrEmpty(accountId) || string.IsNullOrEmpty(tokenId))
                {
                    return BadRequest(new { error = "账户ID和Token ID不能为空" });
                }
                
                var service = new AccountService();
                
                // 不使用AddExtraParam方法，而是直接设置ExternalAccount属性
                var options = new AccountUpdateOptions
                {
                    ExternalAccount = tokenId
                };
                
                var account = await service.UpdateAsync(accountId, options);
                
                return Ok(new { 
                    success = true, 
                    accountId = account.Id,
                    externalAccountsCount = account.ExternalAccounts?.Data?.Count ?? 0
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }
} 