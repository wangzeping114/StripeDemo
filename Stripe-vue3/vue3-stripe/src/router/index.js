import { createRouter, createWebHistory } from 'vue-router'
import PaymentSelection from '../components/PaymentSelection.vue'
import StripePayment from '../components/StripePayment.vue'
import JetonPayment from '../components/JetonPayment.vue'
import StripeWithdrawal from '../components/StripeWithdrawal.vue'
import ConnectOnboarding from '../components/ConnectOnboarding.vue'

const routes = [
  {
    path: '/',
    name: 'PaymentSelection',
    component: PaymentSelection
  },
  {
    path: '/stripe-payment',
    name: 'StripePayment',
    component: StripePayment
  },
  {
    path: '/jeton-payment',
    name: 'JetonPayment',
    component: JetonPayment
  },
  {
    path: '/withdrawal',
    name: 'StripeWithdrawal',
    component: StripeWithdrawal
  },
  {
    path: '/connect',
    name: 'ConnectOnboarding',
    component: ConnectOnboarding
  },
  {
    path: '/payment-success',
    name: 'PaymentSuccess',
    component: () => import('../components/PaymentSuccess.vue')
  },
  {
    // 将所有未匹配的路径重定向到支付选择页面
    path: '/:pathMatch(.*)*',
    redirect: '/'
  }
]

const router = createRouter({
  history: createWebHistory(),
  routes
})

export default router 