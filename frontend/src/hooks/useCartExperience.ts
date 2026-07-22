import { useState } from 'react'
import type { FormEvent } from 'react'
import { useMutation, useQueryClient } from '@tanstack/react-query'
import type { CartItem, Product } from '../types'
import { useAuth } from '../context/AuthContext'
import { useToast } from '../context/ToastContext'
import { login } from '../services/authApi'
import { useCartMutations, useCartQuery } from './useCart'

export function useCartExperience() {
    const { session, isLoading, signIn, signOut } = useAuth()
    const { showToast } = useToast()
    const queryClient = useQueryClient()
    const [email, setEmail] = useState('shopper@abysalto.dev')
    const [name, setName] = useState('Demo Shopper')

    const cartQuery = useCartQuery(session?.token)
    const cartMutations = useCartMutations(session?.token, {
        onAddSuccess: productName => showToast(`Added ${productName} to cart.`, 'success'),
        onRemoveSuccess: () => showToast('Removed item from cart.', 'info'),
        onClearSuccess: () => showToast('Cart cleared.', 'info'),
    })

    const loginMutation = useMutation({
        mutationFn: login,
        onSuccess: async (response, variables) => {
            signIn(response, variables.name)
            showToast(`Welcome, ${variables.name}.`, 'success')
            await queryClient.invalidateQueries({ queryKey: ['cart'] })
        },
    })

    const handleSignOut = () => {
        signOut()
        showToast('Signed out successfully.', 'info')
    }

    const handleLogin = (event: FormEvent<HTMLFormElement>) => {
        event.preventDefault()
        loginMutation.mutate({ email, name })
    }

    const addItem = (product: Product, quantity: number) => {
        cartMutations.addItem.mutate({ ...product, quantity })
    }

    const addDemoItem = (product: Product) => {
        cartMutations.addItem.mutate({ ...product, quantity: 1 })
    }

    const increase = (item: CartItem) => {
        cartMutations.updateQuantity.mutate({
            cartItemId: item.id,
            quantity: item.quantity + 1,
        })
    }

    const decrease = (item: CartItem) => {
        if (item.quantity <= 1) {
            const shouldRemove = window.confirm(`Remove ${item.productName} from your cart?`)
            if (!shouldRemove) {
                return
            }

            cartMutations.removeItem.mutate({ cartItemId: item.id })
            return
        }

        cartMutations.updateQuantity.mutate({
            cartItemId: item.id,
            quantity: Math.max(1, item.quantity - 1),
        })
    }

    const remove = (cartItemId: string) => {
        cartMutations.removeItem.mutate({ cartItemId })
    }

    const clear = () => {
        cartMutations.clearCart.mutate()
    }

    const cartItems = cartQuery.data?.items ?? []
    const cartItemCount = cartItems.reduce((sum, item) => sum + item.quantity, 0)
    const cartTotal = cartQuery.data?.total ?? 0

    return {
        session,
        signOut: handleSignOut,
        email,
        name,
        setEmail,
        setName,
        handleLogin,
        loginPending: loginMutation.isPending,
        authMessage: loginMutation.error instanceof Error ? loginMutation.error.message : null,
        cart: cartQuery.data,
        cartItems,
        cartItemCount,
        cartTotal,
        cartLoading: cartQuery.isLoading,
        cartMessage: cartQuery.error instanceof Error ? cartQuery.error.message : null,
        authLoading: isLoading,
        isAuthenticated: Boolean(session),
        isMutating:
            cartMutations.addItem.isPending ||
            cartMutations.updateQuantity.isPending ||
            cartMutations.removeItem.isPending ||
            cartMutations.clearCart.isPending,
        isClearing: cartMutations.clearCart.isPending,
        pendingProductId: cartMutations.addItem.variables?.productId,
        addItem,
        addDemoItem,
        increase,
        decrease,
        remove,
        clear,
    }
}