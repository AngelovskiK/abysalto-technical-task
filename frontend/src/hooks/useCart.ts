import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { ApiError } from '../services/client'
import { addCartItem, clearCart, getCart, removeCartItem, updateCartItemQuantity } from '../services/cartApi'
import type { AddCartItemPayload, Cart } from '../types'

const cartQueryKey = ['cart']

interface CartMutationCallbacks {
    onAddSuccess?: (productName: string) => void
    onRemoveSuccess?: () => void
    onClearSuccess?: () => void
}

interface OptimisticMutationContext {
    previousCarts: Array<[readonly unknown[], Cart | null | undefined]>
}

function calculateTotal(items: Cart['items']) {
    return items.reduce((sum, item) => sum + item.unitPrice * item.quantity, 0)
}

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

export function useCartMutations(token?: string, callbacks?: CartMutationCallbacks) {
    const queryClient = useQueryClient()

    const invalidateCart = async () => {
        await queryClient.invalidateQueries({ queryKey: cartQueryKey })
    }

    const snapshotCarts = () => queryClient.getQueriesData<Cart | null>({ queryKey: cartQueryKey })

    const rollbackCarts = (context?: OptimisticMutationContext) => {
        context?.previousCarts.forEach(([queryKey, previousCart]) => {
            queryClient.setQueryData(queryKey, previousCart)
        })
    }

    return {
        addItem: useMutation({
            mutationFn: async (payload: AddCartItemPayload) => {
                if (!token?.trim()) {
                    throw new Error('Sign in before adding items to the cart.')
                }

                return addCartItem(token, payload)
            },
            onMutate: async payload => {
                await queryClient.cancelQueries({ queryKey: cartQueryKey })
                const previousCarts = snapshotCarts()

                previousCarts.forEach(([queryKey, currentCart]) => {
                    if (!currentCart) {
                        return
                    }

                    const existingItem = currentCart.items.find(item => item.productId === payload.productId)
                    const nextItems = existingItem
                        ? currentCart.items.map(item =>
                            item.productId === payload.productId ? { ...item, quantity: item.quantity + payload.quantity } : item,
                        )
                        : [
                            ...currentCart.items,
                            {
                                id: `optimistic-${payload.productId}`,
                                productId: payload.productId,
                                productName: payload.productName,
                                unitPrice: payload.unitPrice,
                                quantity: payload.quantity,
                                imageUrl: payload.imageUrl,
                            },
                        ]

                    queryClient.setQueryData<Cart>(queryKey, {
                        ...currentCart,
                        items: nextItems,
                        total: calculateTotal(nextItems),
                        updatedAt: new Date().toISOString(),
                    })
                })

                return { previousCarts }
            },
            onError: (_error, _variables, context) => {
                rollbackCarts(context)
            },
            onSuccess: async (_result, variables) => {
                callbacks?.onAddSuccess?.(variables.productName)
            },
            onSettled: invalidateCart,
        }),
        updateQuantity: useMutation({
            mutationFn: async ({ cartItemId, quantity }: { cartItemId: string; quantity: number }) => {
                if (!token?.trim()) {
                    throw new Error('Sign in before updating the cart.')
                }

                return updateCartItemQuantity(token, cartItemId, quantity)
            },
            onMutate: async ({ cartItemId, quantity }) => {
                await queryClient.cancelQueries({ queryKey: cartQueryKey })
                const previousCarts = snapshotCarts()

                previousCarts.forEach(([queryKey, currentCart]) => {
                    if (!currentCart) {
                        return
                    }

                    const nextItems = currentCart.items.map(item => (item.id === cartItemId ? { ...item, quantity } : item))

                    queryClient.setQueryData<Cart>(queryKey, {
                        ...currentCart,
                        items: nextItems,
                        total: calculateTotal(nextItems),
                        updatedAt: new Date().toISOString(),
                    })
                })

                return { previousCarts }
            },
            onError: (_error, _variables, context) => {
                rollbackCarts(context)
            },
            onSettled: invalidateCart,
        }),
        removeItem: useMutation({
            mutationFn: async ({ cartItemId }: { cartItemId: string }) => {
                if (!token?.trim()) {
                    throw new Error('Sign in before removing items from the cart.')
                }

                return removeCartItem(token, cartItemId)
            },
            onMutate: async ({ cartItemId }) => {
                await queryClient.cancelQueries({ queryKey: cartQueryKey })
                const previousCarts = snapshotCarts()

                previousCarts.forEach(([queryKey, currentCart]) => {
                    if (!currentCart) {
                        return
                    }

                    const nextItems = currentCart.items.filter(item => item.id !== cartItemId)

                    queryClient.setQueryData<Cart>(queryKey, {
                        ...currentCart,
                        items: nextItems,
                        total: calculateTotal(nextItems),
                        updatedAt: new Date().toISOString(),
                    })
                })

                return { previousCarts }
            },
            onError: (_error, _variables, context) => {
                rollbackCarts(context)
            },
            onSuccess: async () => {
                callbacks?.onRemoveSuccess?.()
            },
            onSettled: invalidateCart,
        }),
        clearCart: useMutation({
            mutationFn: async () => {
                if (!token?.trim()) {
                    throw new Error('Sign in before clearing the cart.')
                }

                return clearCart(token)
            },
            onMutate: async () => {
                await queryClient.cancelQueries({ queryKey: cartQueryKey })
                const previousCarts = snapshotCarts()

                previousCarts.forEach(([queryKey, currentCart]) => {
                    if (!currentCart) {
                        return
                    }

                    queryClient.setQueryData<Cart>(queryKey, {
                        ...currentCart,
                        items: [],
                        total: 0,
                        updatedAt: new Date().toISOString(),
                    })
                })

                return { previousCarts }
            },
            onError: (_error, _variables, context) => {
                rollbackCarts(context)
            },
            onSuccess: async () => {
                callbacks?.onClearSuccess?.()
            },
            onSettled: invalidateCart,
        }),
    }
}