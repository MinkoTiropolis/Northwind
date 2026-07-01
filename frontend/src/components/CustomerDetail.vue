<script setup lang="ts">
import { computed, ref, watch } from 'vue'
import { getCustomer, type CustomerDetail } from '../api'

const props = defineProps<{ customerId: string | null }>()

const customer = ref<CustomerDetail | null>(null)
const loading = ref(false)
const error = ref('')

// Presentation formatting lives in the UI; the API returns a plain number.
const money = new Intl.NumberFormat('en-US', { style: 'currency', currency: 'USD' })
const formatMoney = (value: number) => money.format(value)
const formatDate = (value: string | null) => (value ? new Date(value).toLocaleDateString() : '—')

const location = computed(() =>
  [customer.value?.city, customer.value?.country].filter(Boolean).join(', '),
)

// Guard against out-of-order responses: only the most recent request writes state.
let requestId = 0

watch(() => props.customerId, load, { immediate: true })

async function load() {
  customer.value = null
  if (!props.customerId) {
    return
  }
  const current = ++requestId
  loading.value = true
  error.value = ''
  try {
    const result = await getCustomer(props.customerId)
    if (current !== requestId) {
      return
    }
    customer.value = result
  } catch (e) {
    if (current === requestId) {
      error.value = (e as Error).message
    }
  } finally {
    if (current === requestId) {
      loading.value = false
    }
  }
}
</script>

<template>
  <section class="card detail">
    <div v-if="!customerId" class="placeholder">
      <p>Select a customer to see their details.</p>
    </div>
    <p v-else-if="loading" class="state">Loading…</p>
    <p v-else-if="error" class="state error">{{ error }}</p>

    <template v-else-if="customer">
      <div class="head">
        <span class="avatar">{{ customer.companyName.charAt(0) }}</span>
        <div>
          <h2>{{ customer.companyName }}</h2>
          <div class="chips">
            <span v-if="customer.contactName" class="chip">👤 {{ customer.contactName }}</span>
            <span v-if="location" class="chip">📍 {{ location }}</span>
            <span v-if="customer.phone" class="chip">📞 {{ customer.phone }}</span>
          </div>
        </div>
      </div>

      <h3>Order history <span class="count">{{ customer.orders.length }}</span></h3>

      <table v-if="customer.orders.length">
        <thead>
          <tr>
            <th>Order</th>
            <th>Date</th>
            <th class="num">Products</th>
            <th class="num">Total</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="o in customer.orders" :key="o.orderId">
            <td class="mono">#{{ o.orderId }}</td>
            <td>{{ formatDate(o.orderDate) }}</td>
            <td class="num">{{ o.productCount }}</td>
            <td class="num total">{{ formatMoney(o.totalValue) }}</td>
          </tr>
        </tbody>
      </table>
      <p v-else class="state">This customer has no orders.</p>
    </template>

    <p v-else class="state">Customer not found.</p>
  </section>
</template>

<style scoped>
.card {
  background: var(--surface);
  border: 1px solid var(--border);
  border-radius: var(--radius);
  box-shadow: var(--shadow);
  padding: 1.5rem;
  min-height: 260px;
}

.placeholder {
  display: grid;
  place-content: center;
  justify-items: center;
  gap: 0.5rem;
  min-height: 220px;
  color: var(--muted);
}

.head {
  display: flex;
  gap: 1rem;
  align-items: center;
}

.avatar {
  display: grid;
  place-items: center;
  width: 52px;
  height: 52px;
  flex-shrink: 0;
  border-radius: 14px;
  background: var(--primary-soft);
  color: var(--primary);
  font-size: 1.5rem;
  font-weight: 700;
}

h2 {
  margin: 0 0 0.35rem;
  font-size: 1.25rem;
  letter-spacing: -0.01em;
}

.chips {
  display: flex;
  flex-wrap: wrap;
  gap: 0.4rem;
}

.chip {
  padding: 0.15rem 0.6rem;
  border-radius: 999px;
  background: #f2f4f8;
  color: #475467;
  font-size: 0.8rem;
}

h3 {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  margin: 1.75rem 0 0.75rem;
  font-size: 1rem;
}

.count {
  padding: 0.05rem 0.5rem;
  border-radius: 999px;
  background: #eef0f4;
  color: var(--muted);
  font-size: 0.78rem;
  font-weight: 600;
}

table {
  width: 100%;
  border-collapse: collapse;
}

thead th {
  text-align: left;
  padding: 0.55rem 0.7rem;
  background: #f8f9fc;
  color: var(--muted);
  font-size: 0.78rem;
  font-weight: 600;
  text-transform: uppercase;
  letter-spacing: 0.03em;
  border-bottom: 1px solid var(--border);
}

thead th:first-child {
  border-top-left-radius: 8px;
}

thead th:last-child {
  border-top-right-radius: 8px;
}

tbody td {
  padding: 0.6rem 0.7rem;
  border-bottom: 1px solid #f0f1f5;
  font-size: 0.9rem;
}

tbody tr:hover {
  background: #fafbfe;
}

.num {
  text-align: right;
}

.mono {
  font-variant-numeric: tabular-nums;
  color: var(--muted);
}

.total {
  font-weight: 600;
  font-variant-numeric: tabular-nums;
}

.state {
  padding: 1.25rem 0;
  color: var(--muted);
  font-size: 0.9rem;
}

.state.error {
  color: var(--danger);
}
</style>
