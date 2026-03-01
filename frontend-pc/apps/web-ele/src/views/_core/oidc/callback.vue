<script lang="ts" setup>
import { onMounted } from 'vue';

import { ElLoading, ElMessageBox } from 'element-plus';

import { useOidcStore } from '#/store';

defineOptions({ name: 'OidcCallback' });

const oidcStore = useOidcStore();

onMounted(async () => {
  await callback();
});

async function callback() {
  const loadingInstance = ElLoading.service({
    fullscreen: true,
    text: '正在登录，请稍候...',
  });

  try {
    const user = await oidcStore.callback();
    if (!user) {
      onError();
    }
  } catch {
    onError();
  } finally {
    loadingInstance.close();
  }
}

async function onError() {
  ElMessageBox.alert('登录出现异常，请重新登录', '错误', {
    // if you want to disable its autofocus
    // autofocus: false,
    confirmButtonText: '重新登录',
    showClose: false,
    type: 'error',
    callback: () => {
      oidcStore.signIn();
    },
  });
}
</script>

<template>
  <div></div>
</template>
