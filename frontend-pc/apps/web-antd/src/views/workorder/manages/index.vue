<script lang="ts" setup>
import { onMounted, reactive, ref } from 'vue';
import { useRouter } from 'vue-router';

import {
  Table,
  Button,
  Form,
  FormItem,
  Input,
  Select,
  SelectOption,
  Tag,
  Modal,
  message,
  Card,
} from 'ant-design-vue';

import { addApi, deleteApi, editApi, pageApi } from '#/api/workorder/work-orders';

import Write from './components/write.vue';
import WorkOrderStatsCard from './components/WorkOrderStatsCard.vue';
import Detail from './detail.vue';
import { actionEnum, WorkOrderPriority, WorkOrderStatus } from './enums';

const router = useRouter();
const tableData = ref<any[]>([]);
const statsCardRef = ref<any>();
const loading = ref(false);
const dialogVisible = ref(false);
const detailVisible = ref(false);
const actionType = ref(actionEnum.none);
const currentRow = ref<any>(null);
const writeRef = ref<any>();

const pager = reactive({
  currentPage: 1,
  maxResultCount: 10,
  totalCount: 0,
});

const searchForm = reactive({
  title: '',
  orderNo: '',
  currentStatus: undefined,
});

const columns = [
  { title: '工单号', dataIndex: 'orderNo', width: 180 },
  { title: '标题', dataIndex: 'title', ellipsis: true },
  { title: '优先级', dataIndex: 'priority', width: 100 },
  { title: '状态', dataIndex: 'currentStatus', width: 100 },
  { title: '创建时间', dataIndex: 'createdTime', width: 180 },
  { title: '操作', key: 'action', width: 220, fixed: 'right' },
];

const fetchData = async () => {
  loading.value = true;
  try {
    const res = await pageApi(pager, searchForm);
    tableData.value = res.items;
    pager.totalCount = res.totalCount;
    statsCardRef.value?.refresh();
  } finally {
    loading.value = false;
  }
};

onMounted(fetchData);

const onAdd = () => {
  router.push('/workorder/manages/create');
};

const onView = (row: any) => {
  currentRow.value = row;
  detailVisible.value = true;
};

const onEdit = (row: any) => {
  currentRow.value = row;
  actionType.value = actionEnum.edit;
  dialogVisible.value = true;
};

const onDelete = (row: any) => {
  currentRow.value = row;
  Modal.confirm({
    title: '确定要删除此工单吗？',
    content: row.title,
    onOk: async () => {
      await deleteApi(row.id);
      message.success('删除成功');
      fetchData();
    },
  });
};

const handleSave = async () => {
  const data = await writeRef.value?.submit();
  if (data) {
    if (actionType.value === actionEnum.edit) {
      await editApi(currentRow.value.id, data);
    } else {
      await addApi(data);
    }
    message.success('保存成功');
    dialogVisible.value = false;
    fetchData();
  }
};
</script>

<template>
  <div class="p-4">
    <WorkOrderStatsCard ref="statsCardRef" />

    <Card class="mb-4">
      <Form layout="inline" :model="searchForm">
        <FormItem label="标题">
          <Input v-model:value="searchForm.title" placeholder="请输入标题" />
        </FormItem>
        <FormItem label="状态">
          <Select v-model:value="searchForm.currentStatus" style="width: 150px" allow-clear>
            <SelectOption :value="0">草稿</SelectOption>
            <SelectOption :value="1">待分派</SelectOption>
            <SelectOption :value="2">处理中</SelectOption>
          </Select>
        </FormItem>
        <FormItem>
          <Button type="primary" @click="fetchData">查询</Button>
          <Button class="ml-2" @click="() => { Object.assign(searchForm, { title: '', currentStatus: undefined }); fetchData(); }">重置</Button>
        </FormItem>
      </Form>
    </Card>

    <Card>
      <div class="mb-4">
        <Button type="primary" @click="onAdd">新增工单</Button>
      </div>
      <Table :columns="columns" :data-source="tableData" :loading="loading" :pagination="{
        current: pager.currentPage,
        pageSize: pager.maxResultCount,
        total: pager.totalCount,
        onChange: (page, size) => { pager.currentPage = page; pager.maxResultCount = size; fetchData(); }
      }">
        <template #bodyCell="{ column, record }">
          <template v-if="column.dataIndex === 'priority'">
            <Tag>{{ WorkOrderPriority[record.priority] }}</Tag>
          </template>
          <template v-if="column.dataIndex === 'currentStatus'">
            <Tag color="blue">{{ WorkOrderStatus[record.currentStatus] }}</Tag>
          </template>
          <template v-if="column.key === 'action'">
            <Button type="link" @click="onView(record)">详情</Button>
            <Button type="link" @click="onEdit(record)">编辑</Button>
            <Button type="link" danger @click="onDelete(record)">删除</Button>
          </template>
        </template>
      </Table>
    </Card>

    <Modal v-model:visible="dialogVisible" title="编辑工单" @ok="handleSave" width="600px">
      <Write ref="writeRef" :current-row="currentRow" />
    </Modal>

    <Modal v-model:visible="detailVisible" title="工单详情" :footer="null" width="800px">
      <Detail :current-row="currentRow" />
    </Modal>
  </div>
</template>
