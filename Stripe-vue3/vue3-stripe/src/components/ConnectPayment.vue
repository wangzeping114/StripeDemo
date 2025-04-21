<script setup>
import { ref, onMounted } from 'vue';

const isLoading = ref(false);
const paymentAmount = ref(1999); // 金额，单位为分（19.99元）
const currency = ref('usd'); // 币种
const description = ref('商品购买'); // 描述
const connectAccountId = ref('');
const accountBalance = ref(null);
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

// 当组件挂载时，检查是否已有Connect账户ID
onMounted(async () => {
  try {
    // 从本地存储或会话中检索已存在的Connect账户ID
    const savedAccountId = localStorage.getItem('stripe_connect_account_id');
    if (savedAccountId) {
      connectAccountId.value = savedAccountId;
      await checkConnectBalance();
    }
  } catch (error) {
    console.error('初始化Connect账户检查失败:', error);
    errorMessage.value = '无法检查Connect账户状态，请刷新页面重试';
  }
});

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
        'Content-Type': 'application/json'
      }
    });

    if (!response.ok) {
      throw new Error('查询账户余额失败');
    }

    const data = await response.json();
    accountBalance.value = data;
    
    // 显示余额信息
    if (data.available && data.available.length > 0) {
      successMessage.value = `可用余额: ${(data.available[0].amount / 100).toFixed(2)} ${data.available[0].currency.toUpperCase()}`;
    } else {
      successMessage.value = '当前没有可用余额';
    }
  } catch (error) {
    console.error('查询账户余额错误:', error);
    errorMessage.value = '查询账户余额失败，请重试';
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

  isLoading.value = true;
  errorMessage.value = '';
  successMessage.value = '';
  
  try {
    // 使用实际的API接口 api/Connect/create-transfer
    const response = await fetch(`${baseApiUrl}/api/Connect/create-transfer`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json'
      },
      body: JSON.stringify({
        amount: paymentAmount.value,
        currency: currency.value,
        destinationAccountId: connectAccountId.value,
        description: description.value || '平台转账'
      })
    });

    if (!response.ok) {
      throw new Error('创建账户转账失败');
    }

    const data = await response.json();
    successMessage.value = `转账成功！转账ID: ${data.transferId}`;
    
    // 转账成功后更新余额
    await checkConnectBalance();
  } catch (error) {
    console.error('创建账户转账错误:', error);
    errorMessage.value = '创建账户转账失败，请重试';
  } finally {
    isLoading.value = false;
  }
};

// 创建Connect账户提现到银行账户
const createConnectedPayout = async () => {
  if (!connectAccountId.value) {
    errorMessage.value = '请先完成商户注册';
    return;
  }

  isLoading.value = true;
  errorMessage.value = '';
  successMessage.value = '';
  
  try {
    // 使用实际的API接口 api/Connect/create-connected-payout
    const response = await fetch(`${baseApiUrl}/api/Connect/create-connected-payout`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json'
      },
      body: JSON.stringify({
        accountId: connectAccountId.value,
        amount: paymentAmount.value,
        currency: currency.value,
        description: '提现到银行账户'
      })
    });

    if (!response.ok) {
      throw new Error('创建提现失败');
    }

    const data = await response.json();
    successMessage.value = `提现申请成功！提现ID: ${data.payoutId}`;
    
    // 提现成功后更新余额
    await checkConnectBalance();
  } catch (error) {
    console.error('创建提现错误:', error);
    errorMessage.value = '创建提现失败，请重试';
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
  
  if (!bankAccountInfo.value.accountNumber || !bankAccountInfo.value.routingNumber || !bankAccountInfo.value.accountHolderName) {
    errorMessage.value = '请填写完整银行账户信息';
    return;
  }

  isLoading.value = true;
  errorMessage.value = '';
  successMessage.value = '';
  
  try {
    // 首先创建银行账户Token
    const tokenResponse = await fetch(`${baseApiUrl}/api/Connect/create-bank-account-token`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json'
      },
      body: JSON.stringify(bankAccountInfo.value)
    });

    if (!tokenResponse.ok) {
      throw new Error('创建银行账户Token失败');
    }

    const tokenData = await tokenResponse.json();
    
    // 然后添加银行账户到Connect账户
    const bankResponse = await fetch(`${baseApiUrl}/api/Connect/add-bank-account/${connectAccountId.value}`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json'
      },
      body: JSON.stringify({
        bankAccountToken: tokenData.tokenId
      })
    });

    if (!bankResponse.ok) {
      throw new Error('添加银行账户失败');
    }

    successMessage.value = '成功添加银行账户';
    
    // 清空表单
    bankAccountInfo.value.accountNumber = '';
    bankAccountInfo.value.routingNumber = '';
    bankAccountInfo.value.accountHolderName = '';
  } catch (error) {
    console.error('添加银行账户错误:', error);
    errorMessage.value = '添加银行账户失败，请重试';
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
          :disabled="isLoading" 
          class="action-button"
        >
          {{ isLoading ? '处理中...' : '向账户转账' }}
        </button>
        
        <button 
          @click="createConnectedPayout" 
          :disabled="isLoading" 
          class="action-button secondary"
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
        <h3>添加银行账户</h3>
        
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
        
        <button 
          @click="addBankAccount" 
          :disabled="isLoading" 
          class="action-button"
        >
          {{ isLoading ? '处理中...' : '添加银行账户' }}
        </button>
      </div>
      
      <div v-if="accountBalance" class="balance-info">
        <h3>账户余额详情</h3>
        
        <div v-if="accountBalance.available && accountBalance.available.length > 0" class="balance-section">
          <h4>可用余额</h4>
          <ul>
            <li v-for="(balance, index) in accountBalance.available" :key="'available-' + index">
              {{ (balance.amount / 100).toFixed(2) }} {{ balance.currency.toUpperCase() }}
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
      
      <div class="connect-account-info">
        <p><strong>Connect账户ID:</strong> {{ connectAccountId }}</p>
      </div>
      
      <div class="payment-notes">
        <p><strong>说明:</strong></p>
        <p>1. 向账户转账: 将平台账户资金转入Connect商户账户。</p>
        <p>2. 提现到银行账户: 将Connect商户账户资金提现到已添加的银行账户。</p>
        <p>3. 添加银行账户: 为Connect商户账户添加收款银行账户。</p>
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
  margin-bottom: 1.5rem;
  color: #32325d;
  font-size: 1.2rem;
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
}

.connect-account-info {
  margin-top: 1rem;
  padding: 0.5rem;
  background: rgba(84, 105, 212, 0.05);
  border-radius: 4px;
  text-align: center;
}

.connect-account-info p {
  margin: 0;
  font-size: 0.9rem;
  color: #5469d4;
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
</style> 