import { Link, NavLink, Outlet } from 'react-router-dom'
import { CartMenu } from '../components/Cart/CartMenu'
import { AccountMenu } from '../components/Common/AccountMenu'
import { useCartExperience } from '../hooks/useCartExperience'

export function AppShell() {
    const cartExperience = useCartExperience()

    return (
        <div className="min-h-screen bg-sand text-slate-900">
            <div className="mx-auto flex min-h-screen max-w-7xl flex-col px-4 py-6 sm:px-6 lg:px-8">
                <nav className="relative z-40 mb-5 flex items-center justify-between rounded-3xl border border-slate-200 bg-white/90 px-5 py-3 shadow-sm backdrop-blur">
                    <Link to="/" className="text-inherit no-underline" aria-label="Go to home page">
                        <p className="text-xs uppercase tracking-[0.35em] text-slate-500">Abysalto Basket</p>
                        <h1 className="text-lg font-semibold text-ink">Retail Demo</h1>
                    </Link>
                    <div className="flex items-center gap-2 text-sm">
                        <NavLink
                            to="/"
                            className={({ isActive }) =>
                                `rounded-full px-4 py-2 transition ${isActive ? 'bg-ink text-white' : 'text-slate-600 hover:bg-slate-100'}`
                            }
                        >
                            Home
                        </NavLink>
                        <CartMenu
                            cart={cartExperience.cart}
                            itemCount={cartExperience.cartItemCount}
                            isAuthenticated={cartExperience.isAuthenticated}
                            isLoading={cartExperience.cartLoading}
                            errorMessage={cartExperience.cartMessage}
                        />

                        <AccountMenu session={cartExperience.session} onSignOut={cartExperience.signOut} />
                    </div>
                </nav>
                <Outlet />
            </div>
        </div>
    )
}