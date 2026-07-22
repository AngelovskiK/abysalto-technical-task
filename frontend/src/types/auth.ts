export interface AuthSession {
    userId: string
    email: string
    name: string
    token: string
}

export interface LoginRequest {
    email: string
    name: string
}

export interface LoginResponse {
    userId: string
    email: string
    token: string
}