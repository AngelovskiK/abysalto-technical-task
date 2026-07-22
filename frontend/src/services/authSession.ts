import type { AuthSession } from '../types'

export const authSessionStorageKey = 'abysalto.auth.session'

function isAuthSession(value: unknown): value is AuthSession {
    if (!value || typeof value !== 'object') {
        return false
    }

    const candidate = value as Partial<AuthSession>

    return [candidate.userId, candidate.email, candidate.name, candidate.token].every(
        entry => typeof entry === 'string' && entry.trim().length > 0,
    )
}

export function readStoredAuthSession(): AuthSession | null {
    if (typeof window === 'undefined') {
        return null
    }

    const rawValue = window.localStorage.getItem(authSessionStorageKey)
    if (!rawValue) {
        return null
    }

    try {
        const parsedValue = JSON.parse(rawValue) as unknown
        return isAuthSession(parsedValue) ? parsedValue : null
    } catch {
        return null
    }
}