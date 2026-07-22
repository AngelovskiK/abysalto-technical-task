export interface Product {
    productId: string
    productName: string
    unitPrice: number
    imageUrl?: string | null
    description: string
    tag: string
    accentClass: string
}