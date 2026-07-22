import { createContext, useCallback, useContext, useMemo, useState } from 'react'
import type { PropsWithChildren } from 'react'

type ToastVariant = 'success' | 'info' | 'error'

interface ToastItem {
    id: number
    message: string
    variant: ToastVariant
}

interface ToastContextValue {
    showToast: (message: string, variant?: ToastVariant) => void
}

const toastStyles: Record<ToastVariant, string> = {
    success: 'border-emerald-200 bg-emerald-50 text-emerald-800',
    info: 'border-slate-200 bg-white text-slate-800',
    error: 'border-rose-200 bg-rose-50 text-rose-800',
}

const ToastContext = createContext<ToastContextValue | undefined>(undefined)

export function ToastProvider({ children }: PropsWithChildren) {
    const [toasts, setToasts] = useState<ToastItem[]>([])

    const showToast = useCallback((message: string, variant: ToastVariant = 'info') => {
        const nextToast: ToastItem = {
            id: Date.now() + Math.floor(Math.random() * 1000),
            message,
            variant,
        }

        setToasts(current => [...current, nextToast])

        window.setTimeout(() => {
            setToasts(current => current.filter(toast => toast.id !== nextToast.id))
        }, 2800)
    }, [])

    const value = useMemo(() => ({ showToast }), [showToast])

    return (
        <ToastContext.Provider value={value}>
            {children}

            <div className="pointer-events-none fixed right-4 top-4 z-[220] flex w-[min(22rem,calc(100vw-2rem))] flex-col gap-2">
                {toasts.map(toast => (
                    <div
                        key={toast.id}
                        className={`rounded-2xl border px-4 py-3 text-sm font-medium shadow-xl backdrop-blur ${toastStyles[toast.variant]}`}
                        role="status"
                        aria-live="polite"
                    >
                        {toast.message}
                    </div>
                ))}
            </div>
        </ToastContext.Provider>
    )
}

export function useToast() {
    const context = useContext(ToastContext)

    if (!context) {
        throw new Error('useToast must be used inside ToastProvider')
    }

    return context
}
