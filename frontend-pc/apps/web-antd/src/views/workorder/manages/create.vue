<script lang="ts" setup>
import { ref } from 'vue';
import { useRouter } from 'vue-router';

import { Card, Button, message, PageHeader } from 'ant-design-vue';

import { addApi } from '#/api/workorder/work-orders';

import Write from './components/write.vue';

const router = useRouter();
const writeRef = ref<any>();
const loading = ref(false);

const handleSave = async () => {
  const data = await writeRef.value?.submit();
  if (data) {
    loading.value = true;
    try {
      await addApi(data);
      message.success('创建成功');
      router.push('/workorder/manages/index');
    } finally {
      loading.value = false;
    }
  }
};

const onBack = () => {
  router.back();
};
</script>

<template>
  <div class="p-4">
    <PageHeader title="新增工单" @back="onBack" />
    <Card>
      <Write ref="writeRef" />
      <div class="mt-4 flex justify-center gap-4">
        <Button @click="onBack">取消</Button>
        <Button type="primary" :loading="loading" @click="handleSave">提交工单</Button>
      </div>
    </Card>
  </div>
</template>
