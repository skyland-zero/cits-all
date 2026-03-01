<script lang="ts" setup>
import { onMounted } from 'vue';

import { ElLoading, ElMessageBox } from 'element-plus';

import { useOidcStore } from '#/store';

defineOptions({ name: 'OidcSignoutCallback' });

const oidcStore = useOidcStore();

onMounted(async () => {
  await callback();
});

async function callback() {
  const loadingInstance = ElLoading.service({
    fullscreen: true,
    text: '正在退出登录，请稍候...',
  });

  try {
    await oidcStore.signoutCallback();
  } catch {
    onError();
  } finally {
    loadingInstance.close();
  }
}

async function onError() {
  ElMessageBox.alert('退出登录出现异常，请重试', '错误', {
    // if you want to disable its autofocus
    // autofocus: false,
    confirmButtonText: '退出登录',
    showClose: false,
    type: 'error',
    callback: () => {
      oidcStore.logout();
    },
  });
}
</script>

<template>
  <!-- 可以在这个页面显示您已退出登录的提示，并且提供一个重新登录的按钮，而不是直接重新跳转到授权中心登录页面 -->
  <div></div>
</template>
