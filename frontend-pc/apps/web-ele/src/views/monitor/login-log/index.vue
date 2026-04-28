<script lang="ts" setup>
import { onMounted, reactive, ref, watch } from 'vue';
import {
  ElButton,
  ElForm,
  ElFormItem,
  ElInput,
  ElPagination,
  ElSelect,
  ElOption,
  ElTable,
  ElTableColumn,
  ElTag,
} from 'element-plus';
import { getLoginLogListApi, type LoginLogDto } from '#/api/monitor/login-log';
import { MyContainer } from '#/components';

const tableData = ref<LoginLogDto[]>([]);
const loading = ref(false);

const initFormSearchData = () => ({
  userName: '',
  ip: '',
  status: undefined as boolean | undefined,
});

const initPager = () => ({
  totalCount: 0,
  maxResultCount: 20,
  currentPage: 1,
});

const formSearchData = reactive(initFormSearchData());
const pager = reactive(initPager());

/**
 * 搜索
 */
const search = async () => {
  loading.value = true;
  try {
    const params = {
      skipCount: (pager.currentPage - 1) * pager.maxResultCount,
      maxResultCount: pager.maxResultCount,
      userName: formSearchData.userName || undefined,
      ip: formSearchData.ip || undefined,
      status: formSearchData.status,
    };
    const res = await getLoginLogListApi(params);
    tableData.value = res.items;
    pager.totalCount = res.totalCount;
  } catch (error) {
    console.error('获取登录日志失败:', error);
  } finally {
    loading.value = false;
  }
};

/**
 * 重置
 */
const onReset = () => {
  Object.assign(pager, initPager());
  Object.assign(formSearchData, initFormSearchData());
  search();
};

// 监听分页变化
watch(
  () => [pager.currentPage, pager.maxResultCount],
  () => {
    search();
  },
);

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
        <ElFormItem label="用户名称">
          <ElInput
            v-model="formSearchData.userName"
            clearable
            placeholder="请输入用户名/姓名"
            style="width: 200px"
          />
        </ElFormItem>
        <ElFormItem label="登录地址">
          <ElInput
            v-model="formSearchData.ip"
            clearable
            placeholder="请输入IP地址"
            style="width: 200px"
          />
        </ElFormItem>
        <ElFormItem label="登录状态">
          <ElSelect
            v-model="formSearchData.status"
            clearable
            placeholder="请选择状态"
            style="width: 150px"
          >
            <ElOption :value="true" label="成功" />
            <ElOption :value="false" label="失败" />
          </ElSelect>
        </ElFormItem>
        <ElFormItem>
          <ElButton type="primary" @click="search">查询</ElButton>
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
      <ElTableColumn label="用户名称" min-width="120">
        <template #default="scope">
          <span>{{ scope.row.userName }}</span>
          <span v-if="scope.row.realName" class="ml-1 text-gray-400">({{ scope.row.realName }})</span>
        </template>
      </ElTableColumn>
      <ElTableColumn label="登录地址 (IP)" prop="ip" width="140" />
      <ElTableColumn label="登录地点" prop="location" width="120">
        <template #default="scope">
          {{ scope.row.location || '-' }}
        </template>
      </ElTableColumn>
      <ElTableColumn label="浏览器" width="160" show-overflow-tooltip>
        <template #default="scope">
          <span>{{ scope.row.browser }}</span>
          <span v-if="scope.row.browserInfo" class="ml-1 text-xs text-gray-400">{{ scope.row.browserInfo }}</span>
        </template>
      </ElTableColumn>
      <ElTableColumn label="操作系统/设备" width="160" show-overflow-tooltip>
        <template #default="scope">
          <span>{{ scope.row.os }}</span>
          <ElTag v-if="scope.row.device" size="small" effect="plain" class="ml-1" type="info">
            {{ scope.row.device }}
          </ElTag>
        </template>
      </ElTableColumn>
      <ElTableColumn align="center" label="状态" prop="status" width="100">
        <template #default="scope">
          <ElTag :type="scope.row.status ? 'success' : 'danger'">
            {{ scope.row.status ? '成功' : '失败' }}
          </ElTag>
        </template>
      </ElTableColumn>
      <ElTableColumn label="操作信息" prop="message" min-width="150" show-overflow-tooltip />
      <ElTableColumn label="User-Agent" prop="userAgent" min-width="150" show-overflow-tooltip />
      <ElTableColumn label="登录时间" prop="loginTime" width="180">
        <template #default="scope">
          {{ new Date(scope.row.loginTime).toLocaleString() }}
        </template>
      </ElTableColumn>
    </ElTable>

    <template #table-footer>
      <ElPagination
        v-model:current-page="pager.currentPage"
        v-model:page-size="pager.maxResultCount"
        :total="pager.totalCount"
        layout="prev, pager, next, jumper, total"
      />
    </template>
  </MyContainer>
</template>

<style scoped>
.ml-1 {
  margin-left: 4px;
}
</style>
