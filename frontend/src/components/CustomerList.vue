<script setup lang="ts">
import { ref, watch } from 'vue'
import { getCustomers, type CustomerSummary } from '../api'

defineProps<{ selectedId: string | null }>()
const emit = defineEmits<{ select: [id: string] }>()

const search = ref('')
const customers = ref<CustomerSummary[]>([])
const loading = ref(false)
const error = ref('')

// Debounce so we don't fire a request on every keystroke.
let timer: ReturnType<typeof setTimeout>
watch(search, () => {
  clearTimeout(timer)
  timer = setTimeout(load, 250)
})

async function load() {
  loading.value = true
  error.value = ''
  try {
    customers.value = await getCustomers(search.value.trim())
  } catch (e) {
    error.value = (e as Error).message
  } finally {
    loading.value = false
  }
}

load()
</script>

<template>
  <section class="card list">
    <div class="search">
      <span class="search-icon" aria-hidden="true">🔍</span>
      <input v-model="search" type="search" placeholder="Search by name…" />
    </div>

    <div class="list-body">
      <p v-if="loading" class="state">Loading…</p>
      <p v-else-if="error" class="state error">{{ error }}</p>
      <p v-else-if="customers.length === 0" class="state">No customers found.</p>
      <ul v-else>
        <li
          v-for="c in customers"
          :key="c.customerId"
          :class="{ active: c.customerId === selectedId }"
          @click="emit('select', c.customerId)"
        >
          <span class="name">{{ c.companyName }}</span>
          <span class="badge">{{ c.orderCount }}</span>
        </li>
      </ul>
    </div>
  </section>
</template>

<style scoped>
.card {
  background: var(--surface);
  border: 1px solid var(--border);
  border-radius: var(--radius);
  box-shadow: var(--shadow);
  overflow: hidden;
}

.search {
  position: relative;
  padding: 0.85rem;
  border-bottom: 1px solid var(--border);
}

.search-icon {
  position: absolute;
  left: 1.4rem;
  top: 50%;
  transform: translateY(-50%);
  font-size: 0.9rem;
  opacity: 0.6;
  pointer-events: none;
}

input {
  width: 100%;
  padding: 0.6rem 0.75rem 0.6rem 2.3rem;
  border: 1px solid var(--border);
  border-radius: 10px;
  font-size: 0.95rem;
  background: #fbfbfd;
  transition: border-color 0.15s, box-shadow 0.15s, background 0.15s;
}

input:focus {
  outline: none;
  border-color: var(--primary);
  background: #fff;
  box-shadow: 0 0 0 3px var(--primary-soft);
}

.list-body {
  max-height: 68vh;
  overflow: auto;
}

ul {
  list-style: none;
  margin: 0;
  padding: 0.4rem;
}

li {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 0.5rem;
  padding: 0.6rem 0.7rem;
  border-radius: 10px;
  cursor: pointer;
  transition: background 0.12s;
}

li:hover {
  background: #f5f7fa;
}

li.active {
  background: var(--primary-soft);
}

li.active .name {
  color: var(--primary);
  font-weight: 600;
}

.name {
  font-size: 0.94rem;
}

.badge {
  flex-shrink: 0;
  min-width: 1.6rem;
  padding: 0.1rem 0.5rem;
  text-align: center;
  border-radius: 999px;
  background: #eef0f4;
  color: var(--muted);
  font-size: 0.78rem;
  font-weight: 600;
}

li.active .badge {
  background: var(--primary);
  color: #fff;
}

.state {
  padding: 1.5rem;
  text-align: center;
  color: var(--muted);
  font-size: 0.9rem;
}

.state.error {
  color: var(--danger);
}
</style>
