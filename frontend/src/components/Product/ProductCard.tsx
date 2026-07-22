import { useState } from 'react'
import { Button } from '../Common/Button'
import type { Product } from '../../types'

interface ProductCardProps {
    product: Product
    canPurchase: boolean
    isPending: boolean
    onAddItem: (product: Product, quantity: number) => void
}

export function ProductCard({ product, canPurchase, isPending, onAddItem }: ProductCardProps) {
    const [quantity, setQuantity] = useState(1)

    return (
        <article className="flex h-full flex-col rounded-[28px] border border-slate-200 bg-slate-50 p-5">
            <div className={`rounded-[24px] bg-gradient-to-br ${product.accentClass} p-5 text-white`}>
                <div className="flex items-start justify-between gap-4">
                    <span className="rounded-full bg-white/15 px-3 py-1 text-xs uppercase tracking-[0.25em]">
                        {product.tag}
                    </span>
                    <span className="text-sm opacity-90">${product.unitPrice.toFixed(2)}</span>
                </div>
                <h3 className="mt-8 text-2xl font-semibold">{product.productName}</h3>
            </div>
            <p className="mt-4 flex-1 text-sm leading-6 text-slate-600">{product.description}</p>
            <div className="mt-5 flex items-center gap-3">
                <input
                    min={1}
                    type="number"
                    value={quantity}
                    onChange={event => setQuantity(Math.max(1, Number(event.target.value) || 1))}
                    className="w-20 rounded-2xl border border-slate-200 bg-white px-3 py-3 text-sm outline-none focus:border-ink"
                />
                <Button
                    className="flex-1"
                    disabled={!canPurchase}
                    isLoading={isPending}
                    onClick={() => onAddItem(product, quantity)}
                >
                    {canPurchase ? 'Add To Cart' : 'Sign In To Buy'}
                </Button>
            </div>
        </article>
    )
}