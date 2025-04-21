<script setup>
import { ref, onMounted } from 'vue';
import { loadStripe } from '@stripe/stripe-js';
import { useRoute } from 'vue-router';

const route = useRoute();
const isLoading = ref(false);
const paymentMessage = ref('');
const stripe = ref(null);
const elements = ref(null);
const paymentElement = ref(null); // 改用PaymentElement替代CardElement
const cardErrors = ref('');
const clientSecret = ref('');
const baseApiUrl = 'http://localhost:5062'; // 后端API基础地址

// 从URL查询参数获取订单信息（如果有）
const paymentAmount = ref(Number(route.query.amount) || 1999); // 金额，单位为分（19.99元）
const currency = ref(route.query.currency || 'usd'); // 币种
const orderId = ref(route.query.orderId || 'ORD' + Date.now()); // 生成订单ID

// 用户信息
const userId = ref('user123'); // 应从登录系统获取
const userName = ref('测试用户'); // 应从登录系统获取
const email = ref('test@example.com'); // 可从表单中获取
const orderDescription = ref('商品购买'); // 订单描述

onMounted(async () => {
  try {
    // 这里需要替换为您的Stripe公钥
    stripe.value = await loadStripe('pk_test_51RFEzAQ5NEyL6x8cxm2xroRwuRIAXivCgZvbZBApoKXnj9TsRW6Gp11irhMTLOnjawAocU4XZaemQc4PXwQYjuGO00uJV9koVc');
    
    // 从后端获取客户端密钥
    await createPaymentIntent();
    
    // 使用PaymentElement，它支持多种支付方式
    elements.value = stripe.value.elements({
      clientSecret: clientSecret.value,
      appearance: {
        theme: 'stripe',
        variables: {
          colorPrimary: '#5469d4',
        }
      },
      loader: 'auto'
    });
    
    // 创建并挂载PaymentElement，它会自动显示所有可用的支付方式
    paymentElement.value = elements.value.create('payment', {
      layout: {
        type: 'tabs',
        defaultCollapsed: false
      },
      paymentMethodOrder: ['card', 'apple_pay', 'google_pay', 'alipay']
    });
    
    paymentElement.value.mount('#payment-element');

    // 监听变化事件
    paymentElement.value.on('change', (event) => {
      cardErrors.value = event.error ? event.error.message : '';
    });
  } catch (error) {
    console.error('Stripe初始化错误:', error);
    paymentMessage.value = '支付初始化失败，请刷新页面重试';
  }
});

// 调用后端API创建支付意向
const createPaymentIntent = async () => {
  try {
    const response = await fetch(`${baseApiUrl}/api/Payment/create-payment-intent`, {
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
        email: email.value,
        payment_method_types: ['card', 'apple_pay', 'google_pay', 'alipay'] // 支持多种支付方式
      })
    });

    if (!response.ok) {
      throw new Error('服务器响应错误');
    }

    const data = await response.json();
    clientSecret.value = data.clientSecret;
  } catch (error) {
    console.error('创建支付意向失败:', error);
    // 测试模式下，如果API不可用，创建一个测试密钥
    if (process.env.NODE_ENV !== 'production') {
      console.warn('使用测试密钥...');
      clientSecret.value = 'test_secret_key_' + Date.now();
    } else {
      paymentMessage.value = '支付初始化失败，请刷新页面重试';
    }
  }
};

const handleSubmit = async () => {
  isLoading.value = true;
  paymentMessage.value = '';
  
  try {
    // 使用confirmPayment代替confirmCardPayment
    const { error, paymentIntent } = await stripe.value.confirmPayment({
      elements: elements.value,
      confirmParams: {
        return_url: window.location.origin + '/payment-success', // 支付成功后跳转页面
        payment_method_data: {
          billing_details: {
            name: userName.value,
            email: email.value
          }
        }
      },
      redirect: 'if_required' // 仅在需要时重定向（例如3DS验证）
    });

    if (error) {
      // 显示错误消息
      cardErrors.value = error.message;
    } else if (paymentIntent && paymentIntent.status === 'succeeded') {
      // 支付成功
      paymentMessage.value = '支付成功！交易ID: ' + paymentIntent.id;
      
      // 可以调用后端API更新订单状态
      await updateOrderStatus(paymentIntent);
    } else if (paymentIntent) {
      // 其他状态处理
      paymentMessage.value = `支付${paymentIntent.status === 'processing' ? '处理中' : paymentIntent.status}，请稍候...`;
    }
  } catch (error) {
    console.error('支付处理错误:', error);
    paymentMessage.value = '支付处理出错，请重试';
  } finally {
    isLoading.value = false;
  }
};

// 更新订单状态
const updateOrderStatus = async (paymentIntent) => {
  try {
    const response = await fetch(`${baseApiUrl}/api/Payment/update-order-status`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json'
      },
      body: JSON.stringify({
        paymentIntentId: paymentIntent.id,
        amount: paymentAmount.value,
        currency: currency.value,
        orderId: orderId.value,
        description: orderDescription.value,
        userId: userId.value,
        userName: userName.value,
        email: email.value,
        paymentStatus: paymentIntent.status,
        paymentMethod: paymentIntent.payment_method_types ? paymentIntent.payment_method_types[0] : 'card',
        transactionDate: new Date().toISOString(),
        cardLast4: paymentIntent.payment_method?.card?.last4 || ''
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
  <div class="stripe-payment">
    <h1>Stripe支付</h1>
    
    <div class="payment-form">
      <div class="amount-display">
        总计: {{ (paymentAmount / 100).toFixed(2) }} {{ currency.toUpperCase() }}
      </div>
      
      <div class="order-info">
        <p>订单编号: {{ orderId }}</p>
        <p>客户: {{ userName }}</p>
        <p>描述: {{ orderDescription }}</p>
      </div>
      
      <form id="payment-form" @submit.prevent="handleSubmit">
        <div class="payment-element-container">
          <div id="payment-element">
            <!-- Stripe PaymentElement 将在这里挂载 -->
          </div>
          <div class="card-errors" v-if="cardErrors">{{ cardErrors }}</div>
        </div>
        
        <button 
          type="submit"
          :disabled="isLoading || !clientSecret" 
          class="pay-button"
        >
          {{ isLoading ? '处理中...' : '支付' }}
        </button>
      </form>
      
      <div class="payment-message" v-if="paymentMessage">
        {{ paymentMessage }}
      </div>

      <div class="payment-methods-info">
        <p>支持的支付方式：</p>
        <ul>
          <li>信用卡/借记卡</li>
          <li>Apple Pay（在Safari浏览器和苹果设备上可用）</li>
          <li>Google Pay（在支持的浏览器和设备上可用）</li>
          <li>支付宝（适用于中国用户）</li>
        </ul>
      </div>
    </div>
  </div>
</template>

<style scoped>
.stripe-payment {
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

.payment-element-container {
  margin-bottom: 2rem;
}

#payment-element {
  margin-bottom: 1rem;
}

.card-errors {
  color: #fa755a;
  margin-top: 0.5rem;
  font-size: 0.9rem;
}

.pay-button {
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

.payment-methods-info {
  margin-top: 2rem;
  padding-top: 1rem;
  border-top: 1px solid #e6ebf1;
  font-size: 0.9rem;
  color: #6b7c93;
}

.payment-methods-info p {
  margin-bottom: 0.5rem;
}

.payment-methods-info ul {
  padding-left: 1.5rem;
}

.payment-methods-info li {
  margin-bottom: 0.3rem;
}
</style> 