using Microsoft.AspNetCore.Mvc;
using Stripe;
using stripeapi.Dtos;
using System.IO;
using System.Threading.Tasks;
using System.Linq;

namespace stripeapi.Controllers
{
    /// <summary>
    /// 提供Stripe Connect相关的API接口
    /// </summary>
    [Route("api/[controller]")]
    public class ConnectController : ControllerBase
    {
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
                var options = new AccountCreateOptions
                {
                    Type = request.Type,
                    Country = request.Country,
                    Email = request.Email,
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
                        // 直接使用DateTime类型
                        Date = DateTime.UtcNow,
                        Ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1"
                    } : null,
                    
                    Metadata = request.Metadata
                };
                
                if (!string.IsNullOrEmpty(request.BusinessId))
                {
                    options.Metadata = options.Metadata ?? new Dictionary<string, string>();
                    options.Metadata.Add("business_id", request.BusinessId);
                }
                
                var service = new AccountService();
                var account = await service.CreateAsync(options);
                
                return Ok(new 
                {
                    accountId = account.Id,
                    email = account.Email,
                    country = account.Country,
                    defaultCurrency = account.DefaultCurrency,
                    capabilities = account.Capabilities,
                    chargesEnabled = account.ChargesEnabled,
                    payoutsEnabled = account.PayoutsEnabled,
                    requirementsDisabled = account.Requirements.DisabledReason,
                    requirementsPending = account.Requirements.PendingVerification,
                    requirementsCurrentlyDue = account.Requirements.CurrentlyDue
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
        /// <param name="accountId">Connect账户ID</param>
        /// <param name="refreshUrl">验证失败后的重定向URL</param>
        /// <param name="returnUrl">验证成功后的重定向URL</param>
        /// <returns>账户链接URL</returns>
        /// <remarks>
        /// 该API创建一个临时URL，引导用户完成身份验证和银行账户添加等流程。
        /// 该链接在创建后15分钟内有效，且只能使用一次。
        /// </remarks>
        [HttpPost("create-account-link")]
        public async Task<IActionResult> CreateAccountLink(
            [FromQuery] string accountId,
            [FromQuery] string refreshUrl,
            [FromQuery] string returnUrl)
        {
            try
            {
                if (string.IsNullOrEmpty(accountId))
                {
                    return BadRequest(new { error = "Connect账户ID不能为空" });
                }
                
                var options = new AccountLinkCreateOptions
                {
                    Account = accountId,
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
        /// 向Connect账户转账
        /// </summary>
        /// <param name="request">转账请求</param>
        /// <returns>转账结果</returns>
        /// <remarks>
        /// 该API用于从平台账户向Connect账户转账资金。
        /// 通常用于向商家、创作者等支付佣金或结算款项。
        /// 转账金额将从平台Stripe账户余额中扣除。
        /// </remarks>
        [HttpPost("create-transfer")]
        public async Task<IActionResult> CreateTransfer([FromBody] ConnectTransferRequest request)
        {
            try
            {
                var options = new TransferCreateOptions
                {
                    Amount = request.Amount,
                    Currency = request.Currency,
                    Destination = request.Destination,
                    Description = request.Description,
                    SourceTransaction = request.SourceTransaction,
                    TransferGroup = request.TransferGroup,
                    Metadata = request.Metadata
                };
                
                if (request.ApplicationFeeAmount.HasValue)
                {
                    // 这里需要在定义ConnectTransferRequest时添加ApplicationFeeAmount字段
                    // 或者通过其他方式在平台内部处理费用
                }
                
                var service = new TransferService();
                var transfer = await service.CreateAsync(options);
                
                return Ok(new
                {
                    transferId = transfer.Id,
                    amount = transfer.Amount,
                    currency = transfer.Currency,
                    destination = transfer.Destination,
                    created = transfer.Created,
                    reversed = transfer.Reversed,
                    sourceType = transfer.SourceType
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
        /// <param name="accountId">Connect账户ID</param>
        /// <param name="amount">提现金额（最小货币单位）</param>
        /// <param name="currency">货币类型，默认USD</param>
        /// <param name="destination">目标银行账户ID (可选)</param>
        /// <returns>提现结果</returns>
        /// <remarks>
        /// 该API用于从Connect账户提现到已关联的银行账户。
        /// 提现处理时间取决于银行和国家/地区，通常为1-7个工作日。
        /// 需要先为Connect账户添加银行账户才能使用此功能。
        /// </remarks>
        [HttpPost("create-connected-payout")]
        public async Task<IActionResult> CreateConnectedPayout(
            [FromQuery] string accountId,
            [FromQuery] long amount,
            [FromQuery] string currency = "usd",
            [FromQuery] string? destination = null)
        {
            try
            {
                if (string.IsNullOrEmpty(accountId))
                {
                    return BadRequest(new { error = "Connect账户ID不能为空" });
                }
                
                var options = new PayoutCreateOptions
                {
                    Amount = amount,
                    Currency = currency,
                    Destination = destination
                };
                
                var requestOptions = new RequestOptions
                {
                    StripeAccount = accountId // 指定为哪个Connect账户创建提现
                };
                
                var service = new PayoutService();
                var payout = await service.CreateAsync(options, requestOptions);
                
                return Ok(new
                {
                    payoutId = payout.Id,
                    amount = payout.Amount,
                    currency = payout.Currency,
                    arrivalDate = payout.ArrivalDate,
                    status = payout.Status,
                    destination = payout.Destination,
                    connectAccountId = accountId
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
        /// 获取Connect账户余额
        /// </summary>
        /// <param name="accountId">Connect账户ID</param>
        /// <returns>账户余额信息</returns>
        /// <remarks>
        /// 该API用于查询Connect账户的可用余额和待结算余额。
        /// 可用余额可以提现，待结算余额需要等待结算周期完成后才可以提现。
        /// </remarks>
        [HttpGet("balance/{accountId}")]
        public async Task<IActionResult> GetConnectBalance(string accountId)
        {
            try
            {
                if (string.IsNullOrEmpty(accountId))
                {
                    return BadRequest(new { error = "Connect账户ID不能为空" });
                }
                
                var requestOptions = new RequestOptions
                {
                    StripeAccount = accountId
                };
                
                var service = new BalanceService();
                var balance = await service.GetAsync(requestOptions);
                var balanceInfo = new
                {

                    available = balance.Available.Select(b => new
                    {
                        amount = b.Amount,
                        currency = b.Currency,
                        sourceTypes = b.SourceTypes
                    }),
                    pending = balance.Pending.Select(b => new
                    {
                        amount = b.Amount,
                        currency = b.Currency,
                        sourceTypes = b.SourceTypes
                    }),
                    connectAccountId = accountId
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
        /// 处理Connect账户相关的Webhook事件
        /// </summary>
        /// <returns>处理结果</returns>
        /// <remarks>
        /// 该API用于接收和处理Stripe的Webhook事件通知，特别是Connect账户相关的事件。
        /// 需要在Stripe后台配置Webhook，URL指向该接口，并选择connect相关事件类型。
        /// </remarks>
        [HttpPost("webhook")]
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
                        var account = stripeEvent.Data.Object as Account;
                        Console.WriteLine($"Connect账户已更新: {account.Id}, 支付已启用: {account.ChargesEnabled}, 提现已启用: {account.PayoutsEnabled}");
                        // 处理账户更新逻辑
                        // await _connectAccountRepository.UpdateStatusAsync(account.Id, account.ChargesEnabled, account.PayoutsEnabled);
                        break;
                        
                    case "account.application.deauthorized":
                        var deauthorizedAccount = stripeEvent.Data.Object as Account;
                        Console.WriteLine($"Connect账户已解除授权: {deauthorizedAccount.Id}");
                        // 处理账户解除授权逻辑
                        // await _connectAccountRepository.DeactivateAsync(deauthorizedAccount.Id);
                        break;
                        
                    case "account.external_account.created":
                        var externalAccountObj = stripeEvent.Data.Object;
                        // 根据外部账户类型处理（可能是银行账户或卡）
                        if (externalAccountObj is BankAccount bankAccount)
                        {
                            Console.WriteLine($"Connect账户添加了新的银行账户: {bankAccount.Id}");
                            // 处理银行账户创建逻辑
                        }
                        else if (externalAccountObj is Card card)
                        {
                            Console.WriteLine($"Connect账户添加了新的卡: {card.Id}");
                            // 处理卡创建逻辑
                        }
                        break;
                        
                    case "transfer.created":
                        var transfer = stripeEvent.Data.Object as Transfer;
                        Console.WriteLine($"转账已创建: {transfer.Id}, 金额: {transfer.Amount / 100.0m} {transfer.Currency}");
                        // 处理转账创建逻辑
                        break;
                        
                    case "transfer.paid":
                        var paidTransfer = stripeEvent.Data.Object as Transfer;
                        Console.WriteLine($"转账已完成: {paidTransfer.Id}");
                        // 处理转账完成逻辑
                        break;
                        
                    case "transfer.failed":
                        var failedTransfer = stripeEvent.Data.Object as Transfer;
                        Console.WriteLine($"转账失败: {failedTransfer.Id}");
                        // 处理转账失败逻辑
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