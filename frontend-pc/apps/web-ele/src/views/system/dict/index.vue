<script lang="ts" setup>
import { onMounted, reactive, ref } from 'vue';
import { useRouter } from 'vue-router';
import {
  ElButton,
  ElDialog,
  ElForm,
  ElFormItem,
  ElInput,
  ElMessage,
  ElMessageBox,
  ElPagination,
  ElTable,
  ElTableColumn,
} from 'element-plus';
import {
  getDictTypes,
  createDictType,
  updateDictType,
  deleteDictType,
} from '#/api/system/dict';
import { MyContainer } from '#/components';

const router = useRouter();

const tableData = ref<any[]>([]);
const loading = ref(false);
const total = ref(0);

const queryForm = reactive({
  keyword: '',
  currentPage: 1,
  maxResultCount: 10,
});

const dialogVisible = ref(false);
const dialogTitle = ref('');
const formLoading = ref(false);
const isEdit = ref(false);

const formData = reactive({
  id: '',
  code: '',
  name: '',
  description: '',
});

const formRef = ref();
const rules = {
  code: [{ required: true, message: '请输入字典编码', trigger: 'blur' }],
  name: [{ required: true, message: '请输入字典名称', trigger: 'blur' }],
};

const fetchData = async () => {
  try {
    loading.value = true;
    const res = await getDictTypes(
      {
        currentPage: queryForm.currentPage,
        maxResultCount: queryForm.maxResultCount,
      },
      { keyword: queryForm.keyword },
    );
    tableData.value = res.items || [];
    total.value = res.totalCount || 0;
  } catch (error) {
    console.error(error);
  } finally {
    loading.value = false;
  }
};

const handleSearch = () => {
  queryForm.currentPage = 1;
  fetchData();
};

const handleReset = () => {
  queryForm.keyword = '';
  handleSearch();
};

const handleAdd = () => {
  isEdit.value = false;
  dialogTitle.value = '新增字典分类';
  formData.id = '';
  formData.code = '';
  formData.name = '';
  formData.description = '';
  dialogVisible.value = true;
};

const handleEdit = (row: any) => {
  isEdit.value = true;
  dialogTitle.value = '编辑字典分类';
  formData.id = row.id;
  formData.code = row.code;
  formData.name = row.name;
  formData.description = row.description;
  dialogVisible.value = true;
};

const handleDelete = (row: any) => {
  ElMessageBox.confirm(`确定要删除字典分类 [${row.name}] 吗？`, '提示', {
    type: 'warning',
  })
    .then(async () => {
      try {
        await deleteDictType(row.id);
        ElMessage.success('删除成功');
        fetchData();
      } catch (e) {
        console.error(e);
      }
    })
    .catch(() => {});
};

const handleManageItems = (row: any) => {
  router.push(`/system/dict/items/${row.id}?code=${row.code}&name=${row.name}`);
};

const submitForm = () => {
  formRef.value.validate(async (valid: boolean) => {
    if (!valid) return;
    try {
      formLoading.value = true;
      if (isEdit.value) {
        await updateDictType(formData.id, {
          name: formData.name,
          description: formData.description,
        });
        ElMessage.success('修改成功');
      } else {
        await createDictType(formData);
        ElMessage.success('新增成功');
      }
      dialogVisible.value = false;
      fetchData();
    } catch (e) {
      console.error(e);
    } finally {
      formLoading.value = false;
    }
  });
};

const handleSizeChange = (val: number) => {
  queryForm.maxResultCount = val;
  fetchData();
};

const handleCurrentChange = (val: number) => {
  queryForm.currentPage = val;
  fetchData();
};

onMounted(() => {
  fetchData();
});
</script>

<template>
  <MyContainer :show-footer="true" :show-header="true">
    <template #header>
      <ElForm
        :inline="true"
        :model="queryForm"
        class="demo-form-inline ml-[18px] mr-[18px] mt-[18px]"
      >
        <ElFormItem label="关键字">
          <ElInput
            v-model="queryForm.keyword"
            clearable
            placeholder="编码/名称"
            style="width: 240px"
            @keyup.enter="handleSearch"
          />
        </ElFormItem>
      </ElForm>
    </template>

    <template #table-header>
      <div class="flex w-full items-center justify-between gap-3">
        <div class="flex items-center gap-2">
          <ElButton type="primary" @click="handleSearch">查询</ElButton>
          <ElButton @click="handleReset">重置</ElButton>
          <div class="mx-1 h-4 border-l border-gray-200"></div>
          <ElButton plain type="primary" @click="handleAdd">新增字典</ElButton>
        </div>
      </div>
    </template>

    <ElTable v-loading="loading" :data="tableData" class="w-full" height="100%">
      <ElTableColumn prop="code" label="字典编码" width="200" />
      <ElTableColumn prop="name" label="字典名称" width="200" />
      <ElTableColumn prop="description" label="描述" />
      <ElTableColumn prop="creationTime" label="创建时间" width="180" />
      <ElTableColumn align="center" fixed="right" label="操作" width="250">
        <template #default="{ row }">
          <ElButton
            size="small"
            text
            type="primary"
            @click="handleManageItems(row)"
          >
            字典数据
          </ElButton>
          <ElButton size="small" text type="primary" @click="handleEdit(row)">
            编辑
          </ElButton>
          <ElButton size="small" text type="danger" @click="handleDelete(row)">
            删除
          </ElButton>
        </template>
      </ElTableColumn>
    </ElTable>

    <template #table-footer>
      <div class="flex w-full items-center justify-between">
        <span class="text-sm text-gray-500">共 {{ total }} 条</span>
        <ElPagination
          v-model:current-page="queryForm.currentPage"
          v-model:page-size="queryForm.maxResultCount"
          :page-sizes="[10, 20, 50, 100]"
          layout="sizes, prev, pager, next, jumper"
          :total="total"
          @size-change="handleSizeChange"
          @current-change="handleCurrentChange"
        />
      </div>
    </template>

    <template #dialog>
      <ElDialog
        v-model="dialogVisible"
        :close-on-click-modal="false"
        :show-close="false"
        :title="dialogTitle"
        destroy-on-close
        draggable
        lock-scroll
        width="500"
      >
        <ElForm
          ref="formRef"
          :model="formData"
          :rules="rules"
          label-width="80px"
        >
          <ElFormItem label="字典编码" prop="code">
            <ElInput
              v-model="formData.code"
              :disabled="isEdit"
              placeholder="如: sys_user_status"
            />
          </ElFormItem>
          <ElFormItem label="字典名称" prop="name">
            <ElInput v-model="formData.name" placeholder="如: 用户状态" />
          </ElFormItem>
          <ElFormItem label="描述" prop="description">
            <ElInput v-model="formData.description" :rows="3" type="textarea" />
          </ElFormItem>
        </ElForm>
        <template #footer>
          <div class="dialog-footer">
            <ElButton :loading="formLoading" @click="dialogVisible = false">
              关闭
            </ElButton>
            <ElButton :loading="formLoading" type="primary" @click="submitForm">
              保存
            </ElButton>
          </div>
        </template>
      </ElDialog>
    </template>
  </MyContainer>
</template>
