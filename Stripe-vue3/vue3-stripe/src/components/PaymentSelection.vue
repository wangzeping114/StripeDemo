<script setup>
import { ref } from 'vue';
import { useRouter } from 'vue-router';

const router = useRouter();
const paymentAmount = ref(1999); // 金额，单位为分（19.99元）
const currency = ref('usd'); // 币种
const orderId = ref('ORD' + Date.now()); // 生成订单ID

// 支付方式选择
const selectPaymentMethod = (method) => {
  if (method === 'stripe') {
    // 使用URL参数传递订单信息
    router.push({
      path: '/stripe-payment',
      query: {
        amount: paymentAmount.value,
        currency: currency.value,
        orderId: orderId.value
      }
    });
  } else if (method === 'jeton') {
    // 跳转到Jeton支付页面
    router.push({
      path: '/jeton-payment',
      query: {
        amount: paymentAmount.value,
        currency: currency.value,
        orderId: orderId.value
      }
    });
  } else if (method === 'withdrawal') {
    // 跳转到提现页面
    router.push('/withdrawal');
  } else if (method === 'connect') {
    // 跳转到Connect注册页面
    router.push('/connect');
  }
};
</script>

<template>
  <div class="payment-selection">
    <h1>支付与提现</h1>
    
    <div class="payment-container">
      <div class="amount-display">
        订单总计: {{ (paymentAmount / 100).toFixed(2) }} {{ currency.toUpperCase() }}
      </div>
      
      <div class="order-info">
        <p>订单编号: {{ orderId }}</p>
      </div>
      
      <div class="payment-options">
        <div class="payment-option" @click="selectPaymentMethod('stripe')">
          <div class="option-icon stripe-icon"></div>
          <div class="option-content">
            <h3>Stripe支付</h3>
            <p>使用信用卡、Apple Pay、Google Pay或支付宝等方式支付</p>
          </div>
        </div>
        
        <div class="payment-option" @click="selectPaymentMethod('jeton')">
          <div class="option-icon jeton-icon"></div>
          <div class="option-content">
            <h3>Jeton Wallet</h3>
            <p>使用Jeton电子钱包快速支付</p>
          </div>
        </div>
        
        <div class="payment-option" @click="selectPaymentMethod('withdrawal')">
          <div class="option-icon withdrawal-icon"></div>
          <div class="option-content">
            <h3>账户提现</h3>
            <p>将账户余额提现至您的银行账户</p>
          </div>
        </div>
        
        <div class="payment-option" @click="selectPaymentMethod('connect')">
          <div class="option-icon connect-icon"></div>
          <div class="option-content">
            <h3>成为商户</h3>
            <p>注册为Stripe Connect平台商户</p>
          </div>
        </div>
      </div>
      
      <div class="withdrawal-section">
        <div class="section-divider">
          <span>或者</span>
        </div>
        
        <div class="withdrawal-option" @click="selectPaymentMethod('withdrawal')">
          <div class="option-icon withdrawal-icon"></div>
          <div class="option-content">
            <h3>账户提现</h3>
            <p>将账户余额提现至您的银行账户</p>
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

.stripe-icon {
  background-image: url('data:image/svg+xml;base64,PHN2ZyB3aWR0aD0iNjAiIGhlaWdodD0iNjAiIHZpZXdCb3g9IjAgMCA2MCA2MCIgZmlsbD0ibm9uZSIgeG1sbnM9Imh0dHA6Ly93d3cudzMub3JnLzIwMDAvc3ZnIj48cGF0aCBkPSJNMzAgNjBDNDYuNTY4NSA2MCA2MCA0Ni41Njg1IDYwIDMwQzYwIDEzLjQzMTUgNDYuNTY4NSAwIDMwIDBDMTMuNDMxNSAwIDAgMTMuNDMxNSAwIDMwQzAgNDYuNTY4NSAxMy40MzE1IDYwIDMwIDYwWiIgZmlsbD0id2hpdGUiLz48cGF0aCBkPSJNMjYuOTUzMSAyNi4yNUMyNi45NTMxIDI0LjU1MTMgMjguMzI1IDE0LjkyNSAzNy45NTMxIDI0LjU1MTNDMzcuOTUzMSAyNi4yNSAzNi41ODEyIDM1Ljg3NjMgMjYuOTUzMSAyNi4yNVoiIGZpbGw9IiM2NzU2RkYiLz48cGF0aCBkPSJNMzQuMDc4MSAzMi44OTg0QzM0LjA3ODEgMzIuMDIzNCAzNC44MjgxIDMwLjg5ODQgMzcuOTUzMSAzMC44OTg0QzQxLjA3ODEgMzAuODk4NCA0My41NzgxIDMyLjY0ODQgNDMuNTc4MSAzMi42NDg0TDQyLjcwMzEgMzYuNjQ4NEM0Mi43MDMxIDM2LjY0ODQgNDAuMjAzMSAzNC44OTg0IDM2LjcwMzEgMzQuODk4NEMzNC4yMDMxIDM0Ljg5ODQgMzQuMDc4MSAzNi4xNDg0IDM0LjA3ODEgMzYuMTQ4NEwzNC4wNzgxIDMyLjg5ODRaIiBmaWxsPSIjNjc1NkZGIi8+PHBhdGggZD0iTTIxLjk1MzEgMjAuMzk4NEMyMS45NTMxIDIwLjM5ODQgMTkuNDUzMSAyMy4xNDg0IDE5LjQ1MzEgMjYuMjQ4NEMxOS40NTMxIDI5LjM0ODQgMjEuMjAzMSAzMS43MjM0IDI0LjQ1MzEgMzIuNDczNEwyNi4yMDMxIDI3LjIyMzRDMjYuMjAzMSAyNy4yMjM0IDI0LjgyODEgMjYuMzQ4NCAyNC44MjgxIDI0LjQ5ODRDMjQuODI4MSAyMi42NDg0IDI1LjgyODEgMjEuMTQ4NCAyNS44MjgxIDIxLjE0ODRMMjEuOTUzMSAyMC4zOTg0WiIgZmlsbD0iIzY3NTZGRiIvPjxwYXRoIGQ9Ik0yNS44MjgxIDM0LjcyMzRDMjUuODI4MSAzNC43MjM0IDI0LjU3ODEgMzUuODQ4NCAyMS40NTMxIDM1Ljg0ODRDMTguMzI4MSAzNS44NDg0IDE1LjgyODEgMzQuMDk4NCAxNS44MjgxIDM0LjA5ODRMMTYuNzAzMSAzMC4wOTg0QzE2LjcwMzEgMzAuMDk4NCAxOS4yMDMxIDMxLjg0ODQgMjIuNzAzMSAzMS44NDg0QzI1LjIwMzEgMzEuODQ4NCAyNS4zMjgxIDMwLjU5ODQgMjUuMzI4MSAzMC41OTg0TDI1LjgyODEgMzQuNzIzNFoiIGZpbGw9IiM2NzU2RkYiLz48cGF0aCBkPSJNMzEuODI4MSAyNi4yNUMzMS44MjgxIDI3Ljk0ODcgMzAuNDU2MiAzNy41NzUgMjAuODI4MSAyNy45NDg3QzIwLjgyODEgMjYuMjUgMjIuMiAxNi42MjM3IDMxLjgyODEgMjYuMjVaIiBmaWxsPSIjNjc1NkZGIi8+PHBhdGggZD0iTTM3Ljk1MzEgMzkuODQ4NEMzNy45NTMxIDM5Ljg0ODQgNDAuNDUzMSAzNy4wOTg0IDQwLjQ1MzEgMzMuOTk4NEMzNy4zMjgxIDMzLjg3MzQgMzUuNzAzMSAzNi42MjM0IDM1LjcwMzEgMzYuNjIzNEwzNy45NTMxIDM5Ljg0ODRaIiBmaWxsPSIjNjc1NkZGIi8+PC9zdmc+');
}

.jeton-icon {
  background-image: url('data:image/svg+xml;base64,PHN2ZyB3aWR0aD0iNjAiIGhlaWdodD0iNjAiIHZpZXdCb3g9IjAgMCA2MCA2MCIgZmlsbD0ibm9uZSIgeG1sbnM9Imh0dHA6Ly93d3cudzMub3JnLzIwMDAvc3ZnIj48Y2lyY2xlIGN4PSIzMCIgY3k9IjMwIiByPSIzMCIgZmlsbD0iIzE2OTVBMyIvPjxwYXRoIGQ9Ik0xNC4wNDY5IDI0LjQwODVIMTguMjg4MlYzNS41OTE1SDE0LjA0NjlWMjQuNDA4NVoiIGZpbGw9IndoaXRlIi8+PHBhdGggZD0iTTI2LjA4NTkgMjQuNDA4NUgzMC4zMjcyTDI2LjMwMDQgMzUuNTkxNUgyMi4wNTkyTDI2LjA4NTkgMjQuNDA4NVoiIGZpbGw9IndoaXRlIi8+PHBhdGggZD0iTTM4LjEyMjEgMjQuNDA4NUg0Mi4zNjM0TDM4LjMzNjUgMzUuNTkxNUgzNC4wOTUzTDM4LjEyMjEgMjQuNDA4NVoiIGZpbGw9IndoaXRlIi8+PHBhdGggZD0iTTQ2Ljc1MzEgMjQuNDA4NUg0NC4wNDY5TDQyLjc5NTcgMjguNzMyNEw0NC4wNDY5IDI0LjQwODVMNDYuNzUzMSAyNC40MDg1WiIgZmlsbD0id2hpdGUiLz48cGF0aCBkPSJNMTkuMzA2MSAyNC40MDg1SDI0LjU0MzlMMjIuNDY5OSAzMC43NTM1TDIwLjYxNjcgMzUuNTkxNUwxOS4zMDYxIDMxLjU3ODhWMjQuNDA4NVoiIGZpbGw9IndoaXRlIi8+PHBhdGggZD0iTTMxLjM0NTEgMjQuNDA4NUgzNi41ODMxTDM0LjUwODkgMzAuNzUzNUwzMi42NTU5IDM1LjU5MTVMMzEuMzQ1MSAzMS41Nzg4VjI0LjQwODVaIiBmaWxsPSJ3aGl0ZSIvPjwvc3ZnPg==');
}

.withdrawal-icon {
  background-image: url('data:image/svg+xml;base64,PHN2ZyB3aWR0aD0iNjAiIGhlaWdodD0iNjAiIHZpZXdCb3g9IjAgMCA2MCA2MCIgZmlsbD0ibm9uZSIgeG1sbnM9Imh0dHA6Ly93d3cudzMub3JnLzIwMDAvc3ZnIj48Y2lyY2xlIGN4PSIzMCIgY3k9IjMwIiByPSIzMCIgZmlsbD0iIzQzYTA0NyIvPjxwYXRoIGQ9Ik0zMCAyMFYzNiIgc3Ryb2tlPSJ3aGl0ZSIgc3Ryb2tlLXdpZHRoPSI0IiBzdHJva2UtbGluZWNhcD0icm91bmQiLz48cGF0aCBkPSJNMjAgMzZIMzUiIHN0cm9rZT0id2hpdGUiIHN0cm9rZS13aWR0aD0iNCIgc3Ryb2tlLWxpbmVjYXA9InJvdW5kIi8+PHBhdGggZD0iTTIwIDQ1SDQwIiBzdHJva2U9IndoaXRlIiBzdHJva2Utd2lkdGg9IjQiIHN0cm9rZS1saW5lY2FwPSJyb3VuZCIvPjxwYXRoIGQ9Ik0yNSAzMUwzMCAzNkwzNSAzMSIgc3Ryb2tlPSJ3aGl0ZSIgc3Ryb2tlLXdpZHRoPSI0IiBzdHJva2UtbGluZWNhcD0icm91bmQiIHN0cm9rZS1saW5lam9pbj0icm91bmQiLz48L3N2Zz4=');
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

.withdrawal-section {
  margin-top: 2rem;
}

.section-divider {
  display: flex;
  align-items: center;
  text-align: center;
  margin-bottom: 1.5rem;
}

.section-divider::before,
.section-divider::after {
  content: '';
  flex: 1;
  border-bottom: 1px solid #e6ebf1;
}

.section-divider span {
  padding: 0 1rem;
  color: #6b7c93;
  font-size: 0.9rem;
}

.withdrawal-option {
  display: flex;
  align-items: center;
  padding: 1.5rem;
  background: rgba(67, 160, 71, 0.05);
  border-radius: 8px;
  border: 1px solid #e6ebf1;
  cursor: pointer;
  transition: all 0.2s ease;
}

.withdrawal-option:hover {
  transform: translateY(-2px);
  box-shadow: 0 7px 14px rgba(50, 50, 93, 0.1), 0 3px 6px rgba(0, 0, 0, 0.08);
  border-color: #43a047;
}

.withdrawal-option .option-content h3 {
  color: #43a047;
}

.payment-notes {
  margin-top: 2rem;
  text-align: center;
  color: #6b7c93;
  font-size: 0.85rem;
}

.payment-notes p {
  margin: 0.5rem 0;
}

@media (min-width: 768px) {
  .payment-options {
    flex-direction: row;
  }
  
  .payment-option {
    flex: 1;
  }
}
</style> 