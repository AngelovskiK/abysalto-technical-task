import { useEffect, useRef } from 'react'
import type { ReactElement } from 'react'
import { useQueryClient } from '@tanstack/react-query'
import { Navigate, Route, Routes, useLocation, useNavigate } from 'react-router-dom'
import { useAuth } from './context/AuthContext'
import { RouteSkeleton } from './components/Common/RouteSkeleton'
import { useToast } from './context/ToastContext'
import { AppShell } from './layout/AppShell'
import { CartPage } from './pages/CartPage'
import { HomePage } from './pages/HomePage'
import { LoginPage } from './pages/LoginPage'
import { setUnauthorizedHandler } from './services/client'

function RequireAuth({ children }: { children: ReactElement }) {
    const { session, isLoading } = useAuth()
    const location = useLocation()

    if (isLoading) {
        return <RouteSkeleton blocks={2} />
    }

    if (!session) {
        return <Navigate replace to="/login" state={{ from: `${location.pathname}${location.search}` }} />
    }

    return children
}

function UnauthorizedRedirectHandler() {
    const navigate = useNavigate()
    const queryClient = useQueryClient()
    const { signOut } = useAuth()
    const { showToast } = useToast()
    const isHandlingUnauthorized = useRef(false)

    useEffect(() => {
        setUnauthorizedHandler(() => {
            if (isHandlingUnauthorized.current) {
                return
            }

            isHandlingUnauthorized.current = true
            signOut()
            queryClient.removeQueries({ queryKey: ['cart'] })
            showToast('Session expired. Please sign in again.', 'error')

            if (window.location.pathname !== '/login') {
                const currentPath = `${window.location.pathname}${window.location.search}`
                navigate('/login', { replace: true, state: { from: currentPath } })
            }

            window.setTimeout(() => {
                isHandlingUnauthorized.current = false
            }, 250)
        })

        return () => setUnauthorizedHandler(null)
    }, [navigate, queryClient, showToast, signOut])

    return null
}

export default function App() {
    return (
        <>
            <UnauthorizedRedirectHandler />

            <Routes>
                <Route element={<AppShell />}>
                    <Route path="/" element={<HomePage />} />
                    <Route path="/login" element={<LoginPage />} />
                    <Route
                        path="/cart"
                        element={
                            <RequireAuth>
                                <CartPage />
                            </RequireAuth>
                        }
                    />
                </Route>
                <Route path="*" element={<Navigate replace to="/" />} />
            </Routes>
        </>
    )
}