<script lang="ts" setup>
import { onMounted } from 'vue';

import { useOidcStore } from '#/store';

defineOptions({ name: 'OidcSignoutSilentCallback' });

const oidcStore = useOidcStore();

onMounted(async () => {
  await oidcStore.signoutSilentCallback();
});
</script>

<template>
  <div></div>
</template>
