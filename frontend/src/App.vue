<script setup lang="ts">
import { ref } from 'vue'
import CustomerList from './components/CustomerList.vue'
import CustomerDetail from './components/CustomerDetail.vue'

const selectedId = ref<string | null>(null)
</script>

<template>
  <div class="layout">
    <header class="app-header">
      <span class="logo">N</span>
      <div>
        <h1>Northwind Customer Lookup</h1>
        <p class="subtitle">Search customers and review their order history</p>
      </div>
    </header>
    <main class="content">
      <CustomerList :selected-id="selectedId" @select="selectedId = $event" />
      <CustomerDetail :customer-id="selectedId" />
    </main>
  </div>
</template>

<style>
:root {
  --bg: #f4f6fb;
  --surface: #ffffff;
  --border: #e6e8ee;
  --text: #1f2430;
  --muted: #6b7280;
  --primary: #4f46e5;
  --primary-hover: #4338ca;
  --primary-soft: #eef2ff;
  --danger: #b00020;
  --radius: 14px;
  --shadow: 0 1px 2px rgba(16, 24, 40, 0.04), 0 4px 12px rgba(16, 24, 40, 0.06);
}

* {
  box-sizing: border-box;
}

body {
  margin: 0;
  background: var(--bg);
  color: var(--text);
  font-family: system-ui, -apple-system, 'Segoe UI', Roboto, Helvetica, Arial, sans-serif;
  line-height: 1.5;
}
</style>

<style scoped>
.layout {
  max-width: 1080px;
  margin: 0 auto;
  padding: 2rem 1.5rem 3rem;
}

.app-header {
  display: flex;
  align-items: center;
  gap: 0.9rem;
  margin-bottom: 1.75rem;
}

.logo {
  display: grid;
  place-items: center;
  width: 44px;
  height: 44px;
  border-radius: 12px;
  background: linear-gradient(135deg, var(--primary), #7c73ff);
  color: #fff;
  font-weight: 700;
  font-size: 1.35rem;
  box-shadow: var(--shadow);
}

h1 {
  margin: 0;
  font-size: 1.35rem;
  letter-spacing: -0.01em;
}

.subtitle {
  margin: 0.1rem 0 0;
  color: var(--muted);
  font-size: 0.9rem;
}

.content {
  display: grid;
  grid-template-columns: 340px 1fr;
  gap: 1.5rem;
  align-items: start;
}

@media (max-width: 760px) {
  .content {
    grid-template-columns: 1fr;
  }
}
</style>
