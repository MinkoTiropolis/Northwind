// Thin, typed wrapper around the back-end HTTP API. All URLs are relative
// ("/api/..."), which the Vite dev server proxies to the ASP.NET Core service.

export interface CustomerSummary {
  customerId: string
  companyName: string
  orderCount: number
}

export interface OrderSummary {
  orderId: number
  orderDate: string | null
  totalValue: number
  productCount: number
}

export interface CustomerDetail {
  customerId: string
  companyName: string
  contactName: string | null
  city: string | null
  country: string | null
  phone: string | null
  orders: OrderSummary[]
}

async function toJson<T>(response: Response): Promise<T> {
  if (!response.ok) {
    throw new Error(`Request failed (${response.status})`)
  }
  return (await response.json()) as T
}

export function getCustomers(search: string): Promise<CustomerSummary[]> {
  const query = search ? `?search=${encodeURIComponent(search)}` : ''
  return fetch(`/api/customers${query}`).then((r) => toJson<CustomerSummary[]>(r))
}

export async function getCustomer(id: string): Promise<CustomerDetail | null> {
  const response = await fetch(`/api/customers/${encodeURIComponent(id)}`)
  if (response.status === 404) {
    return null
  }
  return toJson<CustomerDetail>(response)
}
