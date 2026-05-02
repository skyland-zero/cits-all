<script lang="ts" setup>
import { onMounted, reactive, ref } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import {
  ElButton,
  ElDialog,
  ElForm,
  ElFormItem,
  ElInput,
  ElInputNumber,
  ElMessage,
  ElMessageBox,
  ElPagination,
  ElSwitch,
  ElTable,
  ElTableColumn,
  ElTag,
} from 'element-plus';
import {
  getDictItems,
  createDictItem,
  updateDictItem,
  deleteDictItem,
} from '#/api/system/dict';
import { MyContainer } from '#/components';

const route = useRoute();
const router = useRouter();

const dictTypeId = route.params.id as string;
const dictCode = route.query.code as string;
const dictName = route.query.name as string;

const tableData = ref<any[]>([]);
const loading = ref(false);
const total = ref(0);

const queryForm = reactive({
  currentPage: 1,
  maxResultCount: 20,
});

const dialogVisible = ref(false);
const dialogTitle = ref('');
const formLoading = ref(false);
const isEdit = ref(false);

const formData = reactive({
  id: '',
  dictTypeId: dictTypeId,
  label: '',
  value: '',
  sort: 0,
  isEnabled: true,
});

const formRef = ref();
const rules = {
  label: [{ required: true, message: '请输入显示标签', trigger: 'blur' }],
  value: [{ required: true, message: '请输入数据值', trigger: 'blur' }],
};

const fetchData = async () => {
  try {
    loading.value = true;
    const res = await getDictItems(
      {
        currentPage: queryForm.currentPage,
        maxResultCount: queryForm.maxResultCount,
      },
      { dictTypeId: dictTypeId },
    );
    tableData.value = res.items || [];
    total.value = res.totalCount || 0;
  } catch (error) {
    console.error(error);
  } finally {
    loading.value = false;
  }
};

const handleAdd = () => {
  isEdit.value = false;
  dialogTitle.value = '新增字典数据';
  formData.id = '';
  formData.label = '';
  formData.value = '';
  formData.sort = 0;
  formData.isEnabled = true;
  dialogVisible.value = true;
};

const handleEdit = (row: any) => {
  isEdit.value = true;
  dialogTitle.value = '编辑字典数据';
  formData.id = row.id;
  formData.label = row.label;
  formData.value = row.value;
  formData.sort = row.sort;
  formData.isEnabled = row.isEnabled;
  dialogVisible.value = true;
};

const handleDelete = (row: any) => {
  ElMessageBox.confirm(`确定要删除字典数据 [${row.label}] 吗？`, '提示', {
    type: 'warning',
  })
    .then(async () => {
      try {
        await deleteDictItem(row.id);
        ElMessage.success('删除成功');
        fetchData();
      } catch (e) {
        console.error(e);
      }
    })
    .catch(() => {});
};

const submitForm = () => {
  formRef.value.validate(async (valid: boolean) => {
    if (!valid) return;
    try {
      formLoading.value = true;
      if (isEdit.value) {
        await updateDictItem(formData.id, formData);
        ElMessage.success('修改成功');
      } else {
        await createDictItem(formData);
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

const handleBack = () => {
  router.push('/system/dict');
};

onMounted(() => {
  if (!dictTypeId) {
    ElMessage.error('无效的字典类型ID');
    handleBack();
    return;
  }
  fetchData();
});
</script>

<template>
  <MyContainer :show-footer="true" :show-header="true">
    <template #header>
      <div class="flex items-center gap-3 px-[18px] pt-[18px]">
        <ElButton text type="primary" @click="handleBack">返回</ElButton>
        <span class="text-lg font-semibold text-gray-700">
          字典数据：{{ dictName }} ({{ dictCode }})
        </span>
      </div>
    </template>

    <template #table-header>
      <div class="flex w-full items-center justify-between gap-3">
        <div class="flex items-center gap-2">
          <ElButton plain type="primary" @click="handleAdd"
            >新增字典数据</ElButton
          >
        </div>
      </div>
    </template>

    <ElTable v-loading="loading" :data="tableData" class="w-full" height="100%">
      <ElTableColumn prop="label" label="显示标签" width="200" />
      <ElTableColumn prop="value" label="数据值" width="200" />
      <ElTableColumn align="center" prop="sort" label="排序" width="100" />
      <ElTableColumn align="center" label="状态" prop="isEnabled" width="100">
        <template #default="{ row }">
          <ElTag :type="row.isEnabled ? 'success' : 'danger'">
            {{ row.isEnabled ? '启用' : '禁用' }}
          </ElTag>
        </template>
      </ElTableColumn>
      <ElTableColumn prop="creationTime" label="创建时间" width="180" />
      <ElTableColumn align="center" fixed="right" label="操作" width="150">
        <template #default="{ row }">
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
          label-width="100px"
        >
          <ElFormItem label="显示标签" prop="label">
            <ElInput v-model="formData.label" placeholder="如: 启用" />
          </ElFormItem>
          <ElFormItem label="数据值" prop="value">
            <ElInput v-model="formData.value" placeholder="如: 1" />
          </ElFormItem>
          <ElFormItem label="排序" prop="sort">
            <ElInputNumber v-model="formData.sort" :min="0" :max="9999" />
          </ElFormItem>
          <ElFormItem label="状态" prop="isEnabled">
            <ElSwitch v-model="formData.isEnabled" />
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
