<script setup>
import { ref, onMounted } from 'vue';
import { useRoute } from 'vue-router';

const route = useRoute();
const isLoading = ref(false);
const paymentMessage = ref('');
const baseApiUrl = 'http://localhost:5062'; // 后端API基础地址

// 从URL查询参数获取订单信息
const paymentAmount = ref(Number(route.query.amount) || 1999);
const currency = ref(route.query.currency || 'usd');
const orderId = ref(route.query.orderId || 'ORD' + Date.now());

// 用户信息
const userId = ref('user123');
const userName = ref('测试用户');
const email = ref('test@example.com');
const walletId = ref('');
const orderDescription = ref('商品购买');

// 初始化支付
onMounted(async () => {
  // 可以从后端获取初始化参数
  try {
    await initJetonPayment();
  } catch (error) {
    console.error('初始化Jeton支付失败:', error);
    paymentMessage.value = '支付初始化失败，请刷新页面重试';
  }
});

// 初始化Jeton支付
const initJetonPayment = async () => {
  try {
    const response = await fetch(`${baseApiUrl}/api/Payment/init-jeton-payment`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json'
      },
      body: JSON.stringify({
        amount: paymentAmount.value,
        currency: currency.value,
        orderId: orderId.value,
        description: orderDescription.value,
        userId: userId.value,
        userName: userName.value,
        email: email.value
      })
    });

    if (!response.ok) {
      throw new Error('服务器响应错误');
    }

    const data = await response.json();
    // 存储支付相关参数（实际实现中可能有所不同）
    console.log('Jeton支付初始化成功', data);
  } catch (error) {
    console.error('初始化Jeton支付失败:', error);
    paymentMessage.value = '支付初始化失败，请刷新页面重试';
  }
};

// 处理支付
const handleSubmit = async () => {
  if (!walletId.value.trim()) {
    paymentMessage.value = '请输入有效的Jeton钱包ID';
    return;
  }

  isLoading.value = true;
  paymentMessage.value = '';
  
  try {
    const response = await fetch(`${baseApiUrl}/api/Payment/process-jeton-payment`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json'
      },
      body: JSON.stringify({
        walletId: walletId.value,
        amount: paymentAmount.value,
        currency: currency.value,
        orderId: orderId.value,
        description: orderDescription.value,
        userId: userId.value,
        userName: userName.value,
        email: email.value
      })
    });

    if (!response.ok) {
      throw new Error('支付处理失败');
    }

    const result = await response.json();
    
    if (result.success) {
      paymentMessage.value = '支付成功！交易ID: ' + result.transactionId;
      // 更新订单状态
      await updateOrderStatus(result.transactionId);
    } else {
      paymentMessage.value = result.message || '支付失败，请重试';
    }
  } catch (error) {
    console.error('支付处理错误:', error);
    paymentMessage.value = '支付处理出错，请重试';
  } finally {
    isLoading.value = false;
  }
};

// 更新订单状态
const updateOrderStatus = async (transactionId) => {
  try {
    const response = await fetch(`${baseApiUrl}/api/Payment/update-jeton-order-status`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json'
      },
      body: JSON.stringify({
        transactionId: transactionId,
        amount: paymentAmount.value,
        currency: currency.value,
        orderId: orderId.value,
        description: orderDescription.value,
        userId: userId.value,
        userName: userName.value,
        email: email.value,
        paymentStatus: 'succeeded',
        paymentMethod: 'jeton_wallet',
        transactionDate: new Date().toISOString()
      })
    });

    if (!response.ok) {
      console.warn('订单状态更新可能失败');
    }
  } catch (error) {
    console.error('更新订单状态错误:', error);
  }
};
</script>

<template>
  <div class="jeton-payment">
    <h1>Jeton Wallet支付</h1>
    
    <div class="payment-form">
      <div class="amount-display">
        总计: {{ (paymentAmount / 100).toFixed(2) }} {{ currency.toUpperCase() }}
      </div>
      
      <div class="order-info">
        <p>订单编号: {{ orderId }}</p>
        <p>客户: {{ userName }}</p>
        <p>描述: {{ orderDescription }}</p>
      </div>
      
      <div class="jeton-logo">
        <div class="logo-image"></div>
      </div>
      
      <form id="jeton-form">
        <div class="form-group">
          <label for="wallet-id">Jeton钱包ID</label>
          <input 
            type="text" 
            id="wallet-id" 
            v-model="walletId" 
            placeholder="请输入您的Jeton钱包ID" 
            required
          />
        </div>
        
        <button 
          type="button"
          @click="handleSubmit" 
          :disabled="isLoading" 
          class="pay-button"
        >
          {{ isLoading ? '处理中...' : '确认支付' }}
        </button>
      </form>
      
      <div class="payment-message" v-if="paymentMessage">
        {{ paymentMessage }}
      </div>
      
      <div class="payment-notes">
        <p>使用Jeton Wallet支付无需信用卡，安全快捷。</p>
        <p>如果您没有Jeton钱包，<a href="https://www.jeton.com" target="_blank">点击此处注册</a>。</p>
      </div>
    </div>
  </div>
</template>

<style scoped>
.jeton-payment {
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

.amount-display {
  font-size: 1.5rem;
  font-weight: bold;
  text-align: center;
  margin-bottom: 1rem;
  color: #32325d;
}

.order-info {
  margin-bottom: 1.5rem;
  padding: 1rem;
  background: rgba(255, 255, 255, 0.5);
  border-radius: 4px;
}

.order-info p {
  margin: 0.3rem 0;
  font-size: 0.9rem;
  color: #525f7f;
}

.jeton-logo {
  text-align: center;
  margin-bottom: 2rem;
}

.logo-image {
  display: inline-block;
  width: 120px;
  height: 120px;
  background-image: url('data:image/svg+xml;base64,PHN2ZyB3aWR0aD0iNjAiIGhlaWdodD0iNjAiIHZpZXdCb3g9IjAgMCA2MCA2MCIgZmlsbD0ibm9uZSIgeG1sbnM9Imh0dHA6Ly93d3cudzMub3JnLzIwMDAvc3ZnIj48Y2lyY2xlIGN4PSIzMCIgY3k9IjMwIiByPSIzMCIgZmlsbD0iIzE2OTVBMyIvPjxwYXRoIGQ9Ik0xNC4wNDY5IDI0LjQwODVIMTguMjg4MlYzNS41OTE1SDE0LjA0NjlWMjQuNDA4NVoiIGZpbGw9IndoaXRlIi8+PHBhdGggZD0iTTI2LjA4NTkgMjQuNDA4NUgzMC4zMjcyTDI2LjMwMDQgMzUuNTkxNUgyMi4wNTkyTDI2LjA4NTkgMjQuNDA4NVoiIGZpbGw9IndoaXRlIi8+PHBhdGggZD0iTTM4LjEyMjEgMjQuNDA4NUg0Mi4zNjM0TDM4LjMzNjUgMzUuNTkxNUgzNC4wOTUzTDM4LjEyMjEgMjQuNDA4NVoiIGZpbGw9IndoaXRlIi8+PHBhdGggZD0iTTQ2Ljc1MzEgMjQuNDA4NUg0NC4wNDY5TDQyLjc5NTcgMjguNzMyNEw0NC4wNDY5IDI0LjQwODVMNDYuNzUzMSAyNC40MDg1WiIgZmlsbD0id2hpdGUiLz48cGF0aCBkPSJNMTkuMzA2MSAyNC40MDg1SDI0LjU0MzlMMjIuNDY5OSAzMC43NTM1TDIwLjYxNjcgMzUuNTkxNUwxOS4zMDYxIDMxLjU3ODhWMjQuNDA4NVoiIGZpbGw9IndoaXRlIi8+PHBhdGggZD0iTTMxLjM0NTEgMjQuNDA4NUgzNi41ODMxTDM0LjUwODkgMzAuNzUzNUwzMi42NTU5IDM1LjU5MTVMMzEuMzQ1MSAzMS41Nzg4VjI0LjQwODVaIiBmaWxsPSJ3aGl0ZSIvPjwvc3ZnPg==');
  background-size: contain;
  background-position: center;
  background-repeat: no-repeat;
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

input:focus {
  outline: none;
  border-color: #1695A3;
  box-shadow: 0 0 0 1px #1695A3;
}

.pay-button {
  background: #1695A3;
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

.pay-button:hover {
  filter: brightness(1.1);
}

.pay-button:disabled {
  opacity: 0.5;
  cursor: default;
}

.payment-message {
  margin-top: 1.5rem;
  text-align: center;
  color: #43a047;
  font-weight: 500;
}

.payment-notes {
  margin-top: 2rem;
  padding-top: 1rem;
  border-top: 1px solid #e6ebf1;
  text-align: center;
  color: #6b7c93;
  font-size: 0.85rem;
}

.payment-notes p {
  margin: 0.5rem 0;
}

.payment-notes a {
  color: #1695A3;
  text-decoration: none;
}

.payment-notes a:hover {
  text-decoration: underline;
}
</style> 