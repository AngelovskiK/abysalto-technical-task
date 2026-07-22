import { useEffect } from 'react'
import type { ReactElement } from 'react'
import { useQueryClient } from '@tanstack/react-query'
import { Navigate, Route, Routes, useLocation, useNavigate } from 'react-router-dom'
import { useAuth } from './context/AuthContext'
import { AppShell } from './layout/AppShell'
import { CartPage } from './pages/CartPage'
import { HomePage } from './pages/HomePage'
import { LoginPage } from './pages/LoginPage'
import { setUnauthorizedHandler } from './services/client'

function RequireAuth({ children }: { children: ReactElement }) {
    const { session } = useAuth()
    const location = useLocation()

    if (!session) {
        return <Navigate replace to="/login" state={{ from: `${location.pathname}${location.search}` }} />
    }

    return children
}

function UnauthorizedRedirectHandler() {
    const navigate = useNavigate()
    const queryClient = useQueryClient()
    const { signOut } = useAuth()

    useEffect(() => {
        setUnauthorizedHandler(() => {
            signOut()
            queryClient.removeQueries({ queryKey: ['cart'] })

            if (window.location.pathname !== '/login') {
                const currentPath = `${window.location.pathname}${window.location.search}`
                navigate('/login', { replace: true, state: { from: currentPath } })
            }
        })

        return () => setUnauthorizedHandler(null)
    }, [navigate, queryClient, signOut])

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