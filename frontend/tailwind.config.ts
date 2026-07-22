import type { Config } from 'tailwindcss'

const config: Config = {
    content: ['./index.html', './src/**/*.{ts,tsx}'],
    theme: {
        extend: {
            colors: {
                ink: '#14213d',
                sand: '#f7f3eb',
                ember: '#f25f4c',
                gold: '#f7b267',
                moss: '#1f7a8c',
            },
            boxShadow: {
                panel: '0 28px 80px rgba(20, 33, 61, 0.12)',
            },
            fontFamily: {
                sans: ['"Sora"', 'ui-sans-serif', 'system-ui', 'sans-serif'],
            },
        },
    },
    plugins: [],
}

export default config