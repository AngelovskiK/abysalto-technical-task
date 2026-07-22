interface RouteSkeletonProps {
    blocks?: number
}

export function RouteSkeleton({ blocks = 3 }: RouteSkeletonProps) {
    return (
        <main className="flex-1 rounded-[32px] bg-white p-6 shadow-panel sm:p-8">
            <div className="animate-pulse space-y-4">
                <div className="h-3 w-40 rounded bg-slate-200" />
                <div className="h-10 w-3/5 rounded bg-slate-200" />
                <div className="h-4 w-full rounded bg-slate-200" />
                <div className="h-4 w-5/6 rounded bg-slate-200" />
                {Array.from({ length: blocks }).map((_, index) => (
                    <div key={index} className="h-24 w-full rounded-2xl bg-slate-100" />
                ))}
            </div>
        </main>
    )
}
