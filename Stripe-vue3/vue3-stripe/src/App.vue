<script setup>
// 无需导入任何组件，路由将管理组件的渲染
</script>

<template>
  <div class="app">
    <router-view />
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
  padding: 2rem;
  font-weight: normal;
}

@media (min-width: 1024px) {
  body {
    display: flex;
    place-items: center;
    min-height: 100vh;
  }

  .app {
    display: flex;
    flex-direction: column;
    padding: 0 2rem;
  }
}
</style>
