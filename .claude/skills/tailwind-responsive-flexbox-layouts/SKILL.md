---
name: tailwind-responsive-flexbox-layouts
description: Mobile-first responsive flexbox layouts with Tailwind CSS. Use when building responsive components, creating mobile-first layouts, or implementing flexbox patterns that adapt across breakpoints. Covers flex direction, sizing, alignment, and spacing decisions for responsive design.
---

# Tailwind CSS Mobile-First Flexbox Layouts

Create responsive flexbox components with mobile-first breakpoint strategy.

## When to Apply

- Converting static layouts to responsive flexbox components
- Implementing mobile-first responsive navigation or card grids
- Building layouts that stack on mobile and arrange horizontally on larger screens

## Critical Rules

**Mobile-First Breakpoint Logic**: Apply base styles without prefixes, then layer responsive variants

```html
<!-- WRONG - Using sm: for mobile -->
<div class="sm:flex-col md:flex-row">

<!-- RIGHT - Mobile first, then responsive overrides -->
<div class="flex-col sm:flex-row">
```

**Flex Direction Switching**: Change direction at specific breakpoints for responsive behavior

```html
<!-- WRONG - No mobile consideration -->
<div class="flex flex-row gap-4">

<!-- RIGHT - Stack on mobile, row on larger screens -->
<div class="flex flex-col sm:flex-row gap-4">
```

**Flex Sizing Control**: Use specific flex utilities instead of generic width classes

```html
<!-- WRONG - Fixed widths that don't adapt -->
<div class="w-1/2">Content</div>

<!-- RIGHT - Responsive flex behavior -->
<div class="w-full sm:flex-1">Content</div>
```

## Key Patterns

### Mobile Stack → Desktop Row

```html
<div class="flex flex-col sm:flex-row gap-4">
  <div class="w-full sm:w-1/2">Left content</div>
  <div class="w-full sm:w-1/2">Right content</div>
</div>
```

### Responsive Flex Sizing

```html
<nav class="flex">
  <div class="flex-none w-14">Logo</div>
  <div class="flex-1">Navigation</div>
  <div class="flex-none">Actions</div>
</nav>
```

### Adaptive Alignment

```html
<!-- Center on mobile, space-between on desktop -->
<div class="flex flex-col sm:flex-row justify-center sm:justify-between items-center gap-4">
  <h1>Title</h1>
  <button>Action</button>
</div>
```

### Responsive Card Grid

```html
<div class="flex flex-col sm:flex-row sm:flex-wrap gap-4">
  <article class="flex-1 sm:flex-none sm:w-64">Card 1</article>
  <article class="flex-1 sm:flex-none sm:w-64">Card 2</article>
  <article class="flex-1 sm:flex-none sm:w-64">Card 3</article>
</div>
```

### Complex Responsive Layout

```html
<!-- Mobile: Stack everything -->
<!-- Tablet: Main content + sidebar -->
<!-- Desktop: Header bar + content below -->
<div class="flex flex-col lg:flex-row min-h-screen">
  <header class="flex flex-col sm:flex-row lg:flex-col lg:w-64 p-4 gap-4">
    <div class="flex-none">Logo</div>
    <nav class="flex-1 lg:flex-none">Navigation</nav>
  </header>
  <main class="flex-1 p-4">
    <div class="flex flex-col md:flex-row gap-6">
      <article class="flex-1">Main content</article>
      <aside class="w-full md:w-80">Sidebar</aside>
    </div>
  </main>
</div>
```

## Breakpoint Reference

- **Base (0px)**: Mobile styles, no prefix
- **sm (640px)**: `sm:flex-row`, `sm:justify-between`
- **md (768px)**: `md:flex-wrap`, `md:gap-6`  
- **lg (1024px)**: `lg:flex-nowrap`, `lg:items-start`
- **xl (1280px)**: `xl:gap-8`, `xl:max-w-none`

## Common Mistakes

- **Using `sm:` for mobile** — `sm:` means 640px+, not small screens
- **Forgetting flex-wrap** — Items overflow instead of wrapping: add `flex-wrap` or `sm:flex-nowrap`
- **Fixed gaps everywhere** — Use responsive gaps: `gap-2 sm:gap-4 lg:gap-6`
- **Overriding flex-1** — Don't mix `flex-1` with fixed widths on same element