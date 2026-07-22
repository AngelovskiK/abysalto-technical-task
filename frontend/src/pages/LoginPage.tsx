import { useEffect } from 'react'
import { Link, useLocation, useNavigate } from 'react-router-dom'
import { Button } from '../components/Common/Button'
import { useCartExperience } from '../hooks/useCartExperience'

interface LoginLocationState {
    from?: string
}

export function LoginPage() {
    const cartExperience = useCartExperience()
    const navigate = useNavigate()
    const location = useLocation()
    const state = location.state as LoginLocationState | null
    const targetPath = state?.from || '/'

    useEffect(() => {
        if (cartExperience.session) {
            navigate(targetPath, { replace: true })
        }
    }, [cartExperience.session, navigate, targetPath])

    return (
        <main className="mx-auto w-full max-w-xl flex-1">
            <section className="rounded-[32px] bg-white p-6 shadow-panel sm:p-8">
                <p className="text-sm uppercase tracking-[0.3em] text-moss">Local Auth</p>
                <h2 className="mt-3 text-3xl font-semibold text-ink">Sign in to continue</h2>
                <p className="mt-3 text-sm leading-6 text-slate-600">
                    Use any name and email to request a local token from the backend. Protected routes like cart require an active session.
                </p>

                <form className="mt-6 space-y-4" onSubmit={cartExperience.handleLogin}>
                    <label className="block text-sm text-slate-700">
                        Name
                        <input
                            className="mt-2 w-full rounded-2xl border border-slate-200 bg-white px-4 py-3 outline-none transition focus:border-ink"
                            value={cartExperience.name}
                            onChange={event => cartExperience.setName(event.target.value)}
                            placeholder="Demo Shopper"
                            required
                        />
                    </label>
                    <label className="block text-sm text-slate-700">
                        Email
                        <input
                            className="mt-2 w-full rounded-2xl border border-slate-200 bg-white px-4 py-3 outline-none transition focus:border-ink"
                            type="email"
                            value={cartExperience.email}
                            onChange={event => cartExperience.setEmail(event.target.value)}
                            placeholder="shopper@abysalto.dev"
                            required
                        />
                    </label>
                    {cartExperience.authMessage ? <p className="text-sm text-rose-600">{cartExperience.authMessage}</p> : null}
                    <Button className="w-full" isLoading={cartExperience.loginPending} type="submit">
                        Sign In
                    </Button>
                </form>

                <Link to="/" className="mt-5 inline-flex text-sm font-medium text-slate-600 transition hover:text-slate-900">
                    Back to home
                </Link>
            </section>
        </main>
    )
}
