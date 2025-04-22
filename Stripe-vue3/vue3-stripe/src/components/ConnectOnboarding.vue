<script setup>
import { ref, onMounted } from 'vue';

const isLoading = ref(false);
const connectAccountId = ref('');
const onboardingStatus = ref('pending'); // pending, completed, failed
const accountLink = ref('');
const errorMessage = ref('');
const baseApiUrl = 'http://localhost:5062'; // 后端API基础地址

// 当组件挂载时，检查是否已有Connect账户ID
onMounted(async () => {
  try {
    // 从本地存储或会话中检索已存在的Connect账户ID
    const savedAccountId = localStorage.getItem('stripe_connect_account_id');
    if (savedAccountId) {
      connectAccountId.value = savedAccountId;
      await checkAccountStatus();
    }
  } catch (error) {
    console.error('初始化Connect账户检查失败:', error);
    errorMessage.value = '无法检查Connect账户状态，请刷新页面重试';
  }
});

// 创建新的Stripe Connect Custom账户
const createConnectAccount = async () => {
  isLoading.value = true;
  errorMessage.value = '';
  
  try {
    // 使用实际的API接口 api/Connect/create-account
    const response = await fetch(`${baseApiUrl}/api/Connect/create-account`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json'
      },
      body: JSON.stringify({
        country: 'US', // 可根据实际需要修改国家/地区代码
        email: 'testaccount@example.com', // 在生产环境中应该使用实际用户邮箱
        businessType: 'individual', // 或'company', 视业务需求而定
        businessProfile: {
          url: 'https://example.com', // 您的网站URL
          mcc: '5734' // 商户类别代码，可根据实际业务类型选择
        }
      })
    });

    if (!response.ok) {
      throw new Error('创建Connect账户失败');
    }

    const data = await response.json();
    connectAccountId.value = data.accountId;
    localStorage.setItem('stripe_connect_account_id', data.accountId);
    
    // 创建账户成功后，创建账户链接进行验证
    await createAccountLink();
  } catch (error) {
    console.error('创建Connect账户错误:', error);
    errorMessage.value = '创建Connect账户失败，请重试';
  } finally {
    isLoading.value = false;
  }
};

// 创建账户链接以进行验证和信息收集
const createAccountLink = async () => {
  isLoading.value = true;
  errorMessage.value = '';
  
  try {
    // 按照实际API接口格式调用 api/Connect/create-account-link
    const queryParams = new URLSearchParams({
      accountId: connectAccountId.value,
      refreshUrl: window.location.href,
      returnUrl: window.location.href + '?onboarding=success'
    }).toString();
    
    // 使用不带请求体的POST请求，参数通过查询字符串传递
    const response = await fetch(`${baseApiUrl}/api/Connect/create-account-link?${queryParams}`, {
      method: 'POST',
      headers: {
        'accept': '*/*'
      }
    });

    if (!response.ok) {
      throw new Error('创建账户链接失败');
    }

    const data = await response.json();
    accountLink.value = data.url;
    
    // 重定向到Stripe的账户验证页面
    window.location.href = accountLink.value;
  } catch (error) {
    console.error('创建账户链接错误:', error);
    errorMessage.value = '无法完成账户验证流程，请重试';
  } finally {
    isLoading.value = false;
  }
};

// 检查账户状态 - 这个API可能不存在，但我们可以使用balance接口来间接检查账户状态
const checkAccountStatus = async () => {
  if (!connectAccountId.value) return;
  
  isLoading.value = true;
  errorMessage.value = '';
  
  try {
    // 使用balance接口检查账户状态
    const response = await fetch(`${baseApiUrl}/api/Connect/balance/${connectAccountId.value}`, {
      method: 'GET',
      headers: {
        'Content-Type': 'application/json'
      }
    });

    if (response.ok) {
      // 如果可以成功获取余额，说明账户已验证
      onboardingStatus.value = 'completed';
    } else {
      // 如果获取余额失败，可能是账户尚未完成验证
      onboardingStatus.value = 'pending';
      // 创建新的账户链接继续验证流程
      await createAccountLink();
    }
  } catch (error) {
    console.error('检查账户状态错误:', error);
    errorMessage.value = '无法检查账户状态，请重试';
    onboardingStatus.value = 'failed';
  } finally {
    isLoading.value = false;
  }
};

// 添加银行账户
const addBankAccount = async () => {
  isLoading.value = true;
  errorMessage.value = '';
  successMessage.value = '';
  
  try {
    // 首先通过后端API创建银行账户Token
    const tokenResponse = await fetch(`${baseApiUrl}/api/Connect/create-bank-account-token`, {
      method: 'POST',
      headers: {
        'accept': '*/*',
        'Content-Type': 'application/json'
      },
      body: JSON.stringify({
        accountNumber: '000123456789', // 测试账号，实际应从表单获取
        routingNumber: '110000000', // 美国银行路由号码，测试值
        accountHolderName: 'John Doe', // 账户持有人姓名
        accountHolderType: 'individual', // individual或company
        country: 'US', // 国家代码
        currency: 'usd' // 货币
      })
    });

    if (!tokenResponse.ok) {
      const errorData = await tokenResponse.json().catch(() => ({}));
      console.error('创建银行账户Token失败:', errorData);
      throw new Error('创建银行账户Token失败');
    }

    // 获取返回的Token
    const tokenData = await tokenResponse.json();
    console.log('成功创建银行账户Token:', tokenData);
    
    if (!tokenData.tokenId) {
      throw new Error('创建银行账户Token失败: 未返回有效的Token ID');
    }
    
    // 然后添加银行账户到Connect账户
    const bankResponse = await fetch(`${baseApiUrl}/api/Connect/add-bank-account/${connectAccountId.value}`, {
      method: 'POST',
      headers: {
        'accept': '*/*',
        'Content-Type': 'application/json'
      },
      body: JSON.stringify({
        tokenId: tokenData.tokenId // 使用后端返回的真实Token
      })
    });

    if (!bankResponse.ok) {
      const errorData = await bankResponse.json().catch(() => ({}));
      console.error('添加银行账户失败:', errorData);
      throw new Error('添加银行账户失败');
    }

    const bankData = await bankResponse.json();
    console.log('成功添加银行账户:', bankData);
    successMessage.value = '成功添加银行账户';
  } catch (error) {
    console.error('添加银行账户错误:', error);
    errorMessage.value = '添加银行账户失败，请重试: ' + error.message;
  } finally {
    isLoading.value = false;
  }
};

// 重新开始验证流程
const restartOnboarding = () => {
  createAccountLink();
};

// 成功消息显示
const successMessage = ref('');
</script>

<template>
  <div class="connect-onboarding">
    <h1>Stripe Connect 商户注册</h1>
    
    <div v-if="errorMessage" class="error-message">
      {{ errorMessage }}
    </div>
    
    <div v-if="successMessage" class="success-message">
      {{ successMessage }}
    </div>
    
    <div class="connect-status">
      <template v-if="!connectAccountId">
        <p>您需要注册成为我们的平台商户以接收支付。</p>
        <button 
          @click="createConnectAccount" 
          :disabled="isLoading" 
          class="connect-button"
        >
          {{ isLoading ? '处理中...' : '开始注册' }}
        </button>
      </template>
      
      <template v-else>
        <p v-if="onboardingStatus === 'completed'" class="status-message success">
          恭喜！您的商户账户已完成注册，现在可以接收支付了。
        </p>
        
        <p v-else-if="onboardingStatus === 'pending'" class="status-message warning">
          您的账户注册流程尚未完成，请继续完成注册以接收支付。
        </p>
        
        <p v-else class="status-message error">
          账户注册遇到问题，请重试。
        </p>
        
        <button 
          v-if="onboardingStatus !== 'completed'"
          @click="restartOnboarding" 
          :disabled="isLoading" 
          class="connect-button"
        >
          {{ isLoading ? '处理中...' : '继续注册流程' }}
        </button>
        
        <div v-if="onboardingStatus === 'completed'" class="account-actions">
          <p class="account-id">
            账户ID: {{ connectAccountId }}
          </p>
          
          <button 
            @click="addBankAccount" 
            :disabled="isLoading" 
            class="action-button"
          >
            {{ isLoading ? '处理中...' : '添加银行账户' }}
          </button>
        </div>
      </template>
    </div>
  </div>
</template>

<style scoped>
.connect-onboarding {
  max-width: 600px;
  margin: 0 auto;
  padding: 2rem;
}

h1 {
  text-align: center;
  margin-bottom: 2rem;
  color: #32325d;
}

.connect-status {
  background: #f8f9fa;
  border-radius: 8px;
  padding: 2rem;
  box-shadow: 0 4px 6px rgba(50, 50, 93, 0.11), 0 1px 3px rgba(0, 0, 0, 0.08);
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

.status-message {
  padding: 0.8rem;
  border-radius: 4px;
  margin-bottom: 1.5rem;
  text-align: center;
}

.success {
  background-color: #efffe5;
  color: #3c8505;
}

.warning {
  background-color: #fff8e1;
  color: #b36d00;
}

.error {
  background-color: #fee;
  color: #fa755a;
}

.connect-button {
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

.connect-button:hover {
  filter: brightness(1.1);
}

.connect-button:disabled {
  opacity: 0.5;
  cursor: default;
}

.account-id {
  margin-top: 1rem;
  text-align: center;
  color: #525f7f;
  font-size: 0.9rem;
}

.account-actions {
  margin-top: 1.5rem;
}

.action-button {
  background: #5469d4;
  color: white;
  border-radius: 4px;
  border: 0;
  padding: 10px 16px;
  font-size: 0.9rem;
  font-weight: 600;
  cursor: pointer;
  display: block;
  width: 100%;
  margin-top: 1rem;
  transition: all 0.2s ease;
}

.action-button:hover {
  filter: brightness(1.1);
}

.action-button:disabled {
  opacity: 0.5;
  cursor: default;
}
</style> 