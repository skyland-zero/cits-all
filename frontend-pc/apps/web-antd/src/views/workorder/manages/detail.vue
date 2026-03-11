<script lang="ts" setup>
import { onMounted, ref } from 'vue';

import {
  Descriptions,
  DescriptionsItem,
  Tag,
  Timeline,
  TimelineItem,
  Button,
  message,
} from 'ant-design-vue';

import { fireApi, getApi } from '#/api/workorder/work-orders';

import { WorkOrderPriority, WorkOrderStatus } from './enums';

const props = defineProps({
  currentRow: {
    type: Object,
    required: true,
  },
});

const detail = ref<any>({});
const logs = ref<any[]>([]);
const loading = ref(false);

const fetchData = async () => {
  loading.value = true;
  try {
    const res = await getApi(props.currentRow.id);
    detail.value = res;
  } catch (error) {
    console.error(error);
  } finally {
    loading.value = false;
  }
};

const handleAction = async (trigger: string) => {
  try {
    await fireApi(props.currentRow.id, trigger);
    message.success('操作成功');
    fetchData();
  } catch (error) {
    console.error(error);
  }
};

onMounted(() => {
  fetchData();
});

const getStatusColor = (status: number) => {
  switch (status) {
    case WorkOrderStatus.Draft: return 'default';
    case WorkOrderStatus.PendingAssignment: return 'orange';
    case WorkOrderStatus.InProgress: return 'blue';
    case WorkOrderStatus.PendingApproval: return 'purple';
    case WorkOrderStatus.Completed: return 'green';
    case WorkOrderStatus.Canceled: return 'red';
    default: return 'default';
  }
};
</script>

<template>
  <div v-loading="loading">
    <div class="mb-4 flex gap-2">
      <Button v-if="detail.currentStatus === WorkOrderStatus.Draft" type="primary" @click="handleAction('submit')">提交</Button>
      <Button v-if="detail.currentStatus === WorkOrderStatus.PendingAssignment" type="primary" @click="handleAction('assign')">指派</Button>
      <Button v-if="detail.currentStatus === WorkOrderStatus.InProgress" type="primary" @click="handleAction('finish')">上报完成</Button>
      <Button v-if="detail.currentStatus === WorkOrderStatus.PendingApproval" type="primary" @click="handleAction('approve')">审核通过</Button>
      <Button v-if="detail.currentStatus === WorkOrderStatus.PendingApproval" danger @click="handleAction('reject')">驳回</Button>
    </div>

    <Descriptions bordered title="基础信息">
      <DescriptionsItem label="标题" :span="3">{{ detail.title }}</DescriptionsItem>
      <DescriptionsItem label="工单号">{{ detail.orderNo }}</DescriptionsItem>
      <DescriptionsItem label="状态">
        <Tag :color="getStatusColor(detail.currentStatus)">{{ WorkOrderStatus[detail.currentStatus] }}</Tag>
      </DescriptionsItem>
      <DescriptionsItem label="优先级">
        <Tag>{{ WorkOrderPriority[detail.priority] }}</Tag>
      </DescriptionsItem>
      <DescriptionsItem label="联系人">{{ detail.contactName }}</DescriptionsItem>
      <DescriptionsItem label="联系电话">{{ detail.contactPhone }}</DescriptionsItem>
      <DescriptionsItem label="截止时间">{{ detail.deadlineTime }}</DescriptionsItem>
      <DescriptionsItem label="描述" :span="3">{{ detail.description }}</DescriptionsItem>
    </Descriptions>

    <div v-if="detail.attachmentList?.length" class="mt-4">
      <div class="font-bold mb-2">附件列表：</div>
      <div v-for="file in detail.attachmentList" :key="file.id" class="mb-1">
        <a :href="file.url" target="_blank" class="text-blue-500 underline">{{ file.originalName }}</a>
      </div>
    </div>
  </div>
</template>
