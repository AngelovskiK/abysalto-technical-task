import { useEffect, useRef, useState } from 'react'
import type { Cart } from '../../types'
import { CartDropdown } from './CartDropdown'

interface CartMenuProps {
    cart: Cart | null | undefined
    itemCount: number
    isAuthenticated: boolean
    isLoading: boolean
    errorMessage: string | null
}

export function CartMenu({ cart, itemCount, isAuthenticated, isLoading, errorMessage }: CartMenuProps) {
    const [open, setOpen] = useState(false)
    const areaRef = useRef<HTMLDivElement | null>(null)

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
                className="relative inline-flex h-11 w-11 items-center justify-center rounded-full border border-slate-200 text-slate-700 transition hover:bg-slate-100"
                aria-label="Open cart preview"
                aria-expanded={open}
                aria-controls="cart-preview-dropdown"
                onClick={() => setOpen(value => !value)}
            >
                <svg viewBox="0 0 24 24" className="h-5 w-5" fill="none" stroke="currentColor" strokeWidth="1.9">
                    <path d="M3 4h2l1.5 9.2a1.7 1.7 0 0 0 1.7 1.4h8.9a1.7 1.7 0 0 0 1.7-1.4L20 7H7" />
                    <circle cx="10" cy="19" r="1.5" />
                    <circle cx="17" cy="19" r="1.5" />
                </svg>
                {itemCount > 0 ? (
                    <span className="absolute -right-1 -top-1 inline-flex min-h-5 min-w-5 items-center justify-center rounded-full bg-ember px-1.5 text-xs font-semibold text-white">
                        {itemCount}
                    </span>
                ) : null}
            </button>

            {open ? (
                <div id="cart-preview-dropdown" className="cart-dropdown-enter absolute right-0 top-14 z-[120] origin-top-right">
                    <CartDropdown
                        cart={cart}
                        isAuthenticated={isAuthenticated}
                        isLoading={isLoading}
                        errorMessage={errorMessage}
                        onClose={() => setOpen(false)}
                    />
                </div>
            ) : null}
        </div>
    )
}
