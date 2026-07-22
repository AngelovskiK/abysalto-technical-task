import { useEffect, useRef, useState } from 'react'
import { Link } from 'react-router-dom'
import type { AuthSession } from '../../types'

interface AccountMenuProps {
    session: AuthSession | null
    onSignOut: () => void
}

export function AccountMenu({ session, onSignOut }: AccountMenuProps) {
    const [open, setOpen] = useState(false)
    const areaRef = useRef<HTMLDivElement | null>(null)
    const userInitial = session?.name?.trim().charAt(0).toUpperCase() || 'G'

    useEffect(() => {
        if (!open) {
            return
        }

        function handlePointerDown(event: MouseEvent | TouchEvent) {
            const target = event.target as Node | null
            if (target && areaRef.current && !areaRef.current.contains(target)) {
                setOpen(false)
            }
        }

        function handleEscape(event: KeyboardEvent) {
            if (event.key === 'Escape') {
                setOpen(false)
            }
        }

        document.addEventListener('mousedown', handlePointerDown)
        document.addEventListener('touchstart', handlePointerDown)
        document.addEventListener('keydown', handleEscape)

        return () => {
            document.removeEventListener('mousedown', handlePointerDown)
            document.removeEventListener('touchstart', handlePointerDown)
            document.removeEventListener('keydown', handleEscape)
        }
    }, [open])

    return (
        <div className="relative" ref={areaRef}>
            <button
                type="button"
                className="inline-flex items-center gap-2 rounded-full border border-slate-200 px-3 py-2 text-slate-700 transition hover:bg-slate-100"
                aria-label="Open account menu"
                aria-expanded={open}
                aria-controls="account-menu-dropdown"
                onClick={() => setOpen(value => !value)}
            >
                <span className="inline-flex h-7 w-7 items-center justify-center rounded-full bg-ink text-xs font-semibold text-white">{userInitial}</span>
                <span className="max-w-24 truncate text-sm font-medium">{session ? session.name : 'Guest'}</span>
            </button>

            {open ? (
                <div
                    id="account-menu-dropdown"
                    className="cart-dropdown-enter absolute right-0 top-14 z-[120] w-56 overflow-hidden rounded-2xl border border-slate-200 bg-white shadow-2xl"
                >
                    {session ? (
                        <>
                            <div className="border-b border-slate-100 px-4 py-3">
                                <p className="text-sm font-semibold text-slate-800">{session.name}</p>
                                <p className="text-xs text-slate-500">{session.email}</p>
                            </div>
                            <button
                                type="button"
                                className="w-full px-4 py-3 text-left text-sm font-medium text-rose-600 transition hover:bg-rose-50"
                                onClick={() => {
                                    onSignOut()
                                    setOpen(false)
                                }}
                            >
                                Sign Out
                            </button>
                        </>
                    ) : (
                        <Link
                            to="/login"
                            className="block px-4 py-3 text-sm font-medium text-ink transition hover:bg-slate-50"
                            onClick={() => setOpen(false)}
                        >
                            Sign In
                        </Link>
                    )}
                </div>
            ) : null}
        </div>
    )
}
