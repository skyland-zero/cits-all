<script setup lang="ts">
import { onMounted } from 'vue';

import { useOidcStore } from '#/store';

defineOptions({ name: 'OidcLogin' });

onMounted(async () => {
  await login();
});

const oidcStore = useOidcStore();

async function login() {
  // eslint-disable-next-line no-console
  console.log('login');
  await oidcStore.signIn();
}
</script>

<template>
  <div></div>
</template>
