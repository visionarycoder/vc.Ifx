---
title: Fluent 2 Vue 3 Implementation Patterns
doc_type: reference
status: active
last_updated: 2026-08-30
---

# Fluent 2 Vue 3 Implementation Patterns

Agent uses these patterns when implementing Fluent 2 design in Vue 3 web applications using Fluent UI Web Components or custom Fluent-styled components.

## Setup

### Install Fluent UI Web Components

```bash
npm install @fluentui/web-components
```

### Register Components (main.ts)

```typescript
import { provideFluentDesignSystem, fluentButton, fluentCard, fluentTextField } from '@fluentui/web-components';

provideFluentDesignSystem()
    .register(
        fluentButton(),
        fluentCard(),
        fluentTextField()
    );
```

### Or Use Fluent UI React Components (via Vue wrapper)

```bash
npm install @fluentui/react-components @fluentui/vue-icons
```

## Typography

Agent defines Fluent 2 typography tokens in CSS custom properties.

```css
/* styles/tokens/typography.css */
:root {
  /* Font family */
  --font-family-base: 'Segoe UI', -apple-system, BlinkMacSystemFont, 'Roboto', 'Helvetica Neue', sans-serif;
  
  /* Font sizes */
  --font-size-100: 10px;
  --font-size-200: 12px;
  --font-size-300: 14px;
  --font-size-400: 16px;
  --font-size-500: 20px;
  --font-size-600: 24px;
  --font-size-700: 28px;
  --font-size-800: 32px;
  --font-size-900: 40px;
  
  /* Line heights */
  --line-height-100: 14px;
  --line-height-200: 16px;
  --line-height-300: 20px;
  --line-height-400: 22px;
  --line-height-500: 28px;
  --line-height-600: 32px;
  --line-height-700: 36px;
  --line-height-800: 40px;
  --line-height-900: 52px;
  
  /* Font weights */
  --font-weight-regular: 400;
  --font-weight-semibold: 600;
  --font-weight-bold: 700;
}
```

### Typography Classes

```css
/* styles/typography.css */
.text-display {
  font-size: var(--font-size-900);
  line-height: var(--line-height-900);
  font-weight: var(--font-weight-semibold);
}

.text-title-large {
  font-size: var(--font-size-700);
  line-height: var(--line-height-700);
  font-weight: var(--font-weight-semibold);
}

.text-title {
  font-size: var(--font-size-600);
  line-height: var(--line-height-600);
  font-weight: var(--font-weight-semibold);
}

.text-subtitle {
  font-size: var(--font-size-500);
  line-height: var(--line-height-500);
  font-weight: var(--font-weight-semibold);
}

.text-body-strong {
  font-size: var(--font-size-300);
  line-height: var(--line-height-300);
  font-weight: var(--font-weight-semibold);
}

.text-body {
  font-size: var(--font-size-300);
  line-height: var(--line-height-300);
  font-weight: var(--font-weight-regular);
}

.text-caption {
  font-size: var(--font-size-200);
  line-height: var(--line-height-200);
  font-weight: var(--font-weight-regular);
  color: var(--color-text-secondary);
}
```

### Usage in Vue

```vue
<template>
  <div class="page">
    <h1 class="text-title-large">Dashboard</h1>
    <h2 class="text-subtitle">Recent Activity</h2>
    <p class="text-body">Welcome to your dashboard...</p>
    <p class="text-caption">Last updated: 2 hours ago</p>
  </div>
</template>
```

## Color Tokens

```css
/* styles/tokens/colors.css */
:root {
  /* Neutral colors (light theme) */
  --color-neutral-foreground-1: #242424;
  --color-neutral-foreground-2: #424242;
  --color-neutral-foreground-3: #616161;
  --color-neutral-background-1: #ffffff;
  --color-neutral-background-2: #fafafa;
  --color-neutral-background-3: #f5f5f5;
  
  /* Brand colors */
  --color-brand-background: #0078d4;
  --color-brand-background-hover: #106ebe;
  --color-brand-background-pressed: #005a9e;
  --color-brand-foreground: #0078d4;
  
  /* Semantic text */
  --color-text-primary: var(--color-neutral-foreground-1);
  --color-text-secondary: var(--color-neutral-foreground-2);
  --color-text-disabled: var(--color-neutral-foreground-3);
  
  /* Backgrounds */
  --color-background-primary: var(--color-neutral-background-1);
  --color-background-secondary: var(--color-neutral-background-2);
  --color-background-tertiary: var(--color-neutral-background-3);
  
  /* Borders */
  --color-border-default: #e0e0e0;
  --color-border-subtle: #f0f0f0;
  
  /* Shadows */
  --shadow-2: 0 0 2px rgba(0, 0, 0, 0.12), 0 2px 4px rgba(0, 0, 0, 0.14);
  --shadow-4: 0 0 2px rgba(0, 0, 0, 0.12), 0 4px 8px rgba(0, 0, 0, 0.14);
  --shadow-8: 0 0 2px rgba(0, 0, 0, 0.12), 0 8px 16px rgba(0, 0, 0, 0.14);
}

/* Dark theme */
@media (prefers-color-scheme: dark) {
  :root {
    --color-neutral-foreground-1: #ffffff;
    --color-neutral-foreground-2: #d1d1d1;
    --color-neutral-foreground-3: #a3a3a3;
    --color-neutral-background-1: #1f1f1f;
    --color-neutral-background-2: #292929;
    --color-neutral-background-3: #333333;
    
    --color-border-default: #424242;
    --color-border-subtle: #333333;
  }
}
```

## Spacing System

```css
/* styles/tokens/spacing.css */
:root {
  --spacing-horizontal-xxs: 2px;
  --spacing-horizontal-xs: 4px;
  --spacing-horizontal-s: 8px;
  --spacing-horizontal-sNudge: 6px;
  --spacing-horizontal-m: 12px;
  --spacing-horizontal-mNudge: 10px;
  --spacing-horizontal-l: 16px;
  --spacing-horizontal-xl: 20px;
  --spacing-horizontal-xxl: 24px;
  --spacing-horizontal-xxxl: 32px;
  
  --spacing-vertical-xxs: 2px;
  --spacing-vertical-xs: 4px;
  --spacing-vertical-s: 8px;
  --spacing-vertical-sNudge: 6px;
  --spacing-vertical-m: 12px;
  --spacing-vertical-mNudge: 10px;
  --spacing-vertical-l: 16px;
  --spacing-vertical-xl: 20px;
  --spacing-vertical-xxl: 24px;
  --spacing-vertical-xxxl: 32px;
}
```

## Button Component

```vue
<script setup lang="ts">
interface Props {
  appearance?: 'primary' | 'secondary' | 'subtle' | 'transparent';
  size?: 'small' | 'medium' | 'large';
  disabled?: boolean;
  loading?: boolean;
}

const props = withDefaults(defineProps<Props>(), {
  appearance: 'secondary',
  size: 'medium',
  disabled: false,
  loading: false
});

const emit = defineEmits<{
  click: [event: MouseEvent];
}>();
</script>

<template>
  <button 
    :class="['fluent-button', `fluent-button--${appearance}`, `fluent-button--${size}`]"
    :disabled="disabled || loading"
    @click="emit('click', $event)"
  >
    <span v-if="loading" class="fluent-button__spinner">
      <svg class="spinner" viewBox="0 0 20 20">
        <circle cx="10" cy="10" r="8" />
      </svg>
    </span>
    <slot />
  </button>
</template>

<style scoped>
.fluent-button {
  font-family: var(--font-family-base);
  font-weight: var(--font-weight-semibold);
  border-radius: 4px;
  border: 1px solid transparent;
  cursor: pointer;
  display: inline-flex;
  align-items: center;
  justify-content: center;
  gap: var(--spacing-horizontal-s);
  transition: all 0.1s ease;
}

.fluent-button--small {
  font-size: var(--font-size-200);
  padding: 4px 12px;
  min-height: 24px;
}

.fluent-button--medium {
  font-size: var(--font-size-300);
  padding: 8px 16px;
  min-height: 32px;
}

.fluent-button--large {
  font-size: var(--font-size-400);
  padding: 10px 20px;
  min-height: 40px;
}

.fluent-button--primary {
  background: var(--color-brand-background);
  color: white;
}

.fluent-button--primary:hover:not(:disabled) {
  background: var(--color-brand-background-hover);
}

.fluent-button--primary:active:not(:disabled) {
  background: var(--color-brand-background-pressed);
}

.fluent-button--secondary {
  background: var(--color-background-primary);
  border-color: var(--color-border-default);
  color: var(--color-text-primary);
}

.fluent-button--secondary:hover:not(:disabled) {
  background: var(--color-background-secondary);
}

.fluent-button--subtle {
  background: transparent;
  color: var(--color-text-primary);
}

.fluent-button--subtle:hover:not(:disabled) {
  background: var(--color-background-secondary);
}

.fluent-button:disabled {
  opacity: 0.4;
  cursor: not-allowed;
}

.spinner {
  width: 16px;
  height: 16px;
  animation: spin 1s linear infinite;
}

@keyframes spin {
  to { transform: rotate(360deg); }
}
</style>
```

## Card Component

```vue
<script setup lang="ts">
interface Props {
  elevation?: 'none' | 'small' | 'medium' | 'large';
  padding?: 'none' | 'small' | 'medium' | 'large';
}

const props = withDefaults(defineProps<Props>(), {
  elevation: 'small',
  padding: 'medium'
});
</script>

<template>
  <div :class="['fluent-card', `fluent-card--${elevation}`, `fluent-card--padding-${padding}`]">
    <slot />
  </div>
</template>

<style scoped>
.fluent-card {
  background: var(--color-background-primary);
  border-radius: 8px;
  border: 1px solid var(--color-border-subtle);
}

.fluent-card--none {
  box-shadow: none;
}

.fluent-card--small {
  box-shadow: var(--shadow-2);
}

.fluent-card--medium {
  box-shadow: var(--shadow-4);
}

.fluent-card--large {
  box-shadow: var(--shadow-8);
}

.fluent-card--padding-none {
  padding: 0;
}

.fluent-card--padding-small {
  padding: var(--spacing-vertical-m) var(--spacing-horizontal-m);
}

.fluent-card--padding-medium {
  padding: var(--spacing-vertical-l) var(--spacing-horizontal-l);
}

.fluent-card--padding-large {
  padding: var(--spacing-vertical-xxl) var(--spacing-horizontal-xxl);
}
</style>
```

## Layout Components

### Stack

```vue
<script setup lang="ts">
interface Props {
  direction?: 'horizontal' | 'vertical';
  spacing?: 'xs' | 's' | 'm' | 'l' | 'xl' | 'xxl';
  align?: 'start' | 'center' | 'end' | 'stretch';
  justify?: 'start' | 'center' | 'end' | 'space-between';
}

const props = withDefaults(defineProps<Props>(), {
  direction: 'vertical',
  spacing: 'm',
  align: 'stretch'
});
</script>

<template>
  <div 
    :class="['fluent-stack', `fluent-stack--${direction}`, `fluent-stack--spacing-${spacing}`]"
    :style="{
      alignItems: align,
      justifyContent: justify
    }"
  >
    <slot />
  </div>
</template>

<style scoped>
.fluent-stack {
  display: flex;
}

.fluent-stack--vertical {
  flex-direction: column;
}

.fluent-stack--horizontal {
  flex-direction: row;
}

.fluent-stack--spacing-xs { gap: var(--spacing-vertical-xs); }
.fluent-stack--spacing-s { gap: var(--spacing-vertical-s); }
.fluent-stack--spacing-m { gap: var(--spacing-vertical-m); }
.fluent-stack--spacing-l { gap: var(--spacing-vertical-l); }
.fluent-stack--spacing-xl { gap: var(--spacing-vertical-xl); }
.fluent-stack--spacing-xxl { gap: var(--spacing-vertical-xxl); }
</style>
```

## Page Layout Example

```vue
<script setup lang="ts">
import { ref } from 'vue';
import FluentButton from '@/components/FluentButton.vue';
import FluentCard from '@/components/FluentCard.vue';
import Stack from '@/components/Stack.vue';

const items = ref([
  { id: 1, title: 'Item 1', description: 'Description 1' },
  { id: 2, title: 'Item 2', description: 'Description 2' }
]);
</script>

<template>
  <div class="page">
    <header class="page-header">
      <h1 class="text-title-large">Dashboard</h1>
      <FluentButton appearance="primary">
        Add Item
      </FluentButton>
    </header>
    
    <Stack spacing="l" class="page-content">
      <FluentCard v-for="item in items" :key="item.id">
        <Stack spacing="s">
          <h3 class="text-subtitle">{{ item.title }}</h3>
          <p class="text-body">{{ item.description }}</p>
        </Stack>
      </FluentCard>
    </Stack>
  </div>
</template>

<style scoped>
.page {
  padding: var(--spacing-vertical-xxl) var(--spacing-horizontal-xxl);
  max-width: 1200px;
  margin: 0 auto;
}

.page-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: var(--spacing-vertical-xxl);
}

.page-content {
  /* Stack component handles spacing */
}
</style>
```

## Form Components

```vue
<script setup lang="ts">
import { ref } from 'vue';

const formData = ref({
  name: '',
  email: '',
  category: ''
});
</script>

<template>
  <form class="form" @submit.prevent>
    <Stack spacing="l">
      <div class="form-field">
        <label class="text-body-strong">Name</label>
        <input 
          v-model="formData.name"
          type="text"
          class="fluent-input"
          placeholder="Enter name"
        />
      </div>
      
      <div class="form-field">
        <label class="text-body-strong">Email</label>
        <input 
          v-model="formData.email"
          type="email"
          class="fluent-input"
          placeholder="Enter email"
        />
      </div>
      
      <div class="form-field">
        <label class="text-body-strong">Category</label>
        <select v-model="formData.category" class="fluent-select">
          <option value="">Select category</option>
          <option value="cat1">Category 1</option>
          <option value="cat2">Category 2</option>
        </select>
      </div>
      
      <FluentButton appearance="primary" type="submit">
        Save
      </FluentButton>
    </Stack>
  </form>
</template>

<style scoped>
.form-field {
  display: flex;
  flex-direction: column;
  gap: var(--spacing-vertical-xs);
}

.fluent-input,
.fluent-select {
  font-family: var(--font-family-base);
  font-size: var(--font-size-300);
  padding: 8px 12px;
  border: 1px solid var(--color-border-default);
  border-radius: 4px;
  background: var(--color-background-primary);
  color: var(--color-text-primary);
  transition: border-color 0.1s ease;
}

.fluent-input:focus,
.fluent-select:focus {
  outline: none;
  border-color: var(--color-brand-foreground);
  box-shadow: 0 0 0 1px var(--color-brand-foreground);
}
</style>
```

## Icons (Fluent UI System Icons)

```bash
npm install @fluentui/vue-icons
```

```vue
<script setup lang="ts">
import { Home20Regular, Settings20Regular } from '@fluentui/vue-icons';
</script>

<template>
  <Stack direction="horizontal" spacing="m">
    <Home20Regular />
    <span>Home</span>
  </Stack>
</template>
```

## Composables for Fluent Patterns

```typescript
// composables/useTheme.ts
import { ref, onMounted } from 'vue';

export function useTheme() {
  const theme = ref<'light' | 'dark'>('light');
  
  const setTheme = (newTheme: 'light' | 'dark') => {
    theme.value = newTheme;
    document.documentElement.setAttribute('data-theme', newTheme);
  };
  
  const toggleTheme = () => {
    setTheme(theme.value === 'light' ? 'dark' : 'light');
  };
  
  onMounted(() => {
    const prefersDark = window.matchMedia('(prefers-color-scheme: dark)').matches;
    setTheme(prefersDark ? 'dark' : 'light');
  });
  
  return { theme, setTheme, toggleTheme };
}
```

## Verification Checklist

| Check | Test | Pass |
|-------|------|------|
| Typography | Review one page. | Clear hierarchy with Fluent 2 type ramp. |
| Color tokens | Switch themes. | All colors use CSS custom properties, readable in both themes. |
| Spacing | Inspect layout. | Consistent spacing using token variables. |
| Components | Test buttons, cards, forms. | Components match Fluent 2 design patterns. |
| Responsive | Test on mobile, tablet, desktop. | Layout adapts appropriately. |
| Accessibility | Tab through UI, use screen reader. | Semantic HTML, ARIA labels, keyboard navigation work. |

## References

- [Fluent UI Web Components](https://docs.microsoft.com/fluent-ui/web-components)
- [Fluent UI React](https://react.fluentui.dev)
- [Fluent 2 Design Tokens](https://fluent2.microsoft.design/tokens)
- [Fluent UI System Icons](https://github.com/microsoft/fluentui-system-icons)
- [Vue 3 Composition API](https://vuejs.org/guide/extras/composition-api-faq.html)
