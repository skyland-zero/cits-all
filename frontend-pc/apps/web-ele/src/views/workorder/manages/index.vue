<!-- eslint-disable no-console -->
<script lang="ts" setup>
import { onActivated, onMounted, reactive, ref, watch } from 'vue';
import { useRouter } from 'vue-router';

import {
  CircleX as MdiDelete,
  Eye as MdiEye,
  Plus as MdiAdd,
  UserRoundPen as MdiEdit,
} from '@vben/icons';

import {
  ElButton,
  ElCol,
  ElDialog,
  ElForm,
  ElFormItem,
  ElInput,
  ElMessage,
  ElOption,
  ElPagination,
  ElRow,
  ElSelect,
  ElTable,
  ElTableColumn,
  ElTag,
} from 'element-plus';

import {
  addApi,
  deleteApi,
  editApi,
  getApi,
  pageApi,
} from '#/api/workorder/work-orders';
import { MyContainer } from '#/components';

import Write from './components/write.vue';
import WorkOrderStatsCard from './components/WorkOrderStatsCard.vue';
import Detail from './detail.vue';
import { actionEnum, WorkOrderPriority, WorkOrderStatus } from './enums';

defineOptions({ name: 'WorkOrderManages' });

const router = useRouter();

const tableData = ref<any[]>([]);
const statsCardRef = ref<InstanceType<typeof WorkOrderStatsCard>>();
const currentRow = ref<any | null>(null);
const dialogFormVisible = ref(false);
const detailVisible = ref(false);
const actionType = ref(actionEnum.none);
const dialogTitle = ref('');
const writeRef = ref<InstanceType<typeof Write>>();
const saveLoading = ref(false);
const loading = ref(false);

const initFormSearchData = () => ({
  title: undefined,
  orderNo: undefined,
  currentStatus: undefined,
});
const initPager = () => ({
  totalCount: 0,
  maxResultCount: 20,
  currentPage: 1,
});
const formSearchData = reactive(initFormSearchData());
const pager = reactive(initPager());

const statusOptions = [
  { label: '草稿', value: WorkOrderStatus.Draft },
  { label: '待分派', value: WorkOrderStatus.PendingAssignment },
  { label: '处理中', value: WorkOrderStatus.InProgress },
  { label: '待审核', value: WorkOrderStatus.PendingApproval },
  { label: '已完成', value: WorkOrderStatus.Completed },
  { label: '已作废', value: WorkOrderStatus.Canceled },
];

/**
 * 搜索
 */
const search = async () => {
  loading.value = true;
  try {
    const res = await pageApi(pager, formSearchData);
    tableData.value = res.items;
    pager.totalCount = res.totalCount;
    // 刷新统计数据
    statsCardRef.value?.refresh();
  } catch (error) {
    console.error(error);
  } finally {
    loading.value = false;
  }
};

/**
 * 重置
 */
const onReset = () => {
  Object.assign(pager, initPager()); // 重置分页
  Object.assign(formSearchData, initFormSearchData()); // 重置搜索条件
  search();
};

/**
 * 点击新建
 */
const onAdd = () => {
  router.push('/workorder/manages/create');
};

/**
 * 点击编辑
 * @param row
 */
const loadCurrentRow = async (id: string) => {
  loading.value = true;
  try {
    return await getApi(id);
  } catch (error) {
    console.error(error);
    return null;
  } finally {
    loading.value = false;
  }
};

const onEdit = async (row: any) => {
  actionType.value = actionEnum.edit;
  dialogTitle.value = '编辑工单';
  const detail = await loadCurrentRow(row.id);
  if (!detail) {
    return;
  }
  currentRow.value = detail;
  dialogFormVisible.value = true;
};

/**
 * 查看详情
 * @param row
 */
const onView = async (row: any) => {
  const detail = await loadCurrentRow(row.id);
  if (!detail) {
    return;
  }
  currentRow.value = detail;
  detailVisible.value = true;
};

/**
 * 删除
 * @param row
 */
const onDelete = (row: any) => {
  actionType.value = actionEnum.delete;
  dialogTitle.value = '提示';
  currentRow.value = row;
  dialogFormVisible.value = true;
};

/**
 * 保存数据
 */
const save = async () => {
  switch (actionType.value) {
    case actionEnum.add:
    case actionEnum.edit: {
      const write = writeRef.value;
      const formData = await write?.submit();
      if (formData) {
        saveLoading.value = true;
        const api =
          actionType.value === actionEnum.add
            ? addApi(formData)
            : editApi(currentRow.value.id, formData);
        api
          .then(() => {
            ElMessage({
              message: '保存成功',
              type: 'success',
            });
            dialogFormVisible.value = false;
            search();
          })
          .catch(() => { })
          .finally(() => {
            saveLoading.value = false;
          });
      }
      break;
    }
    case actionEnum.delete: {
      saveLoading.value = true;
      deleteApi(currentRow.value.id)
        .then(() => {
          ElMessage({
            message: '删除成功',
            type: 'success',
          });
          dialogFormVisible.value = false;
          search();
        })
        .catch(() => { })
        .finally(() => {
          saveLoading.value = false;
        });
      break;
    }
  }
};

const close = () => {
  dialogFormVisible.value = false;
};

const closeDetail = () => {
  detailVisible.value = false;
};

// 深度监听对象变化
watch(
  () => ({
    page: pager.currentPage,
    size: pager.maxResultCount,
  }),
  () => {
    search();
  },
);

onMounted(() => {
  search();
});

onActivated(() => {
  search();
});

const getPriorityLabel = (val: number) => {
  switch (val) {
    case WorkOrderPriority.Critical: {
      return '紧急';
    }
    case WorkOrderPriority.High: {
      return '高';
    }
    case WorkOrderPriority.Low: {
      return '低';
    }
    case WorkOrderPriority.Medium: {
      return '中';
    }
    default: {
      return '未知';
    }
  }
};

const getPriorityTagType = (val: number) => {
  switch (val) {
    case WorkOrderPriority.Critical: {
      return 'danger';
    }
    case WorkOrderPriority.High: {
      return 'warning';
    }
    case WorkOrderPriority.Low: {
      return 'info';
    }
    case WorkOrderPriority.Medium: {
      return undefined;
    } // default
    default: {
      return 'info';
    }
  }
};

const getStatusLabel = (val: number) => {
  const item = statusOptions.find((opt) => opt.value === val);
  return item ? item.label : '未知';
};
const getStatusTagType = (val: number) => {
  switch (val) {
    case WorkOrderStatus.Canceled: {
      return 'danger';
    }
    case WorkOrderStatus.Completed: {
      return 'success';
    }
    case WorkOrderStatus.Draft: {
      return 'info';
    }
    case WorkOrderStatus.InProgress: {
      return undefined;
    }
    case WorkOrderStatus.PendingApproval: {
      return 'success';
    } // or warning
    case WorkOrderStatus.PendingAssignment: {
      return 'warning';
    }
    default: {
      return 'info';
    }
  }
};
</script>

<template>
  <MyContainer :show-aside="false" :show-footer="true" :show-left-aside="false">
    <template #header>
      <div class="p-4 bg-white mb-4">
        <WorkOrderStatsCard ref="statsCardRef" />
      </div>
      <ElForm :inline="true" :model="formSearchData"
        class="demo-form-inline ml-[18px] mr-[18px] mt-[18px]">
        <ElFormItem label="标题">
          <ElInput v-model="formSearchData.title" clearable placeholder="请输入标题" style="width: 200px" />
        </ElFormItem>
        <ElFormItem label="工单号">
          <ElInput v-model="formSearchData.orderNo" clearable placeholder="请输入工单号" style="width: 200px" />
        </ElFormItem>
        <ElFormItem label="状态">
          <ElSelect v-model="formSearchData.currentStatus" clearable placeholder="请选择状态" style="width: 200px">
            <ElOption v-for="item in statusOptions" :key="item.value" :label="item.label" :value="item.value" />
          </ElSelect>
        </ElFormItem>
        <ElFormItem>
          <ElButton type="primary" @click="search">查询</ElButton>
          <ElButton type="" @click="onReset">重置</ElButton>
        </ElFormItem>
      </ElForm>
    </template>
    <template #table-header>
      <ElRow>
        <ElCol :span="12" class="pl-[8px]">
          <ElButton :icon="MdiAdd" plain size="small" type="primary" @click="onAdd()">
            新增
          </ElButton>
        </ElCol>
      </ElRow>
    </template>
    <ElTable :data="tableData" class="w-full" height="100%" row-key="id" v-loading="loading">
      <ElTableColumn label="工单号" prop="orderNo" width="180" />
      <ElTableColumn label="标题" min-width="200" prop="title" show-overflow-tooltip />
      <ElTableColumn align="center" label="优先级" prop="priority" width="100">
        <template #default="scope">
          <ElTag :type="getPriorityTagType(scope.row.priority)">
            {{ getPriorityLabel(scope.row.priority) }}
          </ElTag>
        </template>
      </ElTableColumn>
      <ElTableColumn align="center" label="状态" prop="currentStatus" width="100">
        <template #default="scope">
          <ElTag :type="getStatusTagType(scope.row.currentStatus)">
            {{ getStatusLabel(scope.row.currentStatus) }}
          </ElTag>
        </template>
      </ElTableColumn>
      <ElTableColumn label="创建时间" prop="createdTime" width="180">
        <template #default="scope">
          {{
            scope.row.createdTime
              ? new Date(scope.row.createdTime).toLocaleString()
              : '-'
          }}
        </template>
      </ElTableColumn>
      <ElTableColumn label="截止时间" prop="deadlineTime" width="180">
        <template #default="scope">
          {{
            scope.row.deadlineTime
              ? new Date(scope.row.deadlineTime).toLocaleString()
              : '-'
          }}
        </template>
      </ElTableColumn>

      <ElTableColumn align="center" class="operation" fixed="right" label="操作" width="220">
        <template #default="scope">
          <ElButton :icon="MdiEye" size="small" text type="primary" @click="onView(scope.row)">
            详情
          </ElButton>
          <ElButton :icon="MdiEdit" size="small" text type="primary" @click="onEdit(scope.row)">
            编辑
          </ElButton>
          <ElButton :icon="MdiDelete" size="small" text type="danger" @click="onDelete(scope.row)">
            删除
          </ElButton>
        </template>
      </ElTableColumn>
    </ElTable>
    <template #table-footer>
      <ElPagination v-model:current-page="pager.currentPage" v-model:page-size="pager.maxResultCount"
        :total="pager.totalCount" layout="prev, pager, next, jumper, total" />
    </template>
    <template #dialog>
      <ElDialog v-model="dialogFormVisible" :close-on-click-modal="false" :show-close="false" :title="dialogTitle"
        destroy-on-close draggable lock-scroll width="600px">
        <Write v-if="actionType === actionEnum.add || actionType === actionEnum.edit" ref="writeRef"
          :current-row="currentRow" />
        <span v-if="actionType === actionEnum.delete">
          确定要删工单【{{ currentRow.title }}】?
        </span>
        <template #footer>
          <div class="dialog-footer">
            <ElButton :loading="saveLoading" @click="close">关闭</ElButton>
            <ElButton :loading="saveLoading" type="primary" @click="save">
              保存
            </ElButton>
          </div>
        </template>
      </ElDialog>

      <!-- Detail Dialog -->
      <ElDialog v-model="detailVisible" :close-on-click-modal="true" destroy-on-close draggable title="工单详情"
        width="800px">
        <Detail v-if="detailVisible" :current-row="currentRow" />
        <template #footer>
          <ElButton @click="closeDetail">关闭</ElButton>
        </template>
      </ElDialog>
    </template>
  </MyContainer>
</template>
