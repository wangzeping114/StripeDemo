<script setup>
import { ref, onMounted } from 'vue';
import { useRoute, useRouter } from 'vue-router';

const route = useRoute();
const router = useRouter();

const transactionId = ref(route.query.payment_intent || '未知');
const paymentStatus = ref(route.query.payment_status || 'succeeded');
const orderId = ref(route.query.order_id || '未知');

const goToHome = () => {
  router.push('/');
};
</script>

<template>
  <div class="payment-success">
    <div class="success-container">
      <div class="success-icon">✓</div>
      <h1>支付成功</h1>
      
      <div class="payment-details">
        <p>您的支付已成功处理。</p>
        <p><strong>订单编号:</strong> {{ orderId }}</p>
        <p><strong>交易ID:</strong> {{ transactionId }}</p>
        <p><strong>支付状态:</strong> {{ paymentStatus }}</p>
      </div>
      
      <div class="action-buttons">
        <button @click="goToHome" class="home-button">返回首页</button>
      </div>
    </div>
  </div>
</template>

<style scoped>
.payment-success {
  max-width: 600px;
  margin: 0 auto;
  padding: 2rem;
}

.success-container {
  background: #f8f9fa;
  border-radius: 8px;
  padding: 2rem;
  box-shadow: 0 4px 6px rgba(50, 50, 93, 0.11), 0 1px 3px rgba(0, 0, 0, 0.08);
  text-align: center;
}

.success-icon {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 80px;
  height: 80px;
  margin: 0 auto 1.5rem;
  background-color: #43a047;
  color: white;
  font-size: 40px;
  border-radius: 50%;
}

h1 {
  margin-bottom: 2rem;
  color: #32325d;
}

.payment-details {
  margin-bottom: 2rem;
  line-height: 1.6;
}

.payment-details p {
  margin: 0.5rem 0;
  color: #525f7f;
}

.action-buttons {
  margin-top: 2rem;
}

.home-button {
  background: #5469d4;
  color: white;
  border-radius: 4px;
  border: 0;
  padding: 12px 24px;
  font-size: 1rem;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.2s ease;
}

.home-button:hover {
  filter: brightness(1.1);
}
</style> 