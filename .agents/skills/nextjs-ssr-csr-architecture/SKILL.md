---
name: nextjs-ssr-csr-architecture
description: Next.js Server vs Client Component architectural decisions for content-heavy sites. Use when choosing between SSR/SSG and CSR patterns, optimizing performance and SEO, implementing streaming and caching strategies, or refactoring components for better Core Web Vitals.
---

# Next.js SSR vs CSR Architecture

Architectural decision framework for Server vs Client Components in content-heavy sites with dynamic data requirements.

## When to Apply

- Choosing between Server and Client Components for content sections
- Optimizing First Contentful Paint and SEO for heavy content
- Implementing progressive loading and streaming strategies
- Refactoring existing components for better performance

## Critical Rules

**Server-First Pattern**: Default to Server Components, add Client Components only for interactivity

```tsx
// WRONG - Everything as Client Component
'use client'
export default function BlogPost({ post }) {
  return (
    <article>
      <h1>{post.title}</h1>
      <div>{post.content}</div>
      <LikeButton postId={post.id} />
    </article>
  )
}

// RIGHT - Server Component with nested Client Component
export default function BlogPost({ post }) {
  return (
    <article>
      <h1>{post.title}</h1>
      <div>{post.content}</div>
      <LikeButton postId={post.id} />
    </article>
  )
}
```

**Data Fetching Location**: Fetch close to the data source in Server Components

```tsx
// WRONG - Client-side fetching for static content
'use client'
export default function PostList() {
  const [posts, setPosts] = useState([])
  useEffect(() => {
    fetch('/api/posts').then(res => res.json()).then(setPosts)
  }, [])
  
  return posts.map(post => <PostCard key={post.id} post={post} />)
}

// RIGHT - Server Component data fetching
async function getPosts() {
  const res = await fetch('https://api.example.com/posts')
  return res.json()
}

export default async function PostList() {
  const posts = await getPosts()
  return posts.map(post => <PostCard key={post.id} post={post} />)
}
```

## Key Patterns

### Static Generation with ISR

```tsx
// Enable ISR with revalidation
export const revalidate = 3600 // 1 hour

async function getPost(id: string) {
  const res = await fetch(`https://api.example.com/posts/${id}`)
  return res.json()
}

export async function generateStaticParams() {
  const posts = await fetch('https://api.example.com/posts').then(res => res.json())
  return posts.map(post => ({ id: String(post.id) }))
}

export default async function Post({ params }: { params: Promise<{ id: string }> }) {
  const { id } = await params
  const post = await getPost(id)
  
  return (
    <article>
      <h1>{post.title}</h1>
      <div>{post.content}</div>
    </article>
  )
}
```

### Streaming for Performance

```tsx
import { Suspense } from 'react'

// Don't await - pass promise to enable streaming
function getAnalytics() {
  return fetch('https://api.example.com/analytics')
}

export default function Dashboard() {
  const analyticsPromise = getAnalytics()
  
  return (
    <div>
      <Header /> {/* Renders immediately */}
      <Suspense fallback={<AnalyticsSkeleton />}>
        <Analytics dataPromise={analyticsPromise} />
      </Suspense>
    </div>
  )
}
```

### Progressive Component Loading

```tsx
import dynamic from 'next/dynamic'

// Lazy load heavy interactive components
const CommentSection = dynamic(() => import('./comments'), {
  loading: () => <CommentsSkeleton />,
  ssr: false // Disable SSR for client-only components
})

export default function Article({ post }) {
  return (
    <article>
      <h1>{post.title}</h1>
      <div>{post.content}</div>
      <CommentSection postId={post.id} />
    </article>
  )
}
```

### Cache Optimization

```tsx
// Static content with cache tags
async function getProducts() {
  const res = await fetch('https://api.example.com/products', {
    next: { tags: ['products'] }
  })
  return res.json()
}

// On-demand revalidation
'use server'
import { revalidateTag } from 'next/cache'

export async function updateProduct() {
  // Update product...
  revalidateTag('products')
}
```

## Rendering Strategy Decision Tree

**Static Generation (SSG)**: Content changes infrequently, SEO critical
- Blog posts, documentation, marketing pages
- Use `generateStaticParams()` + ISR for updates

**Dynamic Rendering (SSR)**: Personalized content, real-time data
- User dashboards, search results, live feeds
- Use Server Components with `{ cache: 'no-store' }`

**Hybrid Approach**: Mix static shell with dynamic content
- E-commerce pages, social feeds
- Use Partial Prerendering (PPR) with Suspense boundaries

## Common Mistakes

- **Over-clientification**: Making entire pages Client Components for minor interactivity
- **Waterfall fetching**: Sequential data fetching instead of parallel promises
- **Missing Suspense boundaries**: Blocking entire pages for slow data fetching
- **Ignoring cache strategies**: Not using ISR or on-demand revalidation for content sites