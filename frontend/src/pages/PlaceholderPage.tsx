import { findNavItem } from '../app/navigation'

export function PlaceholderPage({ pathname }: { pathname: string }) {
  const item = findNavItem(pathname)
  return (
    <section>
      <h1 className="page-title">{item.label}</h1>
      <p className="page-hint">{item.hint}</p>
    </section>
  )
}
