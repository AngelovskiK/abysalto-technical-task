export interface CartItem {
    id: string
    productId: string
    productName: string
    unitPrice: number
    quantity: number
    imageUrl?: string | null
}

export interface Cart {
    id: string
    userId: string
    total: number
    updatedAt: string
    items: CartItem[]
}

export interface AddCartItemPayload {
    productId: string
    productName: string
    unitPrice: number
    quantity: number
    imageUrl?: string | null
}