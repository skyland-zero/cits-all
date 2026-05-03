<script lang="ts" setup>
import { onMounted, reactive, ref, watch } from 'vue';

import {
  CircleX as MdiOffline,
  RotateCw as MdiReset,
  Search as MdiSearch,
} from '@vben/icons';

import {
  ElButton,
  ElForm,
  ElFormItem,
  ElInput,
  ElMessage,
  ElMessageBox,
  ElPagination,
  ElSwitch,
  ElTable,
  ElTableColumn,
  ElTag,
} from 'element-plus';

import {
  getOnlineUsers,
  type OnlineUserSessionDto,
  revokeOnlineUser,
  revokeOnlineUserSession,
} from '#/api/monitor/online-users';
import { MyContainer } from '#/components';

const initFormSearchData = () => ({
  ip: '',
  keyword: '',
  onlineOnly: true,
});

const initPager = () => ({
  currentPage: 1,
  maxResultCount: 20,
  totalCount: 0,
});

const tableData = ref<OnlineUserSessionDto[]>([]);
const formSearchData = reactive(initFormSearchData());
const pager = reactive(initPager());
const loading = ref(false);

const search = async () => {
  loading.value = true;
  try {
    const res = await getOnlineUsers(pager, formSearchData);
    tableData.value = res.items ?? [];
    pager.totalCount = res.totalCount ?? 0;
  } finally {
    loading.value = false;
  }
};

const onSearch = () => {
  pager.currentPage = 1;
  search();
};

const onReset = () => {
  Object.assign(pager, initPager());
  Object.assign(formSearchData, initFormSearchData());
  search();
};

const formatDateTime = (value?: null | string) => {
  if (!value) return '-';
  return value.replace('T', ' ').slice(0, 19);
};

const onRevokeSession = (row: OnlineUserSessionDto) => {
  ElMessageBox.confirm(
    `确定要强制下线用户【${row.surname || row.userName}】当前会话吗？`,
    '提示',
    { type: 'warning' },
  )
    .then(async () => {
      await revokeOnlineUserSession(row.sessionId, '管理员强制下线当前会话');
      ElMessage.success('已强制下线');
      search();
    })
    .catch(() => {});
};

const onRevokeUser = (row: OnlineUserSessionDto) => {
  ElMessageBox.confirm(
    `确定要强制下线用户【${row.surname || row.userName}】全部会话吗？`,
    '提示',
    { type: 'warning' },
  )
    .then(async () => {
      await revokeOnlineUser(row.userId, '管理员强制下线全部会话');
      ElMessage.success('已强制下线该用户全部会话');
      search();
    })
    .catch(() => {});
};

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
</script>

<template>
  <MyContainer :show-footer="true" :show-header="true">
    <template #header>
      <ElForm
        :inline="true"
        :model="formSearchData"
        class="demo-form-inline no-action-align ml-[18px] mr-[18px] mt-[18px]"
      >
        <ElFormItem label="用户">
          <ElInput
            v-model="formSearchData.keyword"
            clearable
            placeholder="账号/姓名"
            style="width: 200px"
            @keyup.enter="onSearch"
          />
        </ElFormItem>
        <ElFormItem label="IP">
          <ElInput
            v-model="formSearchData.ip"
            clearable
            placeholder="请输入 IP"
            style="width: 200px"
            @keyup.enter="onSearch"
          />
        </ElFormItem>
        <ElFormItem label="仅在线">
          <ElSwitch v-model="formSearchData.onlineOnly" @change="onSearch" />
        </ElFormItem>
      </ElForm>
    </template>

    <template #table-header>
      <div class="flex w-full items-center justify-between gap-3">
        <div class="flex items-center gap-2">
          <ElButton :icon="MdiSearch" type="primary" @click="onSearch">
            查询
          </ElButton>
          <ElButton :icon="MdiReset" @click="onReset">重置</ElButton>
        </div>
        <span class="text-sm text-gray-500">在线窗口：最近 5 分钟活跃</span>
      </div>
    </template>

    <ElTable
      v-loading="loading"
      :data="tableData"
      class="w-full"
      height="100%"
      row-key="id"
    >
      <ElTableColumn fixed label="账号" min-width="140" prop="userName" />
      <ElTableColumn label="姓名" min-width="120" prop="surname" />
      <ElTableColumn label="IP" min-width="140" prop="ip" />
      <ElTableColumn label="状态" width="100">
        <template #default="{ row }">
          <ElTag :type="row.isOnline ? 'success' : 'info'">
            {{ row.isOnline ? '在线' : row.isRevoked ? '已下线' : '离线' }}
          </ElTag>
        </template>
      </ElTableColumn>
      <ElTableColumn label="登录时间" width="170">
        <template #default="{ row }">
          {{ formatDateTime(row.loginTime) }}
        </template>
      </ElTableColumn>
      <ElTableColumn label="最后活跃" width="170">
        <template #default="{ row }">
          {{ formatDateTime(row.lastActiveTime) }}
        </template>
      </ElTableColumn>
      <ElTableColumn
        label="User-Agent"
        min-width="260"
        prop="userAgent"
        show-overflow-tooltip
      />
      <ElTableColumn align="center" fixed="right" label="操作" width="220">
        <template #default="{ row }">
          <ElButton
            :disabled="row.isRevoked"
            :icon="MdiOffline"
            size="small"
            text
            type="danger"
            @click="onRevokeSession(row)"
          >
            下线会话
          </ElButton>
          <ElButton
            :disabled="row.isRevoked"
            size="small"
            text
            type="danger"
            @click="onRevokeUser(row)"
          >
            下线用户
          </ElButton>
        </template>
      </ElTableColumn>
    </ElTable>

    <template #table-footer>
      <div class="flex w-full items-center justify-between">
        <span class="text-sm text-gray-500">共 {{ pager.totalCount }} 条</span>
        <ElPagination
          v-model:current-page="pager.currentPage"
          v-model:page-size="pager.maxResultCount"
          :page-sizes="[10, 20, 50, 100]"
          :total="pager.totalCount"
          layout="sizes, prev, pager, next, jumper"
        />
      </div>
    </template>
  </MyContainer>
</template>
