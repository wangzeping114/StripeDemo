<script setup>
import { ref, onMounted } from 'vue';
import { loadStripe } from '@stripe/stripe-js';

const withdrawalAmount = ref(1000); // 默认提现金额，单位为分（10元）
const minAmount = ref(500); // 最小提现金额，单位为分（5元）
const balance = ref(5000); // 账户余额，单位为分（50元）
const isLoading = ref(false);
const withdrawalMessage = ref('');
const baseApiUrl = 'http://localhost:5062'; // 后端API基础地址

// 用户信息
const userId = ref('user123');
const userName = ref('测试用户');
const email = ref('test@example.com');

// 银行账户信息
const accountHolder = ref('');
const accountNumber = ref('');
const bankName = ref('');
const swiftCode = ref('');

// 提现状态
const withdrawalStatus = ref(null);
const withdrawalId = ref('');

onMounted(async () => {
  try {
    // 从后端获取用户余额
    await fetchUserBalance();
  } catch (error) {
    console.error('获取账户余额失败:', error);
  }
});

// 获取用户余额
const fetchUserBalance = async () => {
  try {
    const response = await fetch(`${baseApiUrl}/api/Payment/user-balance?userId=${userId.value}`, {
      method: 'GET',
      headers: {
        'Content-Type': 'application/json'
      }
    });

    if (!response.ok) {
      throw new Error('获取余额失败');
    }

    const data = await response.json();
    balance.value = data.balance || 5000; // 如果API不可用，使用默认值
  } catch (error) {
    console.error('获取余额失败:', error);
    // 使用默认值
  }
};

// 验证提现请求
const validateWithdrawal = () => {
  if (withdrawalAmount.value < minAmount.value) {
    withdrawalMessage.value = `提现金额不能低于 ${(minAmount.value / 100).toFixed(2)} 元`;
    return false;
  }
  
  if (withdrawalAmount.value > balance.value) {
    withdrawalMessage.value = '提现金额不能超过账户余额';
    return false;
  }
  
  if (!accountHolder.value.trim() || !accountNumber.value.trim() || !bankName.value.trim()) {
    withdrawalMessage.value = '请填写完整的银行账户信息';
    return false;
  }
  
  return true;
};

// 处理提现请求
const handleWithdrawal = async () => {
  withdrawalMessage.value = '';
  
  if (!validateWithdrawal()) {
    return;
  }
  
  isLoading.value = true;
  
  try {
    const response = await fetch(`${baseApiUrl}/api/Payment/process-withdrawal`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json'
      },
      body: JSON.stringify({
        userId: userId.value,
        userName: userName.value,
        email: email.value,
        amount: withdrawalAmount.value,
        accountHolder: accountHolder.value,
        accountNumber: accountNumber.value,
        bankName: bankName.value,
        swiftCode: swiftCode.value,
        withdrawalDate: new Date().toISOString()
      })
    });

    if (!response.ok) {
      throw new Error('提现请求处理失败');
    }

    const result = await response.json();
    
    withdrawalStatus.value = 'success';
    withdrawalId.value = result.withdrawalId || 'WD' + Date.now();
    withdrawalMessage.value = `提现请求已成功提交，处理ID: ${withdrawalId.value}`;
    
    // 更新余额
    balance.value -= withdrawalAmount.value;
  } catch (error) {
    console.error('提现处理错误:', error);
    withdrawalStatus.value = 'error';
    withdrawalMessage.value = '提现处理失败，请稍后重试';
  } finally {
    isLoading.value = false;
  }
};
</script>

<template>
  <div class="stripe-withdrawal">
    <h1>账户提现</h1>
    
    <div class="withdrawal-form">
      <div class="balance-display">
        <div class="balance-label">当前余额</div>
        <div class="balance-amount">{{ (balance / 100).toFixed(2) }} 元</div>
      </div>
      
      <div v-if="withdrawalStatus === 'success'" class="success-message">
        <div class="success-icon">✓</div>
        <h3>提现请求已提交</h3>
        <p>您的提现请求已成功提交，将在1-3个工作日内处理。</p>
        <p>提现ID: {{ withdrawalId }}</p>
        <p>提现金额: {{ (withdrawalAmount / 100).toFixed(2) }} 元</p>
      </div>
      
      <form v-else id="withdrawal-form">
        <div class="form-group">
          <label for="withdrawal-amount">提现金额</label>
          <div class="input-group">
            <input 
              type="number" 
              id="withdrawal-amount" 
              v-model="withdrawalAmount" 
              :min="minAmount"
              :max="balance"
              step="100"
              @input="withdrawalMessage = ''"
            />
            <span class="input-addon">分 ({{ (withdrawalAmount / 100).toFixed(2) }} 元)</span>
          </div>
          <div class="input-hint">最小提现金额: {{ (minAmount / 100).toFixed(2) }} 元</div>
        </div>
        
        <div class="form-group">
          <label for="account-holder">账户持有人</label>
          <input 
            type="text" 
            id="account-holder" 
            v-model="accountHolder" 
            placeholder="请输入账户持有人姓名"
            @input="withdrawalMessage = ''"
          />
        </div>
        
        <div class="form-group">
          <label for="account-number">银行账号</label>
          <input 
            type="text" 
            id="account-number" 
            v-model="accountNumber" 
            placeholder="请输入银行账号"
            @input="withdrawalMessage = ''"
          />
        </div>
        
        <div class="form-group">
          <label for="bank-name">银行名称</label>
          <input 
            type="text" 
            id="bank-name" 
            v-model="bankName" 
            placeholder="请输入银行名称"
            @input="withdrawalMessage = ''"
          />
        </div>
        
        <div class="form-group">
          <label for="swift-code">SWIFT代码 (可选)</label>
          <input 
            type="text" 
            id="swift-code" 
            v-model="swiftCode" 
            placeholder="请输入SWIFT代码(国际转账)"
          />
        </div>
        
        <div class="error-message" v-if="withdrawalMessage">{{ withdrawalMessage }}</div>
        
        <button 
          type="button"
          @click="handleWithdrawal" 
          :disabled="isLoading || withdrawalAmount > balance || withdrawalAmount < minAmount" 
          class="withdrawal-button"
        >
          {{ isLoading ? '处理中...' : '提交提现请求' }}
        </button>
      </form>
      
      <div class="withdrawal-notes">
        <p>提现说明:</p>
        <ul>
          <li>提现将在1-3个工作日内处理完成</li>
          <li>银行可能会收取手续费</li>
          <li>请确保您的银行账户信息准确无误</li>
        </ul>
      </div>
    </div>
  </div>
</template>

<style scoped>
.stripe-withdrawal {
  max-width: 600px;
  margin: 0 auto;
  padding: 2rem;
}

h1 {
  text-align: center;
  margin-bottom: 2rem;
  color: #32325d;
}

.withdrawal-form {
  background: #f8f9fa;
  border-radius: 8px;
  padding: 2rem;
  box-shadow: 0 4px 6px rgba(50, 50, 93, 0.11), 0 1px 3px rgba(0, 0, 0, 0.08);
}

.balance-display {
  margin-bottom: 2rem;
  text-align: center;
  padding: 1rem;
  background: rgba(255, 255, 255, 0.7);
  border-radius: 4px;
}

.balance-label {
  font-size: 0.9rem;
  color: #6b7c93;
  margin-bottom: 0.3rem;
}

.balance-amount {
  font-size: 2rem;
  font-weight: bold;
  color: #32325d;
}

.form-group {
  margin-bottom: 1.5rem;
}

label {
  display: block;
  margin-bottom: 0.5rem;
  color: #6b7c93;
  font-size: 0.9rem;
}

input {
  width: 100%;
  padding: 12px;
  border-radius: 4px;
  border: 1px solid #e6ebf1;
  font-size: 1rem;
  box-sizing: border-box;
}

.input-group {
  display: flex;
  align-items: center;
}

.input-group input {
  flex: 1;
  border-top-right-radius: 0;
  border-bottom-right-radius: 0;
}

.input-addon {
  padding: 12px;
  background: #e9ecef;
  border: 1px solid #e6ebf1;
  border-left: none;
  border-top-right-radius: 4px;
  border-bottom-right-radius: 4px;
  color: #6b7c93;
  white-space: nowrap;
}

.input-hint {
  margin-top: 0.3rem;
  font-size: 0.8rem;
  color: #6b7c93;
}

input:focus {
  outline: none;
  border-color: #5469d4;
  box-shadow: 0 0 0 1px #5469d4;
}

.error-message {
  color: #e25950;
  margin: 1rem 0;
  font-size: 0.9rem;
}

.withdrawal-button {
  background: #5469d4;
  color: white;
  border-radius: 4px;
  border: 0;
  padding: 12px 16px;
  font-size: 1rem;
  font-weight: 600;
  cursor: pointer;
  display: block;
  width: 100%;
  transition: all 0.2s ease;
}

.withdrawal-button:hover {
  filter: brightness(1.1);
}

.withdrawal-button:disabled {
  opacity: 0.5;
  cursor: default;
}

.withdrawal-notes {
  margin-top: 2rem;
  padding-top: 1rem;
  border-top: 1px solid #e6ebf1;
  font-size: 0.85rem;
  color: #6b7c93;
}

.withdrawal-notes p {
  margin: 0.5rem 0;
}

.withdrawal-notes ul {
  padding-left: 1.2rem;
  margin-top: 0.5rem;
}

.withdrawal-notes li {
  margin-bottom: 0.3rem;
}

.success-message {
  text-align: center;
  padding: 2rem 1rem;
}

.success-icon {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 60px;
  height: 60px;
  margin: 0 auto 1.5rem;
  background-color: #43a047;
  color: white;
  font-size: 28px;
  border-radius: 50%;
}

.success-message h3 {
  margin-bottom: 1rem;
  color: #43a047;
}

.success-message p {
  margin: 0.5rem 0;
  color: #525f7f;
}
</style> 