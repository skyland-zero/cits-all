<script lang="ts" setup>
import { onMounted, ref } from 'vue';
import { RotateCw as MdiRefresh } from '@vben/icons';
import {
  ElButton,
  ElMessage,
  ElMessageBox,
  ElTabPane,
  ElTabs,
  ElTable,
  ElTableColumn,
  ElTag,
} from 'element-plus';
import {
  getJobStats,
  getRecurringJobs,
  getProcessingJobs,
  getFailedJobs,
  triggerRecurringJob,
  removeRecurringJob,
  deleteJob,
} from '#/api/system/job';
import { MyContainer } from '#/components';

const activeName = ref('recurring');
const loading = ref(false);

const stats = ref<any>({});
const recurringJobs = ref<any[]>([]);
const processingJobs = ref<any[]>([]);
const failedJobs = ref<any[]>([]);

const fetchStats = async () => {
  try {
    stats.value = await getJobStats();
  } catch (error) {
    console.error(error);
  }
};

const fetchRecurring = async () => {
  try {
    loading.value = true;
    recurringJobs.value = await getRecurringJobs();
  } finally {
    loading.value = false;
  }
};

const fetchProcessing = async () => {
  try {
    loading.value = true;
    processingJobs.value = await getProcessingJobs(0, 50);
  } finally {
    loading.value = false;
  }
};

const fetchFailed = async () => {
  try {
    loading.value = true;
    failedJobs.value = await getFailedJobs(0, 50);
  } finally {
    loading.value = false;
  }
};

const handleTabClick = () => {
  if (activeName.value === 'recurring') fetchRecurring();
  if (activeName.value === 'processing') fetchProcessing();
  if (activeName.value === 'failed') fetchFailed();
};

const handleRefresh = () => {
  fetchStats();
  handleTabClick();
};

const handleTrigger = (id: string) => {
  ElMessageBox.confirm('确定要立即执行该任务吗？', '提示')
    .then(async () => {
      await triggerRecurringJob(id);
      ElMessage.success('已触发执行');
      fetchStats();
    })
    .catch(() => {});
};

const handleRemoveRecurring = (id: string) => {
  ElMessageBox.confirm('确定要删除该周期任务吗？', '提示', { type: 'warning' })
    .then(async () => {
      await removeRecurringJob(id);
      ElMessage.success('已删除');
      fetchRecurring();
      fetchStats();
    })
    .catch(() => {});
};

const handleDeleteJob = (id: string) => {
  ElMessageBox.confirm('确定要删除该任务记录吗？', '提示', { type: 'warning' })
    .then(async () => {
      await deleteJob(id);
      ElMessage.success('已删除');
      if (activeName.value === 'processing') fetchProcessing();
      if (activeName.value === 'failed') fetchFailed();
      fetchStats();
    })
    .catch(() => {});
};

onMounted(() => {
  fetchStats();
  fetchRecurring();
});
</script>

<template>
  <MyContainer :show-header="true">
    <template #header>
      <div class="grid gap-3 px-[18px] pt-[18px] sm:grid-cols-2 xl:grid-cols-5">
        <div
          class="rounded-lg border border-gray-200 bg-white/80 px-4 py-3 shadow-sm"
        >
          <div class="mb-2 text-sm text-gray-500">执行中</div>
          <div class="text-2xl font-bold text-blue-500">
            {{ stats.processing ?? 0 }}
          </div>
        </div>
        <div
          class="rounded-lg border border-gray-200 bg-white/80 px-4 py-3 shadow-sm"
        >
          <div class="mb-2 text-sm text-gray-500">排队中</div>
          <div class="text-2xl font-bold text-yellow-500">
            {{ stats.enqueued ?? 0 }}
          </div>
        </div>
        <div
          class="rounded-lg border border-gray-200 bg-white/80 px-4 py-3 shadow-sm"
        >
          <div class="mb-2 text-sm text-gray-500">已完成</div>
          <div class="text-2xl font-bold text-green-500">
            {{ stats.succeeded ?? 0 }}
          </div>
        </div>
        <div
          class="rounded-lg border border-gray-200 bg-white/80 px-4 py-3 shadow-sm"
        >
          <div class="mb-2 text-sm text-gray-500">失败</div>
          <div class="text-2xl font-bold text-red-500">
            {{ stats.failed ?? 0 }}
          </div>
        </div>
        <div
          class="rounded-lg border border-gray-200 bg-white/80 px-4 py-3 shadow-sm"
        >
          <div class="mb-2 text-sm text-gray-500">周期任务</div>
          <div class="text-2xl font-bold text-purple-500">
            {{ stats.recurring ?? 0 }}
          </div>
        </div>
      </div>
    </template>

    <template #table-header>
      <div class="flex w-full items-center justify-between gap-3">
        <ElTabs v-model="activeName" class="flex-1" @tab-click="handleTabClick">
          <ElTabPane label="周期任务" name="recurring" />
          <ElTabPane label="执行中" name="processing" />
          <ElTabPane label="失败" name="failed" />
        </ElTabs>
        <ElButton
          :icon="MdiRefresh"
          plain
          size="small"
          type="primary"
          @click="handleRefresh"
        >
          刷新
        </ElButton>
      </div>
    </template>

    <ElTable
      v-if="activeName === 'recurring'"
      v-loading="loading"
      :data="recurringJobs"
      class="w-full"
      height="100%"
    >
      <ElTableColumn prop="id" label="任务ID" width="150" />
      <ElTableColumn prop="jobName" label="任务名称" min-width="200" />
      <ElTableColumn prop="cron" label="Cron 表达式" width="150" />
      <ElTableColumn prop="nextExecution" label="下次执行" width="180" />
      <ElTableColumn prop="lastExecution" label="上次执行" width="180" />
      <ElTableColumn label="上次状态" prop="lastJobState" width="100">
        <template #default="{ row }">
          <ElTag v-if="row.lastJobState">{{ row.lastJobState }}</ElTag>
        </template>
      </ElTableColumn>
      <ElTableColumn align="center" fixed="right" label="操作" width="150">
        <template #default="{ row }">
          <ElButton
            size="small"
            text
            type="primary"
            @click="handleTrigger(row.id)"
          >
            执行
          </ElButton>
          <ElButton
            size="small"
            text
            type="danger"
            @click="handleRemoveRecurring(row.id)"
          >
            删除
          </ElButton>
        </template>
      </ElTableColumn>
    </ElTable>

    <ElTable
      v-else-if="activeName === 'processing'"
      v-loading="loading"
      :data="processingJobs"
      class="w-full"
      height="100%"
    >
      <ElTableColumn prop="id" label="Job ID" width="120" />
      <ElTableColumn prop="jobName" label="任务名称" min-width="200" />
      <ElTableColumn prop="serverId" label="服务器" width="200" />
      <ElTableColumn prop="startedAt" label="开始时间" width="180" />
      <ElTableColumn align="center" fixed="right" label="操作" width="100">
        <template #default="{ row }">
          <ElButton
            size="small"
            text
            type="danger"
            @click="handleDeleteJob(row.id)"
          >
            删除
          </ElButton>
        </template>
      </ElTableColumn>
    </ElTable>

    <ElTable
      v-else
      v-loading="loading"
      :data="failedJobs"
      class="w-full"
      height="100%"
    >
      <ElTableColumn prop="id" label="Job ID" width="120" />
      <ElTableColumn prop="jobName" label="任务名称" min-width="200" />
      <ElTableColumn prop="failedAt" label="失败时间" width="180" />
      <ElTableColumn
        prop="exceptionDetails"
        label="异常信息"
        show-overflow-tooltip
      />
      <ElTableColumn align="center" fixed="right" label="操作" width="100">
        <template #default="{ row }">
          <ElButton
            size="small"
            text
            type="danger"
            @click="handleDeleteJob(row.id)"
          >
            删除
          </ElButton>
        </template>
      </ElTableColumn>
    </ElTable>
  </MyContainer>
</template>
