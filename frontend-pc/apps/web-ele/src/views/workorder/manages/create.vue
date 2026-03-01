<script lang="ts" setup>
import { ref } from 'vue';
import { useRouter } from 'vue-router';

import { ElButton, ElCard, ElMessage, ElPageHeader } from 'element-plus';

import { addApi } from '#/api/workorder/work-orders';

import Write from './components/write.vue';

const router = useRouter();
const writeRef = ref();
const loading = ref(false);

const goBack = () => {
  router.push('/workorder/manages/index');
};

const onSubmit = async () => {
  const write = writeRef.value;
  const formData = await write?.submit();
  if (formData) {
    loading.value = true;
    try {
      await addApi(formData);
      ElMessage.success('创建成功');
      router.push('/workorder/manages/index');
    } catch (error) {
      console.error(error);
    } finally {
      loading.value = false;
    }
  }
};
</script>

<template>
  <div class="p-4">
    <ElCard shadow="never">
      <template #header>
        <ElPageHeader content="新增工单" @back="goBack" />
      </template>
      <div class="mx-auto max-w-3xl py-6">
        <Write ref="writeRef" />
        <div class="mt-8 flex justify-end">
          <ElButton @click="goBack">取消</ElButton>
          <ElButton :loading="loading" type="primary" @click="onSubmit">
            提交
          </ElButton>
        </div>
      </div>
    </ElCard>
  </div>
</template>
