<script setup>
import { ref, onMounted, watch } from 'vue';
import { useRouter } from 'vue-router';

const router = useRouter();
const isLoading = ref(false);
const paymentAmount = ref(1999); // 金额，单位为分（19.99元）
const currency = ref('usd'); // 币种
const description = ref('商品购买'); // 描述
const connectAccountId = ref('');
const accountBalance = ref(null);
const accountVerified = ref(false); // 账户是否已验证
const bankAccounts = ref([]); // 银行账户列表
const errorMessage = ref('');
const successMessage = ref('');
const baseApiUrl = 'http://localhost:5062';

// 银行账户信息
const bankAccountInfo = ref({
  accountNumber: '',
  routingNumber: '',
  accountHolderName: '',
  accountHolderType: 'individual',
  country: 'US',
  currency: 'usd'
});

// 已有的银行账户ID
const existingBankAccountId = ref('ba_1RGKLiQ6ABK1O6QPkR3DGSX2');
const useExistingBankAccount = ref(false);

// 自动同步银行账户货币与选择的货币
watch(currency, (newCurrency) => {
  bankAccountInfo.value.currency = newCurrency;
});

// 当组件挂载时，检查是否已有Connect账户ID
onMounted(async () => {
  try {
    // 使用已知的Connect账户ID
    const savedAccountId = localStorage.getItem('stripe_connect_account_id') || 'acct_1RGKLhQ6ABK1O6QP';
    
    if (savedAccountId) {
      connectAccountId.value = savedAccountId;
      // 保存到本地存储
      localStorage.setItem('stripe_connect_account_id', savedAccountId);
      
      // 检查账户验证状态
      await checkAccountStatus();
      // 获取账户余额
      await checkConnectBalance();
      // 获取已添加的银行账户
      await loadBankAccounts();
    }
  } catch (error) {
    console.error('初始化Connect账户检查失败:', error);
    errorMessage.value = '无法检查Connect账户状态，请刷新页面重试';
  }
});

// 加载已添加的银行账户列表
const loadBankAccounts = async () => {
  if (!connectAccountId.value) return;
  
  try {
    // 这里假设有获取银行账户的API
    // 如果没有实际的API，我们可以使用本地存储的数据
    const currencies = ['usd', 'eur', 'gbp'];
    const savedAccounts = [];
    
    for (const curr of currencies) {
      const hasAccount = localStorage.getItem(`has_added_bank_account_${connectAccountId.value}_${curr}`);
      if (hasAccount === 'true') {
        savedAccounts.push({
          currency: curr,
          status: 'active'
        });
      }
    }
    
    bankAccounts.value = savedAccounts;
    console.log('已加载银行账户列表:', bankAccounts.value);
  } catch (error) {
    console.error('加载银行账户失败:', error);
  }
};

// 检查账户验证状态
const checkAccountStatus = async () => {
  if (!connectAccountId.value) return;
  
  try {
    // 尝试获取余额作为验证状态检查
    const response = await fetch(`${baseApiUrl}/api/Connect/balance/${connectAccountId.value}`, {
      method: 'GET',
      headers: {
        'accept': '*/*'
      }
    });
    
    // 如果可以成功获取余额，说明账户可能已验证
    if (response.ok) {
      accountVerified.value = true;
      console.log('账户验证状态: 已验证');
    } else {
      const errorText = await response.text();
      
      // 根据错误消息判断账户状态
      if (errorText.includes('activate your account') || 
          errorText.includes('onboarding') || 
          errorText.includes('verification')) {
        accountVerified.value = false;
        console.log('账户验证状态: 未验证');
      }
    }
  } catch (error) {
    console.error('检查账户验证状态失败:', error);
    accountVerified.value = false;
  }
};

// 跳转到Connect账户验证页面
const goToAccountVerification = () => {
  router.push('/connect');
};

// 检查Connect账户余额
const checkConnectBalance = async () => {
  if (!connectAccountId.value) {
    errorMessage.value = '请先完成商户注册';
    return;
  }

  isLoading.value = true;
  errorMessage.value = '';
  successMessage.value = '';
  
  try {
    // 使用实际的API接口 api/Connect/balance/{accountId}
    const response = await fetch(`${baseApiUrl}/api/Connect/balance/${connectAccountId.value}`, {
      method: 'GET',
      headers: {
        'accept': '*/*',
        'Content-Type': 'application/json'
      }
    });

    if (!response.ok) {
      const errorText = await response.text();
      
      // 如果是账户验证问题
      if (errorText.includes('activate your account') || 
          errorText.includes('onboarding') || 
          errorText.includes('verification')) {
        accountVerified.value = false;
        throw new Error('您的账户尚未完成验证，请先完成账户验证流程');
      }
      
      throw new Error('查询账户余额失败');
    }

    const data = await response.json();
    accountBalance.value = data;
    accountVerified.value = true; // 能查询余额，说明账户已验证
    
    // 显示余额信息
    if (data.available && data.available.length > 0) {
      successMessage.value = `可用余额: ${(data.available[0].amount / 100).toFixed(2)} ${data.available[0].currency.toUpperCase()}`;
    } else {
      successMessage.value = '当前没有可用余额';
    }
  } catch (error) {
    console.error('查询账户余额错误:', error);
    errorMessage.value = error.message || '查询账户余额失败，请重试';
  } finally {
    isLoading.value = false;
  }
};

// 创建账户转账 - 使用Connect API
const createAccountTransfer = async () => {
  if (!connectAccountId.value) {
    errorMessage.value = '请先完成商户注册';
    return;
  }
  
  // 验证账户状态
  if (!accountVerified.value) {
    if (confirm('您的账户尚未完成验证，无法创建转账。是否现在前往完成验证流程？')) {
      goToAccountVerification();
    }
    return;
  }

  isLoading.value = true;
  errorMessage.value = '';
  successMessage.value = '';
  
  try {
    console.log('发起转账请求...');
    
    // 使用实际的API接口 api/Connect/create-transfer
    const response = await fetch(`${baseApiUrl}/api/Connect/create-transfer`, {
      method: 'POST',
      headers: {
        'accept': '*/*',
        'Content-Type': 'application/json'
      },
      body: JSON.stringify({
        amount: paymentAmount.value,
        currency: currency.value,
        destination: connectAccountId.value,
        description: description.value || '平台转账'
      })
    });

    if (!response.ok) {
      const errorText = await response.text();
      console.error('创建账户转账失败:', errorText);
      
      // 如果是账户验证问题
      if (errorText.includes('activate your account') || 
          errorText.includes('onboarding') || 
          errorText.includes('verification')) {
        accountVerified.value = false;
        throw new Error('您的账户尚未完成验证，请先完成账户验证流程');
      }
      
      throw new Error('创建账户转账失败: ' + errorText);
    }

    const data = await response.json();
    console.log('转账成功:', data);
    successMessage.value = `转账成功！转账ID: ${data.transferId}`;
    
    // 转账成功后更新余额
    await checkConnectBalance();
  } catch (error) {
    console.error('创建账户转账错误:', error);
    errorMessage.value = error.message || '创建账户转账失败，请重试';
    
    // 如果错误原因是账户未验证，提示用户进行验证
    if (error.message && (
        error.message.includes('activate your account') || 
        error.message.includes('verification') || 
        error.message.includes('onboarding'))) {
      if (confirm('您的账户需要验证才能进行转账。是否现在前往完成验证流程？')) {
        goToAccountVerification();
      }
    }
  } finally {
    isLoading.value = false;
  }
};

// 检查是否已为当前货币添加了银行账户
const hasAddedBankAccountForCurrency = () => {
  // 先检查银行账户列表
  if (bankAccounts.value && bankAccounts.value.length > 0) {
    return bankAccounts.value.some(account => account.currency === currency.value);
  }
  
  // 如果没有银行账户数据，回退到本地存储检查
  return localStorage.getItem(`has_added_bank_account_${connectAccountId.value}_${currency.value}`) === 'true';
};

// 创建Connect账户提现到银行账户
const createConnectedPayout = async () => {
  if (!connectAccountId.value) {
    errorMessage.value = '请先完成商户注册';
    return;
  }
  
  // 验证账户状态
  if (!accountVerified.value) {
    if (confirm('您的账户尚未完成验证，无法提现。是否现在前往完成验证流程？')) {
      goToAccountVerification();
    }
    return;
  }
  
  // 检查是否已为当前货币添加了银行账户
  if (!hasAddedBankAccountForCurrency()) {
    if (!confirm(`您尚未为 ${currency.value.toUpperCase()} 货币添加银行账户，提现很可能会失败。建议先添加银行账户再提现。是否继续？`)) {
      return;
    }
  }

  isLoading.value = true;
  errorMessage.value = '';
  successMessage.value = '';
  
  try {
    // 使用实际的API接口 api/Connect/create-connected-payout 
    // 使用查询参数而不是JSON请求体，并且不指定destination参数
    const queryParams = new URLSearchParams({
      accountId: connectAccountId.value,
      amount: paymentAmount.value.toString(),
      currency: currency.value
      // 移除destination参数，让Stripe API使用账户默认的银行账户
    }).toString();
    
    console.log('发起提现请求：', `${baseApiUrl}/api/Connect/create-connected-payout?${queryParams}`);
    
    const response = await fetch(`${baseApiUrl}/api/Connect/create-connected-payout?${queryParams}`, {
      method: 'POST',
      headers: {
        'accept': '*/*'
      }
      // 不需要请求体
    });

    if (!response.ok) {
      const errorText = await response.text();
      console.error('创建提现失败:', errorText);
      
      // 检查错误类型
      if (errorText.includes('activate your account') || 
          errorText.includes('onboarding') || 
          errorText.includes('verification')) {
        accountVerified.value = false;
        throw new Error('您的账户尚未完成验证，请先完成账户验证流程');
      }
      
      if (errorText.includes('No external account') || 
          errorText.includes('No such external account') || 
          errorText.includes('you don\'t have any external accounts in that currency')) {
        throw new Error(`提现失败：您尚未添加${currency.value.toUpperCase()}货币的银行账户，请先添加对应货币的银行账户`);
      }
      
      throw new Error(`创建提现失败: ${errorText}`);
    }

    const data = await response.json();
    console.log('提现成功:', data);
    successMessage.value = `提现申请成功！提现ID: ${data.payoutId || '已处理'}`;
    
    // 提现成功后更新余额
    await checkConnectBalance();
  } catch (error) {
    console.error('创建提现错误:', error);
    errorMessage.value = error.message || '创建提现失败，请重试';
    
    // 如果错误原因是账户未验证，提示用户进行验证
    if (error.message && (
        error.message.includes('activate your account') || 
        error.message.includes('verification') || 
        error.message.includes('onboarding'))) {
      if (confirm('您的账户需要验证才能进行提现。是否现在前往完成验证流程？')) {
        goToAccountVerification();
      }
    }
  } finally {
    isLoading.value = false;
  }
};

// 添加银行账户
const addBankAccount = async () => {
  if (!connectAccountId.value) {
    errorMessage.value = '请先完成商户注册';
    return;
  }
  
  // 如果选择使用已有的银行账户ID
  if (useExistingBankAccount.value) {
    if (!existingBankAccountId.value) {
      errorMessage.value = '请输入有效的银行账户ID';
      return;
    }
    
    isLoading.value = true;
    errorMessage.value = '';
    successMessage.value = '';
    
    try {
      console.log(`准备关联已有银行账户: ${existingBankAccountId.value}`);
      
      // 发送已有的银行账户ID作为tokenId参数
      const bankResponse = await fetch(`${baseApiUrl}/api/Connect/add-bank-account/${connectAccountId.value}`, {
        method: 'POST',
        headers: {
          'accept': '*/*',
          'Content-Type': 'application/json'
        },
        body: JSON.stringify({
          tokenId: existingBankAccountId.value
        })
      });

      if (!bankResponse.ok) {
        let errorInfo = '';
        try {
          const errorData = await bankResponse.json();
          console.error('添加银行账户失败 (JSON):', errorData);
          errorInfo = JSON.stringify(errorData);
        } catch (e) {
          const errorText = await bankResponse.text();
          console.error('添加银行账户失败 (Text):', errorText);
          errorInfo = errorText;
        }
        throw new Error(`添加银行账户失败: ${errorInfo}`);
      }

      const bankData = await bankResponse.json().catch(e => {
        console.warn('解析bankResponse响应失败:', e);
        return { message: '操作成功但无法解析响应' };
      });
      
      console.log('成功添加银行账户:', bankData);
      successMessage.value = `成功关联已有银行账户`;
      
      // 添加银行账户成功后，保存标记到本地存储
      localStorage.setItem(`has_added_bank_account_${connectAccountId.value}_${currency.value}`, 'true');
      
      // 更新银行账户列表
      bankAccounts.value.push({
        currency: currency.value,
        status: 'active',
        id: existingBankAccountId.value
      });
    } catch (error) {
      console.error('添加银行账户错误:', error);
      errorMessage.value = error.message || '添加银行账户失败，请重试';
    } finally {
      isLoading.value = false;
    }
    
    return;
  }
  
  // 正常添加新银行账户的流程
  if (!bankAccountInfo.value.accountNumber || !bankAccountInfo.value.routingNumber || !bankAccountInfo.value.accountHolderName) {
    errorMessage.value = '请填写完整银行账户信息';
    return;
  }
  
  // 确保银行账户货币与当前选择的货币一致
  bankAccountInfo.value.currency = currency.value;

  // 确保账号格式正确（只包含数字，无空格）
  bankAccountInfo.value.accountNumber = bankAccountInfo.value.accountNumber.replace(/\D/g, '');
  bankAccountInfo.value.routingNumber = bankAccountInfo.value.routingNumber.replace(/\D/g, '');

  isLoading.value = true;
  errorMessage.value = '';
  successMessage.value = '';
  
  try {
    console.log(`准备添加${bankAccountInfo.value.currency.toUpperCase()}货币的银行账户...`);

    // 步骤1: 创建银行账户Token
    console.log('步骤1: 创建银行账户Token');
    const tokenResponse = await fetch(`${baseApiUrl}/api/Connect/create-bank-account-token`, {
      method: 'POST',
      headers: {
        'accept': '*/*',
        'Content-Type': 'application/json'
      },
      body: JSON.stringify({
        accountNumber: bankAccountInfo.value.accountNumber,
        routingNumber: bankAccountInfo.value.routingNumber,
        accountHolderName: bankAccountInfo.value.accountHolderName,
        accountHolderType: bankAccountInfo.value.accountHolderType,
        country: bankAccountInfo.value.country,
        currency: bankAccountInfo.value.currency
      })
    });

    if (!tokenResponse.ok) {
      let errorInfo = '';
      try {
        const errorData = await tokenResponse.json();
        console.error('创建银行账户Token失败 (JSON):', errorData);
        errorInfo = JSON.stringify(errorData);
      } catch (e) {
        const errorText = await tokenResponse.text();
        console.error('创建银行账户Token失败 (Text):', errorText);
        errorInfo = errorText;
      }
      throw new Error(`创建银行账户Token失败: ${errorInfo}`);
    }

    const tokenData = await tokenResponse.json().catch(e => {
      console.warn('解析token响应失败:', e);
      throw new Error('创建银行账户Token失败: 响应格式错误');
    });

    console.log('成功创建银行账户Token:', tokenData);

    if (!tokenData.tokenId) {
      throw new Error('创建银行账户Token失败: 未返回有效的Token ID');
    }

    // 步骤2: 使用Token添加银行账户
    console.log('步骤2: 使用Token添加银行账户');
    const bankResponse = await fetch(`${baseApiUrl}/api/Connect/add-bank-account/${connectAccountId.value}`, {
      method: 'POST',
      headers: {
        'accept': '*/*',
        'Content-Type': 'application/json'
      },
      body: JSON.stringify({
        tokenId: tokenData.tokenId
      })
    });

    if (!bankResponse.ok) {
      let errorInfo = '';
      try {
        const errorData = await bankResponse.json();
        console.error('添加银行账户失败 (JSON):', errorData);
        errorInfo = JSON.stringify(errorData);
      } catch (e) {
        const errorText = await bankResponse.text();
        console.error('添加银行账户失败 (Text):', errorText);
        errorInfo = errorText;
      }
      throw new Error(`添加银行账户失败: ${errorInfo}`);
    }

    const bankData = await bankResponse.json().catch(e => {
      console.warn('解析bankResponse响应失败:', e);
      return { message: '操作成功但无法解析响应' };
    });
    
    console.log('成功添加银行账户:', bankData);
    successMessage.value = `成功添加${currency.value.toUpperCase()}货币的银行账户`;
    
    // 添加银行账户成功后，保存标记到本地存储
    localStorage.setItem(`has_added_bank_account_${connectAccountId.value}_${currency.value}`, 'true');
    
    // 更新银行账户列表
    bankAccounts.value.push({
      currency: currency.value,
      status: 'active'
    });
    
    // 清空表单
    bankAccountInfo.value.accountNumber = '';
    bankAccountInfo.value.routingNumber = '';
    bankAccountInfo.value.accountHolderName = '';
  } catch (error) {
    console.error('添加银行账户错误:', error);
    errorMessage.value = error.message || '添加银行账户失败，请重试';
  } finally {
    isLoading.value = false;
  }
};

// 是否显示银行账户表单
const showBankForm = ref(false);
const toggleBankForm = () => {
  showBankForm.value = !showBankForm.value;
};
</script>

<template>
  <div class="connect-payment">
    <h1>Stripe Connect 资金管理</h1>
    
    <div v-if="!connectAccountId" class="warning-message">
      您尚未注册为平台商户，请先完成<router-link to="/connect">商户注册</router-link>。
    </div>
    
    <div v-else-if="!accountVerified" class="warning-message account-verification">
      <p><strong>您的账户尚未完成验证!</strong> 在使用转账、提现等功能前，需要完成Stripe账户验证流程。</p>
      <button @click="goToAccountVerification" class="verification-button">
        立即完成验证
      </button>
    </div>
    
    <div v-else class="payment-form">
      <div v-if="errorMessage" class="error-message">
        {{ errorMessage }}
      </div>
      
      <div v-if="successMessage" class="success-message">
        {{ successMessage }}
      </div>
      
      <div class="form-group">
        <label for="amount">金额</label>
        <input 
          id="amount" 
          v-model="paymentAmount" 
          type="number" 
          min="50" 
          step="1" 
          :disabled="isLoading"
        />
        <span class="amount-display">{{ (paymentAmount / 100).toFixed(2) }} {{ currency.toUpperCase() }}</span>
      </div>
      
      <div class="form-group">
        <label for="description">描述</label>
        <input 
          id="description" 
          v-model="description" 
          type="text" 
          :disabled="isLoading"
        />
      </div>
      
      <div class="form-group">
        <label for="currency">货币</label>
        <select 
          id="currency" 
          v-model="currency" 
          :disabled="isLoading"
        >
          <option value="usd">美元 (USD)</option>
          <option value="eur">欧元 (EUR)</option>
          <option value="gbp">英镑 (GBP)</option>
        </select>
        <span class="currency-note">
          注意：在提现前，请确保您已添加了对应货币的银行账户
        </span>
      </div>
      
      <div class="payment-actions">
        <button 
          @click="checkConnectBalance" 
          :disabled="isLoading" 
          class="action-button"
        >
          {{ isLoading ? '查询中...' : '查询账户余额' }}
        </button>
        
        <button 
          @click="createAccountTransfer" 
          :disabled="isLoading || !accountVerified" 
          class="action-button"
          :class="{'disabled': !accountVerified}"
          :title="!accountVerified ? '请先完成账户验证' : ''"
        >
          {{ isLoading ? '处理中...' : '向账户转账' }}
        </button>
        
        <button 
          @click="createConnectedPayout" 
          :disabled="isLoading || !accountVerified" 
          class="action-button secondary"
          :class="{'disabled': !accountVerified}"
          :title="!accountVerified ? '请先完成账户验证' : ''"
        >
          {{ isLoading ? '处理中...' : '提现到银行账户' }}
        </button>
        
        <button 
          @click="toggleBankForm" 
          class="action-button secondary"
        >
          {{ showBankForm ? '隐藏银行表单' : '添加银行账户' }}
        </button>
      </div>
      
      <div v-if="showBankForm" class="bank-account-form">
        <h3>添加银行账户 ({{ currency.toUpperCase() }})</h3>
        
        <div class="bank-account-toggle">
          <button 
            @click="useExistingBankAccount = false" 
            class="toggle-button" 
            :class="{ active: !useExistingBankAccount }"
          >
            添加新银行账户
          </button>
          <button 
            @click="useExistingBankAccount = true" 
            class="toggle-button" 
            :class="{ active: useExistingBankAccount }"
          >
            使用已有银行账户
          </button>
        </div>
        
        <!-- 使用已有银行账户 -->
        <div v-if="useExistingBankAccount" class="existing-account-form">
          <p class="bank-currency-info">
            使用已有的Stripe银行账户ID直接添加银行账户
          </p>
          
          <div class="form-group">
            <label for="existingBankAccountId">银行账户ID</label>
            <input 
              id="existingBankAccountId" 
              v-model="existingBankAccountId" 
              type="text" 
              placeholder="例如: ba_1RGKLiQ6ABK1O6QPkR3DGSX2" 
              :disabled="isLoading"
            />
            <span class="input-hint">您提供的ID: {{ existingBankAccountId }}</span>
          </div>
          
          <button 
            @click="addBankAccount" 
            :disabled="isLoading" 
            class="action-button"
          >
            {{ isLoading ? '处理中...' : '使用已有银行账户' }}
          </button>
        </div>
        
        <!-- 添加新银行账户 -->
        <div v-else>
          <p class="bank-currency-info">
            请注意：您正在添加<strong>{{ currency.toUpperCase() }}</strong>货币的银行账户，如需添加其他货币的银行账户，请先切换上方的货币选项。
          </p>
          
          <div class="form-group">
            <label for="accountNumber">账号</label>
            <input 
              id="accountNumber" 
              v-model="bankAccountInfo.accountNumber" 
              type="text" 
              placeholder="在测试模式下使用 000123456789" 
              :disabled="isLoading"
            />
          </div>
          
          <div class="form-group">
            <label for="routingNumber">路由号码</label>
            <input 
              id="routingNumber" 
              v-model="bankAccountInfo.routingNumber" 
              type="text" 
              placeholder="在测试模式下使用 110000000" 
              :disabled="isLoading"
            />
          </div>
          
          <div class="form-group">
            <label for="accountHolderName">账户持有人姓名</label>
            <input 
              id="accountHolderName" 
              v-model="bankAccountInfo.accountHolderName" 
              type="text" 
              placeholder="例如: John Doe" 
              :disabled="isLoading"
            />
          </div>
          
          <div class="form-group">
            <label for="accountHolderType">账户类型</label>
            <select 
              id="accountHolderType" 
              v-model="bankAccountInfo.accountHolderType" 
              :disabled="isLoading"
            >
              <option value="individual">个人</option>
              <option value="company">公司</option>
            </select>
          </div>
          
          <div class="form-group">
            <label for="bankCountry">国家</label>
            <select
              id="bankCountry"
              v-model="bankAccountInfo.country"
              :disabled="isLoading"
            >
              <option value="US">美国 (US)</option>
              <option value="GB">英国 (GB)</option>
              <option value="CA">加拿大 (CA)</option>
            </select>
          </div>
          
          <button 
            @click="addBankAccount" 
            :disabled="isLoading" 
            class="action-button"
          >
            {{ isLoading ? '处理中...' : `添加${currency.toUpperCase()}货币银行账户` }}
          </button>
        </div>
      </div>
      
      <div v-if="accountBalance" class="balance-info">
        <h3>账户余额详情</h3>
        
        <div v-if="accountBalance.available && accountBalance.available.length > 0" class="balance-section">
          <h4>可用余额</h4>
          <ul>
            <li v-for="(balance, index) in accountBalance.available" :key="'available-' + index">
              {{ (balance.amount / 100).toFixed(2) }} {{ balance.currency.toUpperCase() }}
              <span 
                v-if="hasAddedBankAccountForCurrency() && balance.currency === currency" 
                class="bank-status success"
              >
                已添加银行账户
              </span>
              <span 
                v-else-if="balance.currency === currency" 
                class="bank-status warning"
              >
                未添加银行账户
              </span>
            </li>
          </ul>
        </div>
        
        <div v-if="accountBalance.pending && accountBalance.pending.length > 0" class="balance-section">
          <h4>待处理余额</h4>
          <ul>
            <li v-for="(balance, index) in accountBalance.pending" :key="'pending-' + index">
              {{ (balance.amount / 100).toFixed(2) }} {{ balance.currency.toUpperCase() }}
            </li>
          </ul>
        </div>
      </div>
      
      <div class="account-status">
        <p>
          <span class="status-label">Connect账户ID:</span> 
          <span class="status-value">{{ connectAccountId }}</span>
        </p>
        <p>
          <span class="status-label">验证状态:</span>
          <span v-if="accountVerified" class="status-value verified">
            <span class="status-icon">✓</span> 已验证
          </span>
          <span v-else class="status-value unverified">
            <span class="status-icon">!</span> 未验证
          </span>
        </p>
        <p v-if="hasAddedBankAccountForCurrency()">
          <span class="status-label">银行账户状态:</span>
          <span class="status-value verified">
            <span class="status-icon">✓</span> 已添加{{ currency.toUpperCase() }}货币银行账户
          </span>
        </p>
        <p v-else>
          <span class="status-label">银行账户状态:</span>
          <span class="status-value unverified">
            <span class="status-icon">!</span> 未添加{{ currency.toUpperCase() }}货币银行账户
          </span>
        </p>
      </div>
      
      <div class="payment-notes">
        <p><strong>说明:</strong></p>
        <p>1. 向账户转账: 将平台账户资金转入Connect商户账户。</p>
        <p>2. 提现到银行账户: 将Connect商户账户资金提现到已添加的银行账户。</p>
        <p>3. 添加银行账户: 为Connect商户账户添加收款银行账户。</p>
        <p><strong>重要提示</strong>: 提现时，必须先添加对应货币的银行账户。</p>
        <p><strong>测试模式</strong>下，请使用以下银行账户信息:</p>
        <p>账号: 000123456789</p>
        <p>路由号码: 110000000 (美国)</p>
      </div>
    </div>
  </div>
</template>

<style scoped>
.connect-payment {
  max-width: 600px;
  margin: 0 auto;
  padding: 2rem;
}

h1 {
  text-align: center;
  margin-bottom: 2rem;
  color: #32325d;
}

.payment-form {
  background: #f8f9fa;
  border-radius: 8px;
  padding: 2rem;
  box-shadow: 0 4px 6px rgba(50, 50, 93, 0.11), 0 1px 3px rgba(0, 0, 0, 0.08);
}

.warning-message {
  background-color: #fff8e1;
  color: #b36d00;
  padding: 1rem;
  border-radius: 4px;
  margin-bottom: 1rem;
  text-align: center;
}

.warning-message a {
  color: #b36d00;
  font-weight: bold;
  text-decoration: underline;
}

.account-verification {
  background-color: #feeff0;
  color: #d84451;
  padding: 1.5rem;
  border: 1px solid #fccfd3;
  margin-bottom: 2rem;
}

.verification-button {
  background: #d84451;
  color: white;
  border: none;
  border-radius: 4px;
  padding: 0.75rem 1.5rem;
  margin-top: 0.75rem;
  font-size: 0.9rem;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.2s ease;
}

.verification-button:hover {
  background: #c73643;
}

.error-message {
  color: #fa755a;
  background-color: #fee;
  padding: 1rem;
  border-radius: 4px;
  margin-bottom: 1rem;
  text-align: center;
}

.success-message {
  color: #3c8505;
  background-color: #efffe5;
  padding: 1rem;
  border-radius: 4px;
  margin-bottom: 1rem;
  text-align: center;
}

.form-group {
  margin-bottom: 1.5rem;
}

.form-group label {
  display: block;
  margin-bottom: 0.5rem;
  color: #525f7f;
  font-weight: 500;
}

.form-group input,
.form-group select {
  width: 100%;
  padding: 0.75rem;
  border: 1px solid #e6ebf1;
  border-radius: 4px;
  font-size: 1rem;
  transition: border-color 0.2s ease;
}

.form-group input:focus,
.form-group select:focus {
  outline: none;
  border-color: #5469d4;
  box-shadow: 0 0 0 1px #5469d4;
}

.amount-display {
  display: block;
  margin-top: 0.5rem;
  color: #525f7f;
  font-size: 0.9rem;
}

.currency-note {
  display: block;
  margin-top: 0.5rem;
  color: #b36d00;
  font-size: 0.85rem;
}

.payment-actions {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 1rem;
  margin-bottom: 1.5rem;
}

.action-button {
  background: #5469d4;
  color: white;
  border-radius: 4px;
  border: 0;
  padding: 12px 16px;
  font-size: 0.9rem;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.2s ease;
}

.action-button:hover {
  filter: brightness(1.1);
}

.action-button:disabled {
  opacity: 0.5;
  cursor: default;
}

.action-button.secondary {
  background: #f7fafc;
  color: #5469d4;
  border: 1px solid #5469d4;
}

.action-button.secondary:hover {
  background: #edf2f7;
}

.bank-account-form {
  margin-top: 2rem;
  padding: 1.5rem;
  background-color: #f7fafc;
  border-radius: 8px;
  border: 1px solid #e6ebf1;
}

.bank-account-form h3 {
  margin-top: 0;
  margin-bottom: 1rem;
  color: #32325d;
  font-size: 1.2rem;
  text-align: center;
}

.bank-currency-info {
  background-color: #fff8e1;
  color: #b36d00;
  padding: 0.75rem;
  border-radius: 4px;
  margin-bottom: 1.5rem;
  font-size: 0.9rem;
  text-align: center;
}

.balance-info {
  margin-top: 2rem;
  padding: 1.5rem;
  background-color: #f7fafc;
  border-radius: 8px;
  border: 1px solid #e6ebf1;
}

.balance-info h3 {
  margin-top: 0;
  margin-bottom: 1rem;
  color: #32325d;
  font-size: 1.2rem;
  text-align: center;
}

.balance-section {
  margin-bottom: 1rem;
}

.balance-section h4 {
  color: #525f7f;
  font-size: 1rem;
  margin-bottom: 0.5rem;
}

.balance-section ul {
  list-style: none;
  padding: 0;
  margin: 0;
}

.balance-section li {
  padding: 0.5rem;
  background-color: white;
  border-radius: 4px;
  margin-bottom: 0.5rem;
  font-weight: 500;
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.bank-status {
  font-size: 0.8rem;
  font-weight: normal;
  padding: 0.25rem 0.5rem;
  border-radius: 50px;
}

.bank-status.success {
  background-color: #efffe5;
  color: #3c8505;
}

.bank-status.warning {
  background-color: #fff8e1;
  color: #b36d00;
}

.account-status {
  margin-top: 1rem;
  padding: 0.5rem;
  background: rgba(84, 105, 212, 0.05);
  border-radius: 4px;
  text-align: center;
}

.status-label {
  font-weight: 500;
}

.status-value {
  margin-left: 0.5rem;
  font-weight: normal;
}

.status-value.verified {
  color: #3c8505;
}

.status-value.unverified {
  color: #fa755a;
}

.status-icon {
  display: inline-block;
  width: 18px;
  height: 18px;
  border-radius: 50%;
  text-align: center;
  line-height: 18px;
  font-size: 12px;
  margin-right: 5px;
}

.verified .status-icon {
  background-color: #3c8505;
  color: white;
}

.unverified .status-icon {
  background-color: #fa755a;
  color: white;
}

.payment-notes {
  margin-top: 2rem;
  padding-top: 1rem;
  border-top: 1px solid #e6ebf1;
  font-size: 0.85rem;
  color: #6b7c93;
}

.payment-notes p {
  margin: 0.5rem 0;
}

.bank-account-toggle {
  display: flex;
  justify-content: center;
  margin-bottom: 1.5rem;
  border-radius: 4px;
  overflow: hidden;
  border: 1px solid #e6ebf1;
}

.toggle-button {
  flex: 1;
  padding: 0.75rem;
  background: #f7fafc;
  border: none;
  cursor: pointer;
  font-size: 0.9rem;
  transition: all 0.2s ease;
}

.toggle-button.active {
  background: #5469d4;
  color: white;
  font-weight: 600;
}

.existing-account-form {
  margin-top: 1rem;
}

.input-hint {
  display: block;
  margin-top: 0.5rem;
  color: #6b7c93;
  font-size: 0.8rem;
  font-family: monospace;
}
</style> 