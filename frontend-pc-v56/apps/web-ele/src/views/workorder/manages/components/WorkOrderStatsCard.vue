<script lang="ts" setup>
import { onMounted, ref } from 'vue';

import { ElCard, ElCol, ElRow, ElStatistic } from 'element-plus';

import { statsApi } from '#/api/workorder/work-orders';

const stats = ref<any>({
  totalCount: 0,
  completionRate: 0,
  averageProcessingTimeMinutes: 0,
});

const loading = ref(false);

const fetchData = async () => {
  loading.value = true;
  try {
    const res = await statsApi();
    stats.value = res;
  } catch (error) {
    console.error(error);
  } finally {
    loading.value = false;
  }
};

onMounted(() => {
  fetchData();
});

defineExpose({
  refresh: fetchData,
});
</script>

<template>
  <div class="mb-4" v-loading="loading">
    <ElRow :gutter="20">
      <ElCol :span="6">
        <ElCard shadow="hover">
          <ElStatistic :value="stats.totalCount" title="总工单数" />
        </ElCard>
      </ElCol>
      <ElCol :span="6">
        <ElCard shadow="hover">
          <ElStatistic :precision="2" :value="stats.completionRate * 100" suffix="%" title="结单率" />
        </ElCard>
      </ElCol>
      <ElCol :span="6">
        <ElCard shadow="hover">
          <ElStatistic :precision="1" :value="stats.averageProcessingTimeMinutes" suffix=" min" title="平均处理时长" />
        </ElCard>
      </ElCol>
      <ElCol :span="6">
        <ElCard shadow="hover">
          <ElStatistic :value="stats.statusDistribution?.find((x: any) => x.key === 1)?.count || 0" title="待分派工单" />
        </ElCard>
      </ElCol>
    </ElRow>
  </div>
</template>

<style scoped>
:deep(.el-statistic__head) {
  font-size: 14px;
  color: #606266;
}

:deep(.el-statistic__content) {
  font-size: 24px;
  font-weight: bold;
  color: #303133;
}
</style>
