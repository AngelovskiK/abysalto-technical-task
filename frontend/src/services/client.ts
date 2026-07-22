import { readStoredAuthSession } from './authSession'

interface ApiProblemDetails {
    title?: string
    detail?: string
    status?: number
}

export class ApiError extends Error {
    readonly status: number
    readonly code?: string

    constructor(message: string, status: number, code?: string) {
        super(message)
        this.name = 'ApiError'
        this.status = status
        this.code = code
    }
}

const apiBaseUrl = (import.meta.env.VITE_API_BASE_URL as string | undefined)?.trim() || ''
let unauthorizedHandler: (() => void) | null = null

export function setUnauthorizedHandler(handler: (() => void) | null) {
    unauthorizedHandler = handler
}

function buildUrl(path: string) {
    if (!apiBaseUrl) {
        return path
    }

    return new URL(path, apiBaseUrl).toString()
}

export async function request<T>(path: string, init: RequestInit = {}, token?: string): Promise<T> {
    const headers = new Headers(init.headers)
    const trimmedToken = token?.trim()
    const storedToken = readStoredAuthSession()?.token.trim()
    const resolvedToken = trimmedToken || storedToken

    if (!headers.has('Content-Type') && init.body) {
        headers.set('Content-Type', 'application/json')
    }

    if (resolvedToken) {
        headers.set('Authorization', `Bearer ${resolvedToken}`)
    }

    const response = await fetch(buildUrl(path), {
        ...init,
        headers,
    })

    if (!response.ok) {
        const payload = (await response.json().catch(() => null)) as ApiProblemDetails | null

        if (response.status === 401) {
            unauthorizedHandler?.()
        }

        throw new ApiError(
            payload?.detail || `Request failed with status ${response.status}.`,
            response.status,
            payload?.title,
        )
    }

    if (response.status === 204) {
        return undefined as T
    }

    return (await response.json()) as T
}