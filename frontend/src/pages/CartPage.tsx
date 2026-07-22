import { CartPanel } from '../components/Cart/CartPanel'
import { useCartExperience } from '../hooks/useCartExperience'
import { products } from '../services/catalog'

export function CartPage() {
    const cartExperience = useCartExperience()

    return (
        <main className="grid flex-1 gap-6 lg:grid-cols-[0.85fr_1.15fr]">
            <section className="rounded-[32px] bg-white p-6 shadow-panel">
                <p className="text-sm uppercase tracking-[0.3em] text-moss">Cart Focus</p>
                <h2 className="mt-3 text-3xl font-semibold text-ink">Checkout prep</h2>
                <p className="mt-3 text-sm leading-6 text-slate-600">
                    This page isolates cart operations so upcoming steps like vouchers, shipping options, and checkout validation can be added without crowding the product catalog view.
                </p>

                <div className="mt-6 rounded-3xl border border-slate-200 bg-slate-50 p-4">
                    <p className="text-sm text-slate-500">Signed in as</p>
                    <p className="text-lg font-semibold text-ink">{cartExperience.session?.email}</p>
                    <p className="mt-2 text-sm text-slate-600">Use the account menu in the top bar to sign out.</p>
                </div>
            </section>

            <CartPanel
                cart={cartExperience.cart}
                isAuthenticated={cartExperience.isAuthenticated}
                isLoading={cartExperience.cartLoading}
                errorMessage={cartExperience.cartMessage}
                onAddDemoItem={() => cartExperience.addDemoItem(products[0])}
                onIncrease={cartExperience.increase}
                onDecrease={cartExperience.decrease}
                onRemove={cartExperience.remove}
                onClear={cartExperience.clear}
                isMutating={cartExperience.isMutating}
            />
        </main>
    )
}