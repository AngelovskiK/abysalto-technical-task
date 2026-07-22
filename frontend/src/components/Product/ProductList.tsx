import { ProductCard } from './ProductCard'
import type { Product } from '../../types'

interface ProductListProps {
    products: Product[]
    canPurchase: boolean
    pendingProductId?: string
    onAddItem: (product: Product, quantity: number) => void
}

export function ProductList({ products, canPurchase, onAddItem, pendingProductId }: ProductListProps) {
    return (
        <div className="grid gap-4 md:grid-cols-2">
            {products.map(product => (
                <ProductCard
                    key={product.productId}
                    product={product}
                    canPurchase={canPurchase}
                    isPending={pendingProductId === product.productId}
                    onAddItem={onAddItem}
                />
            ))}
        </div>
    )
}