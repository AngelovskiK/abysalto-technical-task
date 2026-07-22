import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { ApiError } from '../services/client'
import { addCartItem, clearCart, getCart, removeCartItem, updateCartItemQuantity } from '../services/cartApi'
import type { AddCartItemPayload, Cart } from '../types'

const cartQueryKey = ['cart']

export function useCartQuery(token?: string) {
    return useQuery<Cart | null>({
        queryKey: [...cartQueryKey, token],
        enabled: Boolean(token?.trim()),
        queryFn: async () => {
            if (!token?.trim()) {
                return null
            }

            try {
                return await getCart(token)
            } catch (error) {
                if (error instanceof ApiError && error.status === 404) {
                    return null
                }

                throw error
            }
        },
    })
}

export function useCartMutations(token?: string) {
    const queryClient = useQueryClient()

    const invalidateCart = async () => {
        await queryClient.invalidateQueries({ queryKey: cartQueryKey })
    }

    return {
        addItem: useMutation({
            mutationFn: async (payload: AddCartItemPayload) => {
                if (!token?.trim()) {
                    throw new Error('Sign in before adding items to the cart.')
                }

                return addCartItem(token, payload)
            },
            onSuccess: invalidateCart,
        }),
        updateQuantity: useMutation({
            mutationFn: async ({ cartItemId, quantity }: { cartItemId: string; quantity: number }) => {
                if (!token?.trim()) {
                    throw new Error('Sign in before updating the cart.')
                }

                return updateCartItemQuantity(token, cartItemId, quantity)
            },
            onSuccess: invalidateCart,
        }),
        removeItem: useMutation({
            mutationFn: async ({ cartItemId }: { cartItemId: string }) => {
                if (!token?.trim()) {
                    throw new Error('Sign in before removing items from the cart.')
                }

                return removeCartItem(token, cartItemId)
            },
            onSuccess: invalidateCart,
        }),
        clearCart: useMutation({
            mutationFn: async () => {
                if (!token?.trim()) {
                    throw new Error('Sign in before clearing the cart.')
                }

                return clearCart(token)
            },
            onSuccess: invalidateCart,
        }),
    }
}