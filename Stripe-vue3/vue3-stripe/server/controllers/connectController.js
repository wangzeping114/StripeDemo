const stripe = require('stripe')(process.env.STRIPE_SECRET_KEY);

/**
 * 创建Stripe Connect Custom账户
 */
exports.createAccount = async (req, res) => {
  try {
    // 从请求体中获取参数
    const {
      type,
      country,
      capabilities,
      business_type,
      business_profile,
      settings
    } = req.body;

    // 创建Custom账户
    const account = await stripe.accounts.create({
      type: type || 'custom',
      country: country || 'US',
      capabilities: capabilities || {
        card_payments: { requested: true },
        transfers: { requested: true }
      },
      business_type: business_type || 'individual',
      business_profile: business_profile || {},
      settings: settings || {},
      tos_acceptance: {
        date: Math.floor(Date.now() / 1000),
        ip: req.ip // 获取客户端IP
      }
    });

    // 返回账户ID
    res.status(201).json({
      accountId: account.id,
      message: 'Stripe Connect Custom账户创建成功'
    });
  } catch (error) {
    console.error('创建Connect账户错误:', error);
    res.status(400).json({
      error: error.message || '创建Connect账户失败'
    });
  }
};

/**
 * 创建账户链接，用于引导用户完成验证流程
 */
exports.createAccountLink = async (req, res) => {
  try {
    const { account, refresh_url, return_url, type } = req.body;

    // 创建账户链接
    const accountLink = await stripe.accountLinks.create({
      account,
      refresh_url: refresh_url || 'http://localhost:3000/connect/refresh',
      return_url: return_url || 'http://localhost:3000/connect/return',
      type: type || 'account_onboarding',
      collect: 'eventually_due' // 收集所有需要的信息
    });

    // 返回生成的URL
    res.status(200).json({
      url: accountLink.url,
      message: '账户链接创建成功'
    });
  } catch (error) {
    console.error('创建账户链接错误:', error);
    res.status(400).json({
      error: error.message || '创建账户链接失败'
    });
  }
};

/**
 * 获取Connect账户的验证状态
 */
exports.getAccountStatus = async (req, res) => {
  try {
    const { accountId } = req.query;

    // 获取账户详细信息
    const account = await stripe.accounts.retrieve(accountId);

    // 检查账户是否已经完成验证
    // charges_enabled表示账户可以接收付款，payouts_enabled表示账户可以收到打款
    const isCompleted = account.charges_enabled && account.details_submitted;

    // 返回账户状态
    res.status(200).json({
      accountId: account.id,
      isCompleted,
      charges_enabled: account.charges_enabled,
      payouts_enabled: account.payouts_enabled,
      details_submitted: account.details_submitted,
      requirements: account.requirements
    });
  } catch (error) {
    console.error('获取账户状态错误:', error);
    res.status(400).json({
      error: error.message || '获取账户状态失败'
    });
  }
}; 