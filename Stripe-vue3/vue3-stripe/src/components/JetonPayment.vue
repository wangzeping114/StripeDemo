<script setup>
import { ref, onMounted } from 'vue';
import { useRoute, useRouter } from 'vue-router';

const route = useRoute();
const router = useRouter();
const isLoading = ref(false);
const paymentMessage = ref('');
const paymentSuccess = ref(false);
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
    console.log('初始化Jeton支付...');
    // 实际项目中应调用后端API进行初始化
    // 这里模拟初始化过程
    await new Promise(resolve => setTimeout(resolve, 500));
    console.log('Jeton支付初始化成功');
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
    // 模拟支付处理
    await new Promise(resolve => setTimeout(resolve, 1500));
    
    // 假设支付成功
    const transactionId = 'JTN' + Date.now();
    paymentMessage.value = '支付成功！交易ID: ' + transactionId;
    paymentSuccess.value = true;
    
    // 更新订单状态
    await updateOrderStatus(transactionId);
    
    // 3秒后跳转到成功页面
    setTimeout(() => {
      router.push({
        path: '/payment-success',
        query: {
          amount: paymentAmount.value,
          currency: currency.value,
          orderId: orderId.value,
          transactionId: transactionId,
          paymentMethod: 'jeton_wallet'
        }
      });
    }, 3000);
  } catch (error) {
    console.error('支付处理错误:', error);
    paymentMessage.value = '支付处理出错，请重试';
  } finally {
    isLoading.value = false;
  }
};

// 更新订单状态
const updateOrderStatus = async (transactionId) => {
  console.log('更新订单状态:', {
    transactionId,
    orderId: orderId.value,
    paymentStatus: 'succeeded'
  });
  // 实际项目中应调用后端API更新订单状态
};

// 返回首页
const goBack = () => {
  router.push('/');
};
</script>

<template>
  <div class="jeton-payment">
    <h1>Jeton Wallet支付</h1>
    
    <div class="payment-form">
      <div class="amount-display">
        应支付: <span class="amount">{{ (paymentAmount / 100).toFixed(2) }} {{ currency.toUpperCase() }}</span>
      </div>
      
      <div class="order-info">
        <p><strong>订单编号:</strong> {{ orderId }}</p>
        <p><strong>客户:</strong> {{ userName }}</p>
        <p><strong>描述:</strong> {{ orderDescription }}</p>
      </div>
      
      <div class="jeton-logo">
        <div class="logo-image"></div>
        <h3>通过Jeton Wallet支付</h3>
      </div>
      
      <div v-if="!paymentSuccess" class="form-container">
        <form id="jeton-form" @submit.prevent="handleSubmit">
          <div class="form-group">
            <label for="wallet-id">Jeton钱包ID</label>
            <input 
              type="text" 
              id="wallet-id" 
              v-model="walletId" 
              placeholder="请输入您的Jeton钱包ID" 
              :disabled="isLoading"
              required
            />
            <small class="form-hint">您可以在Jeton账户中心找到您的钱包ID</small>
          </div>
          
          <div class="form-actions">
            <button 
              type="button"
              @click="goBack" 
              :disabled="isLoading" 
              class="back-button"
            >
              返回
            </button>
            
            <button 
              type="submit"
              :disabled="isLoading" 
              class="pay-button"
            >
              <span v-if="isLoading" class="spinner"></span>
              {{ isLoading ? '处理中...' : '确认支付' }}
            </button>
          </div>
        </form>
      </div>
      
      <div 
        class="payment-message" 
        v-if="paymentMessage"
        :class="{ 'success': paymentSuccess, 'error': !paymentSuccess && paymentMessage }"
      >
        {{ paymentMessage }}
      </div>
      
      <div class="payment-notes">
        <h4>Jeton Wallet支付</h4>
        <p>安全快捷，全球通用的电子钱包支付方式。</p>
        <p>如果您没有Jeton钱包，<a href="https://www.jeton.com" target="_blank" rel="noopener noreferrer">点击此处注册</a>。</p>
        <div class="security-badges">
          <div class="security-badge ssl"></div>
          <div class="security-badge secure"></div>
        </div>
      </div>
    </div>
  </div>
</template>

<style scoped>
.jeton-payment {
  max-width: 600px;
  margin: 0 auto;
  padding: 2rem 1rem;
}

h1 {
  text-align: center;
  margin-bottom: 2rem;
  color: #1695A3;
}

.payment-form {
  background: #f8f9fa;
  border-radius: 12px;
  padding: 2rem;
  box-shadow: 0 4px 6px rgba(50, 50, 93, 0.11), 0 1px 3px rgba(0, 0, 0, 0.08);
}

.amount-display {
  font-size: 1.25rem;
  text-align: center;
  margin-bottom: 1.5rem;
  color: #525f7f;
}

.amount {
  font-size: 1.5rem;
  font-weight: bold;
  color: #1695A3;
}

.order-info {
  margin-bottom: 2rem;
  padding: 1rem;
  background: rgba(22, 149, 163, 0.05);
  border-radius: 8px;
  border-left: 4px solid #1695A3;
}

.order-info p {
  margin: 0.5rem 0;
  font-size: 0.95rem;
  color: #525f7f;
}

.jeton-logo {
  text-align: center;
  margin-bottom: 2rem;
}

.jeton-logo h3 {
  margin-top: 1rem;
  color: #1695A3;
  font-size: 1.2rem;
}

.logo-image {
  width: 100px;
  height: 100px;
  margin: 0 auto;
  background-image: url('data:image/svg+xml;base64,PHN2ZyB3aWR0aD0iMTAwIiBoZWlnaHQ9IjEwMCIgdmlld0JveD0iMCAwIDEwMCAxMDAiIGZpbGw9Im5vbmUiIHhtbG5zPSJodHRwOi8vd3d3LnczLm9yZy8yMDAwL3N2ZyI+PGNpcmNsZSBjeD0iNTAiIGN5PSI1MCIgcj0iNTAiIGZpbGw9IiMxNjk1QTMiLz48cGF0aCBkPSJNMjMuNDExNSA0MC42ODA4SDMwLjQ4MDRWNTkuMzE5MkgyMy40MTE1VjQwLjY4MDhaIiBmaWxsPSJ3aGl0ZSIvPjxwYXRoIGQ9Ik00My40NzY1IDQwLjY4MDhINTAuNTQ1NEw0My44MzQgNTkuMzE5MkgzNi43NjUyTDQzLjQ3NjUgNDAuNjgwOFoiIGZpbGw9IndoaXRlIi8+PHBhdGggZD0iTTYzLjUzNjkgNDAuNjgwOEg3MC42MDU3TDYzLjg5NDMgNTkuMzE5Mkg1Ni44MjU0TDYzLjUzNjkgNDAuNjgwOFoiIGZpbGw9IndoaXRlIi8+PHBhdGggZD0iTTc3LjkyMTggNDAuNjgwOEg3My40MTE1TDcxLjMyNjEgNDcuODg3M0w3My40MTE1IDQwLjY4MDhINzcuOTIxOFoiIGZpbGw9IndoaXRlIi8+PHBhdGggZD0iTTMyLjE3NjggNDAuNjgwOEg0MC45MDY1TDM3LjQ0OTkgNTEuMjU1OEwzNC4zNjEyIDU5LjMxOTJMMzIuMTc2OCA1Mi42MzE0VjQwLjY4MDhaIiBmaWxsPSJ3aGl0ZSIvPjxwYXRoIGQ9Ik01Mi4yNDE4IDQwLjY4MDhINjAuOTcxOUw1Ny41MTQ5IDUxLjI1NThMNTQuNDI2NiA1OS4zMTkyTDUyLjI0MTggNTIuNjMxNFY0MC42ODA4WiIgZmlsbD0id2hpdGUiLz48L3N2Zz4=');
  background-size: contain;
  background-position: center;
  background-repeat: no-repeat;
}

.form-container {
  margin-bottom: 1.5rem;
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

.form-group input {
  width: 100%;
  padding: 0.75rem;
  border: 1px solid #e6ebf1;
  border-radius: 4px;
  font-size: 1rem;
  transition: border-color 0.2s ease;
}

.form-group input:focus {
  outline: none;
  border-color: #1695A3;
  box-shadow: 0 0 0 1px #1695A3;
}

.form-hint {
  display: block;
  margin-top: 0.5rem;
  color: #6b7c93;
  font-size: 0.8rem;
}

.form-actions {
  display: flex;
  gap: 1rem;
}

.back-button {
  flex: 1;
  background: transparent;
  color: #6b7c93;
  border: 1px solid #e6ebf1;
  border-radius: 4px;
  padding: 0.75rem 1rem;
  font-size: 0.9rem;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.2s ease;
}

.back-button:hover {
  background: #f7fafc;
  color: #1695A3;
  border-color: #1695A3;
}

.pay-button {
  flex: 2;
  background: #1695A3;
  color: white;
  border-radius: 4px;
  border: 0;
  padding: 0.75rem 1rem;
  font-size: 0.9rem;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.2s ease;
  display: flex;
  align-items: center;
  justify-content: center;
}

.pay-button:hover {
  filter: brightness(1.1);
}

.pay-button:disabled {
  opacity: 0.7;
  cursor: default;
}

.spinner {
  display: inline-block;
  width: 1rem;
  height: 1rem;
  margin-right: 0.5rem;
  border: 2px solid rgba(255, 255, 255, 0.3);
  border-radius: 50%;
  border-top-color: white;
  animation: spin 1s ease-in-out infinite;
}

@keyframes spin {
  to { transform: rotate(360deg); }
}

.payment-message {
  padding: 1rem;
  margin-bottom: 1.5rem;
  border-radius: 4px;
  text-align: center;
  font-weight: 500;
}

.payment-message.success {
  background-color: #efffd2;
  color: #3c8505;
}

.payment-message.error {
  background-color: #fff0f0;
  color: #e42222;
}

.payment-notes {
  margin-top: 2rem;
  padding: 1.5rem;
  background: white;
  border-radius: 8px;
  border: 1px solid #e6ebf1;
}

.payment-notes h4 {
  color: #1695A3;
  margin-top: 0;
  margin-bottom: 1rem;
  text-align: center;
}

.payment-notes p {
  margin: 0.5rem 0;
  color: #6b7c93;
  font-size: 0.9rem;
  text-align: center;
}

.payment-notes a {
  color: #1695A3;
  text-decoration: none;
  font-weight: 500;
}

.payment-notes a:hover {
  text-decoration: underline;
}

.security-badges {
  display: flex;
  justify-content: center;
  gap: 1rem;
  margin-top: 1rem;
}

.security-badge {
  width: 40px;
  height: 40px;
  background-size: contain;
  background-position: center;
  background-repeat: no-repeat;
}

.security-badge.ssl {
  background-image: url('data:image/svg+xml;base64,PHN2ZyB3aWR0aD0iNDAiIGhlaWdodD0iNDAiIHZpZXdCb3g9IjAgMCA0MCA0MCIgZmlsbD0ibm9uZSIgeG1sbnM9Imh0dHA6Ly93d3cudzMub3JnLzIwMDAvc3ZnIj48Y2lyY2xlIGN4PSIyMCIgY3k9IjIwIiByPSIyMCIgZmlsbD0iI2VmZmZkMiIvPjxwYXRoIGQ9Ik0yMCAxMkMxNy44IDEyIDE2IDE0IDE2IDE2VjE4SDI0VjE2QzI0IDE0IDIyLjIgMTIgMjAgMTJaTTE0IDE4VjE2QzE0IDEzIDE2LjggMTAgMjAgMTBDMjMuMiAxMCAyNiAxMyAyNiAxNlYxOEgyN0MyOCAxOCAyOSAxOSAyOSAyMFYyOEMyOSAyOSAyOCAzMCAyNyAzMEgxM0MxMiAzMCAxMSAyOSAxMSAyOFYyMEMxMSAxOSAxMiAxOCAxMyAxOEgxNFoiIGZpbGw9IiMzYzg1MDUiLz48L3N2Zz4=');
}

.security-badge.secure {
  background-image: url('data:image/svg+xml;base64,PHN2ZyB3aWR0aD0iNDAiIGhlaWdodD0iNDAiIHZpZXdCb3g9IjAgMCA0MCA0MCIgZmlsbD0ibm9uZSIgeG1sbnM9Imh0dHA6Ly93d3cudzMub3JnLzIwMDAvc3ZnIj48Y2lyY2xlIGN4PSIyMCIgY3k9IjIwIiByPSIyMCIgZmlsbD0iI2VmZmZkMiIvPjxwYXRoIGQ9Ik0yMCAxMEMxNC41IDEwIDEwIDE0LjUgMTAgMjBDMTAgMjUuNSAxNC41IDMwIDIwIDMwQzI1LjUgMzAgMzAgMjUuNSAzMCAyMEMzMCAxNC41IDI1LjUgMTAgMjAgMTBaTTIwIDI4QzE1LjYgMjggMTIgMjQuNSAxMiAyMEMxMiAxNS41IDE1LjYgMTIgMjAgMTJDMjQuNCAxMiAyOCAxNS41IDI4IDIwQzI4IDI0LjUgMjQuNCAyOCAyMCAyOFpNMjQgMTlMMTkgMjRMMTYgMjFMMTcuNSAxOS41TDE5IDIxTDIyLjUgMTcuNUwyNCAxOVoiIGZpbGw9IiMzYzg1MDUiLz48L3N2Zz4=');
}
</style> 