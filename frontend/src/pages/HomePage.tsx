import { Link } from 'react-router-dom'
import { Button } from '../components/Common/Button'
import { ProductList } from '../components/Product/ProductList'
import { useCartExperience } from '../hooks/useCartExperience'
import { products } from '../services/catalog'

export function HomePage() {
    const cartExperience = useCartExperience()

    return (
        <>
            <header className="overflow-hidden rounded-[32px] bg-ink text-white shadow-panel">
                <div className="grid gap-8 px-6 py-8 sm:px-8 lg:grid-cols-[1.4fr_0.8fr] lg:px-10 lg:py-10">
                    <div className="space-y-5">
                        <p className="text-sm uppercase tracking-[0.35em] text-gold/90">Retail Platform Demo</p>
                        <h2 className="max-w-2xl text-4xl font-semibold leading-tight sm:text-5xl">
                            Build a living basket, not a static mock.
                        </h2>
                        <p className="max-w-2xl text-base text-slate-200 sm:text-lg">
                            This client logs into the Basket API, shows a curated product rail, and keeps the cart in sync through TanStack Query mutations.
                        </p>
                        <div className="flex flex-wrap gap-3 text-sm text-slate-200">
                            <span className="rounded-full border border-white/20 px-4 py-2">Vite + React 19</span>
                            <span className="rounded-full border border-white/20 px-4 py-2">TanStack Query</span>
                            <span className="rounded-full border border-white/20 px-4 py-2">Tailwind CSS</span>
                        </div>
                    </div>

                    <section className="rounded-[28px] bg-white/10 p-5 backdrop-blur-sm">
                        {cartExperience.session ? (
                            <div className="space-y-4">
                                <div>
                                    <p className="text-sm uppercase tracking-[0.3em] text-gold/90">Signed In</p>
                                    <h3 className="mt-2 text-2xl font-semibold text-white">{cartExperience.session.name}</h3>
                                    <p className="mt-1 text-sm text-slate-200">{cartExperience.session.email}</p>
                                </div>
                                <div className="rounded-2xl bg-white/10 p-4 text-sm text-slate-100">
                                    New users get a cart created during login, so the first add-to-cart action works immediately.
                                </div>
                                <Button variant="ghost" className="w-full border-white/20 text-white hover:bg-white/10" onClick={cartExperience.signOut}>
                                    Sign Out
                                </Button>
                            </div>
                        ) : (
                            <div className="space-y-4">
                                <div>
                                    <p className="text-sm uppercase tracking-[0.3em] text-gold/90">Session</p>
                                    <h3 className="mt-2 text-2xl font-semibold text-white">You are signed out</h3>
                                    <p className="mt-1 text-sm text-slate-200">Sign in to unlock cart actions and the dedicated cart page.</p>
                                </div>
                                <Link
                                    to="/login"
                                    className="inline-flex w-full items-center justify-center rounded-2xl bg-gold px-4 py-3 text-sm font-semibold text-ink transition hover:bg-amber-300"
                                >
                                    Go to Sign In
                                </Link>
                            </div>
                        )}
                    </section>
                </div>
            </header>

            <main className="mt-6 flex-1">
                <section className="space-y-4 rounded-[32px] bg-white p-6 shadow-panel">
                    <div className="flex items-end justify-between gap-4">
                        <div>
                            <p className="text-sm uppercase tracking-[0.3em] text-moss">Curated Products</p>
                            <h2 className="mt-2 text-3xl font-semibold text-ink">Launch assortment</h2>
                        </div>
                    </div>

                    {cartExperience.cartMessage ? (
                        <p className="rounded-2xl border border-rose-200 bg-rose-50 px-4 py-3 text-sm text-rose-700">
                            {cartExperience.cartMessage}
                        </p>
                    ) : null}

                    <ProductList
                        products={products}
                        canPurchase={cartExperience.isAuthenticated}
                        pendingProductId={cartExperience.pendingProductId}
                        onAddItem={cartExperience.addItem}
                    />
                </section>
            </main>
        </>
    )
}