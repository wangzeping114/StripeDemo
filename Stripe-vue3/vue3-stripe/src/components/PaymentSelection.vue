<script setup>
import { ref, onMounted } from 'vue';
import { useRouter } from 'vue-router';

const router = useRouter();
const paymentAmount = ref(1999); // 金额，单位为分（19.99元）
const currency = ref('usd'); // 币种
const orderId = ref('ORD' + Date.now()); // 生成订单ID

// 支付方式选择
const selectPaymentMethod = (method) => {
  if (method === 'jeton') {
    // 跳转到Jeton支付页面
    router.push({
      path: '/jeton-payment',
      query: {
        amount: paymentAmount.value,
        currency: currency.value,
        orderId: orderId.value
      }
    });
  } else if (method === 'connect') {
    // 跳转到Connect注册页面
    router.push('/connect');
  } else if (method === 'connect-payment') {
    // 跳转到Connect支付页面
    router.push('/connect-payment');
  }
};

// 检查是否已完成Connect注册
const hasConnectAccount = ref(false);

// 当组件挂载时，检查是否已有Connect账户ID
onMounted(() => {
  const savedAccountId = localStorage.getItem('stripe_connect_account_id');
  hasConnectAccount.value = !!savedAccountId;
});
</script>

<template>
  <div class="payment-selection">
    <h1>支付方式选择</h1>
    
    <div class="payment-container">
      <div class="amount-display">
        订单总计: {{ (paymentAmount / 100).toFixed(2) }} {{ currency.toUpperCase() }}
      </div>
      
      <div class="order-info">
        <p>订单编号: {{ orderId }}</p>
      </div>
      
      <div class="payment-options">
        <div class="payment-option" @click="selectPaymentMethod('jeton')">
          <div class="option-icon jeton-icon"></div>
          <div class="option-content">
            <h3>Jeton Wallet</h3>
            <p>使用Jeton电子钱包快速支付</p>
          </div>
        </div>
        
        <div class="payment-option" @click="selectPaymentMethod('connect-payment')">
          <div class="option-icon connect-payment-icon"></div>
          <div class="option-content">
            <h3>Stripe Connect</h3>
            <p>使用Stripe Connect进行支付和转账</p>
            <span v-if="!hasConnectAccount" class="badge warning">需要先注册</span>
          </div>
        </div>
        
        <div class="payment-option" @click="selectPaymentMethod('connect')">
          <div class="option-icon connect-icon"></div>
          <div class="option-content">
            <h3>注册商户账户</h3>
            <p>成为Stripe Connect平台商户</p>
            <span v-if="hasConnectAccount" class="badge success">已注册</span>
          </div>
        </div>
      </div>
      
    </div>
    
    <div class="payment-notes">
      <p>所有交易都是安全的，受到加密保护。</p>
      <p>如果您有任何问题，请联系客户支持。</p>
    </div>
  </div>
</template>

<style scoped>
.payment-selection {
  max-width: 800px;
  margin: 0 auto;
  padding: 2rem;
}

h1 {
  text-align: center;
  margin-bottom: 2rem;
  color: #32325d;
}

.payment-container {
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
  margin-bottom: 2rem;
  padding: 1rem;
  background: rgba(255, 255, 255, 0.5);
  border-radius: 4px;
  text-align: center;
}

.order-info p {
  margin: 0.3rem 0;
  font-size: 1rem;
  color: #525f7f;
}

.payment-options {
  display: flex;
  flex-direction: column;
  gap: 1.5rem;
  margin-bottom: 1.5rem;
}

.payment-option {
  display: flex;
  align-items: center;
  padding: 1.5rem;
  background: white;
  border-radius: 8px;
  border: 1px solid #e6ebf1;
  cursor: pointer;
  transition: all 0.2s ease;
}

.payment-option:hover {
  transform: translateY(-2px);
  box-shadow: 0 7px 14px rgba(50, 50, 93, 0.1), 0 3px 6px rgba(0, 0, 0, 0.08);
  border-color: #5469d4;
}

.option-icon {
  width: 60px;
  height: 60px;
  margin-right: 1.5rem;
  background-size: contain;
  background-position: center;
  background-repeat: no-repeat;
}

.jeton-icon {
  background-image: url('data:image/svg+xml;base64,PHN2ZyB3aWR0aD0iNjAiIGhlaWdodD0iNjAiIHZpZXdCb3g9IjAgMCA2MCA2MCIgZmlsbD0ibm9uZSIgeG1sbnM9Imh0dHA6Ly93d3cudzMub3JnLzIwMDAvc3ZnIj48Y2lyY2xlIGN4PSIzMCIgY3k9IjMwIiByPSIzMCIgZmlsbD0iIzE2OTVBMyIvPjxwYXRoIGQ9Ik0xNC4wNDY5IDI0LjQwODVIMTguMjg4MlYzNS41OTE1SDE0LjA0NjlWMjQuNDA4NVoiIGZpbGw9IndoaXRlIi8+PHBhdGggZD0iTTI2LjA4NTkgMjQuNDA4NUgzMC4zMjcyTDI2LjMwMDQgMzUuNTkxNUgyMi4wNTkyTDI2LjA4NTkgMjQuNDA4NVoiIGZpbGw9IndoaXRlIi8+PHBhdGggZD0iTTM4LjEyMjEgMjQuNDA4NUg0Mi4zNjM0TDM4LjMzNjUgMzUuNTkxNUgzNC4wOTUzTDM4LjEyMjEgMjQuNDA4NVoiIGZpbGw9IndoaXRlIi8+PHBhdGggZD0iTTQ2Ljc1MzEgMjQuNDA4NUg0NC4wNDY5TDQyLjc5NTcgMjguNzMyNEw0NC4wNDY5IDI0LjQwODVMNDYuNzUzMSAyNC40MDg1WiIgZmlsbD0id2hpdGUiLz48cGF0aCBkPSJNMTkuMzA2MSAyNC40MDg1SDI0LjU0MzlMMjIuNDY5OSAzMC43NTM1TDIwLjYxNjcgMzUuNTkxNUwxOS4zMDYxIDMxLjU3ODhWMjQuNDA4NVoiIGZpbGw9IndoaXRlIi8+PHBhdGggZD0iTTMxLjM0NTEgMjQuNDA4NUgzNi41ODMxTDM0LjUwODkgMzAuNzUzNUwzMi42NTU5IDM1LjU5MTVMMzEuMzQ1MSAzMS41Nzg4VjI0LjQwODVaIiBmaWxsPSJ3aGl0ZSIvPjwvc3ZnPg==');
}

.connect-payment-icon {
  background-image: url('data:image/svg+xml;base64,PHN2ZyB3aWR0aD0iNjAiIGhlaWdodD0iNjAiIHZpZXdCb3g9IjAgMCA2MCA2MCIgZmlsbD0ibm9uZSIgeG1sbnM9Imh0dHA6Ly93d3cudzMub3JnLzIwMDAvc3ZnIj48Y2lyY2xlIGN4PSIzMCIgY3k9IjMwIiByPSIzMCIgZmlsbD0iIzZhNzBiZSIvPjxwYXRoIGQ9Ik0yMCAxOUg0MEw0NSAzMEw0MCA0MUgyMEwxNSAzMEwyMCAxOVoiIHN0cm9rZT0id2hpdGUiIHN0cm9rZS13aWR0aD0iMiIgZmlsbD0ibm9uZSIvPjxwYXRoIGQ9Ik0yNSAyOUMyNiAyOS41IDI2IDMwLjUgMjUgMzFNMzUgMjlDMzQgMjkuNSAzNCAzMC41IDM1IDMxIiBzdHJva2U9IndoaXRlIiBzdHJva2Utd2lkdGg9IjIiIHN0cm9rZS1saW5lY2FwPSJyb3VuZCIvPjxwYXRoIGQ9Ik0zMCAzNVYyNiIgc3Ryb2tlPSJ3aGl0ZSIgc3Ryb2tlLXdpZHRoPSIyIiBzdHJva2UtbGluZWNhcD0icm91bmQiLz48cGF0aCBkPSJNMzAgMjZMMjcgMjNIMzNMMzAgMjZaIiBmaWxsPSJ3aGl0ZSIvPjxjaXJjbGUgY3g9IjMwIiBjeT0iMzgiIHI9IjMiIGZpbGw9IndoaXRlIi8+PC9zdmc+');
}

.connect-icon {
  background-image: url('data:image/svg+xml;base64,PHN2ZyB3aWR0aD0iNjAiIGhlaWdodD0iNjAiIHZpZXdCb3g9IjAgMCA2MCA2MCIgZmlsbD0ibm9uZSIgeG1sbnM9Imh0dHA6Ly93d3cudzMub3JnLzIwMDAvc3ZnIj48Y2lyY2xlIGN4PSIzMCIgY3k9IjMwIiByPSIzMCIgZmlsbD0iIzZhNzBiZSIvPjxwYXRoIGQ9Ik0yMCAxOUg0MEw0NSAzMEw0MCA0MUgyMEwxNSAzMEwyMCAxOVoiIHN0cm9rZT0id2hpdGUiIHN0cm9rZS13aWR0aD0iMiIgZmlsbD0ibm9uZSIvPjxwYXRoIGQ9Ik0zMCAxOVY0MSIgc3Ryb2tlPSJ3aGl0ZSIgc3Ryb2tlLXdpZHRoPSIyIi8+PHBhdGggZD0iTTE4IDMwSDQyIiBzdHJva2U9IndoaXRlIiBzdHJva2Utd2lkdGg9IjIiLz48L3N2Zz4=');
}

.option-content h3 {
  margin: 0 0 0.5rem 0;
  color: #32325d;
}

.option-content p {
  margin: 0;
  color: #6b7c93;
  font-size: 0.9rem;
}

.badge {
  display: inline-block;
  padding: 0.25rem 0.5rem;
  margin-top: 0.5rem;
  border-radius: 50px;
  font-size: 0.7rem;
  font-weight: 600;
}

.warning {
  background-color: #fff8e1;
  color: #b36d00;
}

.success {
  background-color: #efffe5;
  color: #3c8505;
}

.payment-notes {
  margin-top: 2rem;
  text-align: center;
  font-size: 0.9rem;
  color: #6b7c93;
}
</style> 