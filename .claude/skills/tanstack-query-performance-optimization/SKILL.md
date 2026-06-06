---
name: tanstack-query-performance-optimization
description: "Performance optimization patterns and complex query strategies for TanStack Query in React. Use when implementing dependent queries, infinite pagination, parallel data fetching, background sync patterns, or optimizing cache performance with staleTime, gcTime, and selective re-rendering techniques."
---

# TanStack Query Performance Optimization

Advanced patterns for optimizing data fetching performance and complex query orchestration in React applications.

## When to Apply

- Implementing dependent or waterfall queries that need optimization
- Building infinite scroll or pagination with performance constraints  
- Managing complex cache strategies with background refetching
- Orchestrating parallel queries with selective updates
- Optimizing re-renders through data transformation and selective subscriptions

## Critical Rules

**staleTime vs gcTime**: Configure based on data freshness needs

```tsx
// WRONG - immediate refetch on every render
const query = useQuery({
  queryKey: ['user'],
  queryFn: fetchUser,
  staleTime: 0, // default - always stale
  gcTime: 5 * 60 * 1000 // 5 minutes
})

// RIGHT - optimize for your use case
const query = useQuery({
  queryKey: ['user'],
  queryFn: fetchUser,
  staleTime: 5 * 60 * 1000, // 5 minutes fresh
  gcTime: 10 * 60 * 1000    // 10 minutes cached
})
```

**Dependent Queries**: Use enabled to control execution flow

```tsx
// WRONG - will fire immediately even without userIds
const { data: messages } = useQuery({
  queryKey: ['messages', userIds],
  queryFn: () => getMessages(userIds),
})

// RIGHT - wait for dependencies
const { data: messages } = useQuery({
  queryKey: ['messages', userIds],
  queryFn: () => getMessages(userIds),
  enabled: !!userIds?.length
})
```

**Select Optimization**: Transform data to prevent unnecessary re-renders

```tsx
// WRONG - component re-renders on any user object change
const { data: user } = useQuery({
  queryKey: ['user', id],
  queryFn: () => fetchUser(id)
})
const username = user?.name

// RIGHT - only re-render when name changes
const { data: username } = useQuery({
  queryKey: ['user', id],
  queryFn: () => fetchUser(id),
  select: (user) => user.name
})
```

## Key Patterns

### Dependent Queries with useQueries

```tsx
// Get user IDs first
const { data: userIds } = useQuery({
  queryKey: ['users'],
  queryFn: getUsersData,
  select: (users) => users.map(user => user.id)
})

// Then fetch messages for each user
const userMessages = useQueries({
  queries: userIds?.map(id => ({
    queryKey: ['messages', id],
    queryFn: () => getMessagesByUser(id),
    staleTime: 2 * 60 * 1000 // 2 minutes
  })) ?? []
})
```

### Infinite Queries with Performance Tuning

```tsx
const {
  data,
  fetchNextPage,
  hasNextPage,
  isFetchingNextPage
} = useInfiniteQuery({
  queryKey: ['projects'],
  queryFn: ({ pageParam = 0 }) => fetchProjects(pageParam),
  initialPageParam: 0,
  getNextPageParam: (lastPage) => lastPage.nextCursor,
  staleTime: 5 * 60 * 1000,
  gcTime: 10 * 60 * 1000,
  maxPages: 10 // Limit memory usage
})
```

### Background Refetching Configuration

```tsx
const query = useQuery({
  queryKey: ['portfolio'],
  queryFn: fetchPortfolio,
  staleTime: 30 * 1000,          // 30 seconds fresh
  refetchInterval: 60 * 1000,     // Poll every minute
  refetchIntervalInBackground: true, // Continue when tab inactive
  refetchOnWindowFocus: false,    // Disable focus refetch
  refetchOnReconnect: 'always'    // Always refetch on reconnect
})
```

### Optimistic Updates with Rollback

```tsx
const queryClient = useQueryClient()

const mutation = useMutation({
  mutationFn: updateTodo,
  onMutate: async (newTodo) => {
    await queryClient.cancelQueries({ queryKey: ['todos'] })
    
    const previousTodos = queryClient.getQueryData(['todos'])
    queryClient.setQueryData(['todos'], old => [...old, newTodo])
    
    return { previousTodos }
  },
  onError: (err, variables, context) => {
    queryClient.setQueryData(['todos'], context.previousTodos)
  },
  onSettled: () => {
    queryClient.invalidateQueries({ queryKey: ['todos'] })
  }
})
```

### Prefetching Strategies

```tsx
// Prefetch single query
await queryClient.prefetchQuery({
  queryKey: ['todos'],
  queryFn: fetchTodos,
  staleTime: 10 * 1000
})

// Prefetch multiple pages
await queryClient.prefetchInfiniteQuery({
  queryKey: ['projects'],
  queryFn: fetchProjects,
  initialPageParam: 0,
  getNextPageParam: (lastPage) => lastPage.nextCursor,
  pages: 3 // Prefetch first 3 pages
})
```

## Common Mistakes

- **Overusing invalidateQueries** — Use `setQueryData` for single updates instead of broad invalidation
- **Not setting staleTime** — Results in excessive background refetching and poor performance
- **Missing enabled conditions** — Dependent queries fire prematurely causing errors and waterfalls  
- **Ignoring select optimization** — Components re-render unnecessarily when only using subset of data
- **Default gcTime in SSR** — Set higher `staleTime` (60s+) to prevent immediate client-side refetching