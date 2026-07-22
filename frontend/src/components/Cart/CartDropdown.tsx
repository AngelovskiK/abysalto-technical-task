import { Link } from 'react-router-dom'
import type { Cart } from '../../types'

interface CartDropdownProps {
    cart: Cart | null | undefined
    isAuthenticated: boolean
    isLoading: boolean
    errorMessage: string | null
    onClose: () => void
}

function formatMoney(value: number) {
    return new Intl.NumberFormat('en-US', {
        style: 'currency',
        currency: 'USD',
    }).format(value)
}

export function CartDropdown({ cart, isAuthenticated, isLoading, errorMessage, onClose }: CartDropdownProps) {
    const items = cart?.items ?? []

    return (
        <div className="w-80 overflow-hidden rounded-3xl border border-slate-200 bg-white shadow-2xl">
            <div className="border-b border-slate-100 px-4 py-3">
                <p className="text-xs uppercase tracking-[0.25em] text-slate-500">Cart Preview</p>
            </div>

            <div className="max-h-72 overflow-y-auto px-4 py-3">
                {!isAuthenticated ? (
                    <p className="text-sm text-slate-500">Sign in to see your cart items.</p>
                ) : isLoading ? (
                    <p className="text-sm text-slate-500">Loading cart...</p>
                ) : errorMessage ? (
                    <p className="text-sm text-rose-600">{errorMessage}</p>
                ) : items.length === 0 ? (
                    <p className="text-sm text-slate-500">Your cart is empty.</p>
                ) : (
                    <ul className="space-y-3">
                        {items.map(item => (
                            <li key={item.id} className="rounded-2xl border border-slate-100 p-3">
                                <p className="text-sm font-semibold text-slate-800">{item.productName}</p>
                                <div className="mt-1 flex items-center justify-between text-xs text-slate-500">
                                    <span>Qty {item.quantity}</span>
                                    <span>{formatMoney(item.unitPrice * item.quantity)}</span>
                                </div>
                            </li>
                        ))}
                    </ul>
                )}
            </div>

            <div className="border-t border-slate-100 px-4 py-3">
                <div className="mb-3 flex items-center justify-between text-sm text-slate-600">
                    <span>Total</span>
                    <span className="font-semibold text-ink">{formatMoney(cart?.total ?? 0)}</span>
                </div>
                {isAuthenticated ? (
                    <Link
                        to="/cart"
                        onClick={onClose}
                        className="block rounded-2xl bg-ink px-4 py-2.5 text-center text-sm font-semibold text-white transition hover:bg-slate-800"
                    >
                        View Cart
                    </Link>
                ) : null}
            </div>
        </div>
    )
}