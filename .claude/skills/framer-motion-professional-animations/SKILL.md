---
name: framer-motion-professional-animations
description: Professional Framer Motion animations for component interactions and micro-animations. Use when building polished UI interactions, creating smooth entrance/exit animations, implementing gesture-based components, orchestrating complex animation sequences, or optimizing animation performance for production React applications.
---

# Framer Motion Professional Animations

Create polished component interactions and micro-animations with optimized performance and professional workflows.

## When to Apply

- Building interactive UI components with hover/tap/drag gestures
- Creating entrance/exit animations for components or page transitions
- Implementing complex animation sequences with proper timing coordination
- Optimizing animation performance for production applications
- Orchestrating staggered animations across multiple elements

## Critical Rules

**Layout Animation Conflicts**: Layout animations can override optimized appear animations

```jsx
// WRONG - Layout animations cancel WAAPI optimizations
<motion.div
  initial={{ opacity: 0 }}
  animate={{ opacity: 1 }}
  layout // This cancels the opacity optimization
  style={{ top: dynamicTop }}
/>

// RIGHT - Use layout="position" for position-only animations
<motion.div
  initial={{ opacity: 0 }}
  animate={{ opacity: 1 }}
  layout="position" // Only animates position changes
  style={{ top: dynamicTop }}
/>
```

**Spring Physics Precision**: Use specific spring parameters for consistent behavior

```jsx
// WRONG - Generic spring settings
<motion.div animate={{ x: 100 }} transition={{ type: "spring" }} />

// RIGHT - Calibrated spring physics
<motion.div
  animate={{ x: 100 }}
  transition={{
    type: "spring",
    stiffness: 300,
    damping: 30,
    mass: 1
  }}
/>
```

**Variants Inheritance**: Parent variants automatically propagate to children unless overridden

```jsx
// WRONG - Redundant variant definitions
<motion.div variants={container} initial="hidden" animate="visible">
  <motion.div variants={item} initial="hidden" animate="visible" />
</motion.div>

// RIGHT - Children inherit parent variant states
<motion.div variants={container} initial="hidden" animate="visible">
  <motion.div variants={item} /> {/* Inherits parent states */}
</motion.div>
```

## Key Patterns

### Gesture-Based Interactions

```jsx
import { motion, useDragControls } from "framer-motion"

const InteractiveCard = () => {
  const dragControls = useDragControls()
  
  return (
    <motion.div
      whileHover={{ scale: 1.02, y: -4 }}
      whileTap={{ scale: 0.98 }}
      drag="x"
      dragConstraints={{ left: -100, right: 100 }}
      dragElastic={0.1}
      dragMomentum={false}
      onDragEnd={(_, info) => {
        if (Math.abs(info.offset.x) > 50) {
          // Trigger action on sufficient drag
        }
      }}
    />
  )
}
```

### Orchestrated Stagger Animations

```jsx
const containerVariants = {
  hidden: { opacity: 0 },
  visible: {
    opacity: 1,
    transition: {
      delayChildren: 0.2,
      staggerChildren: 0.1,
      when: "beforeChildren"
    }
  }
}

const itemVariants = {
  hidden: { y: 20, opacity: 0 },
  visible: {
    y: 0,
    opacity: 1,
    transition: { type: "spring", stiffness: 300, damping: 24 }
  }
}

<motion.ul variants={containerVariants} initial="hidden" animate="visible">
  {items.map(item => (
    <motion.li key={item.id} variants={itemVariants}>
      {item.content}
    </motion.li>
  ))}
</motion.ul>
```

### Height Auto Animation

```jsx
const AccordionItem = ({ isExpanded }) => (
  <motion.div
    initial="collapsed"
    animate={isExpanded ? "open" : "collapsed"}
    variants={{
      open: { opacity: 1, height: "auto" },
      collapsed: { opacity: 0, height: 0 }
    }}
    transition={{ duration: 0.3, ease: [0.04, 0.62, 0.23, 0.98] }}
    style={{ overflow: "hidden" }} // Required for height animation
  >
    <div>Content that expands</div>
  </motion.div>
)
```

### Exit Animations with Mode Control

```jsx
import { AnimatePresence } from "framer-motion"

<AnimatePresence mode="wait" onExitComplete={() => setLoading(false)}>
  <motion.div
    key={currentPage}
    initial={{ opacity: 0, x: 50 }}
    animate={{ opacity: 1, x: 0 }}
    exit={{ opacity: 0, x: -50 }}
    transition={{ duration: 0.2 }}
  >
    {pageContent}
  </motion.div>
</AnimatePresence>
```

### Scroll-Triggered Animations

```jsx
import { motion, useInView } from "framer-motion"
import { useRef } from "react"

const ScrollReveal = ({ children }) => {
  const ref = useRef(null)
  const isInView = useInView(ref, { 
    once: true, 
    amount: 0.3,
    margin: "-100px 0px" 
  })

  return (
    <motion.div
      ref={ref}
      initial={{ opacity: 0, y: 75 }}
      animate={isInView ? { opacity: 1, y: 0 } : { opacity: 0, y: 75 }}
      transition={{ duration: 0.6, ease: [0.22, 1, 0.36, 1] }}
    >
      {children}
    </motion.div>
  )
}
```

## Performance Optimization

**Transform Properties**: Use transform properties for GPU acceleration
```jsx
// Prefer x, y, scale, rotate over left, top, width, height
<motion.div animate={{ x: 100, scale: 1.2 }} />
```

**MotionValues for Reactive Animation**: Use `useSpring` for smooth following behaviors
```jsx
import { useSpring, useMotionValue } from "framer-motion"

const mouseX = useMotionValue(0)
const springX = useSpring(mouseX, { stiffness: 400, damping: 25 })
```

**Layout Prop Selective Usage**: Apply `layout` only when necessary
```jsx
// Only use layout when size/position actually changes
<motion.div layout={isExpanded} />
```

## Common Mistakes

- **Missing overflow: hidden** on height animations — Required for clean expand/collapse
- **Animating non-transform properties** — Use x/y instead of left/top for performance
- **Forgetting AnimatePresence** around conditional components — Exit animations won't work
- **Using layout with optimized appear** — Layout animations cancel WAAPI optimizations
- **Not setting restSpeed/restDelta** for springs — Animations may never complete
- **Missing key prop in lists** — Can cause animation glitches during reordering