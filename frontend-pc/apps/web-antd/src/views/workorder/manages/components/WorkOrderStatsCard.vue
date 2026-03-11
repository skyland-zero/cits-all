<script lang="ts" setup>
import { onMounted, ref } from 'vue';

import { Card, Col, Row, Statistic } from 'ant-design-vue';

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
    <Row :gutter="16">
      <Col :span="6">
        <Card>
          <Statistic title="总工单数" :value="stats.totalCount" />
        </Card>
      </Col>
      <Col :span="6">
        <Card>
          <Statistic title="结单率" :value="stats.completionRate * 100" suffix="%" :precision="2" />
        </Card>
      </Col>
      <Col :span="6">
        <Card>
          <Statistic title="平均处理时长" :value="stats.averageProcessingTimeMinutes" suffix=" min" :precision="1" />
        </Card>
      </Col>
      <Col :span="6">
        <Card>
          <Statistic title="待分派工单" :value="stats.statusDistribution?.find((x: any) => x.key === 1)?.count || 0" />
        </Card>
      </Col>
    </Row>
  </div>
</template>
