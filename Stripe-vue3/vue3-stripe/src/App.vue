<script setup>
// 无需导入任何组件，路由将管理组件的渲染
</script>

<template>
  <div class="app">
    <header class="app-header">
      <div class="logo">
        <span class="logo-text">支付系统</span>
      </div>
      <nav class="app-nav">
        <router-link to="/">首页</router-link>
        <router-link to="/connect">商户注册</router-link>
        <router-link to="/connect-payment">Connect支付</router-link>
      </nav>
    </header>
    
    <main class="app-main">
      <router-view />
    </main>
    
    <footer class="app-footer">
      <p>© 2023 支付演示系统 - Stripe Connect & Jeton Wallet 集成示例</p>
    </footer>
  </div>
</template>

<style>
@import './assets/base.css';

body {
  font-family: 'Helvetica Neue', Helvetica, Arial, sans-serif;
  -webkit-font-smoothing: antialiased;
  -moz-osx-font-smoothing: grayscale;
  color: #2c3e50;
  margin: 0;
  padding: 0;
  background-color: #f7fafc;
}

.app {
  max-width: 1280px;
  margin: 0 auto;
  padding: 1rem;
  font-weight: normal;
}

.app-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 1rem 0;
  margin-bottom: 2rem;
  border-bottom: 1px solid #e6ebf1;
}

.logo {
  display: flex;
  align-items: center;
}

.logo-text {
  font-size: 1.5rem;
  font-weight: bold;
  color: #5469d4;
}

.app-nav {
  display: flex;
  gap: 1.5rem;
}

.app-nav a {
  color: #6b7c93;
  text-decoration: none;
  font-weight: 500;
  padding: 0.5rem 0;
  position: relative;
  transition: color 0.2s ease;
}

.app-nav a:hover {
  color: #5469d4;
}

.app-nav a.router-link-active {
  color: #5469d4;
}

.app-nav a.router-link-active::after {
  content: '';
  position: absolute;
  bottom: 0;
  left: 0;
  width: 100%;
  height: 2px;
  background-color: #5469d4;
}

.app-main {
  min-height: 70vh;
}

.app-footer {
  margin-top: 2rem;
  padding-top: 1.5rem;
  border-top: 1px solid #e6ebf1;
  text-align: center;
  color: #6b7c93;
  font-size: 0.9rem;
}

@media (max-width: 768px) {
  .app-header {
    flex-direction: column;
    gap: 1rem;
  }
  
  .app-nav {
    width: 100%;
    justify-content: center;
  }
}
</style>
