import { Button } from '../Common/Button'
import type { Cart, CartItem } from '../../types'

interface CartPanelProps {
    cart: Cart | null | undefined
    isAuthenticated: boolean
    isLoading: boolean
    errorMessage: string | null
    isMutating: boolean
    isClearing: boolean
    onAddDemoItem: () => void
    onIncrease: (item: CartItem) => void
    onDecrease: (item: CartItem) => void
    onRemove: (cartItemId: string) => void
    onClear: () => void
}

function formatMoney(value: number) {
    return new Intl.NumberFormat('en-US', {
        style: 'currency',
        currency: 'USD',
    }).format(value)
}

export function CartPanel({
    cart,
    errorMessage,
    isAuthenticated,
    isLoading,
    isMutating,
    isClearing,
    onAddDemoItem,
    onClear,
    onDecrease,
    onIncrease,
    onRemove,
}: CartPanelProps) {
    const items = cart?.items ?? []

    return (
        <aside className="flex h-full flex-col rounded-[32px] bg-ink p-6 text-white shadow-panel">
            <div className="flex items-end justify-between gap-4">
                <div>
                    <p className="text-sm uppercase tracking-[0.3em] text-gold/80">Cart</p>
                    <h2 className="mt-2 text-3xl font-semibold">Current basket</h2>
                </div>
                <div className="text-right text-sm text-slate-300">
                    <p>{items.length} line items</p>
                    <p>{formatMoney(cart?.total ?? 0)}</p>
                </div>
            </div>

            {!isAuthenticated ? (
                <div className="mt-6 rounded-[28px] border border-white/10 bg-white/5 p-5 text-sm text-slate-200">
                    Sign in to create a JWT session and load the server-backed cart.
                </div>
            ) : null}

            {errorMessage ? (
                <div className="mt-6 rounded-[28px] border border-rose-300/30 bg-rose-200/10 p-5 text-sm text-rose-100">
                    {errorMessage}
                </div>
            ) : null}

            {isLoading ? (
                <div className="mt-6 rounded-[28px] border border-white/10 bg-white/5 p-5 text-sm text-slate-200">
                    Loading cart state...
                </div>
            ) : null}

            {isAuthenticated && !isLoading && items.length === 0 ? (
                <div className="mt-6 flex flex-1 flex-col justify-between rounded-[28px] border border-dashed border-white/20 bg-white/5 p-5">
                    <div>
                        <p className="text-lg font-semibold">Your basket is empty.</p>
                        <p className="mt-2 text-sm text-slate-300">
                            Choose a product on the left and the cart query will refresh automatically after the mutation succeeds.
                        </p>
                    </div>
                    <Button className="mt-6 w-full" onClick={onAddDemoItem} variant="secondary">
                        Add A Demo Item
                    </Button>
                </div>
            ) : null}

            {items.length > 0 ? (
                <div className="mt-6 flex flex-1 flex-col gap-4">
                    <div className="space-y-3">
                        {items.map(item => (
                            <article key={item.id} className="rounded-[28px] border border-white/10 bg-white/5 p-4">
                                <div className="flex items-start justify-between gap-4">
                                    <div>
                                        <h3 className="text-lg font-semibold">{item.productName}</h3>
                                        <p className="mt-1 text-sm text-slate-300">{formatMoney(item.unitPrice)} each</p>
                                    </div>
                                    <Button variant="ghost" className="px-3 py-2 text-white hover:bg-white/10" onClick={() => onRemove(item.id)}>
                                        Remove
                                    </Button>
                                </div>
                                <div className="mt-4 flex items-center justify-between gap-3">
                                    <div className="inline-flex items-center gap-2 rounded-full bg-white/10 p-1">
                                        <Button
                                            variant="ghost"
                                            className="rounded-full px-3 py-2 text-white hover:bg-white/10"
                                            disabled={isMutating}
                                            onClick={() => onDecrease(item)}
                                        >
                                            -
                                        </Button>
                                        <span className="min-w-10 text-center text-sm font-semibold">{item.quantity}</span>
                                        <Button
                                            variant="ghost"
                                            className="rounded-full px-3 py-2 text-white hover:bg-white/10"
                                            disabled={isMutating}
                                            onClick={() => onIncrease(item)}
                                        >
                                            +
                                        </Button>
                                    </div>
                                    <p className="text-sm font-semibold text-gold">{formatMoney(item.unitPrice * item.quantity)}</p>
                                </div>
                            </article>
                        ))}
                    </div>

                    <div className="mt-auto rounded-[28px] bg-white px-5 py-4 text-ink">
                        <div className="flex items-center justify-between text-sm text-slate-500">
                            <span>Last synced</span>
                            <span>{cart?.updatedAt ? new Date(cart.updatedAt).toLocaleString() : 'n/a'}</span>
                        </div>
                        <div className="mt-3 flex items-center justify-between">
                            <p className="text-lg font-semibold">Order total</p>
                            <p className="text-2xl font-semibold">{formatMoney(cart?.total ?? 0)}</p>
                        </div>
                        <Button className="mt-4 w-full" isLoading={isClearing} onClick={onClear} variant="primary">
                            Clear Cart
                        </Button>
                    </div>
                </div>
            ) : null}
        </aside>
    )
}