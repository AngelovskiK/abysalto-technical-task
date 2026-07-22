import type { LoginRequest, LoginResponse } from '../types'
import { request } from './client'

export function login(payload: LoginRequest) {
    return request<LoginResponse>('/api/auth/login', {
        method: 'POST',
        body: JSON.stringify(payload),
    })
}