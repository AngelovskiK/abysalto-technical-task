import { createContext, useContext, useEffect, useState } from 'react'
import type { PropsWithChildren } from 'react'
import type { AuthSession, LoginResponse } from '../types'
import { authSessionStorageKey, readStoredAuthSession } from '../services/authSession'

interface AuthContextValue {
    session: AuthSession | null
    signIn: (response: LoginResponse, name?: string) => void
    signOut: () => void
}

const AuthContext = createContext<AuthContextValue | undefined>(undefined)

export function AuthProvider({ children }: PropsWithChildren) {
    const [session, setSession] = useState<AuthSession | null>(null)

    useEffect(() => {
        const storedSession = readStoredAuthSession()
        if (!storedSession) {
            window.localStorage.removeItem(authSessionStorageKey)
            return
        }

        setSession(storedSession)
    }, [])

    const signIn = (response: LoginResponse, name = response.email.split('@')[0]) => {
        if (!response.token.trim()) {
            throw new Error('Login response did not contain a usable bearer token.')
        }

        const nextSession: AuthSession = {
            userId: response.userId,
            email: response.email,
            token: response.token,
            name,
        }

        setSession(nextSession)
        window.localStorage.setItem(authSessionStorageKey, JSON.stringify(nextSession))
    }

    const signOut = () => {
        setSession(null)
        window.localStorage.removeItem(authSessionStorageKey)
    }

    return <AuthContext.Provider value={{ session, signIn, signOut }}>{children}</AuthContext.Provider>
}

export function useAuth() {
    const context = useContext(AuthContext)

    if (!context) {
        throw new Error('useAuth must be used inside AuthProvider')
    }

    return context
}