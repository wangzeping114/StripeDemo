<script setup>
import StripePayment from './components/StripePayment.vue'
</script>

<template>
  <div class="app">
    <StripePayment />
  </div>
</template>

<style>
@import './assets/base.css';

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
  }

  .app {
    display: flex;
    flex-direction: column;
    padding: 0 2rem;
  }
}
</style>
