<!-- eslint-disable no-console -->
<script lang="ts" setup>
import type { PropType } from 'vue';
import { ElDescriptions, ElDescriptionsItem, ElTag } from 'element-plus';
import { WorkOrderStatus, WorkOrderPriority } from './enums';

defineProps({
  currentRow: {
    type: Object as PropType<any>,
    default: () => ({}),
  },
});

const getPriorityLabel = (val: number) => {
  switch (val) {
    case WorkOrderPriority.Low: return '低';
    case WorkOrderPriority.Medium: return '中';
    case WorkOrderPriority.High: return '高';
    case WorkOrderPriority.Critical: return '紧急';
    default: return '未知';
  }
};

const getStatusLabel = (val: number) => {
  switch (val) {
    case WorkOrderStatus.Draft: return '草稿';
    case WorkOrderStatus.PendingAssignment: return '待分派';
    case WorkOrderStatus.InProgress: return '处理中';
    case WorkOrderStatus.PendingApproval: return '待审核';
    case WorkOrderStatus.Completed: return '已完成';
    case WorkOrderStatus.Canceled: return '已作废';
    default: return '未知';
  }
};

const getPriorityTagType = (val: number) => {
  switch (val) {
    case WorkOrderPriority.Low: return 'info';
    case WorkOrderPriority.Medium: return '';
    case WorkOrderPriority.High: return 'warning';
    case WorkOrderPriority.Critical: return 'danger';
    default: return 'info';
  }
};

const getStatusTagType = (val: number) => {
  switch (val) {
    case WorkOrderStatus.Draft: return 'info';
    case WorkOrderStatus.PendingAssignment: return 'warning';
    case WorkOrderStatus.InProgress: return '';
    case WorkOrderStatus.PendingApproval: return 'success';
    case WorkOrderStatus.Completed: return 'success';
    case WorkOrderStatus.Canceled: return 'danger';
    default: return 'info';
  }
};

const formatDate = (date: string | null) => {
  if (!date) return '-';
  return new Date(date).toLocaleString();
}
</script>

<template>
  <div class="p-4">
    <ElDescriptions title="基本信息" :column="2" border>
      <ElDescriptionsItem label="工单编号">{{ currentRow.orderNo }}</ElDescriptionsItem>
      <ElDescriptionsItem label="标题">{{ currentRow.title }}</ElDescriptionsItem>

      <ElDescriptionsItem label="状态">
        <ElTag :type="getStatusTagType(currentRow.currentStatus)" size="small">
          {{ getStatusLabel(currentRow.currentStatus) }}
        </ElTag>
      </ElDescriptionsItem>

      <ElDescriptionsItem label="优先级">
        <ElTag :type="getPriorityTagType(currentRow.priority)" size="small">
          {{ getPriorityLabel(currentRow.priority) }}
        </ElTag>
      </ElDescriptionsItem>

      <ElDescriptionsItem label="联系人">{{ currentRow.contactName || '-' }}</ElDescriptionsItem>
      <ElDescriptionsItem label="联系电话">{{ currentRow.contactPhone || '-' }}</ElDescriptionsItem>

      <ElDescriptionsItem label="地点" :span="2">{{ currentRow.location || '-' }}</ElDescriptionsItem>

      <ElDescriptionsItem label="创建时间">{{ formatDate(currentRow.createdTime) }}</ElDescriptionsItem>
      <ElDescriptionsItem label="截止时间">{{ formatDate(currentRow.deadlineTime) }}</ElDescriptionsItem>

      <ElDescriptionsItem label="描述" :span="2">
        <div class="whitespace-pre-wrap">{{ currentRow.description || '-' }}</div>
      </ElDescriptionsItem>
    </ElDescriptions>
  </div>
</template>
