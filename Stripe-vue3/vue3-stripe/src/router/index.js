import { createRouter, createWebHistory } from 'vue-router'
import PaymentSelection from '../components/PaymentSelection.vue'
import JetonPayment from '../components/JetonPayment.vue'
import ConnectOnboarding from '../components/ConnectOnboarding.vue'
import ConnectPayment from '../components/ConnectPayment.vue'

const routes = [
  {
    path: '/',
    name: 'PaymentSelection',
    component: PaymentSelection
  },
  {
    path: '/jeton-payment',
    name: 'JetonPayment',
    component: JetonPayment
  },
  {
    path: '/connect',
    name: 'ConnectOnboarding',
    component: ConnectOnboarding
  },
  {
    path: '/connect-payment',
    name: 'ConnectPayment',
    component: ConnectPayment
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