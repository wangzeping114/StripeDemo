<template>
  <div class="payment-container">
    <div class="payment-card">
      <h2>支付</h2>
      <div class="payment-form">
        <div class="form-group">
          <label>金额 (USD)</label>
          <input type="number" v-model="amount" min="1" step="0.01" />
        </div>
        <div id="card-element"></div>
        <div id="card-errors" role="alert"></div>
        <button @click="handlePayment" :disabled="processing">
          {{ processing ? '处理中...' : '确认支付' }}
        </button>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { loadStripe } from '@stripe/stripe-js'

const stripe = await loadStripe('your_publishable_key')
const elements = ref(null)
const amount = ref(10)
const processing = ref(false)

onMounted(() => {
  elements.value = stripe.elements()
  const card = elements.value.create('card')
  card.mount('#card-element')

  card.addEventListener('change', (event) => {
    const displayError = document.getElementById('card-errors')
    if (event.error) {
      displayError.textContent = event.error.message
    } else {
      displayError.textContent = ''
    }
  })
})

const handlePayment = async () => {
  processing.value = true
  try {
    const { paymentMethod, error } = await stripe.createPaymentMethod({
      type: 'card',
      card: elements.value.getElement('card'),
    })

    if (error) {
      const errorElement = document.getElementById('card-errors')
      errorElement.textContent = error.message
      processing.value = false
      return
    }

    // 这里需要调用您的后端API来处理支付
    // const response = await fetch('/api/charge', {
    //   method: 'POST',
    //   headers: { 'Content-Type': 'application/json' },
    //   body: JSON.stringify({
    //     paymentMethodId: paymentMethod.id,
    //     amount: amount.value * 100,
    //   }),
    // })

    // const result = await response.json()
    // if (result.success) {
    //   alert('支付成功！')
    // }
  } catch (error) {
    console.error('支付失败:', error)
    const errorElement = document.getElementById('card-errors')
    errorElement.textContent = '支付过程中发生错误，请重试。'
  }
  processing.value = false
}
</script>

<style scoped>
.payment-container {
  min-height: 100vh;
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 20px;
}

.payment-card {
  background: white;
  border-radius: 8px;
  box-shadow: 0 2px 10px rgba(0, 0, 0, 0.1);
  padding: 30px;
  width: 100%;
  max-width: 400px;
}

h2 {
  margin: 0 0 20px;
  color: #333;
  text-align: center;
}

.payment-form {
  display: flex;
  flex-direction: column;
  gap: 20px;
}

.form-group {
  display: flex;
  flex-direction: column;
  gap: 8px;
}

label {
  color: #666;
  font-size: 14px;
}

input {
  padding: 10px;
  border: 1px solid #ddd;
  border-radius: 4px;
  font-size: 16px;
}

#card-element {
  padding: 12px;
  border: 1px solid #ddd;
  border-radius: 4px;
  background: white;
}

#card-errors {
  color: #dc3545;
  font-size: 14px;
  min-height: 20px;
}

button {
  background-color: #4CAF50;
  color: white;
  border: none;
  border-radius: 4px;
  padding: 12px;
  font-size: 16px;
  cursor: pointer;
  transition: background-color 0.2s;
}

button:hover:not(:disabled) {
  background-color: #45a049;
}

button:disabled {
  background-color: #cccccc;
  cursor: not-allowed;
}
</style> 