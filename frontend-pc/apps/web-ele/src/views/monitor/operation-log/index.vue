<script lang="ts" setup>
import type {
  OperationLogCursorItemDto,
  OperationLogDetailDto,
} from '#/api/monitor/operation-log';

import { onMounted, reactive, ref } from 'vue';

import { ArrowLeft, ArrowRight } from '@element-plus/icons-vue';
import {
  ElButton,
  ElButtonGroup,
  ElDatePicker,
  ElDescriptions,
  ElDescriptionsItem,
  ElDrawer,
  ElForm,
  ElFormItem,
  ElIcon,
  ElInput,
  ElOption,
  ElSelect,
  ElTable,
  ElTableColumn,
  ElTag,
} from 'element-plus';

import {
  getOperationLogDetailApi,
  getOperationLogListApi,
} from '#/api/monitor/operation-log';
import { MyContainer } from '#/components';

const tableData = ref<OperationLogCursorItemDto[]>([]);
const loading = ref(false);
const detailVisible = ref(false);
const detailData = ref<null | OperationLogDetailDto>(null);
const detailLoading = ref(false);

const timeRange = ref<[Date, Date] | null>(null);

const initFormSearchData = () => ({
  module: '',
  operationType: '',
  status: undefined as boolean | undefined,
  startTime: undefined as string | undefined,
  endTime: undefined as string | undefined,
});

const formSearchData = reactive(initFormSearchData());

// 游标分页状态
const paginationState = reactive({
  maxResultCount: 20,
  hasMore: false,
  nextCursor: undefined as string | undefined,
  nextCursorTime: undefined as string | undefined,
  nextCursorId: undefined as string | undefined,

  // 记录历史游标以支持“上一页”
  // 第一个元素是第一页的游标（全部 undefined）
  history: [
    {
      cursor: undefined as string | undefined,
      cursorTime: undefined as string | undefined,
      cursorId: undefined as string | undefined,
    },
  ],
  currentIndex: 0,
});

/**
 * 搜索
 */
const search = async (isLoadMore = false) => {
  loading.value = true;
  try {
    if (!isLoadMore) {
      // 重新搜索时，重置游标状态为第一页
      paginationState.currentIndex = 0;
      paginationState.history = [
        {
          cursor: undefined,
          cursorTime: undefined,
          cursorId: undefined,
        },
      ];
    }

    if (timeRange.value && timeRange.value.length === 2) {
      // 转换为 ISO 格式并带上时区信息或者转为UTC
      formSearchData.startTime = timeRange.value[0].toISOString();
      formSearchData.endTime = timeRange.value[1].toISOString();
    } else {
      formSearchData.startTime = undefined;
      formSearchData.endTime = undefined;
    }

    const currentCursor = paginationState.history[paginationState.currentIndex];

    const params = {
      maxResultCount: paginationState.maxResultCount,
      module: formSearchData.module || undefined,
      operationType: formSearchData.operationType || undefined,
      status: formSearchData.status,
      startTime: formSearchData.startTime,
      endTime: formSearchData.endTime,
      cursor: currentCursor?.cursor,
      cursorTime: currentCursor?.cursorTime,
      cursorId: currentCursor?.cursorId,
    };

    const res = await getOperationLogListApi(params);
    tableData.value = res.items;

    // 更新下一页状态
    paginationState.hasMore = res.hasMore;
    paginationState.nextCursor = res.nextCursor;
    paginationState.nextCursorTime = res.nextCursorTime;
    paginationState.nextCursorId = res.nextCursorId;
  } catch (error) {
    console.error('获取操作日志失败:', error);
  } finally {
    loading.value = false;
  }
};

/**
 * 下一页
 */
const nextPage = () => {
  if (!paginationState.hasMore) return;

  // 如果当前是历史记录的最后一项，则添加新的游标
  if (paginationState.currentIndex === paginationState.history.length - 1) {
    paginationState.history.push({
      cursor: paginationState.nextCursor,
      cursorTime: paginationState.nextCursorTime,
      cursorId: paginationState.nextCursorId,
    });
  }

  paginationState.currentIndex++;
  search(true);
};

/**
 * 上一页
 */
const prevPage = () => {
  if (paginationState.currentIndex > 0) {
    paginationState.currentIndex--;
    search(true);
  }
};

/**
 * 重置
 */
const onReset = () => {
  Object.assign(formSearchData, initFormSearchData());
  timeRange.value = null;
  search();
};

/**
 * 查看详情
 */
const handleDetail = async (row: OperationLogCursorItemDto) => {
  detailVisible.value = true;
  detailLoading.value = true;
  detailData.value = null;

  try {
    const res = await getOperationLogDetailApi(row.id);
    detailData.value = res;
  } catch (error) {
    console.error('获取详情失败', error);
  } finally {
    detailLoading.value = false;
  }
};

onMounted(() => {
  search();
});
</script>

<template>
  <MyContainer :show-footer="true">
    <template #header>
      <ElForm
        :inline="true"
        :model="formSearchData"
        class="demo-form-inline ml-[18px] mr-[18px] mt-[18px]"
      >
        <ElFormItem label="模块名称">
          <ElInput
            v-model="formSearchData.module"
            clearable
            placeholder="请输入模块名称"
            style="width: 180px"
          />
        </ElFormItem>
        <ElFormItem label="操作类型">
          <ElInput
            v-model="formSearchData.operationType"
            clearable
            placeholder="请输入操作类型"
            style="width: 180px"
          />
        </ElFormItem>
        <ElFormItem label="操作时间">
          <ElDatePicker
            v-model="timeRange"
            type="datetimerange"
            range-separator="至"
            start-placeholder="开始日期"
            end-placeholder="结束日期"
            style="width: 320px"
          />
        </ElFormItem>
        <ElFormItem label="操作状态">
          <ElSelect
            v-model="formSearchData.status"
            clearable
            placeholder="请选择"
            style="width: 120px"
          >
            <ElOption :value="true" label="成功" />
            <ElOption :value="false" label="失败" />
          </ElSelect>
        </ElFormItem>
        <ElFormItem>
          <ElButton type="primary" @click="search(false)">查询</ElButton>
          <ElButton @click="onReset">重置</ElButton>
        </ElFormItem>
      </ElForm>
    </template>

    <ElTable
      v-loading="loading"
      :data="tableData"
      class="w-full"
      height="100%"
      row-key="id"
    >
      <ElTableColumn
        label="模块"
        prop="module"
        min-width="120"
        show-overflow-tooltip
      />
      <ElTableColumn
        label="操作类型"
        prop="operationType"
        min-width="120"
        show-overflow-tooltip
      />
      <ElTableColumn label="操作人" min-width="150" show-overflow-tooltip>
        <template #default="scope">
          <span>{{ scope.row.operatorName || '-' }}</span>
          <span
            v-if="scope.row.departmentPath"
            class="block truncate text-xs text-gray-400"
          >
            {{ scope.row.departmentPath }}
          </span>
        </template>
      </ElTableColumn>
      <ElTableColumn label="IP / 地点" min-width="160" show-overflow-tooltip>
        <template #default="scope">
          <div>{{ scope.row.operationIp || '-' }}</div>
          <div class="text-xs text-gray-400">
            {{ scope.row.operationLocation || '-' }}
          </div>
        </template>
      </ElTableColumn>
      <ElTableColumn label="请求路径" min-width="200" show-overflow-tooltip>
        <template #default="scope">
          <ElTag
            v-if="scope.row.requestMethod"
            size="small"
            type="info"
            class="mr-1"
          >
            {{ scope.row.requestMethod }}
          </ElTag>
          <span class="font-mono text-sm">{{
            scope.row.requestPath || '-'
          }}</span>
        </template>
      </ElTableColumn>
      <ElTableColumn align="center" label="状态" width="100">
        <template #default="scope">
          <ElTag :type="scope.row.status ? 'success' : 'danger'">
            {{ scope.row.status ? '成功' : '失败' }}
          </ElTag>
        </template>
      </ElTableColumn>
      <ElTableColumn
        align="right"
        label="耗时(ms)"
        prop="elapsedMilliseconds"
        width="100"
      />
      <ElTableColumn label="操作时间" width="170">
        <template #default="scope">
          {{ new Date(scope.row.operationTime).toLocaleString() }}
        </template>
      </ElTableColumn>
      <ElTableColumn label="操作" width="90" fixed="right" align="center">
        <template #default="scope">
          <ElButton type="primary" link @click="handleDetail(scope.row)">
            详情
          </ElButton>
        </template>
      </ElTableColumn>
    </ElTable>

    <template #table-footer>
      <div class="flex w-full items-center justify-between">
        <span class="text-sm text-gray-500">
          基于游标的分页（每次加载 {{ paginationState.maxResultCount }} 条）
        </span>
        <ElButtonGroup>
          <ElButton
            :icon="ArrowLeft"
            :disabled="paginationState.currentIndex === 0"
            @click="prevPage"
          >
            上一页
          </ElButton>
          <ElButton :disabled="!paginationState.hasMore" @click="nextPage">
            下一页<ElIcon class="el-icon--right"><ArrowRight /></ElIcon>
          </ElButton>
        </ElButtonGroup>
      </div>
    </template>

    <template #dialog>
      <ElDrawer
        v-model="detailVisible"
        title="操作日志详情"
        size="50%"
        destroy-on-close
      >
        <div v-loading="detailLoading" class="p-4">
          <ElDescriptions :column="2" border v-if="detailData">
            <ElDescriptionsItem label="模块名称">
              {{ detailData.module || '-' }}
            </ElDescriptionsItem>
            <ElDescriptionsItem label="操作类型">
              {{ detailData.operationType || '-' }}
            </ElDescriptionsItem>
            <ElDescriptionsItem label="操作人">
              {{ detailData.operatorName || '-' }}
            </ElDescriptionsItem>
            <ElDescriptionsItem label="操作时间">
              {{ new Date(detailData.operationTime).toLocaleString() }}
            </ElDescriptionsItem>
            <ElDescriptionsItem label="IP地址">
              {{ detailData.operationIp || '-' }}
            </ElDescriptionsItem>
            <ElDescriptionsItem label="操作地点">
              {{ detailData.operationLocation || '-' }}
            </ElDescriptionsItem>
            <ElDescriptionsItem label="部门路径" :span="2">
              {{ detailData.departmentPath || '-' }}
            </ElDescriptionsItem>
            <ElDescriptionsItem label="请求方式">
              {{ detailData.requestMethod || '-' }}
            </ElDescriptionsItem>
            <ElDescriptionsItem label="请求路径">
              {{ detailData.requestPath || '-' }}
            </ElDescriptionsItem>
            <ElDescriptionsItem label="耗时(ms)">
              {{ detailData.elapsedMilliseconds }}
            </ElDescriptionsItem>
            <ElDescriptionsItem label="状态">
              <ElTag :type="detailData.status ? 'success' : 'danger'">
                {{ detailData.status ? '成功' : '失败' }}
              </ElTag>
            </ElDescriptionsItem>

            <ElDescriptionsItem
              label="请求参数"
              :span="2"
              v-if="detailData.requestParameters"
            >
              <div
                class="max-h-40 overflow-auto whitespace-pre-wrap break-all rounded bg-gray-50 p-2 font-mono text-sm"
              >
                {{ detailData.requestParameters }}
              </div>
            </ElDescriptionsItem>

            <ElDescriptionsItem
              label="响应参数"
              :span="2"
              v-if="detailData.responseParameters"
            >
              <div
                class="max-h-60 overflow-auto whitespace-pre-wrap break-all rounded bg-gray-50 p-2 font-mono text-sm"
              >
                {{ detailData.responseParameters }}
              </div>
            </ElDescriptionsItem>

            <ElDescriptionsItem
              label="异常信息"
              :span="2"
              v-if="detailData.errorMessage"
            >
              <div
                class="max-h-40 overflow-auto whitespace-pre-wrap break-all rounded bg-red-50 p-2 font-mono text-sm text-red-600"
              >
                {{ detailData.errorMessage }}
              </div>
            </ElDescriptionsItem>
          </ElDescriptions>
        </div>
      </ElDrawer>
    </template>
  </MyContainer>
</template>
