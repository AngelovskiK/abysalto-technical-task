import type { ButtonHTMLAttributes, PropsWithChildren } from 'react'

interface ButtonProps extends ButtonHTMLAttributes<HTMLButtonElement> {
    variant?: 'primary' | 'secondary' | 'ghost'
    isLoading?: boolean
}

const variantClasses: Record<NonNullable<ButtonProps['variant']>, string> = {
    primary: 'bg-ink text-white hover:bg-slate-800',
    secondary: 'bg-gold text-ink hover:bg-amber-300',
    ghost: 'bg-transparent text-ink hover:bg-slate-100',
}

export function Button({
    children,
    className = '',
    disabled,
    isLoading = false,
    variant = 'secondary',
    ...props
}: PropsWithChildren<ButtonProps>) {
    return (
        <button
            {...props}
            disabled={disabled || isLoading}
            className={`inline-flex items-center justify-center rounded-2xl px-4 py-3 text-sm font-semibold transition disabled:cursor-not-allowed disabled:opacity-60 ${variantClasses[variant]} ${className}`.trim()}
        >
            {isLoading ? 'Working...' : children}
        </button>
    )
}