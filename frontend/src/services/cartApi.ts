import type { AddCartItemPayload, Cart } from '../types'
import { request } from './client'

export function getCart(token: string) {
    return request<Cart>('/api/cart', {}, token)
}

export function addCartItem(token: string, payload: AddCartItemPayload) {
    return request<Cart>('/api/cart/items', {
        method: 'POST',
        body: JSON.stringify(payload),
    }, token)
}

export function updateCartItemQuantity(token: string, cartItemId: string, quantity: number) {
    return request<Cart>(`/api/cart/items/${cartItemId}`, {
        method: 'PUT',
        body: JSON.stringify({ quantity }),
    }, token)
}

export function removeCartItem(token: string, cartItemId: string) {
    return request<Cart>(`/api/cart/items/${cartItemId}`, {
        method: 'DELETE',
    }, token)
}

export function clearCart(token: string) {
    return request<void>('/api/cart', {
        method: 'DELETE',
    }, token)
}