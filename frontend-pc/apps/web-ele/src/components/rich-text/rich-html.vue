<script lang="ts" setup>
import { computed } from 'vue';

import DOMPurify from 'dompurify';

const props = withDefaults(
  defineProps<{
    html?: string;
  }>(),
  {
    html: '',
  },
);

const sanitizedHtml = computed(() =>
  DOMPurify.sanitize(props.html ?? '', {
    ADD_ATTR: ['class', 'data-file-id', 'style', 'target'],
    FORBID_TAGS: ['embed', 'iframe', 'object', 'script'],
  }),
);
</script>

<template>
  <div class="rich-html" v-html="sanitizedHtml"></div>
</template>

<style scoped>
.rich-html {
  line-height: 1.7;
  overflow-wrap: anywhere;
}

.rich-html :deep(img) {
  max-width: 100%;
  height: auto;
}

.rich-html :deep(table) {
  width: 100%;
  border-collapse: collapse;
}

.rich-html :deep(td),
.rich-html :deep(th) {
  padding: 6px 8px;
  border: 1px solid var(--el-border-color);
}
</style>
